using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using HG;
using static TurboEdition.Utils.ItemHelpers;

//TODO Get linking effect
//Thinking about siphon but changing the green to red, should be easy
//Get indicator for sphere search radius please
namespace TurboEdition.Equipment
{
    public class Hellchain : EquipmentBase<Hellchain>
    {
        public override string EquipmentName => "Bloody Cross";

        public override string EquipmentLangTokenName => "HELLCHAIN";

        public override string EquipmentPickupDesc => $"Link enemies, making each link take {damageFraction * 100}% of the previous link's damage.";

        public override string EquipmentFullDescription => $"Link enemies around you. Damaging a linked enemy makes that enemy's linked enemy take {damageFraction * 100}% of the damage dealt";

        public override string EquipmentLore => "";

        public override string EquipmentModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        public override string EquipmentIconPath => "@TurboEdition:Assets/Textures/Icons/Equipment/Placeholder_Hellchain.png";
        public override float Cooldown => equipmentRecharge;
        public static BuffIndex linkedBuff;

        private static GameObject linkManager;

        public float equipmentRecharge;
        public bool consumeEquipment;

        public float damageFraction;
        public float sphereSearchRadius;
        public int sphereLinkCount;
        public int sphereSearchCount;
        public int maxLinkCount;
        public float linkedBuffDuration;

        internal override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();
            CreateEquipment();
            Initialization();
            Hooks();
        }

        protected override void CreateConfig(ConfigFile config)
        {
            equipmentRecharge = config.Bind<float>("Equipment : " + EquipmentName, "Recharge time", 60f, "Amount in seconds for this equipment to be available again. For comparison, the highest cooldown is 140s (Preon). Royal Capacitor stands at 20s.").Value;
            //consumeEquipment = config.Bind<bool>("Equipment : " + EquipmentName, "Equipment use if failed", true, "Whenever to consume the equipment and put it on cooldown even if it failed to perform its task.").Value;

            //Equipment stuff
            sphereSearchRadius = config.Bind<float>("Equipment : " + EquipmentName, "Size of the sphereSearch", 30f, "Size in meters for the sphere search to search for enemies in.").Value;
            sphereLinkCount = config.Bind<int>("Equipment : " + EquipmentName, "Sphere link count", 5, "Maximum number of enemies to be linked in each sphere search.").Value;
            linkedBuffDuration = config.Bind<float>("Equipment : " + EquipmentName, "Linking Duration", 25f, "Duration in seconds for the links (linked buff) to stay up.").Value;

            //Component stuff
            damageFraction = config.Bind<float>("Equipment : " + EquipmentName, "Percentage of damage to share", 0.80f, "Percentage of damage that all enemies linked will be damaged for. Hint: The % is taken after all damage reduction calcs, try going after a weak enemy.").Value;
            sphereSearchCount = config.Bind<int>("Equipment : " + EquipmentName, "Additional SphereSearchs", 5, "Number of extra sphere searchs to generate in each use. These generate within enemies.").Value;
            maxLinkCount = config.Bind<int>("Equipment : " + EquipmentName, "Max link count", 100, "Maximum number of BODIES (not just enemies) that can be linked (have the debuff) at the same time.").Value;
           
        }

        private void CreateBuff()
        {
            var linkedBuffDef = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.red,
                canStack = false,
                isDebuff = true,
                name = "Linked",
                iconPath = "@TurboEdition:Assets/Textures/Icons/Buffs/hellchain_linked.png"
            });
            linkedBuff = R2API.BuffAPI.Add(linkedBuffDef);

        }
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override void Initialization()
        {
            //SiphonNearbyController is a component of SiphonNearbyBodyAttatchment
            //The body attachment has the TetherVfxOrigin component. SiphonNearbyController gets it from here
            //SiphonNearbyController has ActiveVfx GameObject, which is a SiphonTetherHealing GameObject
            var linkManagerPrefab = new GameObject("LinkManagerPrefabPrefab");
            var siphonPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/bodyattachments/SiphonNearbyBodyAttachment");
            var siphonController = siphonPrefab.GetComponent<SiphonNearbyController>();

            //Get tetherVFX from literally anywhere
            linkManagerPrefab.AddComponent<LinkComponent>();
            linkManagerPrefab.GetComponent<LinkComponent>().tetherVfxOrigin = siphonPrefab.GetComponent<TetherVfxOrigin>();
            linkManagerPrefab.GetComponent<LinkComponent>().activeVfx = siphonController.GetComponent<GameObject>();
            linkManagerPrefab.AddComponent<NetworkedBodyAttachment>().forceHostAuthority = true;

            linkManager = linkManagerPrefab.InstantiateClone("HitlagManagerPrefabClone");
            UnityEngine.Object.Destroy(linkManagerPrefab);

        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.UpdateBuffs += CheckBuff;
            GlobalEventManager.onServerDamageDealt += RelayLinkDamage;
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
#if DEBUG
            Chat.AddMessage(EquipmentName + " Activated.");
#endif
            GenerateSphereSearch(slot.characterBody);
            AddAditionalLinks(slot.characterBody);

            //if (consumeEquipment) { return true; }
            return true;
        }

        private void GenerateSphereSearch(CharacterBody origin, bool getSameTeam = false)
        {
            List<HurtBox> hurtBoxesList = new List<HurtBox>();
            TeamMask enemyTeams = TeamMask.GetEnemyTeams(origin.teamComponent.teamIndex);
            SphereSearch sphereSearch = new RoR2.SphereSearch()
            {
                mask = LayerIndex.entityPrecise.mask,                       //This should be ok too
                origin = origin.transform.position,                          //This should be ok
                queryTriggerInteraction = QueryTriggerInteraction.Collide,
                radius = sphereSearchRadius
            };
#if DEBUG
            TurboEdition._logger.LogWarning(EquipmentName + ": generated " + sphereSearch + " with radius " + sphereSearchRadius + " and origin " + origin.transform);
#endif
            if (!getSameTeam)
            {
#if DEBUG
                TurboEdition._logger.LogWarning(EquipmentName + ": filtering the sphereSearch by enemy teams.");
#endif
                sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes(hurtBoxesList);
            }
            else
            {
#if DEBUG
                TurboEdition._logger.LogWarning(EquipmentName + ": filtering the sphereSearch by the same team as origin.");
#endif
                TeamMask selfdestructionguaranteed = new TeamMask();
                selfdestructionguaranteed.AddTeam(origin.teamComponent.teamIndex);
                sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(selfdestructionguaranteed).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes(hurtBoxesList);
            }
            
            for (int i = 0;  i < hurtBoxesList.Count && i <= sphereLinkCount; i++)
            {
                var hc = hurtBoxesList[i].healthComponent;
                if (hc)
                {
                    var body = hc.body;
                    if (body)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning(EquipmentName + ": adding linkedBuff to someone, theres " + (i+1) + " out of " + hurtBoxesList.Count + " hurtboxes with max " + sphereLinkCount);
#endif
                        body.AddTimedBuff(linkedBuff, linkedBuffDuration);
                    }
                }
            }
        }

        private bool AddAditionalLinks(CharacterBody cb)
        {
#if DEBUG
            TurboEdition._logger.LogWarning(EquipmentName + ": doing additional links.");
#endif
            TeamIndex ownerTeam = cb.teamComponent.teamIndex;
            for (TeamIndex teamCounter = TeamIndex.Neutral; teamCounter < TeamIndex.Count; teamCounter++)
            {
                if (TeamManager.IsTeamEnemy(ownerTeam, teamCounter))
                {
                    var enemyMember = TeamComponent.GetTeamMembers(teamCounter);
                    if (teamCounter != TeamIndex.Neutral && enemyMember.Count <= 0) { return false; } //It's gonna be hilarious if some enemies will link to barrels and pots 
#if DEBUG
                    TurboEdition._logger.LogWarning(EquipmentName + ": got enemy teams to activator's.");
#endif
                    int link = 0;
                    for (int i = 0; i < enemyMember.Count && link <= sphereSearchCount; i++)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning(EquipmentName + ": doing extra links " + link + 1 + " out of " + sphereSearchCount);
#endif
                        GameObject linkComponent = enemyMember[i].GetComponentInChildren<LinkComponent>()?.gameObject;
                        CharacterBody enemyBody = enemyMember[i].body;

                        //Check if they are already being linked and generate a sphere search on them to get additional targets
                        if (linkComponent && NetworkServer.active)
                        {
#if DEBUG
                            TurboEdition._logger.LogWarning(EquipmentName + ": server is active and someone in the enemy team has a LinkComponent, generating extra SphereSearch within them.");
#endif
                            GenerateSphereSearch(enemyBody, true);
                            link++;
                        }

                    }

                    return true;
                }
            }
            return false;
        }
    

        private void CheckBuff(On.RoR2.CharacterBody.orig_UpdateBuffs orig, CharacterBody self, float deltaTime)
        {
//#if DEBUG
//            TurboEdition._logger.LogWarning(EquipmentName + "'s hooks: checking for linkedBuff.");
//#endif
            var cbGameObject = self.gameObject;
            var linkGameObject = self.GetComponentInChildren<LinkComponent>()?.gameObject;
            if (!self.HasBuff(linkedBuff))
            {
                if (linkGameObject)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning(EquipmentName + "'s hooks: someone doesn't have link debuff but has LinkComponent, destroying.");
#endif
                    UnityEngine.Object.Destroy(linkGameObject);
                }
            }
            else
            {
                if (!linkGameObject)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning(EquipmentName + "'s hooks: someone has the link debuff but no LinkComponent, creating.");
#endif
                    linkGameObject = UnityEngine.Object.Instantiate(linkManager);
                    linkGameObject.GetComponent<LinkComponent>().ownerBody = self;
                    linkGameObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(self.gameObject);
                    cbGameObject.AddComponent<LinkComponent>();
#if DEBUG
                    TurboEdition._logger.LogWarning(EquipmentName + "'s hooks: LinkComponent created.");
#endif
                }
            }
            orig(self, deltaTime);
        }

        private void RelayLinkDamage(DamageReport damageReport)
        {
            var linkGameObject = damageReport.victimBody.gameObject.GetComponentInChildren<LinkComponent>()?.gameObject;
            if (linkGameObject)
            {
                var component = damageReport.victimBody.gameObject.GetComponentInChildren<LinkComponent>();

                //Here we apply damage to the link found on SearchForLink
                if (!NetworkServer.active || !component.hasLinkTarget)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning("LinkComponent has no output link or NetworkServer is not active!");
#endif
                    return;
                }
                DamageInfo damageInfo = new DamageInfo
                {
                    attacker = damageReport.attacker,
                    inflictor = damageReport.victimBody.gameObject,
                    position = damageReport.attacker.transform.position,
                    crit = damageReport.attackerBody.RollCrit(), //Isn't this too much? Who are we refering to? Previous link or the player?
                    damage = damageReport.damageInfo.damage,
                    damageColorIndex = DamageColorIndex.Item,
                    force = Vector3.zero, //Wouldn't it be funny if we set it to non-zero?
                    procCoefficient = 0f, //See: RandomKill line 131
                    damageType = DamageType.Generic, //Thinkan about nonlethal
                    procChainMask = default(ProcChainMask)
                };
#if DEBUG
                TurboEdition._logger.LogWarning("LinkComponent owner got damaged and has output, relaying a new damageInfo.");
#endif
                component.hasLinkTarget.TakeDamage(damageInfo);
            }
        }

        [RequireComponent(typeof(NetworkedBodyAttachment))]
        public class LinkComponent : NetworkBehaviour//, /*IOnDamageDealtServerReceiver,*/ IOnTakeDamageServerReceiver
        {
            [SyncVar] 
            float radius;
            public float NetRadius
            {
                get { return radius; }
                set { base.SetSyncVar<float>(value, ref radius, 1u); }
            }

            [Min(1E-45f)]
            public float tickRate = 0.1f; //One tick every 10 seconds

            protected new Transform transform;
            protected SphereSearch sphereSearch;
            protected float timer;
            public CharacterBody ownerBody;

            //wacky tether thingies

            public TetherVfxOrigin tetherVfxOrigin;
            public GameObject activeVfx;
            public HealthComponent hasLinkTarget;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
            private void Awake()
            {
                this.transform = base.transform;
                //this.networkedBodyAttachment = base.GetComponent<NetworkedBodyAttachment>();
                this.sphereSearch = new SphereSearch();
                this.timer = 0f;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
            private void FixedUpdate()
            {
                this.timer -= Time.fixedDeltaTime;
                if (this.timer <= 0f && !this.hasLinkTarget)
                {
                    this.timer += 1f / this.tickRate;
                    this.SearchForLink();
                }

            }

            /*public void OnTakeDamageServer(DamageReport damageReport)
            {
                //Here we apply damage to the link found on SearchForLink
                if (!NetworkServer.active || !hasLinkTarget)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning("LinkComponent has no output link or NetworkServer is not active!");
#endif
                    return;
                }
                DamageInfo damageInfo = new DamageInfo
                {
                    attacker = damageReport.attacker,
                    inflictor = base.gameObject,
                    position = damageReport.attacker.transform.position,
                    crit = damageReport.attackerBody.RollCrit(), //Isn't this too much? Who are we refering to? Previous link or the player?
                    damage = damageReport.damageInfo.damage,
                    damageColorIndex = DamageColorIndex.Item,
                    force = Vector3.zero, //Wouldn't it be funny if we set it to non-zero?
                    procCoefficient = 0f, //See: RandomKill line 131
                    damageType = DamageType.Generic, //Thinkan about nonlethal
                    procChainMask = default(ProcChainMask)
                };
#if DEBUG
                TurboEdition._logger.LogWarning("LinkComponent owner got damaged and has output, relaying a new damageInfo.");
#endif
                this.hasLinkTarget.TakeDamage(damageInfo);
            }*/

            private void SearchForLink()
            {
                List<HurtBox> hurtBoxList = CollectionPool<HurtBox, List<HurtBox>>.RentCollection(); //Use these to store targets with hurtbox
                List<Transform> transformList = CollectionPool<Transform, List<Transform>>.RentCollection(); //Use this to store the transforms of the targets
                if (ownerBody.HasBuff(linkedBuff)) //This previously used networkedBodyAttachment.attachedBody
                {
#if DEBUG
                    TurboEdition._logger.LogWarning("LinkComponent body has the correct debuff, trying to find a match.");
#endif
                    this.SearchForTargets(hurtBoxList);
                }
                int i = 0;
                while (i < hurtBoxList.Count)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning("LinkComponent searching for a link.");
#endif
                    HurtBox currentHurtbox = hurtBoxList[i];
                    var externalLink = currentHurtbox.GetComponentInChildren<LinkComponent>()?.gameObject; //we get the link component of whatever we are on rn
                    if (!externalLink)
                    {
                        //They dont have a linkComponent, meaning they aren't debuffed and we don't want to do anything with them
#if DEBUG
                        TurboEdition._logger.LogWarning("LinkComponent, match doesn't have LinkComponent, ignoring.");
#endif
                        i++;
                        continue;
                    }
                    if ((!currentHurtbox || !currentHurtbox.healthComponent || !currentHurtbox.healthComponent.alive) /*|| currentHurtbox.GetComponentInChildren<LinkComponent>().hasLinkTarget*/) //Commenting this last part because thats the output link, shouldnt be affect input.
                    {
                        //This means it either has no hurtboxes, no health component, not alive, or is already linked.
#if DEBUG
                        TurboEdition._logger.LogWarning("LinkComponent, match doesn't have a hurtbox, a HealthComponent, or is already dead, ignoring.");
#endif
                        i++;
                        continue;
                    }
                    HealthComponent hcHurtBoxToLink = currentHurtbox.healthComponent;
                    if (!(hcHurtBoxToLink == ownerBody))
                    {
                        //We make sure we aren't linking ourselves
                        Transform transform = hcHurtBoxToLink.body.coreTransform ?? currentHurtbox.transform;
                        transformList.Add(transform);
                        this.hasLinkTarget = hcHurtBoxToLink;
#if DEBUG
                        TurboEdition._logger.LogWarning("LinkComponent, hasLinkTarget: " + hasLinkTarget + " and transform: " + transform);
#endif
                    }
                    if (this.tetherVfxOrigin)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning("LinkComponent, tetherVfxOrigin exists, setting transforms!");
#endif
                        this.tetherVfxOrigin.SetTetheredTransforms(transformList);
                    }
                    if (this.activeVfx)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning("LinkComponent, activeVfx exists, changing enabled/disabled!");
#endif
                        this.activeVfx.SetActive(this.hasLinkTarget);
                    }
                    CollectionPool<Transform, List<Transform>>.ReturnCollection(transformList);
                    CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(hurtBoxList);
                }
#if DEBUG
                TurboEdition._logger.LogWarning("LinkComponent, couldn't find match, what a shame!");
#endif
            }


            protected void SearchForTargets(List<HurtBox> dest)
            {
#if DEBUG
                TurboEdition._logger.LogWarning("LinkComponent did SearchForTargets with destination " + dest);
#endif
                this.sphereSearch.mask = LayerIndex.entityPrecise.mask;
                this.sphereSearch.origin = this.transform.position;
                this.sphereSearch.radius = this.radius;
                this.sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
                this.sphereSearch.RefreshCandidates();
                this.sphereSearch.OrderCandidatesByDistance();
                this.sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                this.sphereSearch.GetHurtBoxes(dest);
                this.sphereSearch.ClearCandidates();
            }

        }
    }
}
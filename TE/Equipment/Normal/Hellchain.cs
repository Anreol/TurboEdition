/*
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

        public override string EquipmentIconPath => "@TurboEdition:Assets/Textures/Icons/Equipment/Placeholder_Scythe.png";
        public override float Cooldown => equipmentRecharge;
        public static BuffIndex linkedBuff;

        private static GameObject linkManager;

        public float equipmentRecharge;
        public bool consumeEquipment;

        public float damageFraction;
        public float sphereSearchRadius;
        public int sphereLinkCount;
        public int linkCount;
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
            consumeEquipment = config.Bind<bool>("Equipment : " + EquipmentName, "Equipment use if failed", true, "Whenever to consume the equipment and put it on cooldown even if it failed to perform its task.").Value;

            //Equipment stuff
            sphereSearchRadius = config.Bind<float>("Equipment : " + EquipmentName, "Size of the sphereSearch", 30f, "Size in meters for the sphere search to search for enemies in.").Value;
            sphereLinkCount = config.Bind<int>("Equipment : " + EquipmentName, "Sphere link count", 20, "Maximum number of enemies to be linked in each equipment activation, within the sphere search.").Value;
            linkedBuffDuration = config.Bind<float>("Equipment : " + EquipmentName, "Linking Duration", 25f, "Duration in seconds for the links (linked buff) to stay up.").Value;

            //Component stuff
            damageFraction = config.Bind<float>("Equipment : " + EquipmentName, "Percentage of damage to share", 0.80f, "Percentage of damage that all enemies linked will be damaged for. Hint: The % is taken after all damage reduction calcs, try going after a weak enemy.").Value;
            linkCount = config.Bind<int>("Equipment : " + EquipmentName, "Link count", 5, "Number of enemies to be linked in each equipment activation, outside the sphere search.").Value;
            maxLinkCount = config.Bind<int>("Equipment : " + EquipmentName, "Max link count", 100, "Maximum number of BODIES (not just enemies) that can be linked (have the debuff) at the same time.").Value;
           
        }

        private void CreateBuff()
        {
            var linkedBuffDef = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.white,
                canStack = false,
                isDebuff = true,
                name = "Shaken",
                iconPath = "@TurboEdition:Assets/Textures/Icons/Buffs/TODO"
            });
            linkedBuff = R2API.BuffAPI.Add(linkedBuffDef);

        }
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override void Initialization()
        {
            var linkManagerPrefab = new GameObject("LinkManagerPrefabPrefab");
            var siphonPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SiphonNearbyController");

            //Get tetherVFX from literally anywhere
            linkManagerPrefab.AddComponent<LinkComponent>();
            linkManagerPrefab.GetComponent<LinkComponent>().tetherVfxOrigin = siphonPrefab.GetComponent<TetherVfxOrigin>;
            linkManagerPrefab.GetComponent<LinkComponent>().activeVfx = siphonPrefab.GetComponent<TetherVfxOrigin>;
            linkManagerPrefab.AddComponent<NetworkedBodyAttachment>().forceHostAuthority = true;

            linkManager = linkManagerPrefab.InstantiateClone("HitlagManagerPrefabClone");
            UnityEngine.Object.Destroy(linkManagerPrefab);

        }

        public override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += CheckLink;
            On.RoR2.CharacterBody.UpdateBuffs += CheckBuff;
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
#if DEBUG
            Chat.AddMessage(EquipmentName + " Activated.");
#endif
            GenerateSphereSearch(slot.characterBody);
            if (FindTarget(slot.characterBody))
            {
#if DEBUG
                TurboEdition._logger.LogWarning(EquipmentName + " Successfully attacked one or more enemies in the last equipment use.");
#endif
                return true;
            }
            if (consumeEquipment) { return true; }
            return false;
        }

        private void GenerateSphereSearch(CharacterBody origin)
        {
            List<HurtBox> hurtBoxesList = new List<HurtBox>();
            TeamMask enemyTeams = TeamMask.GetEnemyTeams(origin.teamComponent.teamIndex);
            hurtBoxesList = new RoR2.SphereSearch()
            {
                mask = LayerIndex.entityPrecise.mask,                       //This should be ok too
                origin = origin.transform.position,                          //This should be ok
                queryTriggerInteraction = QueryTriggerInteraction.Collide,
                radius = sphereSearchRadius
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().ToList();

            for (int i = 0; i < hurtBoxesList.Count && i <= sphereLinkCount; i++)
            {
                var hc = hurtBoxesList[i].healthComponent;
                if (hc)
                {
                    var body = hc.body;
                    if (body)
                    {
                        body.AddTimedBuff(linkedBuff, linkedBuffDuration);
                    }
                }
            }
        }

        private void CheckBuff(On.RoR2.CharacterBody.orig_UpdateBuffs orig, CharacterBody self)
        {
            //orig(self);
            var cbGameObject = self.gameObject;
            var linkGameObject = self.GetComponentInChildren<LinkComponent>()?.gameObject;
            if (!self.HasBuff(linkedBuff))
            {
                if (linkGameObject)
                {
                    UnityEngine.Object.Destroy(linkGameObject);
                }
            }
            if (self.HasBuff(linkedBuff))
            {
                
                if (!linkGameObject)
                {
                    linkGameObject = UnityEngine.Object.Instantiate(linkManager);
                    linkGameObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(self.gameObject);
                    cbGameObject.AddComponent<LinkComponent>();
                    cbGameObject.GetComponent<LinkComponent>().isLinked = false;
                }
            }
            
        }

        [RequireComponent(typeof(NetworkedBodyAttachment))]
        public class LinkComponent : NetworkBehaviour, /*IOnDamageDealtServerReceiver,*/ /* IOnTakeDamageServerReceiver
        {
            [SyncVar] 
            float radius;
            public float netRadius
            {
                get { return radius; }
                set { base.SetSyncVar<float>(value, ref radius, 1u); }
            }

            [Min(1E-45f)]
            public float tickRate = 2f;
            protected NetworkedBodyAttachment networkedBodyAttachment;
            protected new Transform transform;
            protected SphereSearch sphereSearch;
            protected float timer;

            //wacky tether thingies
            public TetherVfxOrigin tetherVfxOrigin;
            public GameObject activeVfx;

            public bool isLinked;


            private void Awake()
            {
                this.transform = base.transform;
                this.networkedBodyAttachment = base.GetComponent<NetworkedBodyAttachment>();
                this.sphereSearch = new SphereSearch();
                this.timer = 0f;
            }

            private void FixedUpdate()
            {
                this.timer -= Time.fixedDeltaTime;
                if (this.timer <= 0f)
                {
                    this.timer += 1f / this.tickRate;
                    this.Tick();
                }

            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                //Here we apply damage to the link found on SearchForLink
                if (!NetworkServer.active || !isLinked)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning("LinkComponent is not linked or NetworkServer is not active!");
#endif
                    return;
                }
                DamageInfo damageInfo = new DamageInfo();

                damageInfo.attacker = damageReport.attacker;
                damageInfo.inflictor = base.gameObject;
                damageInfo.position = damageReport.attacker.transform.position;
                damageInfo.crit = damageReport.attackerBody.RollCrit(); //Isn't this too much? Who are we refering to? Previous link or the player?
                damageInfo.damage = damageReport.damageInfo.damage;
                damageInfo.damageColorIndex = DamageColorIndex.Item;
                damageInfo.force = Vector3.zero; //Wouldn't it be funny if we set it to non-zero?
                damageInfo.procCoefficient = 0f; //See: RandomKill line 131
                damageInfo.damageType = DamageType.Generic; //Thinkan about nonlethal
                damageInfo.procChainMask = default(ProcChainMask);
            }

            private void SearchForLink()
            {
                List<HurtBox> hurtBoxList = CollectionPool<HurtBox, List<HurtBox>>.RentCollection(); //Use these to store targets with hurtbox
                List<Transform> transformList = CollectionPool<Transform, List<Transform>>.RentCollection(); //Use this to store the transforms of the targets
                if (this.networkedBodyAttachment.attachedBody.HasBuff(linkedBuff))
                {
                    this.SearchForTargets(hurtBoxList);
                }
                int i = 0;
                while (i < hurtBoxList.Count)
                {
                    HurtBox hurtBoxToLink = hurtBoxList[i];
                    var externalLink = hurtBoxToLink.GetComponentInChildren<LinkComponent>()?.gameObject; //we get the link component of whatever we are on rn
                    if (!externalLink)
                    {
                        //They dont have a linkComponent, meaning they aren't debuffed and we don't want to do anything with them
                        i++;
                        continue;
                    }
                    if ((!hurtBoxToLink || !hurtBoxToLink.healthComponent || !hurtBoxToLink.healthComponent.alive) || hurtBoxToLink.GetComponentInChildren<LinkComponent>().isLinked)
                    {
                        //This means it either has no hurtboxes, no health component, not alive, or is already linked.
                        i++;
                        continue;
                    }
                    HealthComponent hcHurtBoxToLink = hurtBoxToLink.healthComponent;
                    if (!(hurtBoxToLink == this.networkedBodyAttachment.attachedBody))
                    {
                        //We make sure we aren't linking ourselves
                        Transform transform = hcHurtBoxToLink.body.coreTransform ?? hurtBoxToLink.transform;
                        transformList.Add(transform);
                    }
                    if (this.tetherVfxOrigin)
                    {
                        this.tetherVfxOrigin.SetTetheredTransforms(transformList);
                    }
                    if (this.activeVfx)
                    {
                        this.activeVfx.SetActive(this.isLinked);
                    }
                    CollectionPool<Transform, List<Transform>>.ReturnCollection(transformList);
                    CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(hurtBoxList);
                }
            }

            //SiphonNearbyController

            protected void SearchForTargets(List<HurtBox> dest)
            {
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
}*/
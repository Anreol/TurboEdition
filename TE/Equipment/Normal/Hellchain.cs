using BepInEx.Configuration;
using HG;
using R2API;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//TODO Get linking effect       DONE
//Thinking about siphon but changing the green to red, should be easy
//Replacing the tar tether effect with a chain would be kino
//Also I'd like if the bodies with the debuff would get something like death mark, instead of skull its the cross and instead of that weird bound its a chain

//Get indicator for sphere search radius please
//Need a custom gameobject FX that scales its mesh with the sphere search, idk, but thats how someone else does it
//If i ever get that I could recyle it for DebuffNearbyEnemies

//Fix tethers staying after a enemy has died, that shouldn't happen. UPDATE: I think it has to do with the update rate being only every 10 seconds.
//This also makes enemies that got no actual hurtboxes linked to stop generating orbs going to themselves
//It would also make them start searching for new links once they ran out of them
//The method that goes through the tethered hurtboxes list to update them (all it does is check hurtboxes that doesnt exist anymore and removes them) only works if target isnt tethered to anything, but due to the above bug it never actually works. Yet its somehow fine? Maybe its not needed.
//this has to be the most stupid implementation of a really simple item (literally what it would just do is an array, store enemies inside, and hurt everyone as equal, no matter origin or whatever)
namespace TurboEdition.Equipment
{
    public class Hellchain : EquipmentBase<Hellchain>
    {
        public override string EquipmentName => "Bloody Cross";
        public override string EquipmentLangTokenName => "HELLCHAIN";
        public override string EquipmentPickupDesc => $"<style=cIsUtility>Link enemies</style>, making each link take <style=cIsDamage>{damageFraction * 100}%</style> of the previous link's damage.";
        public override string EquipmentFullDescription => $"<style=cIsUtility>Link enemies around you</style>. Damaging a linked enemy makes that enemy's link take <style=cIsDamage>{damageFraction * 100}% of the damage dealt</style>.";
        public override string EquipmentLore => "What is a man?";

        public override GameObject EquipmentModel => TurboEdition.MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Default.prefab");
        public override Sprite EquipmentIcon => TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Equipment/Placeholder_Hellchain.png");
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
            sphereSearchRadius = config.Bind<float>("Equipment : " + EquipmentName, "Size of the sphereSearch", 100f, "Size in meters for the sphere search to search for enemies in.").Value;
            sphereLinkCount = config.Bind<int>("Equipment : " + EquipmentName, "Sphere link count", 8, "Maximum number of enemies to be linked in each sphere search.").Value;
            linkedBuffDuration = config.Bind<float>("Equipment : " + EquipmentName, "Linking Duration", 25f, "Duration in seconds for the links (linked buff) to stay up.").Value;

            //Component stuff
            damageFraction = config.Bind<float>("Equipment : " + EquipmentName, "Percentage of damage to share", 0.25f, "Percentage of damage that all enemies linked will be damaged for. Hint: The % is taken after all damage reduction calcs, try going after a weak enemy.").Value;
            sphereSearchCount = config.Bind<int>("Equipment : " + EquipmentName, "Additional SphereSearchs", 3, "Number of extra sphere searchs to generate in each use. These generate within enemies, not activator's.").Value;
            //maxLinkCount = config.Bind<int>("Equipment : " + EquipmentName, "Max link count", 100, "Maximum number of BODIES (not just enemies) that can be linked (have the debuff) at the same time.").Value;
        }

        private void CreateBuff()
        {
            var linkedBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            linkedBuffDef.buffColor = Color.red;
            linkedBuffDef.canStack = false;
            linkedBuffDef.isDebuff = true;
            linkedBuffDef.name = "Linked";
            linkedBuffDef.iconSprite = TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Buffs/hellchain_linked.png");
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
            //BodyAttatchment   > TetherVFXOrigin > SiphonTetherVFX (this is the tethers that get cloned for each target)
            //                  > Controller > ActiveVFX > SiphonTetherHealing
            var linkManagerPrefab = new GameObject("LinkManagerPrefabPrefab");
            var siphonPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/bodyattachments/SiphonNearbyBodyAttachment");
            var siphonController = siphonPrefab.GetComponent<SiphonNearbyController>();

            //Get tetherVFX from literally anywhere
            linkManagerPrefab.AddComponent<LinkComponent>();
            linkManagerPrefab.AddComponent<TetherVfxOrigin>();
            linkManagerPrefab.GetComponent<TetherVfxOrigin>().tetherPrefab = siphonPrefab.GetComponent<TetherVfxOrigin>().tetherPrefab;

            linkManagerPrefab.GetComponent<LinkComponent>().activeVfx = siphonController.GetComponent<GameObject>();
            linkManagerPrefab.AddComponent<NetworkedBodyAttachment>().forceHostAuthority = true;

            linkManager = linkManagerPrefab.InstantiateClone("LinkManagerPrefab");
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
                mask = LayerIndex.entityPrecise.mask,
                origin = origin.transform.position,
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
                TurboEdition._logger.LogWarning(EquipmentName + ": filtering the sphereSearch by SAME TEAM as origin.");
#endif
                TeamMask selfdestructionguaranteed = new TeamMask();
                selfdestructionguaranteed.AddTeam(origin.teamComponent.teamIndex);
                sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(selfdestructionguaranteed).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes(hurtBoxesList);
            }

            for (int i = 0, j = 0; i < hurtBoxesList.Count && j < sphereLinkCount; i++)
            {
                var hc = hurtBoxesList[i].healthComponent;
                if (hc)
                {
                    var body = hc.body;
                    if (body)
                    {
                        var linkGameObject = body.GetComponentInChildren<LinkComponent>()?.gameObject;
#if DEBUG
                        TurboEdition._logger.LogWarning(EquipmentName + ": adding linkedBuff to someone, theres " + (i + 1) + " out of " + hurtBoxesList.Count + " hurtboxes with max " + sphereLinkCount);
#endif
                        body.AddTimedBuff(linkedBuff, linkedBuffDuration);
                        if (!linkGameObject)
                        {
                            //If they dont have a component that means they are brand new and unique
                            //If they do, this doesn't get executed and they just get their debuff updated
                            j++;
                        }
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
                    for (int i = 0; i < enemyMember.Count && link < sphereSearchCount; i++)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning(EquipmentName + ": doing extra links " + link + 1 + " out of " + sphereSearchCount);
#endif
                        GameObject linkComponent = enemyMember[i].body.GetComponentInChildren<LinkComponent>()?.gameObject;

                        //Check if they are already being linked and generate a sphere search on them to get additional targets
                        if (linkComponent && NetworkServer.active)
                        {
#if DEBUG
                            TurboEdition._logger.LogWarning(EquipmentName + ": server is active and someone in the enemy team has a LinkComponent, generating extra SphereSearch within them.");
#endif
                            GenerateSphereSearch(enemyMember[i].body, true);
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
            orig(self, deltaTime);
            //#if DEBUG
            //            TurboEdition._logger.LogWarning(EquipmentName + "'s hooks: checking for linkedBuff.");
            //#endif
            if (!NetworkServer.active) { return; } //this update apparently runs in server so if server is not active this shouldnt even work? doing a check anyways.
            if (!self) { return; } //I feel like this is also stupid but ok
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
                    TurboEdition._logger.LogWarning(EquipmentName + "'s hooks: " + self + " has the link debuff but no LinkComponent, creating.");
#endif
                    linkGameObject = UnityEngine.Object.Instantiate(linkManager);
                    linkGameObject.GetComponent<LinkComponent>().ownerBody = self;
                    linkGameObject.GetComponent<LinkComponent>().NetRadius = sphereSearchRadius;
                    linkGameObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(self.gameObject);
                    //cbGameObject.AddComponent<LinkComponent>();
#if DEBUG
                    TurboEdition._logger.LogWarning(EquipmentName + "'s hooks: LinkComponent created.");
#endif
                }
            }
        }

        public class LinkedOrb : LightningOrb
        {
            public override void Begin()
            {
#if DEBUG
                TurboEdition._logger.LogWarning("LinkedOrb: Begin.");
#endif
                lightningType = LightningType.Count; //invalid type
                duration = 0.2f; //distanceToTarget / this.speed;
                var effectData = new EffectData
                {
                    origin = origin,
                    genericFloat = duration
                };
                effectData.SetHurtBoxReference(target);
                EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/orbeffects/InfusionOrbEffect"), effectData, true);
            }

            public override void OnArrival()
            {
#if DEBUG
                TurboEdition._logger.LogWarning("LinkedOrb: Arrived. Has a BouncedObjects of " + this.bouncedObjects + " with count " + this.bouncedObjects.Count);
#endif
                if (this.target)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning("LinkedOrb: Target exists (hurtbox) " + this.target);
#endif
                    if (this.target.healthComponent.body.gameObject == this.attacker)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning("LinkedOrb: Target " + this.target + "is the same as attacker " + this.attacker + ", rejecting.");
#endif
                        return;
                    }
                    var targetComponent = target.healthComponent.body.GetComponentInChildren<LinkComponent>();
                    if (targetComponent)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning("LinkedOrb: Target has a link component " + targetComponent);
#endif
                        //var targetComponent = target.healthComponent.body.GetComponentInChildren<LinkComponent>();
                        //we set this component's bounces to the previous one's and we clean the previous one's list
                        targetComponent.PreviousBounces.AddRange(this.bouncedObjects); //Should add this via a method that detects duped entries
                        HealthComponent healthComponent = this.target.healthComponent;
#if DEBUG
                        TurboEdition._logger.LogWarning("LinkedOrb: Added " + this.bouncedObjects + " to target's " + targetComponent + ". Count is now " + targetComponent.PreviousBounces.Count);
#endif
                        /*                       if (targetComponent.PreviousBounces.Contains(healthComponent))
                                               {
                       #if DEBUG
                                                   TurboEdition._logger.LogWarning("LinkedOrb: REJECTED! Target healthComponent exists, " + healthComponent + " and is inside the previous bounces list, not applying damage.");
                       #endif
                                                   return;
                                               }*/
                        if (healthComponent /*&& !targetComponent.PreviousBounces.Contains(healthComponent)*/)
                        {
#if DEBUG
                            TurboEdition._logger.LogWarning("LinkedOrb: Target healthComponent exists, " + healthComponent + " creating new DamageInfo.");
#endif
                            DamageInfo damageInfo = new DamageInfo
                            {
                                damage = this.damageValue,
                                attacker = this.attacker,
                                inflictor = this.inflictor,
                                force = Vector3.zero,
                                crit = this.isCrit,
                                procChainMask = this.procChainMask,
                                procCoefficient = this.procCoefficient,
                                position = this.target.transform.position,
                                damageColorIndex = this.damageColorIndex
                            };
                            healthComponent.TakeDamage(damageInfo);
                            GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                            GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                        }
                        //We are overriding a Lightning orb OnArrive(), we are missing some more code here that is in the original but it uses shit that for some god forsaken reason is privatized so we cannot edit it
                        //the code missing is bouncing behaviour, and since we aren't bouncing at all it should be fine
                        //the original bouncedObjects is in PickNextTarget and we arent touching that
                    }
                }
            }
        }

        private void RelayLinkDamage(DamageReport damageReport)
        {
            var linkGameObject = damageReport.victimBody.GetComponentInChildren<LinkComponent>()?.gameObject;
            if (linkGameObject)
            {
                //We get the component of the body we hit, this stores a list of the elements its linked to, we also add itself to the list so it can be passed on.
                var component = damageReport.victimBody.GetComponentInChildren<LinkComponent>();
                if (!NetworkServer.active)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning("NetworkServer is not active!");
#endif
                    return;
                }
#if DEBUG
                TurboEdition._logger.LogWarning(EquipmentName + "'s hooks: " + component.TetheredHurtBoxList + " has count of " + component.TetheredHurtBoxList.Count);
#endif
                //We go across that list of elements, and in each one we generate a new orb going after them
                if (damageReport.damageDealt > 0f && component.TetheredHurtBoxList.Count > 0)
                {
                    if (!component.PreviousBounces.Contains(damageReport.victimBody.healthComponent)) { component.AddEntryToPreviousBounces(damageReport.victimBody.healthComponent); }

#if DEBUG
                    TurboEdition._logger.LogWarning(EquipmentName + "'s hooks: " + component.TetheredHurtBoxList + " has elements, doing foreach. It also has previousBounces as " + component.PreviousBounces + " with count " + component.PreviousBounces.Count);
#endif
                    foreach (HurtBox hurtbox in component.TetheredHurtBoxList)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning(EquipmentName + "'s hooks: creating one orb for each hurtbox in " + component.TetheredHurtBoxList);
#endif
                        if (component.PreviousBounces.Contains(hurtbox.healthComponent))
                        {
#if DEBUG
                            TurboEdition._logger.LogWarning(EquipmentName + "'s hooks: REJECTED! Target hurtbox's healthComponent exists, " + hurtbox.healthComponent + " and is already inside the previous bounce's list, not creating orb.");
#endif
                            continue;
                        }

                        {
                            LinkedOrb cuteOrb = new LinkedOrb();
                            cuteOrb.origin = damageReport.victimBody.transform.position;
                            cuteOrb.target = hurtbox;
                            cuteOrb.attacker = damageReport.attacker;
                            cuteOrb.inflictor = damageReport.victimBody.gameObject;
                            cuteOrb.teamIndex = TeamIndex.Neutral;

                            cuteOrb.damageCoefficientPerBounce = damageFraction; //used in damageValue = this.damageValue * this.damageCoefficentPerBounce.
                            cuteOrb.damageValue = damageReport.damageDealt;

                            cuteOrb.bouncesRemaining = 0;
                            cuteOrb.isCrit = false;
                            cuteOrb.bouncedObjects = new List<HealthComponent>(component.PreviousBounces);
                            //cuteOrb.bouncedObjects = cuteOrb.bouncedObjects.AddRange(component.PreviousBounces);
                            //cuteOrb.bouncedObjects.AddRange(component.PreviousBounces);
                            cuteOrb.procChainMask = default;
                            cuteOrb.procCoefficient = 0.01f; //hahaha. no.
                            cuteOrb.damageColorIndex = DamageColorIndex.Bleed;
                            cuteOrb.speed = 8f;
                            cuteOrb.damageType = DamageType.AOE;
                            OrbManager.instance.AddOrb(cuteOrb);
                        }
                    }
                    //We have sent orb(s), clear list
                    component.PreviousBounces.Clear();
#if DEBUG
                    TurboEdition._logger.LogWarning(EquipmentName + "'s hooks: LinkComponent owner is done circling thru hurtbox list after getting damaged, CLEARED PreviousBounces.");
#endif
                }
#if DEBUG
                TurboEdition._logger.LogWarning(EquipmentName + "'s hooks: LinkComponent owner got damaged and has output, created orb(s).");
#endif
            }
        }

        //[RequireComponent(typeof(NetworkedBodyAttachment))] //skipping since we are adding ours in initialization
        public class LinkComponent : NetworkBehaviour//, /*IOnDamageDealtServerReceiver,*/ IOnTakeDamageServerReceiver
        {
            [SyncVar]
            private float radius;

            public float NetRadius
            {
                get { return radius; }
                set { base.SetSyncVar<float>(value, ref radius, 1u); }
            }

            [Min(1E-45f)]
            public float tickRate = 0.3f; //One tick every 3.33 seconds

            protected new Transform transform;
            protected SphereSearch sphereSearch;
            protected float timer;
            public CharacterBody ownerBody;

            //component stuff, im tired
            //protected NetworkedBodyAttachment networkedBodyAttachment;

            //wacky tether thingies
            public TetherVfxOrigin tetherVfxOrigin;

            public GameObject activeVfx;

            private bool isLinkedToAtLeastOneObject;

            private List<HurtBox> tetheredHurtBoxList;
            private List<HealthComponent> previousBounces;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
            private void Awake()
            {
#if DEBUG
                TurboEdition._logger.LogWarning("LinkComponent awake, with the following: (panic if theres nothing following)");
#endif
                this.transform = base.transform;
                //this.networkedBodyAttachment = base.GetComponent<NetworkedBodyAttachment>();
                this.sphereSearch = new SphereSearch();
                this.previousBounces = new List<HealthComponent>();
                this.tetheredHurtBoxList = new List<HurtBox>();
                this.timer = 0f;
#if DEBUG
                TurboEdition._logger.LogWarning(/*"networkedBodyAttachment: " + networkedBodyAttachment + */" sphereSearch: " + sphereSearch + " transform " + transform + " and timer " + timer);
#endif
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
            private void FixedUpdate()
            {
                this.timer -= Time.fixedDeltaTime;
                this.tetherVfxOrigin = this.GetComponent<TetherVfxOrigin>();
                if (this.timer <= 0f && !isLinkedToAtLeastOneObject)
                {
                    this.timer += 1f / this.tickRate;
                    SearchForLink();
                    UpdateLinked();
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
                List<HurtBox> hurtBoxList = CollectionPool<HurtBox, List<HurtBox>>.RentCollection(); //Use these to store the sphere search targets that got found
                List<Transform> transformList = CollectionPool<Transform, List<Transform>>.RentCollection(); //Use this to store the transforms of the targets

                //You might say, why not instead of storing two lists you store one and you replace the transforms list with hurtbox and you just go hurtbox by hurtbox getting the transforms? Check around line 468, transforms can be either from the hurtbox or the body if null.

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
                    TurboEdition._logger.LogWarning("LinkComponent searching for a link (" + (i + 1) + ") in a list out of " + hurtBoxList.Count);
#endif
                    HurtBox currentHurtbox = hurtBoxList[i];
                    var externalLink = currentHurtbox.healthComponent.body.GetComponentInChildren<LinkComponent>(); //we get the link component of whatever we are on rn
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
                    //We make sure we aren't linking ourselves
                    if (!(hcHurtBoxToLink.body == ownerBody))
                    {
                        if (!tetheredHurtBoxList.Contains(currentHurtbox)) { tetheredHurtBoxList.Add(currentHurtbox); Transform transform = hcHurtBoxToLink.body.coreTransform ?? currentHurtbox.transform; transformList.Add(transform); }
                        if (!externalLink.tetheredHurtBoxList.Contains(ownerBody.mainHurtBox)) { externalLink.tetheredHurtBoxList.Add(ownerBody.mainHurtBox); }
#if DEBUG
                        TurboEdition._logger.LogWarning("LinkComponent, found link, adding " + currentHurtbox + " to " + tetheredHurtBoxList);
#endif
                        //We also make sure we aren't linking something that is already tethered to avoid drawing two lines
                        if (!externalLink.tetheredHurtBoxList.Contains(ownerBody.mainHurtBox)) //should be transformlist?
                        {
                            //Get the hurtbox transform we are affecting, else the body transform

#if DEBUG
                            TurboEdition._logger.LogWarning("LinkComponent, found link that wasn't linked to ours with transform: " + transform);
#endif
                        }
                    }
                    if (transformList.Count < 1)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning("LinkComponent, transformList doesn't have the needed amount of elements, searching for more.");
#endif
                        i++;
                        continue;
                    }
                    this.isLinkedToAtLeastOneObject = (transformList.Count > 0);
                    if (this.tetherVfxOrigin)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning("LinkComponent, tetherVfxOrigin exists, setting transforms from " + transformList + " with count " + transformList.Count);
#endif
                        this.tetherVfxOrigin.SetTetheredTransforms(transformList);
                    }
                    if (this.activeVfx)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning("LinkComponent, activeVfx exists, changing enabled/disabled!");
#endif
                        this.activeVfx.SetActive(isLinkedToAtLeastOneObject);
                    }
                    CollectionPool<Transform, List<Transform>>.ReturnCollection(transformList);
                    CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(hurtBoxList);
                }
#if DEBUG
                TurboEdition._logger.LogWarning("LinkComponent, couldn't find match, what a shame!");
#endif
            }

            private void UpdateLinked()
            {
                foreach (HurtBox hurtbox in tetheredHurtBoxList)
                {
                    if (!hurtbox)
                    {
                        tetheredHurtBoxList.Remove(hurtbox);
                    }
                }
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

            public List<HurtBox> TetheredHurtBoxList { get => tetheredHurtBoxList; set => tetheredHurtBoxList = value; }
            public List<HealthComponent> PreviousBounces { get => previousBounces; set => previousBounces = value; }

            public void AddEntryToPreviousBounces(HealthComponent input)
            {
                if (!previousBounces.Contains(input))
                {
                    previousBounces.Add(input);
                    return;
                }
#if DEBUG
                TurboEdition._logger.LogWarning("LinkComponent, tried to add a new HealthComponent that is already in list, rejecting.");
#endif
            }
        }
    }
}
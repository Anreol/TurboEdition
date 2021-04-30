using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

//TODO: The player loses the magnet component when switching stages, fix that shit and make him regain it (maybe the list manager can help?)
//Currently doesnt until they get a change in inventory counts or some shit
//Fix the LoS check being so wonky, while debugging an early version that didnt have everything in yet it worked fine
//Check what happens when two players fight for the same pack, thats gonna be funny

//For pack cloning i can get the GO's name or convert to string, remove the (clone) part and add the system path before it
namespace TurboEdition.Items
{
    public class AttractBPs : ItemBase<AttractBPs>
    {
        public override string ItemName => "Magnetic Belt";
        public override string ItemLangTokenName => "ATTRACTBPs";
        public override string ItemPickupDesc => $"<style=cIsUtility>Attract dropped pickups</style>. Have a low chance of <style=cIsUtility>duplicating</style> them.";
        public override string ItemFullDescription => $"Attract dropped pickups in a radius of <style=cIsUtility>{attractInitial} meters</style>. <style=cStack>(+{attractStack} meters per stack)</style>. Have a <style=cIsUtility>{duplicationInitial * 100}% chance of duplicating</style> them.";
        public override string ItemLore => "Making Ghor Tomes, Bandoliers, and Teeth 350% more useful since today, by the simple hack of not needing to walk to them.";

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlacklisted => false; //Do drones dream of stealing your drops?
        public override bool BrotherBlacklisted => false; //Its all fun and jokes til mithrix starts succing the item drops too

        public override GameObject ItemModel => TurboEdition.MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Default.prefab");
        public override Sprite ItemIcon => TurboEdition.MainAssets.LoadAsset<Sprite>("@Assets/Textures/Icons/Items/Tier2.png");

        internal static GameObject magnetManager;

        //Item properties
        public float attractInitial;

        public float attractStack;
        public float duplicationInitial;
        public float duplicationStack;
        public bool useLoS;

        protected override void CreateConfig(ConfigFile config)
        {
            attractInitial = config.Bind<float>("Item: " + ItemName, "Initial attract radius", 5f, "Radius in meters in which bonus packs will get attracted to you when getting the item for the first item.").Value;
            attractStack = config.Bind<float>("Item: " + ItemName, "Stack attract radius", 5f, "Radius in meters in which bonus packs will get attracted to you when stacking the item.").Value;
            duplicationInitial = config.Bind<float>("Item: " + ItemName, "Duplication chance", 0.10f, "Chance for item pickups to be duplicated when touching one (on initial pickup).").Value;
            duplicationStack = config.Bind<float>("Item: " + ItemName, "Stack duplication chance", 0.005f, "Chance for item pickups to be duplicated when touching one (on item stack).").Value;
            useLoS = config.Bind<bool>("Item: " + ItemName, "LoS calculations instead of Vector", false, "Should the item do a slightly more expensive LoS check instead of vector calculation to start attracting items. VERY BUGGY!.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override void Initialization()
        {
            var magnetManagerPrefab = new GameObject("MagnetManagerPrefabPrefab");
            magnetManagerPrefab.AddComponent<MagnetManager>();
            magnetManagerPrefab.AddComponent<NetworkedBodyAttachment>().forceHostAuthority = true;
            magnetManager = magnetManagerPrefab.InstantiateClone("MagnetManagerPrefab");

            UnityEngine.Object.Destroy(magnetManagerPrefab);
        }

        public override void Hooks()
        {
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
            On.RoR2.GravitatePickup.Start += GravitatePickup_Start;
        }

        private void Run_onRunStartGlobal(Run obj)
        {
            if (NetworkServer.active)
            {
                var gravManagerGameObject = obj.gameObject.GetComponentInChildren<ListManager>()?.gameObject;
                if (!gravManagerGameObject)
                {
#if DEBUG
                    TurboEdition._logger.LogInfo("Added ListManager to run.");
#endif
                    obj.gameObject.AddComponent<ListManager>();
                }
            }
        }

        private void GravitatePickup_Start(On.RoR2.GravitatePickup.orig_Start orig, GravitatePickup self)
        {
            orig(self);
            var gravManager = Run.instance.gameObject.GetComponentInChildren<ListManager>();
            if (gravManager)
            {
#if DEBUG
                TurboEdition._logger.LogWarning(ItemName + " added a GravitatePickup (" + self + ") to list.");
#endif
                gravManager.AddNewEntry(self);
                return;
            }
#if DEBUG
            TurboEdition._logger.LogWarning(ItemName + " THERES NO LIST MANAGER WHAT ARE YOU DOING.");
#endif
        }

        private void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
        {
            orig(self);
            var InventoryCount = GetCount(self);
            if (InventoryCount > 0)
            {
                var magnetObject = self.GetBody().GetComponentInChildren<MagnetManager>()?.gameObject;
                if (!magnetObject)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning(ItemName + " " + self + "'s body didn't have a MagnetManager, creating one.");
#endif
                    magnetObject = UnityEngine.Object.Instantiate(magnetManager);
                    magnetObject.GetComponent<MagnetManager>().ownerBody = self.GetBody();
                    magnetObject.GetComponent<MagnetManager>().NetLoS = useLoS;
                    magnetObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(self.GetBody().gameObject);
                }
                //we update the radius after creating / finding out is already created
#if DEBUG
                TurboEdition._logger.LogWarning(ItemName + " updating " + self + "'s body MagnetManager, with radius of " + attractInitial + (InventoryCount - 1) * attractStack);
#endif
                magnetObject.GetComponent<MagnetManager>().NetRadius = attractInitial + ((InventoryCount - 1) * attractStack);
                magnetObject.GetComponent<MagnetManager>().NetDupChance = duplicationInitial + ((InventoryCount - 1) * duplicationStack);
            }
        }

        //making it a component because this will be a PERMANENT sphere search, so instead of creating one constantly, we create it once and we attatch it to the component, then update it when its needed.
        public class MagnetManager : NetworkBehaviour
        {
            [SyncVar]
            private float radius;

            public float NetRadius
            {
                get { return radius; }
                set { base.SetSyncVar<float>(value, ref radius, 1u); }
            }

            [SyncVar]
            private float dupChance;

            public float NetDupChance
            {
                get { return dupChance; }
                set { base.SetSyncVar<float>(value, ref dupChance, 1u); }
            }

            [SyncVar]
            private bool useLoS;

            public bool NetLoS
            {
                get { return useLoS; }
                set { base.SetSyncVar<bool>(value, ref useLoS, 1u); }
            }

            public CharacterBody ownerBody;

            [Min(1E-45f)]
            public float tickRate = 0.3f; //One tick every 3.33 seconds, was 5 previously

            protected float timer;

            //I'm gonna reuse the code from hellchain, not sure if its the best way but whatever
            //Spent a shitload of time on those sphere searchs yknow
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
            private void Awake()
            {
                this.timer = 0f;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
            private void FixedUpdate()
            {
                this.timer -= Time.fixedDeltaTime;
                if (this.timer <= 0f)
                {
                    this.timer += 1f / this.tickRate;
                    Gravitate();
                }
            }

            private void Gravitate()
            {
                var gravManager = Run.instance.gameObject.GetComponentInChildren<ListManager>();
#if DEBUG
                TurboEdition._logger.LogWarning("MagnetManager, updating " + gravManager + " right now it has a count of " + gravManager.GetList().Count);
#endif
                foreach (GravitatePickup gravitate in gravManager.GetList())
                {
                    if (gravitate == null)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning("MagnetManager, a entry was null, ignoring it.");
#endif
                        gravManager.RemoveEntry(gravitate);
                        continue;
                    }
                    if (gravitate.teamFilter.teamIndex != ownerBody.teamComponent.teamIndex)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning("MagnetManager, a entry was not on the owner's team, ignoring it.");
#endif
                        continue;
                    }
                    if (useLoS)
                    {
                        if (HasLoS(ownerBody.gameObject, gravitate.gameObject))
                        {
#if DEBUG
                            TurboEdition._logger.LogWarning("MagnetManager, a entry is in LoS, gravitating. Owner has dupChance of " + dupChance);
#endif
                            gravitate.gravitateTarget = ownerBody.transform;
                            if (Util.CheckRoll(dupChance, ownerBody.master.luck))
                            {
                                gravManager.AddDupeEntry(gravitate, ownerBody.teamComponent.teamIndex);
                            }
                        }
                        else if (!(HasLoS(ownerBody.gameObject, gravitate.gameObject)) && gravitate.gravitateTarget == ownerBody.transform)
                        {
#if DEBUG
                            TurboEdition._logger.LogWarning("MagnetManager, a entry was already being gravitated towards the player but the player went too far and/or lost LOS, removing it.");
#endif
                            gravitate.gravitateTarget = null;
                        }
                        continue;
                    }
                    float distance = Vector3.Distance(ownerBody.transform.position, gravitate.gameObject.transform.position);
                    if (distance <= radius)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning("MagnetManager, a entry is in range, gravitating. Owner has dupChance of " + dupChance);
#endif
                        gravitate.gravitateTarget = ownerBody.transform;
                        if (Util.CheckRoll(dupChance, ownerBody.master.luck))
                        {
                            gravManager.AddDupeEntry(gravitate, ownerBody.teamComponent.teamIndex);
                        }
                    }
                    else if (distance > radius && gravitate.gravitateTarget == ownerBody.transform)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning("MagnetManager, a entry was already being gravitated towards the player but the player went too far, removing it.");
#endif
                        gravitate.gravitateTarget = null;
                    }
                }
            }

            private bool HasLoS(GameObject origin, GameObject target)
            {
#if DEBUG
                TurboEdition._logger.LogWarning("MagnetManager Checking LOS between: " + origin + " and " + target);
#endif
                Ray ray = new Ray(origin.transform.position, target.transform.position - origin.transform.position);
                bool FUCK = !Physics.Raycast(ray, out RaycastHit raycastHit, this.radius, LayerIndex.defaultLayer.mask | LayerIndex.world.mask, QueryTriggerInteraction.Ignore) || raycastHit.collider.gameObject == target;
#if DEBUG
                TurboEdition._logger.LogInfo("MagnetManager LOS check turned out to be: " + FUCK);
#endif
                return FUCK;
            }
        }

        public class ListManager : NetworkBehaviour
        {
            [Min(1E-45f)]
            public float tickRate = 0.3f; //One tick every 3.33 seconds

            protected float timer;

            private List<GravitatePickup> gravitatorList;
            private List<GravitatePickup> duplicateList;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
            private void Awake()
            {
#if DEBUG
                TurboEdition._logger.LogInfo("ListManager, has AWAKEN.");
#endif
                gravitatorList = new List<GravitatePickup>();
                duplicateList = new List<GravitatePickup>();
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
            private void FixedUpdate()
            {
                this.timer -= Time.fixedDeltaTime;
                if (this.timer <= 0f)
                {
#if DEBUG
                    TurboEdition._logger.LogInfo("ListManager, timer's up, updating list.");
#endif
                    this.timer += 1f / this.tickRate;
                    UpdateList();
                }
            }

            private void UpdateList()
            {
                foreach (GravitatePickup gravitate in gravitatorList.ToList())
                {
                    if (gravitate == null)
                    {
                        if (duplicateList.ToList().Contains(gravitate))
                        {
                            duplicateList.Remove(gravitate);
                        }
#if DEBUG
                        TurboEdition._logger.LogInfo("ListManager, a entry was null, deleting it.");
#endif
                        gravitatorList.Remove(gravitate);
                        continue;
                    }
                }
            }

            public void AddDupeEntry(GravitatePickup dupEntry, TeamIndex teamIndex)
            {
                if (!duplicateList.Contains(dupEntry))
                {
#if DEBUG
                    TurboEdition._logger.LogWarning("ListManager, roll passed, duplicating entry.");
#endif
                    GameObject duplication = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>(dupEntry.rigidbody.gameObject.ToString()), dupEntry.gameObject.transform.position, UnityEngine.Random.rotation);
                    duplication.GetComponent<TeamFilter>().teamIndex = teamIndex;
                    NetworkServer.Spawn(duplication);

                    duplicateList.Add(dupEntry);
                    return;
                }
#if DEBUG
                TurboEdition._logger.LogWarning("ListManager, a entry was already duplicated, rejecting.");
#endif
            }

            public void AddNewEntry(GravitatePickup entry)
            {
                gravitatorList.Add(entry);
            }

            public void RemoveEntry(GravitatePickup entry)
            {
#if DEBUG
                TurboEdition._logger.LogInfo("ListManager, manually deleted " + entry);
#endif
                gravitatorList.Remove(entry);
            }

            public List<GravitatePickup> GetList()
            {
                return gravitatorList.ToList();
            }
        }
    }
}
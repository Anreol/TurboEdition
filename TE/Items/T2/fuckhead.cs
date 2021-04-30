/*using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using HG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static TurboEdition.Utils.ItemHelpers;

//TODO: The player loses the magnet component when switching stages, fix that shit and make him regain it
//Currently doesnt until they get a change in inventory counts or some shit
namespace TurboEdition.Items
{
    public class fuckhead : ItemBase<fuckhead>
    {
        public override string ItemName => "Magnetic Belt";
        public override string ItemLangTokenName => "ATTRACTBPs";
        public override string ItemPickupDesc => $"<style=cIsUtility>Attract dropped pickups</style>. Have a low chance of <style=cIsUtility>duplicating</style> them.";
        public override string ItemFullDescription => $"Attract dropped pickups in a radius of of <style=cIsUtility>{attractInitial} meters</style>. <style=cStack>(+{attractStack} meters per stack)</style>. Have a <style=cIsUtility>{duplicationInitial * 100}% chance of duplicating</style> them.";
        public override string ItemLore => "Making Ghor Tomes, Bandoliers, and Teeth 350% more useful since today, by the simple hack of not needing to walk to them.";

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlacklisted => false;
        public override bool BrotherBlacklisted => false; //Its all fun and jokes til mithrix starts succing the item drops too

        public override string ItemModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";
        public override string ItemIconPath => "@TurboEdition:Assets/Textures/Icons/Items/Tier2.png";

        internal static GameObject magnetManager;

        //Item properties
        public float attractInitial;
        public float attractStack;
        public float duplicationInitial;
        public float duplicationStack;

        protected override void CreateConfig(ConfigFile config)
        {
            attractInitial = config.Bind<float>("Item: " + ItemName, "Initial attract radius", 5f, "Radius in meters in which bonus packs will get attracted to you when getting the item for the first item.").Value;
            attractStack = config.Bind<float>("Item: " + ItemName, "Stack attract radius", 5f, "Radius in meters in which bonus packs will get attracted to you when stacking the item.").Value;
            duplicationInitial = config.Bind<float>("Item: " + ItemName, "Duplication chance", 0.05f, "Chance for item pickups to be duplicated when touching one (on initial pickup).").Value;
            duplicationStack = config.Bind<float>("Item: " + ItemName, "Stack duplication chance", 0.005f, "Chance for item pickups to be duplicated when touching one (on item stack).").Value;
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
            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
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

                    magnetObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(self.GetBody().gameObject);
                }
                //we update the radius after creating / finding out is already created
#if DEBUG
                TurboEdition._logger.LogWarning(ItemName + " updating " + self + "'s body MagnetManager, with radius of " + attractInitial + (InventoryCount - 1) * attractStack);
#endif
                magnetObject.GetComponent<MagnetManager>().NetRadius = attractInitial + (InventoryCount - 1) * attractStack;
            }
        }

        //making it a component because this will be a PERMANENT sphere search, so instead of creating one constantly, we create it once and we attatch it to the component, then update it when its needed.
        public class MagnetManager : NetworkBehaviour
        {
            [SyncVar]
            float radius;
            public float NetRadius
            {
                get { return radius; }
                set { base.SetSyncVar<float>(value, ref radius, 1u); }
            }
            public CharacterBody ownerBody;

            [Min(1E-45f)]
            public float tickRate = 0.18f; //One tick every 5.55 seconds

            protected SphereSearch sphereSearch;
            protected float timer;

            //I'm gonna reuse the code from hellchain, not sure if its the best way but whatever
            //Spent a shitload of time on those sphere searchs yknow
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
            private void Awake()
            {
                this.sphereSearch = new SphereSearch();
                this.timer = 0f;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
            private void FixedUpdate()
            {
                this.timer -= Time.fixedDeltaTime;
                if (this.timer <= 0f)
                {
                    this.timer += 1f / this.tickRate;
                    SearchForPickups();
                }
            }

            private void SearchForPickups()
            {
                List<Collider> colliderList = CollectionPool<Collider, List<Collider>>.RentCollection();
                this.SearchForColliders(colliderList);

                int i = 0;
#if DEBUG
               TurboEdition._logger.LogWarning("MagnetManager, searching for pickups in a list of a count " + colliderList.Count + " gonna filter by a objectStringList of " + objectStringList.Length + ".");
#endif
                while (i < colliderList.Count)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning("MagnetManager, GOT COLLIDERS, cycling thru " + colliderList + " " + (i+1) + " out of " + colliderList.Count);
                    TurboEdition._logger.LogWarning("COLLIDER " + (i+1) + " " + colliderList[i] + " NAME: " + colliderList[i].name + " GO: " + colliderList[i].gameObject);
#endif
                    for (int ostia = 0; ostia < objectStringList.Length; ostia++)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning("MagnetManager, cycling through the resources we've set to load " + (ostia+1) + " out of " + objectStringList.Length);
#endif
                        //                      if (colliderList[i].attachedRigidbody.gameObject == Resources.Load<GameObject>(objectStringList[ostia]))
                        if (colliderList[i].name == "GravityTrigger" || colliderList[i].name == "GravitationController")
                        {
#if DEBUG
                            TurboEdition._logger.LogWarning("MagnetManager, found a pickup " + objectStringList[ostia]);
#endif
                            if (!(colliderList[i].attachedRigidbody.gameObject.GetFieldValue<TeamFilter>("teamFilter").teamIndex == ownerBody.teamComponent.teamIndex))
                            {
#if DEBUG
                                TurboEdition._logger.LogWarning("MagnetManager, pickup isnt in the same team as owner's, rejecting it.");
#endif
                                i++;
                                continue;
                            }
#if DEBUG
                            TurboEdition._logger.LogWarning("MagnetManager, found a valid pickup " + colliderList[i].attachedRigidbody.gameObject + " adding gravitator.");
#endif
                            var gravitator = colliderList[i].attachedRigidbody.gameObject.AddComponent<GravitatePickup>();
                            gravitator.gravitateTarget = ownerBody.gameObject.transform;
                            gravitator.rigidbody = colliderList[i].attachedRigidbody;

                            i++;
                            continue;
                            //does it need a maxspeed?
                        }
                    }
                    i++;
                }
#if DEBUG
                TurboEdition._logger.LogWarning("MagnetManager, done doing wacky stuff, returning collection. New Line now.\n");
#endif
                CollectionPool<Collider, List<Collider>>.ReturnCollection(colliderList);
            }

            //Pickups to search for: MoneyPickup, BuffPickup (unused ingame, for now, but anybody can use it), AmmoPickup, HealthPickup
            //They gravitate towards players thanks to GravitatePickup
            protected void SearchForColliders(List<Collider> dest)
            {
#if DEBUG
                TurboEdition._logger.LogWarning("MM, SearchForColliders (R: " + radius + " O: " + this.transform.position + ") to D: " + dest);
#endif
                this.sphereSearch.mask = LayerIndex.pickups.mask;
                this.sphereSearch.origin = this.transform.position;
                this.sphereSearch.radius = this.radius;
                this.sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Collide;
                this.sphereSearch.RefreshCandidates();
                this.sphereSearch.OrderCandidatesByDistance();
                this.sphereSearch.FilterCandidatesByDistinctColliderEntities();
                this.sphereSearch.GetColliders(dest);
                this.sphereSearch.ClearCandidates();
            }

            public static String[] objectStringList = new String[]
            {
                "Prefabs/NetworkedObjects/HealPack",
                "Prefabs/NetworkedObjects/AmmoPack",
                "Prefabs/NetworkedObjects/BonusMoneyPack",
            };
        }
    }
}*/
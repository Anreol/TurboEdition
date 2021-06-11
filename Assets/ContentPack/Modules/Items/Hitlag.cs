using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//TODO check any other methods that could damage the user
//fix DoTs and fall check interferring with damage
//Make a custom health bar
//Make it so healing cannot recover damage that would go to shields (ie get current hp and current shields, if the damage instance wont go to hp dont make it able to heal)
//do the previous but with shields (i do not know how shield generation works)
//if the damage goes to barrier make it so you cannot heal it

//Get damage types that can bypass the lagger
//Something as bypassArmor, for example, could be good
//Also let the user config their own list

//Now that we have a stubbed assembly look again into if its possible to implement a custom healthbar, since it was private before.

//IMPORTANT ===========>
//https://discord.com/channels/562704639141740588/562704639569428506/813439608049500231
namespace TurboEdition.Items
{
    public class Hitlag
    {
        internal static GameObject hitManager;

        //Item properties
        public float hitlagInitial;

        public float hitlagStack;
        public float healValueInitial;
        public float healFractionStack;
        public int recoveryMode;
        public bool storesDoTs;
        public bool storesFall;
        public int storeMaxCapacity;
        public bool storeForgiveness;

        //Healthbar garbage
        //private float cachedFractionalValue = 1f;

        protected override void CreateConfig(ConfigFile config)
        {
            hitlagInitial = config.Bind<float>("Item: " + ItemName, "Initial lag duration", 1f, "Amount of time that the damage will be delayed for when getting the item for the first item.").Value;
            hitlagStack = config.Bind<float>("Item: " + ItemName, "Stack lag duration", 0.5f, "Amount of time that the damage will be delayed for when stacking the item.").Value;
            healValueInitial = config.Bind<float>("Item: " + ItemName, "Initial heal value", 25f, "Amount of healing that will go to the delayed damage when you heal yourself. (On first pickup)").Value;
            healFractionStack = config.Bind<float>("Item: " + ItemName, "Stack heal percentage", 0.02f, "Percentage of healing that will go to the delayed damage when you heal yourself. (On item stack)").Value;
            //I have to figure out how to do substraction mode recoveryMode = config.Bind<int>("Item: " + ItemName, "Recovery mode", 1, "In which way the user will heal, 0 for Clone (Healing will be copied) 1 for Substraction (Heal going to the delayed damage will be substracted from the one going to the HP)").Value;
            storesDoTs = config.Bind<bool>("Item: " + ItemName, "DoT Storage", false, "Should DoT damage reports get delayed too.").Value;
            storesFall = config.Bind<bool>("Item: " + ItemName, "Fall damage Storage", false, "Should fall damage damage reports get delayed too.").Value;
            storeMaxCapacity = config.Bind<int>("Item: " + ItemName, "List Storage", -1, "Add a maximum capacity to SortedList<>. Use it if you fear of performance (shouldn't be an issue) or want to balance the item. New entries won't be added to the list. Keep at -1 for no limit.").Value;
            storeForgiveness = config.Bind<bool>("Item: " + ItemName, "Storage forgiveness", true, "Should the SortedList<> just clear itself or release all stored damage before deleting itself.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override void Initialization()
        {
            var hitManagerPrefab = new GameObject("HitlagManagerPrefabPrefab");
            hitManagerPrefab.AddComponent<HitlagManager>();
            hitManagerPrefab.GetComponent<HitlagManager>().NetMaxCapacity = storeMaxCapacity;
            hitManagerPrefab.AddComponent<NetworkedBodyAttachment>().forceHostAuthority = true;
            hitManager = hitManagerPrefab.InstantiateClone("HitlagManagerPrefab");

            //var hitComponentPrefab = new GameObject("HitlagComponentPrefabPrefab");
            //hitComponentPrefab.AddComponent<HitlagComponent>();
            //hitComponent = hitComponentPrefab.InstantiateClone("HitlagComponentPrefabClone");

            UnityEngine.Object.Destroy(hitManagerPrefab);
            //UnityEngine.Object.Destroy(hitComponentPrefab);
        }

        public override void Hooks()
        {
            //GlobalEventManager.onServerDamageDealt += StoreDamage;
            On.RoR2.HealthComponent.TakeDamage += StoreDamage;
            HealthComponent.onCharacterHealServer += GetIncomingHealing;

            //On.RoR2.UI.HealthBar.UpdateBarInfos += UpdateDelayBar;
            //On.RoR2.UI.HealthBar.ApplyBars += ApplyDelayBar;
            //Theres uh two TakeDamageForces, one is within TakeDamage which works with damageinfo that TakeDamage gives it, other is with a vector
            //Lets hope the vector one doesnt fuck shit up, okay?
        }

        private void StoreDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            //TODO: all of these has to make sure if 1. player is alive 2. player is not teleporting 3. player has a item 4. player has a hc
            var InventoryCount = GetCount(self.body);
            var hcGameObject = self.gameObject;
            var hcHitManager = hcGameObject.GetComponentInChildren<HitlagManager>()?.gameObject; //check if the component exists or not

            if (InventoryCount <= 0)
            {
                if (hcHitManager)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning(ItemName + " HLM was created, but user lost all items, storeForgiveness is: " + storeForgiveness);
#endif
                    if (!storeForgiveness)
                    {
                        hcGameObject.GetComponentInChildren<HitlagManager>().ReleaseAll(true);
                        return;
                    }
                    hcGameObject.GetComponentInChildren<HitlagManager>().CleanseAll(true);
                    //UnityEngine.Object.Destroy(hcHitManager);
                }
            }
            else
            {
                if (!hcHitManager && self) //creates a manager if user has item and a hc
                {
                    hcHitManager = UnityEngine.Object.Instantiate(hitManager);
                    hcHitManager.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(self.gameObject);
                    //hcGameObject.GetComponent<HitlagManager>().NetMaxCapacity = storeMaxCapacity;
#if DEBUG
                    TurboEdition._logger.LogWarning(ItemName + " No HLM created, creating one with capacity: " + storeMaxCapacity);
#endif
                }

                if (hcHitManager && self && damageInfo.damage > 0)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning(ItemName + " Theres a HLM, so we are going to delay " + damageInfo.damage + " of damage.");
#endif
                    if (damageInfo.damageType == DamageType.FallDamage && !storesFall)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning(ItemName + " Damage type was " + damageInfo.damageType + " but storesFall config is " + storesFall);
#endif
                        orig(self, damageInfo);
                        return;
                    }
                    if (damageInfo.dotIndex != DotController.DotIndex.None && !storesDoTs)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning(ItemName + " Damage type was " + damageInfo.damageType + " but storesDoTs config is " + storesDoTs);
#endif
                        orig(self, damageInfo);
                        return;
                    }
                    //We update the manager with the new delay
                    //This is different to the original idea where each instance of damage would have its own release time based on item count of when the damage was taken. i.e if user gets damaged at 5 delay, loses an item, all delayed damage will be released earlier
                    hcHitManager.GetComponent<HitlagManager>().NetTimeToReleaseAt = (hitlagInitial + (InventoryCount - 1) * hitlagStack);
#if DEBUG
                    TurboEdition._logger.LogWarning(ItemName + " Updated " + hcHitManager + " to have delay of " + (hitlagInitial + (InventoryCount - 1) * hitlagStack));
#endif
                    //We define a new instance
                    var hitInstance = new HitlagInstance
                    {
                        //I wonder if its better to pass these as arguments than doing this, desu.
                        CmpOrig = orig,
                        CmpSelf = self,
                        CmpDI = damageInfo
                    };
#if DEBUG
                    TurboEdition._logger.LogWarning(ItemName + " Creating a new " + hitInstance);
#endif
                    var component = hcGameObject.GetComponentInChildren<HitlagManager>();
                    component.AddInstance(Run.FixedTimeStamp.now, hitInstance);
                    component.NetTotalDamage += hitInstance.CmpDI.damage;
#if DEBUG
                    TurboEdition._logger.LogWarning(ItemName + " Added it to the list with timestamp " + Run.FixedTimeStamp.now);
#endif
                    return;
                }
#if DEBUG
                TurboEdition._logger.LogWarning(ItemName + " Didn't delay any damage, calling orig.");
#endif
            }
            orig(self, damageInfo);
        }

        private void GetIncomingHealing(HealthComponent healthComponent, float amount)
        {
            var InventoryCount = GetCount(healthComponent.body);
            var hcGameObject = healthComponent.gameObject;
            var hlmGameObject = hcGameObject.GetComponentInChildren<HitlagManager>()?.gameObject;

            if (hlmGameObject && InventoryCount > 0) //If they have the hitlag manager that means they have at least one item, but lets do the extra check anyways
            {
                var healing = Mathf.Min(amount, healValueInitial);
                if (healing > healValueInitial)
                {
                    healing += ((amount - healValueInitial) * healFractionStack);
                }
                //This seems stupid but I do not know how to do a null check without a identifier for gameobject (do i need to tho?)
                var component = hcGameObject.GetComponentInChildren<HitlagManager>();
                component.AddHealing(healing, healthComponent);
#if DEBUG
                TurboEdition._logger.LogWarning(ItemName + " Healing recieved! Amount: " + healing + " hitlagComponent: " + component);
#endif
            }
        }

        //manager that will be populated by HitlagInstances
        public class HitlagManager : NetworkBehaviour
        {
            [SyncVar] //Syncing just in case, i do not want to know what would happen if clients have different configs
            private int maxCapacity;

            public int NetMaxCapacity
            {
                get { return maxCapacity; }
                set { base.SetSyncVar<int>(value, ref maxCapacity, 1u); }
            }

            [SyncVar]
            private float timeToReleaseAt;

            public float NetTimeToReleaseAt
            {
                get { return timeToReleaseAt; }
                set { base.SetSyncVar<float>(value, ref timeToReleaseAt, 1u); }
            }

            [SyncVar]
            private float totalDamage;

            public float NetTotalDamage
            {
                get { return totalDamage; }
                set { base.SetSyncVar<float>(value, ref totalDamage, 1u); }
            }

            private SortedList<Run.FixedTimeStamp, HitlagInstance> instanceLists;
            //public GameObject owner;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
            private void Awake()
            {
                instanceLists = new SortedList<Run.FixedTimeStamp, HitlagInstance>();
                if (maxCapacity > 0) //i know i said -1 but like putting zero in wouldn't be funny
                {
                    instanceLists.Capacity = maxCapacity;
                }
#if DEBUG
                TurboEdition._logger.LogWarning("HLM: created " + instanceLists);
#endif
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
            private void FixedUpdate()
            {
                IList<Run.FixedTimeStamp> iListKeys = instanceLists.Keys;
                foreach (Run.FixedTimeStamp instance in iListKeys)
                {
                    if (instanceLists.TryGetValue(instance, out HitlagInstance delayedDamageToDel))
                    {
                        if (delayedDamageToDel.CmpDI.damage <= 0)
                        {
#if DEBUG
                            TurboEdition._logger.LogWarning("HLM: " + instance + " had zero or less damage to hurt the player for, removing it.");
#endif
                            instanceLists.Remove(instance);
                            return;
                        }
                    }
                    if (instance.timeSince >= timeToReleaseAt)
                    {
                        //We need to get the hitlag instances, then do cmpOrig(cmpSelf, cmpDI) so we call origin
                        if (instanceLists.TryGetValue(instance, out HitlagInstance delayedDamage))
                        {
                            //Calling origin is literally just releasing the damage
#if DEBUG
                            TurboEdition._logger.LogWarning("HLM: " + "Time of " + instance + " is up, calling orig and removing it.");
#endif
                            delayedDamage.CmpOrig(delayedDamage.CmpSelf, delayedDamage.CmpDI);
                            instanceLists.Remove(instance);
                            return;
                        }
                    }
                }
            }

            //Consider moving if damage < 0 here since this doesn't run on a fixed update
            public void AddHealing(float healAmount, HealthComponent healthComponent = null, bool isShields = false)
            {
                float getDamage = 0;
                float reserveHP = +healAmount;
                for (int i = 0; i < instanceLists.Count && reserveHP > 0; i++)
                {
                    //HealthComponent is not really needed for the item to function, its only used to get current hp values
                    if (healthComponent)
                    {
                        getDamage += instanceLists.Values[i].CmpDI.damage;
                        //If the accumulated damage so far goes to barrier, ignore it
                        if (getDamage < healthComponent.barrier)
                        {
#if DEBUG
                            TurboEdition._logger.LogWarning("HLM: Heal recieved, but went to barrier, so we rejected it. getDamage: " + getDamage + " current barrier: " + healthComponent.barrier);
#endif
                            return;
                        }
                        getDamage -= healthComponent.barrier;
                        //If the accumulated damage so far goes to shields, ignore it, however make an extra check in case theres an item that instantly generates shields or something (not natural regeneration)
                        if (getDamage < healthComponent.shield && !isShields)
                        {
#if DEBUG
                            TurboEdition._logger.LogWarning("HLM: Heal recieved, but went to shields, so we rejected it. getDamage: " + getDamage + " current shields: " + healthComponent.shield);
#endif
                            return;
                        }
                    }
                    if (instanceLists.Values[i].CmpDI.damage > reserveHP)
                    {
                        instanceLists.Values[i].CmpDI.damage -= reserveHP;
                        reserveHP = 0;
#if DEBUG
                        TurboEdition._logger.LogWarning("HLM: Heal recieved, with no reserves left, reduced " + instanceLists.Values[i].CmpDI.damage);
#endif
                    }
                    else
                    {
                        reserveHP -= instanceLists.Values[i].CmpDI.damage;
                        instanceLists.Values[i].CmpDI.damage = 0;
#if DEBUG
                        TurboEdition._logger.LogWarning("HLM: Heal recieved, reserves left: " + reserveHP + ", reduced " + instanceLists.Values[i].CmpDI.damage);
#endif
                    }
                }
            }

            public void AddInstance(Run.FixedTimeStamp time, HitlagInstance instance)
            {
                instanceLists.Add(time, instance);
            }

            public void ReleaseAll(bool andDestroyManager = false)
            {
#if DEBUG
                TurboEdition._logger.LogWarning("HLM: " + "Called ReleaseAll. Will destroy manager: " + andDestroyManager);
#endif
                //Literally just FixedUpdate minus the time check
                IList<Run.FixedTimeStamp> iListKeys = instanceLists.Keys;
                foreach (Run.FixedTimeStamp instance in iListKeys)
                {
                    if (instanceLists.TryGetValue(instance, out HitlagInstance delayedDamageToDel))
                    {
                        if (delayedDamageToDel.CmpDI.damage <= 0)
                        {
                            instanceLists.Remove(instance);
                            return; //should we do this?? no idea.
                        }
                        delayedDamageToDel.CmpOrig(delayedDamageToDel.CmpSelf, delayedDamageToDel.CmpDI);
                        instanceLists.Remove(instance);
                        return;
                    }
                }
                if (andDestroyManager)
                {
                    Destroy(this);
                }
            }

            public void CleanseAll(bool andDestroyManager = false)
            {
#if DEBUG
                TurboEdition._logger.LogWarning("HLM: " + "Called CleanseAll. Will destroy manager: " + andDestroyManager);
#endif
                IList<Run.FixedTimeStamp> iListKeys = instanceLists.Keys;
                foreach (Run.FixedTimeStamp instance in iListKeys)
                {
                    instanceLists.Remove(instance);
                }
                if (andDestroyManager)
                {
                    Destroy(this);
                }
            }
        }

        public class HitlagInstance
        {
            private On.RoR2.HealthComponent.orig_TakeDamage cmpOrig; //Orig
            private HealthComponent cmpSelf; //self
            private DamageInfo cmpDI; //DamageInfo

            public On.RoR2.HealthComponent.orig_TakeDamage CmpOrig { get => cmpOrig; set => cmpOrig = value; }
            public HealthComponent CmpSelf { get => cmpSelf; set => cmpSelf = value; }
            public DamageInfo CmpDI { get => cmpDI; set => cmpDI = value; }
        }

        /*
        public class DelayedBarStyle
        {
            //public GameObject barPrefab;
            public RoR2.UI.HealthBarStyle.BarStyle delayedBar;

            public void CreateBar()
            {
                //barPrefab = new RoR2.UI.HealthBarStyle().barPrefab;
                delayedBar = new RoR2.UI.HealthBarStyle.BarStyle();

                delayedBar.baseColor = new Color(2f, 0.9f, 0.9f, 1f);
                delayedBar.sprite = asdas;
                delayedBar.imageType = UnityEngine.UI.Image.Type.Filled;
            }
        }

        private void UpdateDelayBar(On.RoR2.UI.HealthBar.orig_UpdateBarInfos orig, RoR2.UI.HealthBar self)
        {
            orig(self);
            this.cachedFractionalValue = (self.source ? (self.source.health / self.source.fullCombinedHealth) : 0f);

            float totalDamage = Get total damage from the instance lists;
            delayedBar.enabled = (totalDamage > 0f);
            delayedBar.normalizedXMin = totalDamage; //it starts at the end
            delayedBar.normalizedXMax = cachedFractionalValue; //It ends at the end combined health
        }
        private void ApplyDelayBar(On.RoR2.UI.HealthBar.orig_ApplyBars orig, RoR2.UI.HealthBar self)
        {
            orig(self);
            self.
        }*/
    }
}
using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static TurboEdition.Utils.ItemHelpers;

//TODO okay jesus christ this is going to be hard, so:
//Get a way to store damage, get a way to modify that damage since we will be reducing it if the user heals
//Get a way to delay that damage
//Get a way to apply that damage
//Delete stored damage if the user dies, figure out if anything bad happens if you apply damage and the cb does not exist (i.e user teleporting (?))
//Think about which way is better to heal damage, a percentage? by value? both? percentage is better at scaling, but its kinda shit if the user barely has any incoming healing, but broken if they have plenty
//a value would be more "normalized" but it wont scale at all and the user will be forced to pick more of the same item if they want to heal the damage delayed, which makes more delay, and so it repeats and bla bla bla
//Think about if its better to REMOVE healing that was going to actual hp missing or to clone it, what if the user has full hp and is overhealing with a barrier?
//What the FUCK does happen if the user has RepeatHeal? I'm assuming that reserve =/ actual healing so we shouuuuldnt worry about that
//Get a healthbar component or whatever so we can show how much damage will the (TOTAL) damage that is currently being delayed will cause
//Clean damage that is about to get delayed on death, game end, and stage change

//IMPORTANT ===========>
//https://discord.com/channels/562704639141740588/562704639569428506/813439608049500231
namespace TurboEdition.Items
{
    public class Hitlag : ItemBase<Hitlag>
    {
        public override string ItemName => "Broken Fiber";

        public override string ItemLangTokenName => "HITLAG";

        public override string ItemPickupDesc => $"Delay incoming damage for <style=cIsUtility>{hitlagInitial} seconds</style>. <style=cStack>(+{hitlagStack} second per stack)</style>. Any healing incoming <style=cIsHealing>heals a bit</style> of the delayed damage.";

        public override string ItemFullDescription => $"Delay incoming damage for <style=cIsUtility>{hitlagInitial} seconds</style>. <style=cStack>(+{hitlagStack} second per stack)</style>. <style=cIsHealing>Heal for {healValueInitial}</style> <style=cStack>(+{healFractionStack}% per stack) of incoming healing.";

        public override string ItemLore => "Fuck you I liked it.";
        public override ItemTier Tier => ItemTier.Tier2;
        public override bool AIBlacklisted => false;

        public override string ItemModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        public override string ItemIconPath => "@TurboEdition:Assets/Textures/Icons/Items/Tier2.png";

        internal static GameObject hitManager;
        internal static GameObject hitComponent;

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

        protected override void CreateConfig(ConfigFile config)
        {
            hitlagInitial = config.Bind<float>("Item: " + ItemName, "Initial lag duration", 1f, "Amount of time that the damage will be delayed for when getting the item for the first item.").Value;
            hitlagStack = config.Bind<float>("Item: " + ItemName, "Stack lag duration", 1f, "Amount of time that the damage will be delayed for when stacking the item.").Value;
            healValueInitial = config.Bind<float>("Item: " + ItemName, "Initial heal value", 25f, "Amount of healing that will go to the delayed damage when you heal yourself. (On first pickup)").Value;
            healFractionStack = config.Bind<float>("Item: " + ItemName, "Stack heal percentage", 5f, "Percentage of healing that will go to the delayed damage when you heal yourself. (On item stack)").Value;
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
            //We shouldnt need these since the manager gets added to the healthcomponent gameobject via addComponent
            /*
            var hitManagerPrefab = new GameObject("HitlagManagerPrefabPrefab");
            hitManagerPrefab.AddComponent<HitlagManager>();
            hitManagerPrefab.GetComponent<HitlagManager>().netMaxCapacity = storeMaxCapacity;
            hitManager = hitManagerPrefab.InstantiateClone("HitlagManagerPrefabClone");
            */

            var hitComponentPrefab = new GameObject("HitlagComponentPrefabPrefab");
            hitComponentPrefab.AddComponent<HitlagComponent>();
            hitComponent = hitComponentPrefab.InstantiateClone("HitlagComponentPrefabClone");

            //UnityEngine.Object.Destroy(hitManagerPrefab);
            UnityEngine.Object.Destroy(hitComponentPrefab);
        }

        public override void Hooks()
        {
            //GlobalEventManager.onServerDamageDealt += StoreDamage;
            On.RoR2.HealthComponent.TakeDamage += StoreDamage;
            HealthComponent.onCharacterHealServer += GetIncomingHealing;
            //Theres uh two TakeDamageForces, one is within TakeDamage which works with damageinfo that TakeDamage gives it, other is with a vector
            //Lets hope the vector one doesnt fuck shit up, okay?
        }

        private void StoreDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            //TODO: all of these has to make sure if 1. player is alive 2. player is not teleporting 3. player has a item 4. player has a hc
            var InventoryCount = GetCount(self.body);
            var hcGameObject = self.gameObject;
            var hcHitManager = hcGameObject.GetComponentInChildren<HitlagManager>()?.gameObject; //check if the component exists or not
            //only an inventory check, i do not care if it has a hc or not (right now, not sure if there could be any issues. Doing any extra checks could lead to issues, i.e player has no items but since its dead the component wont get removed). No item, no manager.
            if (InventoryCount <= 0)
            {
                if (hcHitManager)
                {
                    UnityEngine.Object.Destroy(hcHitManager);
                }
            }
            else
            {

                if (!hcHitManager && self) //creates a manager if user has item and a hc
                {
                    hcGameObject.AddComponent<HitlagManager>();
                    hcGameObject.GetComponent<HitlagManager>().netMaxCapacity = storeMaxCapacity;
                }

                if (hcHitManager && damageInfo.damage > 0)
                {
                    if (damageInfo.damageType == DamageType.FallDamage && !storesFall)
                    {
                        orig(self, damageInfo);
                        return;
                    }
                    if (damageInfo.dotIndex == DotController.DotIndex.None && !storesDoTs)
                    {
                        orig(self, damageInfo);
                        return;
                    }
                    //We define a new object component
                    var hitInstance = UnityEngine.Object.Instantiate(hitComponent);
                    //hitInstance.AddComponent<HitlagComponent>();
                    hitInstance.GetComponent<HitlagComponent>().cmpOrig = orig;
                    hitInstance.GetComponent<HitlagComponent>().cmpSelf = self;
                    hitInstance.GetComponent<HitlagComponent>().cmpDI = damageInfo;
                    hitInstance.GetComponent<HitlagComponent>().lifeTime = (hitlagInitial + (InventoryCount - 1) * hitlagStack);
                    //We give it to the manager
                    hcHitManager.GetComponent<SortedList>().Add(hitInstance, Run.FixedTimeStamp.now);
                }
            }
            orig(self, damageInfo);
        }

        private void GetIncomingHealing(HealthComponent healthComponent, float amount)
        {
            var InventoryCount = GetCount(healthComponent.body);
            var ihGameObject = healthComponent.gameObject;
            var ihHealManager = ihGameObject.GetComponentInChildren<HitlagManager>()?.gameObject;

            if (ihHealManager && InventoryCount > 0) //If they have the hitlag manager that means they have at least one item, but lets do the extra check anyways
            {
                var thing = Mathf.Min(amount, healValueInitial);
                if (thing > healValueInitial)
                {
                    thing += ((amount - healValueInitial) * (healFractionStack / 100));
                }
                //We need to get the item of the list with the highest age
                var fuck = ihHealManager.GetComponent<SortedList>();
                fuck.GetKey(0)
                    
                TurboEdition._logger.LogWarning(ItemName + "Healing recieved! Amount: " + thing + " hitlagComponent: " );
            }
            //Get the oldest component and add the hp there

        }

        //manager that will be populated by HitlagComponents
        public class HitlagManager : NetworkBehaviour
        {
            [SyncVar] //Syncing just in case, i do not want to know what would happen if clients have different configs
            int maxCapacity;
            public int netMaxCapacity
            {
                get { return maxCapacity; }
                set { base.SetSyncVar<int>(value, ref maxCapacity, 1u); }
            }

            public SortedList<GameObject, Run.FixedTimeStamp> instanceLists;
            private void Awake()
            {
                instanceLists = new SortedList<GameObject, Run.FixedTimeStamp>();
                if (maxCapacity > 0) //i know i said -1 but like putting zero in wouldn't be funny
                {
                    instanceLists.Capacity = maxCapacity;
                }
            }
            private void FixedUpdate()
            {
                IList<GameObject> iListKeys = instanceLists.Keys;
                foreach (GameObject instance in iListKeys)
                {
                    instance.gameObject
                }
            }
        }

        //we add these to the manager
        public class HitlagComponent : NetworkBehaviour //Sure has to be network Behavior?
        {
            /*
            [SyncVar] //Lets sync so clients dont cheat
            int configFlags;
            public int netConfigFlags
            {
                get { return configFlags; }
                set { base.SetSyncVar<int>(value, ref configFlags, 1u); }
            }
            */

            //Behaviour
            private float stopwatch;
            //private Run.FixedTimeStamp timeCreated;
            private float damageReduction;
            public float healthFractionToRestorePerSecond = 0.1f;
            

            //private const float interval = 0.2f;

            public On.RoR2.HealthComponent.orig_TakeDamage cmpOrig; //Orig
            public HealthComponent cmpSelf; //self
            public DamageInfo cmpDI; //DamageInfo
            public float reserveHP;

            //Attributes given by item
            public float lifeTime;

            private void Update(float stopwatch)
            {
                if (this.stopwatch >= lifeTime)
                {
                    Destroy(this);
                }
                if (this.stopwatch <= lifeTime)
                {
                    if (this.reserveHP > 0f)
                    {
                        //I copy pasted the following two from DoubleHeal or whatever is that god forsaken lunar
                        //Temp
                        float num = Mathf.Min(this.cmpSelf.fullHealth * this.healthFractionToRestorePerSecond * 0.2f, this.reserveHP);
                        this.reserveHP -= num;
                        cmpDI.damage -= reserveHP;
                        //We uh, should, probably, do something about damageInfo.force but like theres one thousand enemies that calculate it in their own way so
                        //I think it would be funny if we just let it be
                    }
                }
            }

            private void OnDestroy()
            {
                //Lets only call orig if the damage info would actually do something
                if (cmpDI.damage > 0 /*|| ((configFlags == 1 || configFlags == 3) && cmpDI.damageType == DamageType.FallDamage) || ((configFlags == 2 || configFlags == 3) && cmpDI.dotIndex != DotController.DotIndex.None)*/)
                {
                    //healthComponent.TakeDamage(damageInfo);
                    cmpOrig(cmpSelf, cmpDI);
                }
            }

            public void AddHealing(float amount, float max)
            {
                this.reserveHP = Mathf.Min(this.reserveHP + amount, max);
            }
        }
    }
}
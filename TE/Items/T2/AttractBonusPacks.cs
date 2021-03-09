using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static TurboEdition.Utils.ItemHelpers;

namespace TurboEdition.Items
{
    public class AttractBonusPacks : ItemBase<AttractBonusPacks>
    {
        public override string ItemName => "Magnetic Belt";

        public override string ItemLangTokenName => "ATTRACTBONUSPACKS";

        public override string ItemPickupDesc => $"Attract dropped pickups. Have a low chance of <style=cIsUtility>duplicating</style> them.";

        public override string ItemFullDescription => $"Attract dropped pickups in a radius of of <style=cIsUtility>{attractInitial} meters</style>. <style=cStack>(+{attractStack} meters per stack)</style>. Have a <style=cIsUtility>{duplicationInitial * 100}% chance of duplicating</style> them.";

        public override string ItemLore => "Fuck you I liked it.";
        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlacklisted => false;
        public override bool BrotherBlacklisted => true;

        public override string ItemModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        public override string ItemIconPath => "@TurboEdition:Assets/Textures/Icons/Items/Tier2.png";

        internal static GameObject hitManager;

        //Item properties
        public float attractInitial;
        public float attractStack;
        public float duplicationInitial;
        public float duplicationStack;
        public int recoveryMode;
        public bool storesDoTs;
        public bool storesFall;
        public int storeMaxCapacity;
        public bool storeForgiveness;

        //Healthbar garbage
        //private float cachedFractionalValue = 1f;

        protected override void CreateConfig(ConfigFile config)
        {
            attractInitial = config.Bind<float>("Item: " + ItemName, "Initial attract radius", 1f, "Amount of time that the damage will be delayed for when getting the item for the first item.").Value;
            attractStack = config.Bind<float>("Item: " + ItemName, "Stack lag duration", 0.5f, "Amount of time that the damage will be delayed for when stacking the item.").Value;
            duplicationInitial = config.Bind<float>("Item: " + ItemName, "Initial heal value", 25f, "Amount of healing that will go to the delayed damage when you heal yourself. (On first pickup)").Value;
            duplicationStack = config.Bind<float>("Item: " + ItemName, "Stack heal percentage", 0.02f, "Percentage of healing that will go to the delayed damage when you heal yourself. (On item stack)").Value;
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


    }
}*/
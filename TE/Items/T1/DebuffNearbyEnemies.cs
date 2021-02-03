using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static TurboEdition.Utils.ItemHelpers;

namespace TurboEdition.Items
{
    public class DebuffNearbyEnemies : ItemBase<DebuffNearbyEnemies>
    {
        public override string ItemName => "Nanomachines";

        public override string ItemLangTokenName => "DEBUFFNEARBYENEMIES";

        public override string ItemPickupDesc => "THEY RESPONSE TO PHYSICAL TRAUMA.";

        public override string ItemFullDescription => $"When damaged by an enemy within <style=cIsUtility>{rangeRadius} meters</style>, gain <style=cIsUtility>Nanomachines</style> {buffPerStack} times for {buffDuration}. <style=cStack>(+{buffPerStack} per stack).</style>";

        public override string ItemLore => "UUUU";

        public override ItemTier Tier => ItemTier.Tier1;

        public override string ItemModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        public override string ItemIconPath => "@TurboEdition:Assets/Textures/Icons/Items/Tier3.png";


        //Item properties
        public float rangeRadius;
        public int buffPerStack;
        public int timesPerStack;
        public int buffDuration;
        public int armorPerBuff;

        public override void CreateConfig(ConfigFile config)
        {
            rangeRadius = config.Bind<float>("Item: " + ItemName, "Distance to enemy", 8f, "If an enemy within this range hits the user, it will add meleearmor buff").Value;
            buffPerStack = config.Bind<int>("Item: " + ItemName, "Buff per stack", 1, "Number of meleearmor to apply when hit.").Value;
            timesPerStack = config.Bind<int>("Item: " + ItemName, "Times per stack", 1, "Maximum number of meleearmor to apply per item / hit").Value;
            buffDuration = config.Bind<int>("Item: " + ItemName, "Buff Duration", 5, "Number in seconds of meleearmor buff duration.").Value;
            armorPerBuff = config.Bind<int>("Item: " + ItemName, "Armor per buff", 40, "Amount of armor that each buff stack gives.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override void Initialization()
        {

        }

        public override void Hooks()
        {

        }
    }
}
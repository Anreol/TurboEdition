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
    public class CritNearbyEnemies : ItemBase<CritNearbyEnemies>
    {
        public override string ItemName => "Nanomachines";

        public override string ItemLangTokenName => "MELEEARMOR";

        public override string ItemPickupDesc => "THEY RESPONSE TO PHYSICAL TRAUMA.";

        public override string ItemFullDescription => $"When damaged by an enemy within <style=cIsUtility>{rangeRadius} meters</style>, gain <style=cIsUtility>Nanomachines</style> {buffPerStack} times for {buffDuration}. <style=cStack>(+{buffPerStack} per stack).</style>";

        public override string ItemLore => "UUUU";

        public override ItemTier Tier => ItemTier.Tier3;

        public static BuffIndex meleeArmorBuff;

        public override string ItemModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        public override string ItemIconPath => "@TurboEdition:Assets/Textures/Icons/Items/Tier3.png";


        //Item properties
        public float rangeRadius;
        public int buffPerStack;
        public int timesPerStack;
        public int buffDuration;
        public int armorPerBuff;
        internal override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();
            CreateItem();
            Hooks();
        }

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
            GlobalEventManager.onServerDamageDealt += MeleeArmorCheckDistance;
            On.RoR2.CharacterBody.RecalculateStats += CheckBuffAndAddArmor;
        }

        private void MeleeArmorCheckDistance(DamageReport damageReport)
        {
            if (damageReport.attackerBody && damageReport.attackerBody != null)
            {
                var InventoryCount = GetCount(damageReport.victimBody);
                if (damageReport.victimBody.inventory)
                {
                    if(InventoryCount > 0)
                    {
                        float distance = Vector3.Distance(damageReport.victimBody.transform.position, damageReport.attackerBody.transform.position);
                        if (distance <= rangeRadius)
                        {
#if DEBUG
                            TurboEdition._logger.LogWarning("TE: " + ItemName + " distance " + distance);
#endif
                            OnDamaged(damageReport.victimBody);
                        }
                    }
                }
            }
        }
        private void OnDamaged(RoR2.CharacterBody self)
        {
            var InventoryCount = GetCount(self);
            if (self.inventory)
            {
                if (InventoryCount > 0 && self.GetBuffCount(meleeArmorBuff) < InventoryCount * timesPerStack)
                {
                    for (int i = 0; i < buffPerStack; i++)
                    {
                        self.AddTimedBuff(meleeArmorBuff, buffDuration);
                    }
                }
                else
                {
                    return;
                }
            }
        }

        private void CheckBuffAndAddArmor(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(meleeArmorBuff))
            {
                Reflection.SetPropertyValue<float>(self, "armor", self.armor + (self.GetBuffCount(meleeArmorBuff) * armorPerBuff));
                #if DEBUG
                Chat.AddMessage("Turbo Edition: " + ItemName + " CheckBuffAndAddArmor run, " + self + "'s armor is now " + self.armor);
                Chat.AddMessage("Turbo Edition: " + ItemName + " Increased armor by " + (self.GetBuffCount(meleeArmorBuff) * armorPerBuff) + ".");
                #endif
            }
        }
    }
}
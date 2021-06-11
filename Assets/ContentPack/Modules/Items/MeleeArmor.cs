using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace TurboEdition.Items
{
    public class MeleeArmor : ItemBase<MeleeArmor>
    {
        public override string ItemName => "Nanomachines";
        public override string ItemLangTokenName => "MELEEARMOR";
        public override string ItemPickupDesc => "THEY RESPONSE TO PHYSICAL TRAUMA.";
        public override string ItemFullDescription => $"When damaged by an enemy within <style=cIsUtility>{rangeRadius} meters</style>, gain <style=cIsUtility>Nanomachines</style> {buffPerStack} times for {buffDuration} seconds. <style=cStack>(+{buffPerStack} times per stack).</style>";

        public override string ItemLore => "You are right about <style=cUserSetting>one thing.</style>\nI do need <style=cIsUtility>capital</style>. And <style=cIsUtility>votes.</style>\nWanna know why?\n<style=cKeywordName>'I have a dream'</style><style=cMono>What?</style>\nThat one day <style=cIsUtility>every person in this nation</style> will control their <style=cIsHealing>OWN</style> destiny.\nA land of the <style=cIsHealing>TRULY</style> free, damnit.\nA nation of <style=cIsHealing>ACTION</style>, <style=cDeath>not words</style>.\nRuled by <style=cIsHealing>STRENGHT</style>, <style=cDeath>not committee</style>.\nWhere the law changes <style=cIsUtility>to suit the individual</style>, not the other way around\nWhere <style=cIsDamage>power</style> and <style=cIsHealth>justice</style> are back where they belong: <style=cIsUtility>in the hands of the people!</style>\nWhere every man is free to think -- <style=cIsUtility>to act</style> -- for <style=cIsHealing>himself</style>!\n" +
                                            "Fuck all these <style=cArtifact>limp-dick lawyers</style> and <style=cDeath>chicken-shit bureaucrats</style>.\nFuck <style=cIsHealth>their 24/7 internet spew of trivia</style> and <style=cDeath>celebrity bullshit.</style>\nFuck <style=cKeywordName>'American Pride.'</style><style=cUserSetting>Fuck the media!</style>\nFuck all of it!\nAmerica is <style=cDeath>diseased</style>. <style=cArtifact>Rotten to the core.</style>\nThere's no saving it -- <style=cIsUtility>we need to pull it out by the roots.</style>\nWipe the slate <style=cEvent>clean</style>. <style=cDeath>BURN IT DOWN!</style>\nAnd from the ashes a <style=cUserSetting>new America</style> will be born.\n<style=cIsUtility>Evolved</style>, but <style=cIsDamage>untamed</style>!\nThe <style=cStack>weak</style> will be <style=cDeath>purged</style>, and the <style=cStack>strongest</style> will <style=cIsHealing>thrive</style> -- <style=cIsUtility>free to live as they see fit</style>,\n<style=cShrine>they'll make America great again!</style>\n\n<style=cMono>What the hell are you talking about...</style>";

        public override ItemTier Tier => ItemTier.Tier3;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlacklisted => false;
        public override bool BrotherBlacklisted => false;
        public static BuffIndex meleeArmorBuff;

        public override GameObject ItemModel => TurboEdition.MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Default.prefab");
        public override Sprite ItemIcon => TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Items/Tier3.png");

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
            //Initialization();
            Hooks();
        }

        protected override void CreateConfig(ConfigFile config)
        {
            rangeRadius = config.Bind<float>("Item: " + ItemName, "Distance to enemy", 8f, "If an enemy within this range hits the user, it will add meleearmor buff").Value;
            buffPerStack = config.Bind<int>("Item: " + ItemName, "Buff per stack", 1, "Number of meleearmor to apply when hit.").Value;
            timesPerStack = config.Bind<int>("Item: " + ItemName, "Times per stack", 1, "Maximum number of meleearmor to apply per item / hit").Value;
            buffDuration = config.Bind<int>("Item: " + ItemName, "Buff Duration", 5, "Number in seconds of meleearmor buff duration.").Value;
            armorPerBuff = config.Bind<int>("Item: " + ItemName, "Armor per buff", 40, "Amount of armor that each buff stack gives.").Value;
        }

        private void CreateBuff()
        {
            var meleeArmorBuff = ScriptableObject.CreateInstance<BuffDef>();
            meleeArmorBuff.buffColor = Color.white;
            meleeArmorBuff.canStack = false;
            meleeArmorBuff.isDebuff = false;
            meleeArmorBuff.name = "Nanomachines";
            meleeArmorBuff.iconSprite = TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Buffs/meleearmor.png");
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
                    if (InventoryCount > 0)
                    {
                        //i thought that this might not be the best way to calculate damage, since this gets the POSITION of the body, like, the core (?), so lets say, if theres a GIANT roboball that has a radius bigger than this distance, the check would fail since you are comparing it to the TRANSFORM, not to hitboxes or anything.
                        //But i also think that doing a raycast to check for hitboxes would be too expensive, so /shrug
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

            if (InventoryCount > 0 && self.GetBuffCount(meleeArmorBuff) < InventoryCount * timesPerStack)
            {
                for (int i = 0; i < buffPerStack; i++)
                {
                    self.AddTimedBuff(meleeArmorBuff, buffDuration);
                }
            }
            return;
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
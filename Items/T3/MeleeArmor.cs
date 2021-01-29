using BepInEx.Configuration;
using R2API;
using RoR2;

namespace TurboEdition.Items
{
    public class MeleeArmor : ItemBase
    {
        public override string ItemName => "Nanomachines";

        public override string ItemLangTokenName => "MELEE_ARMOR";

        public override string ItemPickupDesc => "THEY RESPONSE TO PHYSICAL TRAUMA.";

        public override string ItemFullDescription => $"When damaged by an enemy within <style=cIsUtility>{rangeRadius} meters</style>, gain <style=cIsUtility>Nanomachines</style> {buffPerStack} times. <style=cStack>(+{buffPerStack} per stack).</style>";

        public override string ItemLore => "You are right about <style=cUserSetting>one thing.</style>\nI do need <style=cIsUtility>capital</style>. And <style=cIsUtility>votes.</style>\nWanna know why?\n<style=cKeywordName>'I have a dream'</style><style=cMono>What?</style>\nThat one day <style=cIsUtility>every person in this nation</style> will control their <style=cIsHealing>OWN</style> destiny.\nA land of the <style=cIsHealing>TRULY</style> free, damnit.\nA nation of <style=cIsHealing>ACTION</style>, <style=cDeath>not words</style>.\nRuled by <style=cIsHealing>STRENGHT</style>, <style=cDeath>not committee</style>.\nWhere the law changes <style=cIsUtility>to suit the individual</style>, not the other way around\nWhere <style=cIsDamage>power</style> and <style=cIsHealth>justice</style> are back where they belong: <style=cIsUtility>in the hands of the people!</style>\nWhere every man is free to think -- <style=cIsUtility>to act</style> -- for <style=cIsHealing>himself</style>!\n" +
                                            "Fuck all these <style=cArtifact>limp-dick lawyers</style> and <style=cDeath>chicken-shit bureaucrats</style>.\nFuck <style=cIsHealth>their 24/7 internet spew of trivia</style> and <style=cDeath>celebrity bullshit.</style>\nFuck <style=cKeywordName>'American Pride.'</style><style=cUserSetting>Fuck the media!</style>\nFuck all of it!\nAmerica is <style=cDeath>diseased</style>. <style=cArtifact>Rotten to the core.</style>\nThere's no saving it -- <style=cIsUtility>we need to pull it out by the roots.</style>\nWipe the slate <style=cEvent>clean</style>. <style=cDeath>BURN IT DOWN!</style>\nAnd from the ashes a <style=cUserSetting>new America</style> will be born.\n<style=cIsUtility>Evolved</style>, but <style=cIsDamage>untamed</style>!\nThe <style=cStack>weak</style> will be <style=cDeath>purged</style>, and the <style=cStack>strongest</style> will <style=cIsHealing>thrive</style> -- <style=cIsUtility>free to live as they see fit</style>,\n<style=cShrine>they'll make America great again!</style>\n\n<style=cMono>What the hell are you talking about...</style>";

        public override ItemTier Tier => ItemTier.Tier3;

        public override string ItemModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        public override string ItemIconPath => "@TurboEdition:Assets/Textures/Icons/Items/Tier3.png";


        //Item properties
        public float rangeRadius;
        public float buffPerStack;

        public override void CreateConfig(ConfigFile config)
        {
            rangeRadius = config.Bind<float>("Item: " + ItemName, "Radius in meters", 6f, "If an enemy within this range hits the user, it will add meleearmor buff").Value;
            buffPerStack = config.Bind<int>("Item: " + ItemName, "Buff per stack", 1, "Number of meleearmor to apply when hit.").Value;
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
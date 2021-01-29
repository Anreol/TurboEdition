using BepInEx.Configuration;
using R2API;
using RoR2;

namespace TurboEdition.Items
{
    public class MeleeArmor : ItemBase
    {
        public override string ItemName => "Nanomachines";

        public override string ItemLangTokenName => "TE_MELEE_ARMOR";

        public override string ItemPickupDesc => "Son!";

        public override string ItemFullDescription => "THEY RESPONSE TO PHYSICAL TRAUMA";

        public override string ItemLore => "You are right about one thing.\nI do need capital. And votes.\nWanna know why?\n'I have a dream'\n\nWhat?\n\nThat one day every person in this nation will control their OWN destiny.\nA land of the TRULY free, damnit.\nA nation of ACTION, not words.\nRuled by STRENGHT, not committee.\nWhere the law changes to suit the individual, not the other way around\nWhere power and justice are back where they belong: in the hands of the people!\nWhere every man is free to think -- to act -- for himself!\n" +
                                            "Fuck all these limp-dick lawyers and chicken-shit bureaucrats.\nFuck their 24/7 internet spew of trivia and celebrity bullshit.\nFuck 'American Pride.' Fuck the media!\nFuck all of it!\nAmerica is diseased. Rotten to the core.\nThere's no saving it -- we need to pull it out by the roots.\nWipe the slate clean. BURN IT DOWN!\nAnd from the ashes a new America will be born.\nEvolved, but untamed!\nThe weak will be purged, and the strongest will thrive -- free to live as they see fit,\nthey'll make America great again!\n\nWhat the hell are you talking about...";

        public override ItemTier Tier => ItemTier.Tier3;

        public override string ItemModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        public override string ItemIconPath => "@TurboEdition:Assets/Textures/Icons/Items/Tier3.png";


        public override void CreateConfig(ConfigFile config)
        {

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
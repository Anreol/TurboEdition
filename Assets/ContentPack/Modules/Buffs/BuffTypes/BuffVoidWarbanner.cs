using RoR2;

namespace TurboEdition.Buffs
{
    public class BuffVoidWarbanner : Buff
    {
        public override BuffDef buffDef { get; set; } = Assets.mainAssetBundle.LoadAsset<BuffDef>("BuffVoidWarbanner");

        private float regenBonus = 0;
        private float armorBonus = 0;
        private float sprintBonus = 0;
        private float attackSpeedBonus = 0;

        public override void Initialize()
        {
        }

        public override void BuffStep(ref CharacterBody body, int stack)
        {
            //CalculateBonuses(); //We dont need to calculate in every step of the way. Not until we get storms.
        }

        public override void OnBuffFirstStackGained(ref CharacterBody body)
        {
            ClearAll();
            CalculateBonuses();
        }

        public override void OnBuffLastStackLost(ref CharacterBody body)
        {
            //Clearing just in case
            ClearAll();
        }

        private void CalculateBonuses()
        {
            ClearAll();

            if (SceneCatalog.mostRecentSceneDef.sceneType == SceneType.Stage)
            {
                //if (scenesDay.Contains(SceneCatalog.mostRecentSceneDef.baseSceneName))
                regenBonus += (Run.instance.stageClearCount + 1);
                //if (scenesNight.Contains(SceneCatalog.mostRecentSceneDef.baseSceneName))
                armorBonus += 2 * (Run.instance.stageClearCount + 1);
                sprintBonus += 0.05f * (Run.instance.stageClearCount + 1); //0.20 less than a soda
                attackSpeedBonus += 0.025f * (Run.instance.stageClearCount + 1); //0.125 less than a syringe
            }
            if (SceneCatalog.mostRecentSceneDef.sceneType == SceneType.Intermission || SceneCatalog.mostRecentSceneDef.baseSceneName == "moon2") //Hidden realms and moon will add a bonus no matter what. Original moon counts as HR
            {
                regenBonus += (Run.instance.stageClearCount + 1);
                armorBonus += 2 * (Run.instance.stageClearCount + 1);
                sprintBonus += 0.05f * (Run.instance.stageClearCount + 1); //0.20 less than a soda
                attackSpeedBonus += 0.025f * (Run.instance.stageClearCount + 1); //0.125 less than a syringe
            }
            if (SceneCatalog.mostRecentSceneDef.isFinalStage)
            {
                regenBonus *= 1.15f;
                armorBonus *= 1.15f;
                sprintBonus *= 1.15f;
                attackSpeedBonus *= 1.15f;
            }
            //TODO: Add Starstorm storm events bonus
        }

        /*
        public static String[] scenesDay = new String[]
        {
            "blackbeach",
            "foggyswamp",
            "golemplains",
            "golemplains2",
            "goolake",
            "rootjungle",
            "shipgraveyard",
            "skymeadow",
            "wispgraveyard",
        };

        public static String[] scenesNight = new String[]
        {
            "frozenwall",
            "goldshores"
        };

        public static String[] scenesHRs = new String[]
        {
            "arena",
            "artifactworld",
            "bazaar",
            "goldshores",
            "limbo",
            "mysteryspace"
        };*/

        private void ClearAll()
        {
            regenBonus = 0;
            armorBonus = 0;
            sprintBonus = 0;
            attackSpeedBonus = 0;
        }

        public override void RecalcStatsEnd(ref CharacterBody body)
        {
            body.armor += armorBonus;
            body.regen += regenBonus;
            body.sprintingSpeedMultiplier += sprintBonus;
            body.attackSpeed += attackSpeedBonus;
        }
    }
}
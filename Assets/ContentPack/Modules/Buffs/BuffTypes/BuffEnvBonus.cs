﻿using RoR2;
using System;
using System.Linq;

namespace TurboEdition.Buffs
{
    public class BuffEnvBonus : Buff
    {
        public override BuffDef buffDef { get; set; } = Assets.mainAssetBundle.LoadAsset<BuffDef>("BuffEnvBonus");

        private float regenBonus = 0;
        private float armorBonus = 0;

        public override void Initialize()
        {
        }

        public override void BuffStep(ref CharacterBody body, int stack)
        {
            CalculateBonuses();
        }

        public override void OnBuffFirstStackGained(ref CharacterBody body)
        {
            CalculateBonuses();
        }

        private void CalculateBonuses()
        {
            if (SceneCatalog.mostRecentSceneDef.sceneType == SceneType.Stage)
            {
                if (scenesDay.Contains(SceneCatalog.mostRecentSceneDef.baseSceneName))
                    regenBonus += 2 * (Run.instance.stageClearCount + 1);
                if (scenesNight.Contains(SceneCatalog.mostRecentSceneDef.baseSceneName))
                    armorBonus += 2 * (Run.instance.stageClearCount + 1);
            }
            if (SceneCatalog.mostRecentSceneDef.sceneType == SceneType.Intermission || SceneCatalog.mostRecentSceneDef.baseSceneName == "moon2") //Hidden realms and moon will add a bonus no matter what. Original moon counts as HR
            {
                regenBonus += 2 * (Run.instance.stageClearCount + 1);
                armorBonus += 2 * (Run.instance.stageClearCount + 1);
            }
            if (SceneCatalog.mostRecentSceneDef.isFinalStage)
            {
                regenBonus *= 1.25f;
                armorBonus *= 1.25f;
            }
            //TODO: Add Starstorm storm events bonus
        }

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
            "wispgraveyard"
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
        };

        public override void RecalcStatsEnd(ref CharacterBody body)
        {
            body.armor += armorBonus;
            body.regen += regenBonus;
        }
    }
}
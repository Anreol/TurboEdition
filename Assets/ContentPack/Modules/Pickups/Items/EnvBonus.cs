using RoR2;
using System;
using System.Linq;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class EnvBonus : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("EnvBonus");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<EnvBonusBehavior>(stack);
        }

        internal class EnvBonusBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {
            private float activationWindow = 30f;
            private float regenBonus = 0f;
            private float armorBonus = 0f;

            private void OnEnable()
            {
                if (!NetworkServer.active)
                {
                    return;
                }
                if (Stage.instance.entryTime.timeSince <= activationWindow && body)
                {
                    body.AddTimedBuff(Assets.mainAssetBundle.LoadAsset<BuffDef>("EnvBuff"), 15 + ((stack - 1) * 10));
                }
                CalculateBonuses();
            }

            public void RecalculateStatsEnd()
            {
                if (!body.HasBuff(Assets.mainAssetBundle.LoadAsset<BuffDef>("EnvBuff")))
                {
                    return;
                }
                body.armor += armorBonus;
                body.regen += regenBonus;
            }

            public void RecalculateStatsStart()
            { }

            private void CalculateBonuses()
            {
                if (SceneCatalog.mostRecentSceneDef.sceneType == SceneType.Stage)
                {
                    if (scenesDay.Contains(SceneCatalog.mostRecentSceneDef.baseSceneName))
                        regenBonus += 5 * (Run.instance.stageClearCount + 1);
                    if (scenesNight.Contains(SceneCatalog.mostRecentSceneDef.baseSceneName))
                        armorBonus += 5 * (Run.instance.stageClearCount + 1);
                }
                if (SceneCatalog.mostRecentSceneDef.sceneType == SceneType.Intermission || SceneCatalog.mostRecentSceneDef.baseSceneName == "moon2") //Hidden realms and moon will add a bonus no matter what. Original moon counts as HR
                {
                    regenBonus += 5 * (Run.instance.stageClearCount + 1);
                    armorBonus += 5 * (Run.instance.stageClearCount + 1);
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
        }
    }
}
using RoR2;
using System;
using System.Linq;
using UnityEngine.Networking;

namespace TurboEdition
{
    internal class EnvBonusBehavior : CharacterBody.ItemBehavior
    {
        private float activationWindow = 30f;
        private float regenBonus = 0f;
        private float armorBonus = 0f;

        private void OnEnable()
        {
            body.onInventoryChanged += ItemCheck;
            if (!NetworkServer.active)
            {
                return;
            }
            if (Stage.instance.entryTime.timeSince <= activationWindow)
            {
                body.AddTimedBuff(Assets.mainAssetBundle.LoadAsset<BuffDef>("EnvBuff"), 15 + ((stack - 1) * 10));
            }
        }

        private void ItemCheck()
        {
            if (body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("EnvBonus")) <= 0)
                Destroy(this);
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active || !body.HasBuff(Assets.mainAssetBundle.LoadAsset<BuffDef>("EnvBuff")))
            {
                return;
            }
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
            body.armor += armorBonus;
            body.regen += regenBonus;
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
using RoR2;
using System;
using TurboEdition.ScriptableObjects;
using UnityEngine;

namespace TurboEdition.Utils
{
    internal class SceneDirectorInjector
    {
        private static SerializableDirectorCard[] cards;
        private static bool logCards = false;

        [SystemInitializer(new Type[]
        {
            typeof(SceneCatalog),
        })]
        private static void Init()
        {
            cards = new SerializableDirectorCard[]
            {
                //Shrine Overcharger, one has more weight for the other.
                Assets.mainAssetBundle.LoadAsset<SerializableDirectorCard>("sdcShrineOvercharger"),
                Assets.mainAssetBundle.LoadAsset<SerializableDirectorCard>("sdcShrineOverchargerCommon")
            };

            //TODO: AS OF JULY 4, LEAVE IT FOR NEXT UPDATE
            SceneDirector.onGenerateInteractableCardSelection += AddDirectorCards;
            SceneDirector.onPrePopulateMonstersSceneServer += ExplicitInteracteableGeneration;

            //Scenes
            SceneCollection.SceneEntry observatoryEntry = new SceneCollection.SceneEntry()
            {
                sceneDef = TEContent.Scenes.observatory,
                weightMinusOne = 0
            };
            HG.ArrayUtils.ArrayAppend<SceneCollection.SceneEntry>(ref SceneCatalog.GetSceneDefFromSceneName("dampcavesimple").destinationsGroup._sceneEntries, observatoryEntry);
        }

        private static void ExplicitInteracteableGeneration(SceneDirector obj)
        {
            Xoroshiro128Plus xoroshiro128Plus = new Xoroshiro128Plus(obj.rng.nextUlong);
            if (Util.GetItemCountForTeam(TeamIndex.Player, TEContent.Items.MoneyBank.itemIndex, false, true) > 0)
            {
                Transform moneyBankTarget;
                if (SceneInfo.instance.countsAsStage)
                {
                    moneyBankTarget = TeleporterInteraction.instance ? TeleporterInteraction.instance.transform : SpawnPoint.readOnlyInstancesList[xoroshiro128Plus.RangeInt(0, SpawnPoint.readOnlyInstancesList.Count)].transform;
                }
                else
                {
                    moneyBankTarget = SpawnPoint.readOnlyInstancesList[xoroshiro128Plus.RangeInt(0, SpawnPoint.readOnlyInstancesList.Count)].transform;
                }
                if (moneyBankTarget)
                {
                    DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(Assets.mainAssetBundle.LoadAsset<SpawnCard>("iscMoneyBank"), new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Approximate, //Let's me spawn in the target, and specify min and max distances,
                        maxDistance = 80f,
                        minDistance = 6f,
                        spawnOnTarget = moneyBankTarget
                    }, xoroshiro128Plus));
                }
            }
        }

        private static void LogDirectorCards(SceneDirector arg1, DirectorCardCategorySelection arg2)
        {
            TELog.LogE("\n\nLogging cards:", true);
            foreach (var item in arg2.categories)
            {
                TELog.LogW("Director category: " + item.name, true);
                foreach (var card in item.cards)
                {
                    TELog.LogW("Spawn Card: " + card.spawnCard + " Prefab: " + card.spawnCard.prefab + "Is valid: " + card.IsAvailable(), true);
                }
            }
        }

        private static void AddDirectorCards(SceneDirector arg1, DirectorCardCategorySelection arg2)
        {
            foreach (SerializableDirectorCard sdc in cards)
            {
                for (int i = 0; i < sdc.sceneNamesToBeUsedIn.Length; i++)
                {
                    if (SceneCatalog.mostRecentSceneDef == SceneCatalog.GetSceneDefFromSceneName(sdc.sceneNamesToBeUsedIn[i]))
                    {
                        int index = FixedFindCategoryIndexByName(ref arg2, sdc.categoryName);
                        if (index != -1)
                        {
                            arg2.AddCard(index, sdc.CreateDirectorCard());
                        }
                        continue;
                    }
                }
            }
            if (logCards)
            {
                LogDirectorCards(arg1, arg2);
            }
        }

        public static int FixedFindCategoryIndexByName(ref DirectorCardCategorySelection dccs, string categoryName)
        {
            for (int i = 0; i < dccs.categories.Length; i++)
            {
                if (string.CompareOrdinal(dccs.categories[i].name, categoryName) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        [ConCommand(commandName = "te_list_DirectorInteractableCardsOnStageChange", flags = ConVarFlags.None, helpText = "Lists all ListDirectorInteractable cards whenever a stage changes.")]
        private static void ListDirectorInteractable(ConCommandArgs args)
        {
            logCards = args.TryGetArgBool(0) ?? false;
        }
    }
}
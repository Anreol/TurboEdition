using RoR2;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TurboEdition.Misc
{
    internal class CombatDirectorInjector
    {
        private const bool USE_UNIQUE_DCCS = false;
        public static DccsPool.ConditionalPoolEntry frozenWallExtraEntry;
        public static DccsPool.ConditionalPoolEntry wispGraveyardExtraEntry;
        public static DccsPool.ConditionalPoolEntry dampCaveExtraEntry;
        internal static bool needsRefresh;

        public static DirectorCard cscImpBomber;

        [SystemInitializer]
        public static void Init()
        {
            if (USE_UNIQUE_DCCS)
            {
                frozenWallExtraEntry = new DccsPool.ConditionalPoolEntry();
                frozenWallExtraEntry.requiredExpansions = new ExpansionDef[] { TEContent.Expansions.TurboExpansion };
                frozenWallExtraEntry.weight = 1;
                frozenWallExtraEntry.dccs = Assets.mainAssetBundle.LoadAsset<DirectorCardCategorySelection>("dccsFrozenWallMonstersTE");

                wispGraveyardExtraEntry = new DccsPool.ConditionalPoolEntry();
                wispGraveyardExtraEntry.requiredExpansions = new ExpansionDef[] { TEContent.Expansions.TurboExpansion };
                wispGraveyardExtraEntry.weight = 1;
                wispGraveyardExtraEntry.dccs = Assets.mainAssetBundle.LoadAsset<DirectorCardCategorySelection>("dccsWispGraveyardMonstersTE");

                dampCaveExtraEntry = new DccsPool.ConditionalPoolEntry();
                dampCaveExtraEntry.requiredExpansions = new ExpansionDef[] { TEContent.Expansions.TurboExpansion };
                dampCaveExtraEntry.weight = 1;
                dampCaveExtraEntry.dccs = Assets.mainAssetBundle.LoadAsset<DirectorCardCategorySelection>("dccsDampCaveMonstersTE");

                Stage.onServerStageBegin += onServerStageBeginUnique; //Server has auth over stage
            }
            //RoR2Content.mixEnemyMonsterCards.AddCard(3, frozenWallExtraEntry.dccs.categories[3].cards[0]); //ImpBomber
            Stage.onServerStageBegin += onServerStageBeginNormal; //Server has auth over stage
        }

        private static void onServerStageBeginUnique(Stage obj)
        {
            if (obj.sceneDef == SceneCatalog.GetSceneDefFromSceneName("frozenwall"))
            {
                //Pool categories usually have: Standard, Family event, and (maybe?) VoidInvasion
                if (frozenWallExtraEntry != null)
                    HG.ArrayUtils.ArrayAppend<DccsPool.ConditionalPoolEntry>(ref ClassicStageInfo.instance.monsterDccsPool.poolCategories[0].includedIfConditionsMet, frozenWallExtraEntry);
                ClassicStageInfo.instance.RebuildCards();
                RefreshAllAvailableDirectorCards();
            }
            if (obj.sceneDef == SceneCatalog.GetSceneDefFromSceneName("wispgraveyard"))
            {
                //Pool categories usually have: Standard, Family event, and (maybe?) VoidInvasion
                if (wispGraveyardExtraEntry != null)
                    HG.ArrayUtils.ArrayAppend<DccsPool.ConditionalPoolEntry>(ref ClassicStageInfo.instance.monsterDccsPool.poolCategories[0].includedIfConditionsMet, wispGraveyardExtraEntry);
                ClassicStageInfo.instance.RebuildCards();
                RefreshAllAvailableDirectorCards();
            }
            if (obj.sceneDef == SceneCatalog.GetSceneDefFromSceneName("voidraid"))
            {
                GameObject gameObject = GameObject.Find("EncounterPhases/VoidRaidCrabCombatEncounter Phase 1/");
                if (gameObject)
                {
                    gameObject.GetComponent<ScriptedCombatEncounter>().spawns[0].explicitSpawnPosition.position = new Vector3(3.6765f, 89.98f, 0f);
                    gameObject.GetComponent<ScriptedCombatEncounter>().spawns[0].spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC1/VoidRaidCrab/cscVoidRaidCrab.asset").WaitForCompletion();
                }
            }
        }

        //Pool categories usually have: Standard, Family event, and (maybe?) VoidInvasion
        private static void onServerStageBeginNormal(Stage obj)
        {
            DirectorCardCategorySelection dccs = null;
            if (!ClassicStageInfo.instance) //Bazaar doesn't seem to have it.
            {
                return;
            }
            if (obj.sceneDef == SceneCatalog.GetSceneDefFromSceneName("frozenwall"))
            {
                dccs = Assets.mainAssetBundle.LoadAsset<DirectorCardCategorySelection>("dccsFrozenWallMonstersTE");
            }
            if (obj.sceneDef == SceneCatalog.GetSceneDefFromSceneName("wispgraveyard"))
            {
                dccs = Assets.mainAssetBundle.LoadAsset<DirectorCardCategorySelection>("dccsWispGraveyardMonstersTE");
            }
            if (ClassicStageInfo.instance.monsterDccsPool != null && ClassicStageInfo.instance.monsterDccsPool.poolCategories[0] != null && dccs != null)
            {
                foreach (var item in ClassicStageInfo.instance.monsterDccsPool.poolCategories[0].alwaysIncluded)
                {
                    CopyDccsToDccs(dccs, item.dccs);
                }
                foreach (var item in ClassicStageInfo.instance.monsterDccsPool.poolCategories[0].includedIfConditionsMet)
                {
                    CopyDccsToDccs(dccs, item.dccs);
                }
                foreach (var item in ClassicStageInfo.instance.monsterDccsPool.poolCategories[0].includedIfNoConditionsMet)
                {
                    CopyDccsToDccs(dccs, item.dccs);
                }
            }
            if (needsRefresh)
            {
                ClassicStageInfo.instance.RebuildCards();
                RefreshAllAvailableDirectorCards();
            }
        }

        public static void DumpInfo(DccsPool.ConditionalPoolEntry[] conditionalPoolEntry)
        {
            foreach (var poolEntry in conditionalPoolEntry)
            {
                foreach (var category in poolEntry.dccs.categories)
                {
                    foreach (var card in category.cards)
                    {
                        Debug.LogWarning("Card: " + card + " is valid: " + card.IsAvailable());
                        if (card.spawnCard != null)
                        {
                            Debug.Log("spawncard " + card.spawnCard + " prefab: " + card.spawnCard.prefab);
                        }
                    }
                }
            }
            Debug.LogError("Done dumping Info from conditionalPoolEntry");
        }

        public static void DumpInfo(WeightedSelection<DirectorCard> weightedSelection)
        {
            foreach (var choice in weightedSelection.choices)
            {
                Debug.Log("Choice Weight: " + choice.weight);
                if (choice.value == null)
                {
                    Debug.LogError("No Value, leaving.");
                    continue;
                }
                Debug.Log("Choice IsAvaliable: " + choice.value.IsAvailable());
                if (choice.value.spawnCard != null)
                {
                    Debug.Log("Prefab " + choice.value.spawnCard.prefab + " card: " + choice.value.spawnCard);
                }
            }
            Debug.LogError("Done dumping Info from weightedSelection");
        }

        public static void CopyDccsToDccs(DirectorCardCategorySelection source, DirectorCardCategorySelection dest)
        {
            for (int i = 0; i < source.categories.Length; i++)
            {
                if (i > dest.categories.Length)
                {
                    HG.ArrayUtils.ArrayAppend<DirectorCardCategorySelection.Category>(ref dest.categories, source.categories[i]);
                }
                if (dest.categories[i].name == source.categories[i].name)
                {
                    needsRefresh = true;
                    foreach (var item in source.categories[i].cards)
                    {
                        HG.ArrayUtils.ArrayAppend<DirectorCard>(ref dest.categories[i].cards, item);
                    }
                }
            }
        }

        public static void RefreshAllAvailableDirectorCards()
        {
            TELog.LogW("Refreshing director cards in " + CombatDirector.instancesList.Count + " directors.");
            foreach (CombatDirector director in CombatDirector.instancesList)
            {
                TELog.LogW("Trying to refresh cards... fallback to stage monsters is " + director.fallBackToStageMonsterCards);
                if (director.fallBackToStageMonsterCards) //Is set to true by default, so we hope that nobody has set it to false. It isn't used by the game itself anywhere.
                {
                    TELog.LogW("Falling back to stage monster cards... is _monsterCards null" + director._monsterCards == null);
                    if (director._monsterCards == null) //This is internal and should never change. If its null, it means it doesn't have explicit monster cards and is instead taking from the stage.
                    {
                        TELog.LogW("TE Combat Director Cards refreshed.");
                        director.monsterCardsSelection = null; //Force a reset to refresh cards from the late modified ClassicStageInfo.
                                                               //This shouldn't break anything as we aren't... modifying the internal cards..?
                    }
                }
            }
        }
    }
}
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.Misc
{
    class CombatDirectorInjector
    {
        public static DccsPool.ConditionalPoolEntry frozenWallExtraEntry;
        public static DccsPool.ConditionalPoolEntry wispGraveyardExtraEntry;
        public static DccsPool.ConditionalPoolEntry dampCaveExtraEntry;

        [SystemInitializer]
        public static void Init()
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

            //RoR2Content.mixEnemyMonsterCards.AddCard(3, frozenWallExtraEntry.dccs.categories[3].cards[0]); //ImpBomber
            Stage.onServerStageBegin += onServerStageBegin; //Server has auth over stage
        }

        private static void onServerStageBegin(Stage obj)
        {
            if (obj.sceneDef == SceneCatalog.GetSceneDefFromSceneName("frozenwall"))
            {
                //Pool categories usually have: Standard, Family event, and (maybe?) VoidInvasion
                DumpInfo(ClassicStageInfo.instance.monsterDccsPool.poolCategories[0].includedIfConditionsMet);
                if (ClassicStageInfo.instance.monsterSelection != null)
                {
                    Debug.LogWarning("Dumping ClassicStageInfo.instance.monsterSelection...");
                    DumpInfo(ClassicStageInfo.instance.monsterSelection);
                }
                foreach (var item in CombatDirector.instancesList)
                {
                    Debug.LogWarning("Dumping finalMonsterCardsSelection...");
                    if (item.finalMonsterCardsSelection == null)
                    {
                        Debug.LogWarning("Skipped as it was null.");
                        continue;
                    }
                    DumpInfo(item.finalMonsterCardsSelection);
                }

                Debug.LogWarning("Appending our cards");
                HG.ArrayUtils.ArrayAppend<DccsPool.ConditionalPoolEntry>(ref ClassicStageInfo.instance.monsterDccsPool.poolCategories[0].includedIfConditionsMet, frozenWallExtraEntry);
                ClassicStageInfo.instance.RebuildCards();
                RefreshAllAvailableDirectorCards();

                DumpInfo(ClassicStageInfo.instance.monsterDccsPool.poolCategories[0].includedIfConditionsMet);
                if (ClassicStageInfo.instance.monsterSelection != null)
                {
                    Debug.LogWarning("Dumping ClassicStageInfo.instance.monsterSelection...");
                    DumpInfo(ClassicStageInfo.instance.monsterSelection);
                }
                foreach (var item in CombatDirector.instancesList)
                {
                    Debug.LogWarning("Dumping finalMonsterCardsSelection...");
                    if (item.finalMonsterCardsSelection == null)
                    {
                        Debug.LogWarning("Skipped as it was null.");
                        continue;
                    }
                    DumpInfo(item.finalMonsterCardsSelection);
                }
            }
            if (obj.sceneDef == SceneCatalog.GetSceneDefFromSceneName("wispgraveyard"))
            {
                //Pool categories usually have: Standard, Family event, and (maybe?) VoidInvasion
                Debug.LogWarning("Appending our cards");
                HG.ArrayUtils.ArrayAppend<DccsPool.ConditionalPoolEntry>(ref ClassicStageInfo.instance.monsterDccsPool.poolCategories[0].includedIfConditionsMet, wispGraveyardExtraEntry);
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
        public static void RefreshAllAvailableDirectorCards()
        {
            Debug.Log("Refreshing director cards in " + CombatDirector.instancesList.Count + " directors.");
            foreach (CombatDirector director in CombatDirector.instancesList)
            {
                Debug.LogWarning("Trying to refresh cards... fallback to stage monsters is " + director.fallBackToStageMonsterCards);
                if (director.fallBackToStageMonsterCards) //Is set to true by default, so we hope that nobody has set it to false. It isn't used by the game itself anywhere.
                {
                    Debug.LogWarning("Falling back to stage monster cards... is _monsterCards null" + director._monsterCards == null);
                    if (director._monsterCards == null) //This is internal and should never change. If its null, it means it doesn't have explicit monster cards and is instead taking from the stage. 
                    {
                        Debug.LogWarning("TE Combat Director Cards refreshed.");
                        director.monsterCardsSelection = null; //Force a reset to refresh cards from the late modified ClassicStageInfo.
                                                           //This shouldn't break anything as we aren't... modifying the internal cards..?
                    }
                }
            }
        }
    }
}

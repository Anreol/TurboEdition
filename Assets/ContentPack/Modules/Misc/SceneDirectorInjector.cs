using RoR2;
using System;

namespace TurboEdition.Misc
{
    internal class SceneDirectorInjector
    {
        private static DirectorCard questShrineDirectorCard = new DirectorCard();
        private static bool logCards = false;

        [SystemInitializer(new Type[]
        {
            typeof(SceneCatalog),
        })]
        private static void Init()
        {
            GenerateDirectorCards();
            SceneDirector.onGenerateInteractableCardSelection += SceneDirector_onGenerateInteractableCardSelection;
            //SceneDirector.onGenerateInteractableCardSelection += SceneDirector_onGenerateInteractableCardSelection1;
        }

        private static void SceneDirector_onGenerateInteractableCardSelection1(SceneDirector arg1, DirectorCardCategorySelection arg2)
        {
            if (logCards)
            {
                TELog.LogE("\n\nLogging cards\n\n");
                foreach (var item in arg2.categories)
                {
                    TELog.LogW("Director category: " + item.name);
                    foreach (var card in item.cards)
                    {
                        TELog.LogD("Spawn Card: " + card.spawnCard + " Prefab: " + card.spawnCard.prefab + "Is valid: " + card.IsAvailable()) ;
                    }
                }
            }
        }

        private static void GenerateDirectorCards()
        {
            //Director cards are set in an director card category selection array, its not an asset or something else
            questShrineDirectorCard.spawnCard = Assets.mainAssetBundle.LoadAsset<InteractableSpawnCard>("iscShrineQuest");
            questShrineDirectorCard.selectionWeight = 4; //Same as chance.
            questShrineDirectorCard.spawnDistance = DirectorCore.MonsterSpawnDistance.Standard; //Dont think it matters in interacteables
            questShrineDirectorCard.preventOverhead = false; //only used in the combat director
            questShrineDirectorCard.minimumStageCompletions = 0; //Could be fun to toy with
        }

        private static bool AddQuestShrine(ref SceneDirector arg1, ref DirectorCardCategorySelection arg2)
        {
            foreach (string item in questShrineScenes)
            {
                if (SceneCatalog.mostRecentSceneDef == SceneCatalog.GetSceneDefFromSceneName(item))
                {
                    int index = FixedFindCategoryIndexByName(ref arg2, "Shrines");
                    if (index != -1)
                    {
                        TELog.LogW("Adding card. " + questShrineDirectorCard);
                        TELog.LogW(arg2.categories[index].cards.Length);
                        arg2.AddCard(index, questShrineDirectorCard);
                        TELog.LogW("Added card. " + questShrineDirectorCard);
                        TELog.LogW(arg2.categories[index].cards.Length);
                        return true;
                    }
                }
            }
            return false;
        }

        private static void SceneDirector_onGenerateInteractableCardSelection(SceneDirector arg1, DirectorCardCategorySelection arg2)
        {
            //AddQuestShrine(ref arg1, ref arg2); TODO FOR... uhm, some update, maybe 1.0? god i wish
            if (!logCards)
                return;
            foreach (var item in arg2.categories)
            {
                TELog.LogW("Director category: " + item.name);
                foreach (var card in item.cards)
                {
                    TELog.LogW("Spawn Card: " + card.spawnCard + " Prefab: " + card.spawnCard.prefab + "Is valid: " + card.IsAvailable());
                }
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

        private static string[] questShrineScenes = new string[]
        {
            "blackbeach",
            "foggyswamp",
            "wispgraveyard",
            "goldshores"
        };
    }
}
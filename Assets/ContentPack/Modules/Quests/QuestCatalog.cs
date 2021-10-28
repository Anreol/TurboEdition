using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using TurboEdition.Components;
using QuestCard = TurboEdition.ScriptableObjects.QuestCard;

namespace TurboEdition.Quests
{
    public static class QuestCatalog
    {
        public static int questCount
        {
            get
            {
                return QuestCatalog.allQuestCards.Length;
            }
        }

        public static int timedQuestCount
        {
            get
            {
                return QuestCatalog.allTimedQuestDefs.Length;
            }
        }

        public static List<QuestComponent> activeQuests
        {
            get
            {
                return InstanceTracker.GetInstancesList<QuestComponent>();
            }
        }

        private static readonly Dictionary<string, QuestIndex> questIndexByName = new Dictionary<string, QuestIndex>();
        private static readonly Dictionary<QuestIndex, QuestCard> questDefsByIndex = new Dictionary<QuestIndex, QuestCard>();
        private static QuestCard[] allQuestCards = Array.Empty<QuestCard>();
        private static QuestCard[] allTimedQuestDefs = Array.Empty<QuestCard>();

        //private static GameObject[] questGOs = Array.Empty<GameObject>();
        //private static List<GameObject> activeQuestList = new List<GameObject>();

        public static string[] questNames = Array.Empty<string>();
        public static QuestCard[] questsToLoad = Array.Empty<QuestCard>();

        public static QuestCard GetQuestDef(QuestIndex questDefIndex)
        {
            return QuestCatalog.allQuestCards[(int)questDefIndex];
        }

        public static QuestCard FindQuestDef(string questDefGlobalName)
        {
            return GetQuestDef(FindQuestIndex(questDefGlobalName));
        }

        public static QuestIndex FindQuestIndex(string questDefGlobalName)
        {
            QuestIndex intresult;
            QuestCatalog.questIndexByName.TryGetValue(questDefGlobalName, out intresult);
            return intresult;
        }

        [SystemInitializer(new Type[]
        {
            typeof(ItemCatalog),
            typeof(EquipmentCatalog),
            typeof(ArtifactCatalog),
            typeof(RuleCatalog)
        })]
        private static void Init()
        {
            QuestCard[] questHolders = Assets.mainAssetBundle.LoadAllAssets<QuestCard>();

            HG.ArrayUtils.CloneTo<QuestCard>(questHolders, ref questsToLoad);
            if (questsToLoad.Length <= 0)
            {
                TELog.LogW("The quest catalog initiated without any quests to load, is this OK?");
            }
            foreach (QuestCard item in questsToLoad)
            {
                TELog.LogW(item + " Name: " + item.nameToken);
            }
            SetQuestDefs(questsToLoad);
            //PopulateQuestGOs();
            QuestCatalog.availability.MakeAvailable();
        }

        /// <summary>
        /// Should be called only once. Append any desired QuestDefs to load to QuestCatalog.questsToLoad
        /// </summary>
        /// <param name="newQuestDefs"></param>
        private static void SetQuestDefs(QuestCard[] newQuestDefs)
        {
            QuestCatalog.questIndexByName.Clear();
            QuestCatalog.questDefsByIndex.Clear();
            QuestCatalog.questStackArrays.Clear();

            foreach (QuestCard item in newQuestDefs)
            {
                item.globalIndex = QuestIndex.None;
            }
            HG.ArrayUtils.CloneTo<QuestCard>(newQuestDefs, ref QuestCatalog.allQuestCards);
            Array.Resize<string>(ref QuestCatalog.questNames, newQuestDefs.Length);
            for (int j = 0; j < newQuestDefs.Length; j++)
            {
                QuestCatalog.questNames[j] = newQuestDefs[j].nameToken;
            }
            Array.Sort<string, QuestCard>(QuestCatalog.questNames, QuestCatalog.allQuestCards, StringComparer.Ordinal);
            for (QuestIndex k = 0; k < (QuestIndex)QuestCatalog.allQuestCards.Length; k++)
            {
                QuestCard chosenQuest = QuestCatalog.allQuestCards[(int)k];
                string globalName = QuestCatalog.questNames[(int)k];
                chosenQuest.globalIndex = k;
                QuestCatalog.allTimedQuestDefs = (from FUCK in QuestCatalog.allQuestCards
                                                  where FUCK.isTimed
                                                  select FUCK).ToArray<QuestCard>();
                QuestCatalog.questIndexByName.Add(globalName, k);
                QuestCatalog.questDefsByIndex.Add(k, chosenQuest);
            }
        }

        /// <summary>
        /// Meant to be used after the initial load has been done. Also refreshes the questGOs.
        /// </summary>
        /// <param name="questDef"></param>
        public static void AddQuest(QuestCard questDef)
        {
            QuestIndex lastIndex = (QuestIndex)QuestCatalog.allQuestCards.Length;
            questDef.globalIndex = lastIndex;

            HG.ArrayUtils.ArrayAppend(ref allQuestCards, questDef);
            if (questDef.isTimed)
            {
                HG.ArrayUtils.ArrayAppend(ref QuestCatalog.allTimedQuestDefs, questDef);
            }
            QuestCatalog.questIndexByName.Add(questDef.nameToken, questDef.globalIndex);
            QuestCatalog.questDefsByIndex.Add(questDef.globalIndex, questDef);
            //PopulateQuestGOs();
        }

        public static void ReturnQuesttackArray(int[] questStackArray) //I really dont know what any of these two do
        {
            if (questStackArray.Length != QuestCatalog.questCount)
            {
                return;
            }
            Array.Clear(questStackArray, 0, questStackArray.Length);
            QuestCatalog.questStackArrays.Push(questStackArray);
        }

        public static int[] RequestQuestStackArray()
        {
            if (QuestCatalog.questStackArrays.Count > 0)
            {
                return QuestCatalog.questStackArrays.Pop();
            }
            return new int[QuestCatalog.questCount];
        }

        /// <summary>
        /// Populates questGOs with all distinct questPrefabs from the loaded allQuestDefs
        /// </summary>
        //public static void PopulateQuestGOs()
        //{
        //  if (allQuestDefs.Length == 0)
        //  {
        //		TELog.LogE("Tried to populate questGOs with an empty quest catalog.");
        //		return;
        //	}
        //	questGOs = (from lmfao in QuestCatalog.allQuestDefs
        //				where lmfao.questPrefab
        //				select lmfao.questPrefab).Distinct().ToArray<GameObject>();
        //}

        [ConCommand(commandName = "list_quests", flags = ConVarFlags.None, helpText = "Lists all loaded questsDefs.")]
        private static void ListQuests(ConCommandArgs args)
        {
            for (int i = 0; i < questCount; i++)
                TELog.LogD($"[{i}]\t{questNames[i]}");
        }

        [ConCommand(commandName = "list_quests_activego", flags = ConVarFlags.ExecuteOnServer, helpText = "Lists all active quest game objects.")]
        private static void ListQuestsActiveGo(ConCommandArgs args)
        {
            for (int i = 0; i < activeQuests.Count; i++)
                TELog.LogD($"[{i}]\t{activeQuests[i].titleToken}");
        }

        private static readonly Stack<int[]> questStackArrays = new Stack<int[]>();
        public static ResourceAvailability availability = default(ResourceAvailability);

        public enum QuestIndex
        {
            None = -1
        }

        public enum QuestTag
        {
            Any,
            TeamWide,
            NoScalePrice,
            NoScaleReward,
            WontInverse,
            ItemRelated,
            EnemyRelated,
            Special
        }
    }
}
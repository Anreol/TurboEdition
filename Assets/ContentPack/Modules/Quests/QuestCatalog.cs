using RoR2;
using RoR2.ConVar;
using System;
using System.Collections.Generic;
using System.Linq;
using TurboEdition.Components;
using QuestDef = TurboEdition.ScriptableObjects.QuestDef;

namespace TurboEdition.Quests
{
    public static class QuestCatalog
    {
        public static event Action<QuestDef[]> onPreLoadQuestCards;

        public static int questCount
        {
            get
            {
                return QuestCatalog.allQuestDefs.Length;
            }
        }

        public static int negativeQuestCount
        {
            get
            {
                return QuestCatalog.allQuestsNegative.Length;
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
        private static readonly Dictionary<QuestIndex, QuestDef> questDefsByIndex = new Dictionary<QuestIndex, QuestDef>();
        private static QuestDef[] allQuestDefs = Array.Empty<QuestDef>();
        private static QuestDef[] allQuestsNegative = Array.Empty<QuestDef>();

        //private static GameObject[] questGOs = Array.Empty<GameObject>();
        //private static List<GameObject> activeQuestList = new List<GameObject>();

        public static string[] questNames = Array.Empty<string>();
        public static QuestDef[] questsToLoad = Array.Empty<QuestDef>();

        public static QuestDef GetQuestDef(QuestIndex questDefIndex)
        {
            return QuestCatalog.allQuestDefs[(int)questDefIndex];
        }

        public static QuestDef FindQuestDef(string questDefGlobalName)
        {
            return GetQuestDef(FindQuestIndex(questDefGlobalName));
        }

        public static QuestIndex FindQuestIndex(string questDefGlobalName)
        {
            QuestIndex intresult;
            QuestCatalog.questIndexByName.TryGetValue(questDefGlobalName, out intresult);
            return intresult;
        }

        /// <summary>
        /// Selects a random, non-negative quest. Can use the current run's RNG
        /// </summary>
        /// <param name="useRunRNG"></param>
        /// <returns></returns>
        public static QuestDef SelectSafeQuest(bool useRunRNG = false)
        {
            QuestDef selection;
            do
            {
                if (useRunRNG)
                    selection = QuestCatalog.allQuestDefs[Run.instance.runRNG.nextInt];
                else
                    selection = QuestCatalog.allQuestDefs[UnityEngine.Random.Range(0, questCount)];
            } while (selection.isNegative || selection == null);
            return selection;
        }

        public static int GetQuestCount(CharacterBody cb)
        {
            int count = 0;
            foreach (QuestComponent item in QuestCatalog.activeQuests)
            {
                if (!QuestCatalog.GetQuestDef(item.questIndexSpawner).isNegative)
                {
                    if (item.teamIndex == cb.teamComponent.teamIndex)
                        count++;
                    else if (item.masterNetIdOrigin == cb.master.netId)
                        count++;
                }
            }
            return count;
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
            QuestDef[] questHolders = Assets.mainAssetBundle.LoadAllAssets<QuestDef>();
            HG.ArrayUtils.CloneTo<QuestDef>(questHolders, ref questsToLoad);

            Action<QuestDef[]> action = QuestCatalog.onPreLoadQuestCards;
            if (action != null)
            {
                action(questsToLoad);
            }

            if (questsToLoad.Length <= 0 && cvTurboEditionQuestLogs.value)
            {
                TELog.LogW("The quest catalog initiated without any quests to load, is this OK?");
            }
            if (cvTurboEditionQuestLogs.value)
            {
                foreach (QuestDef item in questsToLoad)
                {
                    TELog.LogW(item + " Name: " + item.questName);
                }
            }
            SetQuestDefs(questsToLoad);
            //PopulateQuestGOs();
            QuestCatalog.availability.MakeAvailable();
        }

        /// <summary>
        /// Should be called only once. Append any desired QuestDefs to load to QuestCatalog.questsToLoad
        /// </summary>
        /// <param name="newQuestDefs"></param>
        private static void SetQuestDefs(QuestDef[] newQuestDefs)
        {
            QuestCatalog.questIndexByName.Clear();
            QuestCatalog.questDefsByIndex.Clear();
            QuestCatalog.questStackArrays.Clear();

            foreach (QuestDef item in newQuestDefs)
            {
                item.globalIndex = QuestIndex.None;
            }
            HG.ArrayUtils.CloneTo<QuestDef>(newQuestDefs, ref QuestCatalog.allQuestDefs);
            Array.Resize<string>(ref QuestCatalog.questNames, newQuestDefs.Length);
            for (int j = 0; j < newQuestDefs.Length; j++)
            {
                QuestCatalog.questNames[j] = newQuestDefs[j].questName;
            }
            Array.Sort<string, QuestDef>(QuestCatalog.questNames, QuestCatalog.allQuestDefs, StringComparer.Ordinal);
            for (QuestIndex k = 0; k < (QuestIndex)QuestCatalog.allQuestDefs.Length; k++)
            {
                QuestDef chosenQuest = QuestCatalog.allQuestDefs[(int)k];
                string globalName = QuestCatalog.questNames[(int)k];
                chosenQuest.globalIndex = k;
                QuestCatalog.allQuestsNegative = (from FUCK in QuestCatalog.allQuestDefs
                                                  where FUCK.isNegative
                                                  select FUCK).ToArray<QuestDef>();
                QuestCatalog.questIndexByName.Add(globalName, k);
                QuestCatalog.questDefsByIndex.Add(k, chosenQuest);
            }
        }

        /// <summary>
        /// Meant to be used after the initial load has been done. Also refreshes the questGOs.
        /// </summary>
        /// <param name="questDef"></param>
        public static void AddQuest(QuestDef questDef)
        {
            QuestIndex lastIndex = (QuestIndex)QuestCatalog.allQuestDefs.Length;
            questDef.globalIndex = lastIndex;

            HG.ArrayUtils.ArrayAppend(ref allQuestDefs, questDef);
            if (questDef.isNegative)
                HG.ArrayUtils.ArrayAppend(ref QuestCatalog.allQuestsNegative, questDef);
            QuestCatalog.questIndexByName.Add(questDef.questName, questDef.globalIndex);
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

        public static readonly BoolConVar cvTurboEditionQuestLogs = new BoolConVar("te_enable_quest_logs", ConVarFlags.None, "0", "Enables all quest components to print internal logging.");

        [ConCommand(commandName = "te_list_quests", flags = ConVarFlags.ExecuteOnServer, helpText = "Lists all loaded questsDefs.")]
        private static void ListQuests(ConCommandArgs args)
        {
            for (int i = 0; i < questCount; i++)
                TELog.LogD($"[{i}]\t{questNames[i]}");
        }

        [ConCommand(commandName = "te_list_quests_activego", flags = ConVarFlags.ExecuteOnServer, helpText = "Lists all active quest game objects.")]
        private static void ListQuestsActiveGo(ConCommandArgs args)
        {
            /*for (int i = 0; i < activeQuests.Count; i++)
                TELog.LogD($"[{i}]\t{activeQuests[i].titleToken}");*/
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
            Negative,
            TeamWide,
            NoScalePrice,
            NoScaleReward,
            ItemRelated,
            EnemyRelated,
            Special
        }
    }
}
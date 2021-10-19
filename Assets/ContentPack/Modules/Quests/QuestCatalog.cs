using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestDef = TurboEdition.ScriptableObjects.QuestDef;

namespace TurboEdition.Quests
{
    public static class QuestCatalog
    {
		public static int questCount
		{
			get
			{
				return QuestCatalog.allQuestDefs.Length;
			}
		}
		public static int timedQuestCount
		{
			get
			{
				return QuestCatalog.allTimedQuestDefs.Length;
			}
		}
		private static readonly Dictionary<string, QuestIndex> questIndexByName = new Dictionary<string, QuestIndex>();
		private static readonly Dictionary<QuestIndex, QuestDef> questDefsByIndex = new Dictionary<QuestIndex, QuestDef>();
		private static QuestDef[] allQuestDefs = Array.Empty<QuestDef>();
		private static QuestDef[] allTimedQuestDefs = Array.Empty<QuestDef>();
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
			SetQuestDefs(questsToLoad);
		}

		//Can only get called once!!!
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
				QuestCatalog.questNames[j] = newQuestDefs[j].globalName;
			}
			Array.Sort<string, QuestDef>(QuestCatalog.questNames, QuestCatalog.allQuestDefs, StringComparer.Ordinal);
			for (QuestIndex k = 0; k < (QuestIndex)QuestCatalog.allQuestDefs.Length; k++)
			{
				QuestDef chosenQuest = QuestCatalog.allQuestDefs[(int)k];
				string globalName = QuestCatalog.questNames[(int)k];
				chosenQuest.globalIndex = k;
				QuestCatalog.allTimedQuestDefs = (from FUCK in QuestCatalog.allQuestDefs
														 where FUCK.isTimed
														 select FUCK).ToArray<QuestDef>();
				QuestCatalog.questIndexByName.Add(globalName, k);
				QuestCatalog.questDefsByIndex.Add(k, chosenQuest);
			}

		}
		/// <summary>
		/// Meant to be used after the initial load has been done.
		/// </summary>
		/// <param name="questDef"></param>
		public static void AddQuest(QuestDef questDef)
        {
			QuestIndex lastIndex = (QuestIndex)QuestCatalog.allQuestDefs.Length;
			questDef.globalIndex = lastIndex;

			HG.ArrayUtils.ArrayAppend(ref allQuestDefs, questDef);
            if (questDef.isTimed)
            {
				HG.ArrayUtils.ArrayAppend(ref QuestCatalog.allTimedQuestDefs, questDef);
			}
			QuestCatalog.questIndexByName.Add(questDef.globalName, questDef.globalIndex);
			QuestCatalog.questDefsByIndex.Add(questDef.globalIndex, questDef);
		}

		public static void ReturnQuesttackArray(int[] questStackArray)
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

		private static readonly Stack<int[]> questStackArrays = new Stack<int[]>();
		public enum QuestIndex
		{
			None = -1
		}
		public enum QuestTag
        {
			Any,
			TeamWide,
			Money,
			Item,
			Special
        }
	}
}

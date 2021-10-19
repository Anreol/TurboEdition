using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TurboEdition.Quests;

namespace TurboEdition.ScriptableObjects
{
    [CreateAssetMenu(menuName = "TurboEdition/QuestDef")]
    public class QuestDef : ScriptableObject
    {
		public readonly string globalName;
		public QuestCatalog.QuestIndex globalIndex { get; set; } = 0;
		public bool isTimed
		{
			get
			{
				return this.maxTime > 0f;
			}
		}
		public Texture questIconTexture
		{
			get
			{
				if (!this.questIconSprite)
				{
					return null;
				}
				return this.questIconSprite.texture;
			}
		}
		[ContextMenu("Auto Populate Tokens")]
		public void AutoPopulateTokens()
		{
			string arg = base.name.ToUpperInvariant();
			this.nameToken = string.Format("QUEST_{0}_NAME", arg);
			this.objectiveToken = string.Format("QUEST_{0}_OBJECTIVE", arg);
			this.loseToken = string.Format("ITEM_{0}_LOSE", arg);
		}
		public bool ContainsTag(QuestCatalog.QuestTag tag)
		{
			return tag == QuestCatalog.QuestTag.Any || Array.IndexOf<QuestCatalog.QuestTag>(this.tags, tag) != -1;
		}
		public bool DoesNotContainTag(QuestCatalog.QuestTag tag)
		{
			return Array.IndexOf<QuestCatalog.QuestTag>(this.tags, tag) == -1;
		}
		public GameObject questPrefab;
		public string nameToken;
		public string objectiveToken;
		public string loseToken;
		public Sprite questIconSprite;
		public QuestCatalog.QuestTag[] tags = Array.Empty<QuestCatalog.QuestTag>();
		public bool hidden;
		public float maxTime;
	}
}

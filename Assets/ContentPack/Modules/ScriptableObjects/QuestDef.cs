﻿using EntityStates;
using System;
using TurboEdition.Quests;
using UnityEngine;

namespace TurboEdition.ScriptableObjects
{
    [CreateAssetMenu(menuName = "TurboEdition/QuestDef")]
    public class QuestDef : ScriptableObject
    {
        [Obsolete("Accessing UnityEngine.Object.Name causes allocations on read. Look up the name from the catalog instead. If absolutely necessary to perform direct access, cast to ScriptableObject first.")]
        public new string name
        {
            get
            {
                return null;
            }
        }

        public bool isNegative
        {
            get
            {
                return ContainsTag(QuestCatalog.QuestTag.Negative);
            }
        }

        public bool isTeamWide
        {
            get
            {
                return ContainsTag(QuestCatalog.QuestTag.TeamWide);
            }
        }

        public QuestCatalog.QuestIndex questIndex { get; set; } = 0;

        [ContextMenu("Auto Populate Tokens")]
        public void AutoPopulateTokens()
        {
            string arg = base.name.ToUpperInvariant();
            this.questName = string.Format("{0}", arg);
            this.questNameToken = string.Format("QUEST_{0}_NAME", arg);
            //this.questDescriptionToken = string.Format("QUEST_{0}_OBJECTIVE", arg);
        }

        public bool ContainsTag(QuestCatalog.QuestTag tag)
        {
            return tag == QuestCatalog.QuestTag.Any || Array.IndexOf<QuestCatalog.QuestTag>(this.tags, tag) != -1;
        }

        public bool DoesNotContainTag(QuestCatalog.QuestTag tag)
        {
            return Array.IndexOf<QuestCatalog.QuestTag>(this.tags, tag) == -1;
        }

        [Tooltip("The name of the quest. Only used internally, and has to be unique.")]
        [Header("Quest Identifier")]
        public string questName;

        [Tooltip("Token with the name of this quest, currently unused.")]
        [Header("User-Facing Info")]
        public string questNameToken;

        [Tooltip("Selection weight for this quest line.")]
        [Header("Chance to appear.")]
        public int selectionWeight;

        //[Tooltip("Token with the description of this quest.")]
        //public string questDescriptionToken;

        [Tooltip("Tags related to this quest.")]
        public QuestCatalog.QuestTag[] tags = Array.Empty<QuestCatalog.QuestTag>();

        [Tooltip("The entitystate steps to use when this quest is activated.")]
        public SerializableEntityStateType[][] activationStates;

        [Tooltip("MainEntityState to be used for the machine.")]
        public SerializableEntityStateType mainEntityState;

        [Tooltip("Entity State to use whenever it expires.")]
        public SerializableEntityStateType expirationState;

        [HideInInspector]
        public QuestCatalog.QuestIndex globalIndex;
    }
}
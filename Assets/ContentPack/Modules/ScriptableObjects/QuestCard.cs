﻿using RoR2;
using System;
using TurboEdition.Components;
using TurboEdition.Quests;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.ScriptableObjects
{
    [CreateAssetMenu(menuName = "TurboEdition/QuestDef")]
    public class QuestCard : ScriptableObject
    {
        public QuestCatalog.QuestIndex globalIndex { get; set; } = 0;

        public bool isNegative
        {
            get
            {
                return ContainsTag(QuestCatalog.QuestTag.Negative);
            }
        }
        public bool hasExpiration
        {
            get
            {
                return this.stageDuration > 0f;
            }
        }
        public bool isTeamWide
        {
            get
            {
                return ContainsTag(QuestCatalog.QuestTag.TeamWide);
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
        }

        public virtual void Spawn(Interactor interactor)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.questPrefab);
            QuestComponent trollNent = gameObject.GetComponent<QuestComponent>();
            trollNent.questIndexSpawner = this.globalIndex;
            trollNent.masterNetIdOrigin = interactor.GetComponent<CharacterBody>().master.netId;
            if (hasExpiration)
            {
                trollNent.stageNumExpiration = Run.instance.stageClearCount + stageDuration;
            }
            if (this.isTeamWide)
            {
                trollNent.teamIndex = interactor.GetComponent<CharacterBody>().teamComponent.teamIndex;
                NetworkServer.Spawn(gameObject);
                return;
            }
        }

        public bool ContainsTag(QuestCatalog.QuestTag tag)
        {
            return tag == QuestCatalog.QuestTag.Any || Array.IndexOf<QuestCatalog.QuestTag>(this.tags, tag) != -1;
        }

        public bool DoesNotContainTag(QuestCatalog.QuestTag tag)
        {
            return Array.IndexOf<QuestCatalog.QuestTag>(this.tags, tag) == -1;
        }

        [Tooltip("Quest gameobject prefab that will be spawned.")]
        public GameObject questPrefab;

        [Tooltip("Icon sprite that will show up in the UI.")]
        public Sprite questIconSprite;

        [Tooltip("Token for the name that will show up in the UI.")]
        public string nameToken;

        [Tooltip("Token for the objective that will show up in the UI.")]
        public string objectiveToken;

        [Tooltip("Base of amount in money to give. For reference, small chests are 25, medium 50, and gold 400. Tagged chests are 30.")]
        public int baseRewardCount;

        [Tooltip("For how many stages this quest should persist. -1 to make it permanent. i.e 1 will make it expire upon finishing the next stage.")]
        public int stageDuration;

        [Tooltip("How common should be this quest.")]
        public int selectionWeight;

        [Tooltip("Whenever it should not appear in the UI.")]
        public bool hidden;

        public QuestCatalog.QuestTag[] tags = Array.Empty<QuestCatalog.QuestTag>();
        public RoR2.CostTypeIndex costType;
    }
}
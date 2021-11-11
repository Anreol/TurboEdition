using RoR2;
using System;
using TurboEdition.Components;
using TurboEdition.Quests;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.ScriptableObjects
{
    [CreateAssetMenu(menuName = "TurboEdition/QuestCard")]
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
                return this.stageDuration >= 0 && questPrefab.GetComponent<SetDontDestroyOnLoad>();
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
            }
            NetworkServer.Spawn(gameObject);
            return;
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

        [Tooltip("Token for the name that will show up in the UI, and that will be registered in the catalog. Has to be unique.")]
        public string nameToken;

        [Tooltip("Token for the objective that will show up in the UI.")]
        public string objectiveToken;

        [Tooltip("Base of amount in money to give. For reference, small chests are 25, medium 50, and gold 400. Tagged chests are 30.")]
        public int baseRewardCount;

        [Tooltip("Number to multiply the base reward by each stage. Ie if 2, by stage 2, it will be multiplied by 4. This calculation is done before it's summed to the base reward scaled by difficulty. Zero to disable.\n(baseRewardCount * (rewardMultiplerPerStage * (stageClearCount)) + baseRewardCount")]
        public double rewardMultiplerPerStage;

        [Tooltip("For how many stages this quest should persist. -1 to make it permanent. i.e 1 will make it expire upon finishing the next stage.")]
        public int stageDuration;

        [Tooltip("How common should be this quest.")]
        public int selectionWeight;

        [Tooltip("How common should raw money rewards be for this quest.")]
        public int moneyWeight;

        [Tooltip("How common should item rewards be for this quest. Tier weights are calculated in the quest prefab, not here!")]
        public int itemWeight;

        [Tooltip("You shove everything else that your specific quest needs to function here.")]
        public string extraData;
        //public object[] extraData = Array.Empty<object>();

        [Tooltip("Whenever it should not appear in the UI.")]
        public bool hidden;

        public QuestCatalog.QuestTag[] tags = Array.Empty<QuestCatalog.QuestTag>();
        public RoR2.CostTypeIndex costType;
    }
}
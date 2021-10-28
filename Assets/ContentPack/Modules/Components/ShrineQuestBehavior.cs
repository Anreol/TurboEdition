using JetBrains.Annotations;
using RoR2;
using System;
using TurboEdition.Quests;
using UnityEngine;
using UnityEngine.Networking;
using QuestCard = TurboEdition.ScriptableObjects.QuestCard;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(PurchaseInteraction))]
    internal class ShrineQuestBehavior : NetworkBehaviour, IInteractable
    {
        public int purchaseCount { get; private set; }

        public string GetContextString([NotNull] Interactor activator)
        {
            return Language.GetString(this.contextToken);
        }

        public static int GetQuestCount(CharacterBody cb)
        {
            int count = 0;
            foreach (QuestComponent item in QuestCatalog.activeQuests)
            {
                if (item.teamIndex == cb.teamComponent.teamIndex)
                    count++;
                else if (item.masterNetIdOrigin == cb.master.netId)
                    count++;
            }
            return count;
        }

        private void FairQuestCosts()
        {
            switch (this.purchaseInteraction.costType)
            {
                case CostTypeIndex.None:
                    this.purchaseInteraction.Networkcost = 0;
                    break;

                case CostTypeIndex.Money:
                    break;

                case CostTypeIndex.PercentHealth:
                    this.purchaseInteraction.Networkcost = (int)(100f * (1f - Mathf.Pow(1f - (float)this.purchaseInteraction.cost / 100f, this.costMultiplierPerPurchase)));
                    break;

                case CostTypeIndex.LunarCoin:
                    this.purchaseInteraction.Networkcost = this.purchaseCount + 1;
                    break;

                case CostTypeIndex.WhiteItem:
                    this.purchaseInteraction.Networkcost = this.purchaseCount + 2;
                    break;

                case CostTypeIndex.GreenItem:
                    this.purchaseInteraction.Networkcost = this.purchaseCount + 1;
                    break;

                case CostTypeIndex.RedItem:
                    this.purchaseInteraction.Networkcost = this.purchaseCount + 1;
                    break;

                case CostTypeIndex.Equipment:
                    this.purchaseInteraction.Networkcost = 1;
                    break;

                case CostTypeIndex.VolatileBattery:
                    this.purchaseInteraction.Networkcost = 1;
                    break;

                case CostTypeIndex.LunarItemOrEquipment:
                    this.purchaseInteraction.Networkcost = this.purchaseCount + 1;
                    break;

                case CostTypeIndex.BossItem:
                    this.purchaseInteraction.Networkcost = this.purchaseCount + 1;
                    break;

                case CostTypeIndex.ArtifactShellKillerItem:
                    this.purchaseInteraction.Networkcost = 1; //what the fuck
                    break;

                case CostTypeIndex.TreasureCacheItem:
                    this.purchaseInteraction.Networkcost = 1;
                    break;

                case CostTypeIndex.Count:
                    break;

                default:
                    break;
            }
        }

        [Server]
        public void AddShrineStack(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                TELog.LogW("[Server] function 'System.Void TurboEdition.ShrineQuestBehavior::AddShrineStack(RoR2.Interactor)' called on client");
                return;
            }
            CharacterBody component = interactor.GetComponent<CharacterBody>();
            chosenQuests[purchaseCount].Spawn(interactor);
            Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
            {
                subjectAsCharacterBody = component,
                baseToken = "SHRINE_QUEST_USE_MESSAGE",
                paramTokens = new string[]
                {
                            chosenQuests[purchaseCount].objectiveToken
                }
            });
            EffectManager.SpawnEffect(activationEffectPrefab, new EffectData
            {
                origin = base.transform.position,
                rotation = Quaternion.identity,
                scale = 1f,
                color = Color.cyan
            }, true);
            this.purchaseCount++;
            this.refreshTimer = refreshDuration;
            Action<ShrineQuestBehavior, Interactor> action = ShrineQuestBehavior.onActivated;
            if (action == null)
            {
                return;
            }
            action(this, interactor);
            //if (this.purchaseCount >= this.maxPurchaseCount)
            //{
            //    this.symbolTransform.gameObject.SetActive(false);
            //}
        }

        public Interactability GetInteractability([NotNull] Interactor activator)
        {
            if (GetQuestCount(activator.GetComponent<CharacterBody>()) >= 3)
            {
                return Interactability.ConditionsNotMet;
            }
            return Interactability.Available;
        }

        public void OnInteractionBegin([NotNull] Interactor activator)
        {
            if (chosenQuests.Length <= 0)
            {
                TELog.LogW("Theres no quests loaded in " + this);
                return;
            }
        }

        public bool ShouldIgnoreSpherecastForInteractibility([NotNull] Interactor activator)
        {
            return false;
        }

        public bool ShouldShowOnScanner()
        {
            return true;
        }

        private void Awake()
        {
            this.purchaseInteraction = base.GetComponent<PurchaseInteraction>();
            for (int i = 0; i < maxPurchaseCount; i++)
            {
                QuestCatalog.QuestIndex questIndex = (QuestCatalog.QuestIndex)UnityEngine.Random.Range(0, QuestCatalog.questCount);
                HG.ArrayUtils.ArrayAppend(ref chosenQuests, QuestCatalog.GetQuestDef(questIndex));
            }
        }

        public void FixedUpdate()
        {
            if (this.waitingForRefresh)
            {
                this.refreshTimer -= Time.fixedDeltaTime;
                if (this.refreshTimer <= 0f && this.purchaseCount < this.maxPurchaseCount)
                {
                    QuestCard questCard = chosenQuests[purchaseCount];
                    this.purchaseInteraction.costType = questCard.costType;
                    this.purchaseInteraction.automaticallyScaleCostWithDifficulty = true;
                    if (questCard.ContainsTag(QuestCatalog.QuestTag.NoScalePrice))
                        this.purchaseInteraction.automaticallyScaleCostWithDifficulty = false;
                    this.purchaseInteraction.SetAvailable(true);
                    this.purchaseInteraction.Networkcost = (int)((float)this.purchaseInteraction.cost * this.costMultiplierPerPurchase);
                    FairQuestCosts();
                    this.waitingForRefresh = false;
                }
            }
        }

        public static event Action<ShrineQuestBehavior, Interactor> onActivated;

        private QuestCard[] chosenQuests = Array.Empty<QuestCard>();

        public int maxPurchaseCount;
        public float costMultiplierPerPurchase;
        public string contextToken;
        public GameObject activationEffectPrefab;

        //public Transform symbolTransform;
        public const float refreshDuration = 2f;

        private float refreshTimer;
        private bool waitingForRefresh;
        private PurchaseInteraction purchaseInteraction;
    }
}
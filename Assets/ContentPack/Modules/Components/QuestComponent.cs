using RoR2;
using System;
using TurboEdition.Quests;
using UnityEngine;
using UnityEngine.Networking;
using QuestCard = TurboEdition.ScriptableObjects.QuestCard;

namespace TurboEdition.Components
{
    public class QuestComponent : NetworkBehaviour
    {
        public static event Action onInstanceEnableGlobal;

        public static event Action onInstanceChangedGlobal;

        public int numCurrentCount
        {
            get
            {
                return this._numCurrentCount;
            }
        }

        public int numRequiredCount
        {
            get
            {
                return this._numRequiredCount;
            }
        }

        public QuestCatalog.QuestIndex questIndexSpawner
        {
            get
            {
                return (QuestCatalog.QuestIndex)this._questIndexSpawner;
            }
            set
            {
                this._questIndexSpawner = (int)value;
            }
        }

        public int stageNumExpiration
        {
            get
            {
                return this._stageNumExpiration;
            }
            set
            {
                this._stageNumExpiration = (int)value;
            }
        }

        public int numTilExpiration
        {
            get
            {
                if (this._stageNumExpiration >= 0)
                {
                    return this._stageNumExpiration - Run.instance.stageClearCount;
                }
                return -1;
            }
        }

        public NetworkInstanceId masterNetIdOrigin
        {
            get
            {
                return this._masterNetIdOrigin;
            }
            set
            {
                this._masterNetIdOrigin = value;
            }
        }

        public int rewardAmount
        {
            get
            {
                return _rewardCredits;
            }
        }

        public virtual void OnEnable()
        {
            InstanceTracker.Add(this);
            Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            if (gameObject.GetComponent<SetDontDestroyOnLoad>() == null)
            {
                TELog.LogI("Quest Prefab did not have SetDontDestroyOnLoad component, quest unable to stay past current scene.");
                _stageNumExpiration = 0; //Unable stay between stages, and will expire after the current one
            }
            Action enabled = QuestComponent.onInstanceEnableGlobal;
            if (enabled != null)
            {
                enabled();
            }
            Action action = QuestComponent.onInstanceChangedGlobal;
            if (action != null)
            {
                action();
            }
        }

        public virtual void OnDisable()
        {
            InstanceTracker.Remove(this);
            Action action = QuestComponent.onInstanceChangedGlobal;
            if (action != null)
            {
                action();
            }
        }

        public virtual void GenerateObjective()
        {
            throw new NotImplementedException();
        }

        public virtual void GenerateReward()
        {
            /*QuestCard questCard = QuestCatalog.GetQuestDef(questIndexSpawner); TODO
            this._rewardCredits += (int)(questCard.baseRewardCount * (questCard.rewardMultiplerPerStage * (Run.instance.stageClearCount)));
            this._rewardCredits += (questCard.ContainsTag(QuestCatalog.QuestTag.NoScaleReward) ? questCard.baseRewardCount : Run.instance.GetDifficultyScaledCost(questCard.baseRewardCount));
            TELog.LogW(_rewardCredits);
            TELog.LogW(Run.instance.GetDifficultyScaledCost(questCard.baseRewardCount));
            TELog.LogW("doesnt scale: " + questCard.ContainsTag(QuestCatalog.QuestTag.NoScaleReward));
            //I LOVE WEIGHTED SELECTIONS
            WeightedSelection<CostTypeIndex> weightedSelection = new WeightedSelection<CostTypeIndex>(2);
            weightedSelection.AddChoice(CostTypeIndex.Money, questCard.moneyWeight);
            weightedSelection.AddChoice(CostTypeIndex.WhiteItem, questCard.itemWeight); //Items in general.
            if (weightedSelection.Evaluate(this.rng.nextNormalizedFloat) == CostTypeIndex.WhiteItem)
            {
                if (rewardInventory == null)
                    rewardInventory = base.gameObject.AddComponent<Inventory>();
                while (this._rewardCredits > 0) //I hate this very much
                {
                    ItemDef chosenItemDef = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(RollItem()).itemIndex);
                    switch (chosenItemDef.tier)
                    {
                        case ItemTier.Tier1:
                            _rewardCredits -= Run.instance.GetDifficultyScaledCost(25);
                            break;

                        case ItemTier.Tier2:
                            _rewardCredits -= Run.instance.GetDifficultyScaledCost(50);
                            break;

                        case ItemTier.Tier3:
                            _rewardCredits -= Run.instance.GetDifficultyScaledCost(200);
                            break;

                        case ItemTier.Lunar:
                            _rewardCredits -= Run.instance.GetDifficultyScaledCost(175);
                            break;

                        case ItemTier.Boss:
                            _rewardCredits -= Run.instance.GetDifficultyScaledCost(175);
                            break;

                        case ItemTier.NoTier: //This is not supposed to happen
                            break;

                        default:
                            break;
                    }
                    rewardInventory.GiveItem(chosenItemDef);
                }
                return;
            }*/
        }

        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();
            this.rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
            GenerateObjective();
            GenerateReward();
        }

        [Server]
        public CharacterBody RollEnemy()
        {
            DirectorCard directorCard;
            do
            {
                CombatDirector combatDirector = this.rng.NextElementUniform<CombatDirector>(CombatDirector.instancesList);
                directorCard = combatDirector.lastAttemptedMonsterCard;
            } while (directorCard == null);

            return directorCard.spawnCard.prefab.GetComponent<CharacterBody>();
        }

        [Server]
        public EliteDef RollElite()
        {
            EliteDef eliteDef;
            CombatDirector combatDirector;
            do
            {
                combatDirector = this.rng.NextElementUniform<CombatDirector>(CombatDirector.instancesList);
                eliteDef = combatDirector.currentActiveEliteDef;
            } while (combatDirector == null);  //Maybe the elite type that it returns is null, so be it.

            return eliteDef;
        }

        [Server]
        public PickupIndex RollItem()
        {
            PickupIndex value = Run.instance.treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableTier1DropList);
            PickupIndex value2 = Run.instance.treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableTier2DropList);
            PickupIndex value3 = Run.instance.treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableTier3DropList);
            PickupIndex value4 = Run.instance.treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableEquipmentDropList);
            PickupIndex value5 = Run.instance.treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableLunarDropList);
            PickupIndex value6 = Run.instance.treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableBossDropList);
            WeightedSelection<PickupIndex> weightedSelection = new WeightedSelection<PickupIndex>(8);
            weightedSelection.AddChoice(value, this.tier1Weight);
            weightedSelection.AddChoice(value2, this.tier2Weight);
            weightedSelection.AddChoice(value3, this.tier3Weight);
            weightedSelection.AddChoice(value4, this.equipmentWeight);
            weightedSelection.AddChoice(value5, this.tierLunarWeight);
            weightedSelection.AddChoice(value6, this.tierBossWeight);
            return weightedSelection.Evaluate(Run.instance.treasureRng.nextNormalizedFloat); //uses treasure rng instead of this's rng to avoid fucking up
        }

        private void Stage_onStageStartGlobal(Stage obj)
        {
            /* This actually does jack fucking shit
            if (_stageNumExpiration >= 0 && gameObject.GetComponent<SetDontDestroyOnLoad>() != null)
            {
                if (Run.instance.stageClearCount >= _stageNumExpiration)
                {
                    UnityEngine.Object.Destroy(gameObject.GetComponent<SetDontDestroyOnLoad>());
                }
            }*/
            if (Run.instance.stageClearCount > _stageNumExpiration)
            {
                NetworkServer.Destroy(this.gameObject); //Doing it in both?
                Destroy(this.gameObject);
            }
        }

        [SyncVar]
        private int _numCurrentCount;

        [SyncVar]
        private int _numRequiredCount;

        [SyncVar]
        private int _questIndexSpawner;

        [SyncVar]
        private int _rewardCredits;

        private int _stageNumExpiration = -1;

        [SyncVar]
        private NetworkInstanceId _masterNetIdOrigin;

        public float tier1Weight = 0.8f; //Weights taken from the chance shrine.
        public float tier2Weight = 0.2f;
        public float tier3Weight = 0.01f;
        public float tierLunarWeight = 0f; //Extra weights, taken from nowhere.
        public float tierBossWeight = 0f;
        public float equipmentWeight = 0f;

        public string extraData;

        //public object[] extraData = null;
        public Xoroshiro128Plus rng;

        [HideInInspector]
        public TeamIndex teamIndex = TeamIndex.None;

        [HideInInspector]
        public Inventory rewardInventory;
    }
}
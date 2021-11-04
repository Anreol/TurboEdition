using RoR2;
using System;
using TurboEdition.Quests;
using UnityEngine.Networking;
using QuestCard = TurboEdition.ScriptableObjects.QuestCard;

namespace TurboEdition.Components
{
    public class QuestComponent : NetworkBehaviour
    {
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

        public string titleToken
        {
            get
            {
                return QuestCatalog.GetQuestDef(questIndexSpawner).nameToken;
            }
        }

        public string objectiveToken
        {
            get
            {
                return QuestCatalog.GetQuestDef(questIndexSpawner).objectiveToken;
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
            QuestCard questCard = QuestCatalog.GetQuestDef(questIndexSpawner);
            this.rewardAmount = (questCard.ContainsTag(QuestCatalog.QuestTag.NoScaleReward) ? questCard.baseRewardCount : Run.instance.GetDifficultyScaledCost(questCard.baseRewardCount));
            
        }

        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();
            GenerateObjective();
            GenerateReward();
            this.rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
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
            return weightedSelection.Evaluate(Run.instance.treasureRng.nextNormalizedFloat);
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
                Destroy(this.gameObject);
            }
        }

        [SyncVar]
        private int _numCurrentCount;

        [SyncVar]
        private int _numRequiredCount;

        [SyncVar]
        private int _questIndexSpawner;

        private int _stageNumExpiration = -1;

        [SyncVar]
        private NetworkInstanceId _masterNetIdOrigin;

        public float tier1Weight = 0.8f; //Weights taken from the chance shrine.
        public float tier2Weight = 0.2f;
        public float tier3Weight = 0.01f;
        public float tierLunarWeight = 0f; //Extra weights, taken from nowhere.
        public float tierBossWeight = 0f;
        public float equipmentWeight = 0f;

        private int rewardAmount;

        public Xoroshiro128Plus rng;
        public TeamIndex teamIndex = TeamIndex.None;

        
    }
}
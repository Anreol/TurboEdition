using EntityStates;
using RoR2;
using RoR2.Skills;
using System;
using TurboEdition.Components;
using UnityEngine.Networking;
using TurboEdition.Misc;

namespace TurboEdition.EntityStates.Quests
{
    class BaseQuestState : EntityState
    {
        private NetworkInstanceId _masterNetIdOrigin;
        private QuestObjectiveProvider _objectiveProvider;
        private int _stageNumExpiration;
        protected QuestMissionController questMissionController
        {
            get
            {
                return QuestMissionController.instance;
            }
        }
        protected QuestObjectiveProvider objectiveProvider
        {
            get
            {
                if (!_objectiveProvider)
                {
                    foreach (var item in questMissionController.questProviderToMachine)
                    {
                        if (item.Value == this.outer)
                        {
                            return _objectiveProvider ??= item.Key;
                        }
                    }
                }
                return _objectiveProvider;
            }
        }
            
        public NetworkInstanceId masterNetIdOrigin //Could also use CharacterMaster..?
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
        protected virtual bool shouldRewardOnStepFinish
        {
            get
            {
                return false;
            }
        }
        protected virtual bool shouldRecalcRewardOnStepEnter
        {
            get
            {
                return false;
            }
        }
        public virtual Type GetNextStateType()
        {
            throw new NotImplementedException();
        }
        public override void OnEnter()
        {
            base.OnEnter();

        }
        public override void OnExit()
        {
            base.OnExit();

        }

        public CharacterBody RollEnemy()
        {
            DirectorCard directorCard;
            do
            {
                CombatDirector combatDirector = questMissionController.rng.NextElementUniform<CombatDirector>(CombatDirector.instancesList);
                directorCard = combatDirector.lastAttemptedMonsterCard;
            } while (directorCard == null);

            return directorCard.spawnCard.prefab.GetComponent<CharacterBody>();
        }

        public EliteDef RollElite()
        {
            EliteDef eliteDef;
            CombatDirector combatDirector;
            do
            {
                combatDirector = questMissionController.rng.NextElementUniform<CombatDirector>(CombatDirector.instancesList);
                eliteDef = combatDirector.currentActiveEliteDef;
            } while (combatDirector == null);  //Maybe the elite type that it returns is null, so be it.

            return eliteDef;
        }

        public virtual PickupIndex RollItem(int tier1Weight = 0, int tier2Weight = 0, int tier3Weight = 0, int equipmentWeight = 0, int tierLunarWeight = 0, int tierBossWeight = 0)
        {
            PickupIndex value = Run.instance.treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableTier1DropList);
            PickupIndex value2 = Run.instance.treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableTier2DropList);
            PickupIndex value3 = Run.instance.treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableTier3DropList);
            PickupIndex value4 = Run.instance.treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableEquipmentDropList);
            PickupIndex value5 = Run.instance.treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableLunarDropList);
            PickupIndex value6 = Run.instance.treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableBossDropList);
            WeightedSelection<PickupIndex> weightedSelection = new WeightedSelection<PickupIndex>(8);
            weightedSelection.AddChoice(value, tier1Weight);
            weightedSelection.AddChoice(value2, tier2Weight);
            weightedSelection.AddChoice(value3, tier3Weight);
            weightedSelection.AddChoice(value4, equipmentWeight);
            weightedSelection.AddChoice(value5, tierLunarWeight);
            weightedSelection.AddChoice(value6, tierBossWeight);
            return weightedSelection.Evaluate(Run.instance.treasureRng.nextNormalizedFloat); //uses treasure rng instead of this's rng to avoid fucking up
        }


    }
}

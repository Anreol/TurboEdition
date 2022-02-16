using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using TurboEdition.Misc;
using TurboEdition.Quests;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    internal class QuestMissionController : NetworkBehaviour
    {
        public static QuestMissionController instance { get; private set; }

        public Xoroshiro128Plus rng;
        private EntityStateMachine[] questStateMachines;
        private GenericOwnership[] ownerships;
        public Dictionary<QuestObjectiveProvider, EntityStateMachine> questProviderToMachine;

        public static event Action<EntityStateMachine> onQuestAdded;

        private void Awake()
        {
            this.questProviderToMachine = new Dictionary<QuestObjectiveProvider, EntityStateMachine>();
            this.questStateMachines = new EntityStateMachine[questProviderToMachine.Count];
            this.ownerships = new GenericOwnership[questProviderToMachine.Count];
        }

        private void OnEnable()
        {
            QuestMissionController.instance = SingletonHelper.Assign<QuestMissionController>(QuestMissionController.instance, this);
        }

        private void OnDisable()
        {
            QuestMissionController.instance = SingletonHelper.Unassign<QuestMissionController>(QuestMissionController.instance, this);
        }

        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();
            this.rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
            Stage.onServerStageBegin += Stage_onServerStageBegin;
        }

        private void Stage_onServerStageBegin(Stage obj)
        {
            this.rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint); //Shuffle rng

            foreach (var item in questProviderToMachine)
            {
                if (Run.instance.stageClearCount > ((EntityStates.Quests.BaseQuestState)item.Value.state).stageNumExpiration)
                {
                    item.Value.SetNextState(new EntityStates.Quests.ExpiredQuestState()); //get expirationstate from quest def
                }
            }
        }

        public void TryAddQuest(QuestCatalog.QuestIndex questIndex)
        {
            if (questProviderToMachine.Count < 0)
            {
                TELog.LogW(this.GetType() + " cannot add quest, " + questProviderToMachine + " unitialized.");
                return;
            }
            AddQuest(questIndex);
        }

        private void AddQuest(QuestCatalog.QuestIndex questIndex)
        {
            QuestObjectiveProvider newProvider = this.gameObject.AddComponent<QuestObjectiveProvider>();
            EntityStateMachine newMachine = this.gameObject.AddComponent<EntityStateMachine>();
            newMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.Quests.BeforeQuestState));
            newMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.Quests.IdleQuestState));

            Action<EntityStateMachine> action = QuestMissionController.onQuestAdded;
            if (action != null)
            {
                action(newMachine);
            }

            questProviderToMachine.Add(newProvider, newMachine);
        }
    }
}
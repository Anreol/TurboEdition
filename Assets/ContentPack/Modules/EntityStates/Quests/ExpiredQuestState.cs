using EntityStates;
using System;

namespace TurboEdition.EntityStates.Quests
{
    //State forcefully entered by the QuestMissionController if the quest hits the expiration stage
    internal class ExpiredQuestState : BaseQuestState
    {
        protected virtual bool shouldSetMainStateToFinish
        {
            get
            {
                return false;
            }
        }

        public override Type GetNextStateType()
        {
            return typeof(FinishQuestState);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (shouldSetMainStateToFinish)
                this.outer.mainStateType = new SerializableEntityStateType(this.GetNextStateType());
        }
    }
}
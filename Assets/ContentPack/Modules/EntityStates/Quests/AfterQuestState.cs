using EntityStates;
using System;

namespace TurboEdition.EntityStates.Quests
{
    //Second to last in quests, assigns the main state type to the finish state
    internal class AfterQuestState : BaseQuestState
    {
        public override Type GetNextStateType()
        {
            return typeof(FinishQuestState);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.outer.mainStateType = new SerializableEntityStateType(this.GetNextStateType());
        }
    }
}
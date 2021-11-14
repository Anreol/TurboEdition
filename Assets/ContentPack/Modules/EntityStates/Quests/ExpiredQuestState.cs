using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.EntityStates.Quests
{
    //State forcefully entered by the QuestMissionController if the quest hits the expiration stage
    class ExpiredQuestState : BaseQuestState
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

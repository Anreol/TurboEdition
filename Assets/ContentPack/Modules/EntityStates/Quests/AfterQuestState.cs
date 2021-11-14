using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.EntityStates.Quests
{
    //Second to last in quests, assigns the main state type to the finish state
    class AfterQuestState : BaseQuestState
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

using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.EntityStates.Grenadier
{
    public class UtilityBodyRollEnter : BaseBodyRollEnter
    {
        public override BaseBodyRoll GetNextState()
        {
            return new UtilityBodyRoll(); 
        }
        public override void DisableSkillSlots()
        {
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}

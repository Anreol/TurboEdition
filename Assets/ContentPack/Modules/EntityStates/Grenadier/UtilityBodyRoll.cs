using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.EntityStates.Grenadier
{
    public class UtilityBodyRoll : BaseBodyRoll
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (isAuthority)
            {
                //characterBody.onSkillActivatedAuthority += onSkillActivatedAuthority;
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (isAuthority)
            {
                //characterBody.onSkillActivatedAuthority -= onSkillActivatedAuthority;
            }
        }
        private void onSkillActivatedAuthority(RoR2.GenericSkill obj)
        {
            if (obj == skillLocator.primary || obj == skillLocator.secondary || obj == skillLocator.special)
            {
                this.outer.SetNextState(new UtilityBodyRollExit());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}

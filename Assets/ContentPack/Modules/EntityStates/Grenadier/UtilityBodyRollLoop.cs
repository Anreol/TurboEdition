namespace TurboEdition.EntityStates.Grenadier
{
    public class UtilityBodyRollLoop : BaseBodyRollLoop
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (isAuthority)
            {
                //characterBody.onSkillActivatedAuthority += onSkillActivatedAuthority;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            if (isAuthority)
            {
                //characterBody.onSkillActivatedAuthority -= onSkillActivatedAuthority;
            }
            base.OnExit();
        }

        private void onSkillActivatedAuthority(RoR2.GenericSkill obj)
        {
            if (obj == skillLocator.primary || obj == skillLocator.secondary || obj == skillLocator.special)
            {
                this.outer.SetNextStateToMain();
            }
        }
    }
}
namespace TurboEdition.EntityStates.Grenadier
{
    public class UtilityBodyRollSingle : BaseBodyRollSingle
    {
        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override BaseBodyRollLoop GetNextState()
        {
            return new UtilityBodyRollLoop();
        }
    }
}
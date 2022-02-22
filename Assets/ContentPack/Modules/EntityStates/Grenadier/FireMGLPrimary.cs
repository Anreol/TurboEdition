using TurboEdition.Components;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    internal class FireMGLPrimary : FireMGLBase
    {
        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            if (skillLocator.primary.stock <= 0)
            {
                GrenadierPassiveController grenadierPassiveController = characterBody.GetComponent<GrenadierPassiveController>();
                if (grenadierPassiveController)
                {
                    grenadierPassiveController.primaryFullyDepleted = true;
                }
            }
            base.OnExit();
        }
    }
}
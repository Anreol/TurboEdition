namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class FireMGLSecondaryAlt : FireMGLBase
    {
        public static float selfForce;

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FireProjectile()
        {
            base.FireProjectile();
            if (base.characterMotor)
            {
                base.characterMotor.ApplyForce(GetAimRay().direction * -selfForce, false, false);
            }
        }
    }
}
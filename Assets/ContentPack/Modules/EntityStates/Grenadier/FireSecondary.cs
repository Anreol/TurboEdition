using EntityStates;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    internal class FireSecondary : GenericProjectileBaseState
    {
        public static float selfForce;

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
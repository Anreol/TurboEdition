using EntityStates;

namespace TurboEdition.EntityStates.Grenadier.SideWeapon
{
    internal class FireSecondary : GenericProjectileBaseState
    {
        public static float minimumDuration;
        public static float selfForce;
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (fixedAge <= minimumDuration)
            {
                return InterruptPriority.PrioritySkill;
            }
            return base.GetMinimumInterruptPriority();
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
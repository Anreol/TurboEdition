using EntityStates;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.SideWeapon
{
    internal class FireSecondary : GenericProjectileBaseState
    {
        [Tooltip("Base duration of an animation, gets multipled by the state duration later.")]
        public static float baseAnimDuration = 1f;
        public static float minimumDuration;
        public static float selfForce;
        public override void PlayAnimation(float duration)
        {
            base.PlayAnimation(duration);
            base.PlayAnimation("Gesture, Left Arm, Additive", "FireSideWeapon", "FireSideWeapon.playbackRate", baseAnimDuration * duration);
        }

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
            if (base.characterMotor && !characterMotor.isGrounded)
            {
                base.characterMotor.ApplyForce(GetAimRay().direction * -selfForce, false, false);
            }
        }
    }
}
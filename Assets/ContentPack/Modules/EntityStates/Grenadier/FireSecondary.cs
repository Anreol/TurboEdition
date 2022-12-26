using EntityStates;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.SideWeapon
{
    internal class FireSecondaryAlt : GenericProjectileBaseState
    {
        [SerializeField]
        public float baseAnimDuration = 1f;

        public static float minimumDuration;
        public static float selfForce;

        public override void OnEnter()
        {
            base.OnEnter();
            if (base.characterBody)
                base.characterBody.SetAimTimer(1.5f); //I have no idea what this does
            base.PlayAnimation("Gesture, Left Arm, Additive", "FireSideWeapon", "FireSideWeapon.playbackRate", baseAnimDuration / this.attackSpeedStat);
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
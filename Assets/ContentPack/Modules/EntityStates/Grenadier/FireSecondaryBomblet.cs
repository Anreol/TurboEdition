using EntityStates;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.SideWeapon
{
    public class FireSecondaryBomblet : GenericProjectileBaseState, SteppedSkillDef.IStepSetter, ISkillState
    {
        [Tooltip("Base duration of an animation, gets multipled by the state duration later.")]
        [SerializeField]
        public float baseAnimDuration = 1;

        [Tooltip("Vertical velocity of the small hop to perform.")]
        [SerializeField]
        public float smallHopVelocity;

        [Tooltip("Max number of throws to perform.")]
        [SerializeField]
        public float comboThrows = 3;

        [Tooltip("How much should consecutive throws be slowed or fasten up")]
        [SerializeField]
        public float comboDurationMod = 0.5f;

        [Tooltip("How much should consecutive throws be spread for. Accumulative")]
        [SerializeField]
        public float comboSpreadMod = 1.5f;

        [Tooltip("How much should consecutive throws apply recoil for. Accumulative")]
        [SerializeField]
        public float comboRecoilMod = 1.2f;

        [Tooltip("How much should consecutive throws add bloom for. Accumulative")]
        [SerializeField]
        public float comboSpreadBloomMod = 1.5f;

        private GenericSkill _skillslot;
        private int step;

        public GenericSkill activatorSkillSlot { get => _skillslot; set => _skillslot = value; }

        public void SetStep(int i)
        {
            step = i;
        }

        public override void ModifyNextState(EntityState nextState)
        {
            base.ModifyNextState(nextState);
            if (step + 1 < comboThrows)
            {
                FireSecondaryBomblet fireSecondaryBomblet;
                if ((fireSecondaryBomblet = (nextState as FireSecondaryBomblet)) != null)
                {
                    fireSecondaryBomblet.baseDelayBeforeFiringProjectile *= comboDurationMod;
                    fireSecondaryBomblet.baseDuration *= comboDurationMod;
                    fireSecondaryBomblet.minSpread = this.minSpread * comboSpreadMod;
                    fireSecondaryBomblet.maxSpread = this.maxSpread * comboSpreadMod;
                    fireSecondaryBomblet.recoilAmplitude = this.recoilAmplitude * comboRecoilMod;
                    fireSecondaryBomblet.bloom = this.bloom * comboSpreadBloomMod;
                }
            }
        }

        public override void PlayAnimation(float duration)
        {
            base.PlayAnimation(duration);
            if (step == 0)
            {
                //TODO: pull out & throw anim
                base.PlayAnimation("Gesture, Left Arm, Additive", "FireSideWeapon", "FireSideWeapon.playbackRate", baseAnimDuration * duration);
                return;
            }
            //TODO: throw anim
            base.PlayAnimation("Gesture, Left Arm, Additive", "FireSideWeapon, FireNow", "FireSideWeapon.playbackRate", baseAnimDuration * duration);
        }

        public override void FireProjectile()
        {
            base.FireProjectile();
            if (!characterMotor.isGrounded)
            {
                SmallHop(characterMotor, smallHopVelocity);
            }
        }
    }
}
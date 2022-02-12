using EntityStates;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class FireMGLBase : GenericProjectileBaseState
    {
        [SerializeField]
        public float baseAnimDuration = 1f;
        [SerializeField]
        public float baseMinDuration = 0f;
        
        private bool buttonReleased;
        private float minimumDuration;
        public override void OnEnter()
        {
            buttonReleased = false;
            base.OnEnter(); //Call Base for it to do its things and then we override its values down here.
            minimumDuration = baseMinDuration / this.attackSpeedStat;
            if (base.characterBody)
                base.characterBody.SetAimTimer(1.5f); //I have no idea what this does
            base.PlayAnimation("Gesture, Additive", "FireMainWeapon", "FireMainWeapon.playbackRate", baseAnimDuration / this.attackSpeedStat);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!buttonReleased && !(base.inputBank && base.inputBank.skill1.down))
            {
                buttonReleased = true;
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (base.fixedAge <= minimumDuration || !buttonReleased)
            {
                return InterruptPriority.PrioritySkill;
            }
            return InterruptPriority.Any;
        }
        public override void OnExit()
        {
            if (!buttonReleased && base.characterBody && base.skillLocator && base.skillLocator.primary.stock > 0)
            {
                base.characterBody.SetSpreadBloom(0f, false);
            }
            base.OnExit();
        }
    }
}
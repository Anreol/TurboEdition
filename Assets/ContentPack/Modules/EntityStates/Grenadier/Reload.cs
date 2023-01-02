using EntityStates;
using RoR2;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class Reload : BaseState
    {
        [SerializeField]
        public float baseDuration;

        [SerializeField]
        public string enterSoundString;

        [SerializeField]
        public string exitSoundString;

        [SerializeField]
        public GameObject reloadEffectPrefab;

        [SerializeField]
        public float enterSoundPitch;

        [SerializeField]
        public float exitSoundPitch;

        [SerializeField]
        public string reloadEffectMuzzleString;

        private bool hasGivenStock;

        public float duration
        {
            get
            {
                return baseDuration / this.attackSpeedStat;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            //base.PlayAnimation("Gesture, Additive", (base.characterBody.isSprinting && base.characterMotor && base.characterMotor.isGrounded) ? "ReloadSimple" : "Reload", "Reload.playbackRate", this.duration);
            base.PlayAnimation("Gesture, Override", "ReloadLoop", "Reload.playbackRate", this.duration);
            Util.PlayAttackSpeedSound(enterSoundString, base.gameObject, enterSoundPitch);
            EffectManager.SimpleMuzzleFlash(reloadEffectPrefab, base.gameObject, reloadEffectMuzzleString, false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration / 2f)
            {
                this.GiveStock();
            }
            if (!base.isAuthority || base.fixedAge < this.duration)
            {
                return;
            }
            if (base.skillLocator.primary.stock < base.skillLocator.primary.maxStock)
            {
                this.outer.SetNextState(GetNextState());
                return;
            }
            Util.PlayAttackSpeedSound(exitSoundString, base.gameObject, exitSoundPitch);
            this.outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void GiveStock()
        {
            if (this.hasGivenStock)
            {
                return;
            }
            if (base.isAuthority && base.skillLocator.primary.stock < base.skillLocator.primary.maxStock)
            {
                base.skillLocator.primary.AddOneStock();
            }
            this.hasGivenStock = true;
        }

        public virtual Reload GetNextState()
        {
            return new Reload();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
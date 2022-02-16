using EntityStates;
using RoR2;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class Reload : BaseState
    {
        public static float baseDuration;
        public static string enterSoundString;
        public static string exitSoundString;
        public static GameObject reloadEffectPrefab;
        public static float enterSoundPitch;
        public static float exitSoundPitch;
        public static string reloadEffectMuzzleString;
        private bool hasGivenStock;

        public float duration
        {
            get
            {
                return Reload.baseDuration / this.attackSpeedStat;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            base.PlayAnimation("Gesture, Additive", (base.characterBody.isSprinting && base.characterMotor && base.characterMotor.isGrounded) ? "ReloadSimple" : "Reload", "Reload.playbackRate", this.duration);
            Util.PlayAttackSpeedSound(Reload.enterSoundString, base.gameObject, Reload.enterSoundPitch);
            EffectManager.SimpleMuzzleFlash(Reload.reloadEffectPrefab, base.gameObject, Reload.reloadEffectMuzzleString, false);
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
                this.outer.SetNextState(new Reload());
                return;
            }
            Util.PlayAttackSpeedSound(Reload.exitSoundString, base.gameObject, Reload.exitSoundPitch);
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
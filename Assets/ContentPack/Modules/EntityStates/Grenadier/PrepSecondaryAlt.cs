using EntityStates;
using RoR2;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class PrepSecondaryAlt : BaseState //We will need a component or something to override the primary depending on the stock count. Use CaptainSupplyDropController as reference
    {
        public static float baseDuration;
        public static string enterSoundString;
        public static string exitSoundString;
        public static GameObject reloadEffectPrefab;
        public static float enterSoundPitch;
        public static float exitSoundPitch;
        public static string reloadEffectMuzzleString;
        private bool hasGivenStock;

        private float duration
        {
            get
            {
                return PrepSecondaryAlt.baseDuration / this.attackSpeedStat;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            base.PlayAnimation("Gesture, Additive", (base.characterBody.isSprinting && base.characterMotor && base.characterMotor.isGrounded) ? "ReloadSimple" : "Reload", "Reload.playbackRate", this.duration);
            Util.PlayAttackSpeedSound(PrepSecondaryAlt.enterSoundString, base.gameObject, PrepSecondaryAlt.enterSoundPitch);
            EffectManager.SimpleMuzzleFlash(PrepSecondaryAlt.reloadEffectPrefab, base.gameObject, PrepSecondaryAlt.reloadEffectMuzzleString, false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration / 2f)
            {
                this.GiveStock();
                //TODO: Call for override component here
            }
            if (!base.isAuthority || base.fixedAge < this.duration)
            {
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
            return InterruptPriority.PrioritySkill;
        }
    }
}
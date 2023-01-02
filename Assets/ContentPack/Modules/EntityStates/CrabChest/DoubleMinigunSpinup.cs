using EntityStates;
using RoR2;
using UnityEngine;

namespace TurboEdition.EntityStates.CrabChest.Weapon
{
    internal class DoubleMinigunSpinup : DoubleMinigunState
    {
        public static float baseDuration;
        public static string enterPlaySoundEvent;
        public static GameObject chargeEffectPrefab;

        private GameObject leftChargeInstance;
        private GameObject rightChargeInstance;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = DoubleMinigunSpinup.baseDuration / this.attackSpeedStat;
            Util.PlaySound(DoubleMinigunSpinup.enterPlaySoundEvent, base.gameObject);
            base.GetModelAnimator().SetBool("WeaponIsReady", true);
            if (DoubleMinigunSpinup.chargeEffectPrefab)
            {
                if (leftMuzzleTransform)
                {
                    this.leftChargeInstance = UnityEngine.Object.Instantiate<GameObject>(DoubleMinigunSpinup.chargeEffectPrefab, this.leftMuzzleTransform.position, this.leftMuzzleTransform.rotation);
                    this.leftChargeInstance.transform.parent = this.leftMuzzleTransform;
                    ScaleParticleSystemDuration component = this.leftChargeInstance.GetComponent<ScaleParticleSystemDuration>();
                    if (component)
                        component.newDuration = this.duration;
                }
                if (rightMuzzleTransform)
                {
                    this.rightChargeInstance = UnityEngine.Object.Instantiate<GameObject>(DoubleMinigunSpinup.chargeEffectPrefab, this.rightMuzzleTransform.position, this.rightMuzzleTransform.rotation);
                    this.rightChargeInstance.transform.parent = this.rightMuzzleTransform;
                    ScaleParticleSystemDuration component = this.rightChargeInstance.GetComponent<ScaleParticleSystemDuration>();
                    if (component)
                        component.newDuration = this.duration;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority && skillButtonState.down)
            {
                this.outer.SetNextState(new DoubleMinigunFire());
            }
            else if (!skillButtonState.down && base.isAuthority)
            {
                this.outer.SetNextState(new DoubleMinigunSpindown());
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (this.leftChargeInstance)
            {
                EntityState.Destroy(this.leftChargeInstance);
            }
        }
    }
}
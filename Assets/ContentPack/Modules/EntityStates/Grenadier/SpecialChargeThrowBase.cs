using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public abstract class SpecialChargeThrowBase : BaseSkillState
    {
        private float duration;
        private uint loopSoundInstanceId;
        private GameObject defaultCrosshairPrefab;
        private GameObject anglesEffectInstance;

        [SerializeField]
        public GameObject crosshairOverridePrefab;
        [SerializeField]
        public string chargeSoundString;
        [SerializeField]
        public float baseDuration;
        [SerializeField]
        public float minChargeDuration;
        [SerializeField]
        public float targetAngles;
        [SerializeField]
        public GameObject anglesEffectPrefab;
        [SerializeField]
        public float minBloomRadius;
        [SerializeField]
        public float maxBloomRadius;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.PlayChargeAnimation();
            this.loopSoundInstanceId = Util.PlayAttackSpeedSound(this.chargeSoundString, base.gameObject, this.attackSpeedStat);
            this.defaultCrosshairPrefab = base.characterBody.crosshairPrefab;
            if (this.crosshairOverridePrefab)
            {
                base.characterBody.crosshairPrefab = this.crosshairOverridePrefab;
            }
            if (transform && this.anglesEffectPrefab)
            {
                this.anglesEffectInstance = UnityEngine.Object.Instantiate<GameObject>(this.anglesEffectPrefab, transform.position, transform.rotation);
                this.anglesEffectInstance.transform.parent = transform;
                ScaleParticleSystemDuration component = this.anglesEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                if (component)
                    component.newDuration = this.duration;
            }
        }
        protected float CalcCharge()
        {
            return Mathf.Clamp01(base.fixedAge / this.duration);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = this.CalcCharge();
            if (anglesEffectInstance)
            {
                ChildLocator childrenLocated = anglesEffectInstance.GetComponent<ChildLocator>();
                if (childrenLocated)
                {
                    //Copy pasted from knife fan because i cannot be fucking bothered to do angles again
                    Vector3 aimDirection = base.characterBody.inputBank.aimDirection;
                    Vector3 crossVector = aimDirection == Vector3.up ? Vector3.down : Vector3.up;
                    Vector3 up = Vector3.Cross(Vector3.Cross(aimDirection, crossVector), aimDirection);

                    childrenLocated.transformPairs[1].transform.rotation = Util.QuaternionSafeLookRotation(Quaternion.AngleAxis(-targetAngles * charge, up) * aimDirection);
                    childrenLocated.transformPairs[2].transform.rotation = Util.QuaternionSafeLookRotation(Quaternion.AngleAxis(targetAngles * charge, up) * aimDirection);
                }
            }
            base.characterBody.SetSpreadBloom(Util.Remap(this.CalcCharge(), 0f, 1f, this.minBloomRadius, this.maxBloomRadius), true);

            if (base.isAuthority && ((!base.IsKeyDownAuthority() && base.fixedAge >= this.minChargeDuration) || base.fixedAge >= this.duration))
            {
                int count = base.skillLocator.special.stock;
                SpecialThrowBase nextState = this.GetNextState();
                nextState.charge = charge;
                nextState.projectileCount = count;
                base.skillLocator.special.DeductStock(count);
                this.outer.SetNextState(nextState);
            }
        }
        protected virtual void PlayChargeAnimation()
        {
            base.PlayAnimation("Gesture, Additive", "ChargeNovaBomb", "ChargeNovaBomb.playbackRate", this.duration);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
        public override void OnExit()
        {
            if (base.characterBody)
            {
                base.characterBody.crosshairPrefab = this.defaultCrosshairPrefab;
            }
            AkSoundEngine.StopPlayingID(this.loopSoundInstanceId);
            if (!this.outer.destroying)
            {
                base.PlayAnimation("Gesture, Additive", "Empty");
            }
            EntityState.Destroy(this.anglesEffectInstance);
            base.OnExit();
        }
        public abstract SpecialThrowBase GetNextState();
    }
}

using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public abstract class SpecialChargeThrowBase : AimThrowableBase
    {
        private float duration;
        private uint loopSoundInstanceId;
        private GameObject defaultCrosshairPrefab;

        //Base values that get modified with charge
        private float trueBaseProjectileSpeed;
        private float trueMaxDistance;

        [SerializeField]
        public GameObject crosshairOverridePrefab;
        [SerializeField]
        public string chargeSoundString;
        [SerializeField]
        public float baseDuration;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.PlayChargeAnimation();
            this.loopSoundInstanceId = Util.PlayAttackSpeedSound(this.chargeSoundString, base.gameObject, this.attackSpeedStat);
            this.defaultCrosshairPrefab = base.characterBody.crosshairPrefab; //Store for later restoring
            base.characterBody.hideCrosshair = false; //Undo base
            this.trueBaseProjectileSpeed = base.projectileBaseSpeed;
            this.trueMaxDistance = base.maxDistance;
            projectileBaseSpeed = 0;
            maxDistance = 0;

            if (this.crosshairOverridePrefab)
            {
                base.characterBody.crosshairPrefab = this.crosshairOverridePrefab;
            }
        }
        protected float CalcCharge()
        {
            return Mathf.Clamp01(base.fixedAge / this.duration);
        }
        public override void FixedUpdate()
        {
            this.fixedAge += Time.fixedDeltaTime;
            float charge = this.CalcCharge();
            this.projectileBaseSpeed = trueBaseProjectileSpeed * charge;
            this.maxDistance = trueMaxDistance * charge;

            if (base.isAuthority && ((!base.IsKeyDownAuthority() && base.fixedAge >= this.minimumDuration) || base.fixedAge >= this.duration))
            {
                this.UpdateTrajectoryInfo(out this.currentTrajectoryInfo);
                int count = base.skillLocator.special.stock;
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    owner = base.gameObject,
                    position = this.currentTrajectoryInfo.finalRay.origin,
                    projectilePrefab = this.projectilePrefab,
                    rotation = Util.QuaternionSafeLookRotation(this.currentTrajectoryInfo.finalRay.direction, Vector3.up),
                    speedOverride = this.currentTrajectoryInfo.speedOverride,
                    damage = this.damageCoefficient * this.damageStat
                };
                if (setFuse)
                {
                    fireProjectileInfo.fuseOverride = this.currentTrajectoryInfo.travelTime;
                }
                SpecialThrowBase nextState = this.GetNextState();
                if (nextState != null)
                {
                    nextState.fireProjectileInfo = fireProjectileInfo;
                    base.skillLocator.special.DeductStock(count);
                    this.outer.SetNextState(nextState);
                    return;
                }
                this.outer.SetNextStateToMain();
                return;
            }
        }
        protected virtual void PlayChargeAnimation()
        {
            base.PlayAnimation("Gesture, Additive", "ChargeTODO", "ChargeTODO.playbackRate", this.duration);
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
            base.OnExit();
        }
        public override void FireProjectile() { } //fuck off lol
        public abstract SpecialThrowBase GetNextState();
    }
}

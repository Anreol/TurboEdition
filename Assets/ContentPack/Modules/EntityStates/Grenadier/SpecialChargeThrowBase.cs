using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public abstract class SpecialChargeThrowBase : AimThrowableBase
    {
        private uint loopSoundInstanceId;
        private GameObject defaultCrosshairPrefab;
        public float chargedAge;
        private bool buttonReleased;

        //Base values that get modified with charge
        private float trueBaseProjectileSpeed;

        private float trueMaxDistance;

        [SerializeField]
        public GameObject crosshairOverridePrefab;

        [SerializeField]
        public string chargeSoundString;

        [SerializeField]
        public float baseDuration;

        [SerializeField]
        public float graceDuration;

        [SerializeField]
        [Tooltip("The frequency  (1/time) at which projectiles are fired. Higher values means faster.")]
        public float baseFireFrequency;

        private float fireStopwatch;

        private float fireFrequency => baseFireFrequency * attackSpeedStat;
        private float duration => this.baseDuration / this.attackSpeedStat;

        public override void OnEnter()
        {
            base.OnEnter();
            //this.PlayChargeAnimation();
            this.defaultCrosshairPrefab = base.characterBody.crosshairPrefab; //Store for later restoring
            base.characterBody.hideCrosshair = false; //Undo base
            this.trueBaseProjectileSpeed = base.projectileBaseSpeed;
            this.trueMaxDistance = base.maxDistance;
            projectileBaseSpeed = 0;
            maxDistance = 0;
            fireStopwatch = 1f;
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

            if (!IsKeyDownAuthority())
                buttonReleased = true;

            int count = base.skillLocator.special.stock;
            if (base.isAuthority)
            {
                if (!buttonReleased)
                {
                    float charge = this.CalcCharge();
                    this.projectileBaseSpeed = trueBaseProjectileSpeed * charge;
                    this.maxDistance = trueMaxDistance * charge;

                    if (base.fixedAge >= this.minimumDuration && charge >= 1f)
                    {
                        if (loopSoundInstanceId == 0U)
                        {
                            Util.PlaySound("Play_GrenadierSpecialReady", base.gameObject);
                            this.loopSoundInstanceId = Util.PlaySound(chargeSoundString, base.gameObject);
                        }
                        if (chargedAge < graceDuration)
                        {
                            chargedAge += Time.fixedDeltaTime;
                            AkSoundEngine.SetRTPCValueByPlayingID("GrenadierSpecial_ChargeAmount", (chargedAge / graceDuration) * 100, this.loopSoundInstanceId);
                        }
                        else
                        {
                            this.UpdateTrajectoryInfo(out this.currentTrajectoryInfo);
                            this.fireStopwatch += Time.fixedDeltaTime;
                            if (fireStopwatch >= 1f / fireFrequency && count > 0)
                            {
                                FireOnce(true);
                                fireStopwatch -= 1f / this.fireFrequency;
                            }
                        }
                    }
                }
                if (buttonReleased && base.fixedAge >= this.minimumDuration)
                {
                    AkSoundEngine.StopPlayingID(loopSoundInstanceId);
                    this.UpdateTrajectoryInfo(out this.currentTrajectoryInfo);
                    this.fireStopwatch += Time.fixedDeltaTime;
                    if (fireStopwatch >= 1f / fireFrequency && count > 0)
                    {
                        FireOnce(false);
                        fireStopwatch -= 1f / this.fireFrequency;
                    }
                }
            }
            if (count <= 0)
            {
                outer.SetNextStateToMain();
            }
        }

        protected virtual void PlayChargeAnimation()
        {
            base.PlayAnimation("Gesture, Additive", "ChargeTODO", "ChargeTODO.playbackRate", this.duration);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (base.fixedAge <= minimumDuration || !buttonReleased)
            {
                return InterruptPriority.PrioritySkill;
            }
            return InterruptPriority.Skill;
        }

        public override void OnExit()
        {
            if (base.characterBody)
            {
                base.characterBody.crosshairPrefab = this.defaultCrosshairPrefab;
            }
            AkSoundEngine.StopPlayingID(loopSoundInstanceId);
            if (!this.outer.destroying)
            {
                base.PlayAnimation("Gesture, Additive", "Empty");
            }
            base.OnExit();
        }

        public virtual void FireOnce(bool wasForced)
        {
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
            if (wasForced)
            {
                fireProjectileInfo.fuseOverride = 0.01f;
            }
            this.ModifyProjectile(ref fireProjectileInfo);
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            base.skillLocator.special.DeductStock(1);
        }

        public override void FireProjectile()
        { } //fuck off lol
    }
}
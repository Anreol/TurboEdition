using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public abstract class AimThrowableBaseChargable : AimThrowableBase
    {
        [SerializeField]
        public GameObject crosshairOverridePrefab;

        [Tooltip("Time it takes to fully charge the skill, scales with attack speed.")]
        [SerializeField]
        public float baseDuration;

        [Tooltip("Time in which the player can hold into their charge before being forcefully thrown.")]
        [SerializeField]
        public float graceDuration;

        [SerializeField]
        [Tooltip("Makes it so it won't leave the state as long as there's stock remaining. Doesn't account for the very first stock lost on skill activation.")]
        public bool fireAllStocks;

        [SerializeField]
        [Tooltip("The frequency (1/time) at which projectiles are fired, if multiple. Higher values means faster, scales with attack speed. Zero to make it instant.")]
        public float baseFireFrequency;

        [SerializeField]
        [Tooltip("Should it already start with some charge to avoid projectile and max distance being zero. Should scale from 0-1, but can be whatever.")]
        public float minimumChargeValue;

        [SerializeField]
        [Tooltip("wWise event to play whenever it is done charging.")]
        public string soundChargeReadyString;

        [Tooltip("wWise event of a looping sound whenever it is done charging")]
        [SerializeField]
        public string soundLoopFullyChargedString;

        [SerializeField]
        [Tooltip("wWise RTPC ID to change from the soundLoopFullyChargedString whenever it starts overcharging.")]
        public string soundLoopRTPCChargeID;

        private uint loopSoundInstanceId;
        private bool buttonReleasedAuthority;

        /// <summary>
        /// Starts counting whenever the fixed age is more than the minimum duration and the charge has exceeded one. Does not exceed graceDuration.
        /// </summary>
        internal float chargedAge;

        /// <summary>
        /// Copy of Base's projectileBaseSpeed, which is the velocity of the ProjectileSimple component
        /// </summary>
        internal float targetProjectileBaseSpeed;

        /// <summary>
        /// Copy of Base's maxDistance, which is a field in the entity state configuration and the target distance to hit.
        /// </summary>
        private float targetMaxDistance;

        private bool attemptedToPlayOverchargeSound;
        private float fireTimer;
        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

        private float fireFrequency => baseFireFrequency * attackSpeedStat;

        /// <summary>
        /// Time that it takes to fully charge, scalewd with attack speed.
        /// </summary>
        private float duration => this.baseDuration / this.attackSpeedStat;

        internal virtual bool throwWasForced => chargedAge > graceDuration;
        internal bool firedAtLeastOnce;

        public override void OnEnter()
        {
            base.OnEnter();
            //this.PlayChargeAnimation();
            //Undo base to show the crosshair
            if (base.characterBody)
            {
                base.characterBody.hideCrosshair = false;
            }

            //Backup data and reset it to zero, or the according initial charge value.
            targetProjectileBaseSpeed = base.projectileBaseSpeed;
            targetMaxDistance = base.maxDistance;
            float charge = CalcCharge();
            projectileBaseSpeed = targetProjectileBaseSpeed * charge;
            maxDistance = targetMaxDistance * charge;

            if (this.crosshairOverridePrefab)
            {
                this.crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, this.crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
            }
        }

        protected float CalcCharge()
        {
            return Mathf.Max(Mathf.Clamp01(base.fixedAge / this.duration), minimumChargeValue);
        }

        public override void UpdateTrajectoryInfo(out TrajectoryInfo dest)
        {
            //Access it once
            float charge = this.CalcCharge();

            //Update trajectories according to charge
            projectileBaseSpeed = targetProjectileBaseSpeed * charge;
            maxDistance = targetMaxDistance * charge;

            base.UpdateTrajectoryInfo(out dest);
        }

        //Override the entire FixedUpdate
        public override void FixedUpdate()
        {
            //Update fixed age ourselves as we aren't calling Base
            this.fixedAge += Time.fixedDeltaTime;

            if (!KeyIsDown())
                buttonReleasedAuthority = true;

            if (!buttonReleasedAuthority)
            {
                //Reminder that minimum duration is base minimum duration / attack speed
                //Is the age exceeding charge?
                if (base.fixedAge >= minimumDuration && CalcCharge() >= 1f)
                {
                    if (!attemptedToPlayOverchargeSound)
                    {
                        attemptedToPlayOverchargeSound = true;
                        Util.PlaySound(soundChargeReadyString, base.gameObject);
                        //The method makes sure the string is not empty. Returns 0U if empty.
                        this.loopSoundInstanceId = Util.PlaySound(soundLoopFullyChargedString, base.gameObject);
                    }
                    if (chargedAge < graceDuration)
                    {
                        chargedAge += Time.fixedDeltaTime;
                        if (loopSoundInstanceId != 0U)
                        {
                            AkSoundEngine.SetRTPCValueByPlayingID(soundLoopRTPCChargeID, (chargedAge / graceDuration) * 100, this.loopSoundInstanceId);
                        }
                    }
                    else //Start blastin because we have overcharged.
                    {
                        fireTimer -= Time.fixedDeltaTime;
                        if (fireTimer <= 0f && activatorSkillSlot.stock > 0)
                        {
                            fireTimer = 1f / fireFrequency;
                            UpdateTrajectoryInfo(out this.currentTrajectoryInfo); //Update trajectory as we are about to fire
                            if (isAuthority)
                            {
                                if (firedAtLeastOnce) //We are beyond the first shot (which is free as using the skill consumes a stock)
                                    activatorSkillSlot.DeductStock(activatorSkillSlot.skillDef.stockToConsume);
                                FireProjectileOnce(currentTrajectoryInfo.finalRay);
                            }
                            base.OnProjectileFiredLocal();
                        }
                    }
                }
            }

            if (buttonReleasedAuthority && base.fixedAge >= minimumDuration)
            {
                if (loopSoundInstanceId != 0U)
                {
                    AkSoundEngine.StopPlayingID(loopSoundInstanceId);
                }
                fireTimer -= Time.fixedDeltaTime;
                if (fireTimer <= 0f && activatorSkillSlot.stock > 0)
                {
                    fireTimer = 1f / fireFrequency;
                    UpdateTrajectoryInfo(out this.currentTrajectoryInfo); //Update trajectory as we are about to fire
                    if (isAuthority)
                    {
                        //if (firedAtLeastOnce) //We are beyond the first shot (which is free as using the skill consumes a stock)
                            activatorSkillSlot.DeductStock(activatorSkillSlot.skillDef.stockToConsume);
                        FireProjectileOnce(currentTrajectoryInfo.finalRay);
                    }
                    base.OnProjectileFiredLocal();
                }
            }

            if (activatorSkillSlot.stock <= 0 || (!fireAllStocks && firedAtLeastOnce))
            {
                EntityState entityState = PickNextState();
                if (entityState != null)
                {
                    outer.SetNextState(entityState);
                    return;
                }
                outer.SetNextStateToMain();
            }
        }

        protected virtual void PlayChargeAnimation()
        {
            base.PlayAnimation("Gesture, Additive", "ChargeTODO", "ChargeTODO.playbackRate", this.duration);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (base.fixedAge <= minimumDuration || !buttonReleasedAuthority)
            {
                return InterruptPriority.PrioritySkill;
            }
            return InterruptPriority.Skill;
        }

        public override void OnExit()
        {
            crosshairOverrideRequest?.Dispose();

            AkSoundEngine.StopPlayingID(loopSoundInstanceId);
            if (!this.outer.destroying)
            {
                base.PlayAnimation("Gesture, Additive", "Empty");
            }
            base.OnExit();
        }

        public virtual void FireProjectileOnce(Ray finalRay)
        {
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                crit = base.RollCrit(),
                owner = base.gameObject,
                position = finalRay.origin,
                projectilePrefab = this.projectilePrefab,
                rotation = Util.QuaternionSafeLookRotation(finalRay.direction, Vector3.up),
                speedOverride = this.currentTrajectoryInfo.speedOverride,
                damage = this.damageCoefficient * this.damageStat,
            };
            if (setFuse)
            {
                fireProjectileInfo.fuseOverride = this.currentTrajectoryInfo.travelTime;
            }
            if (throwWasForced)
            {
                fireProjectileInfo.fuseOverride = 0.01f;
            }
            this.ModifyProjectile(ref fireProjectileInfo);
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            firedAtLeastOnce = true;
        }

        public override void FireProjectile()
        { } //fuck off lol
    }
}
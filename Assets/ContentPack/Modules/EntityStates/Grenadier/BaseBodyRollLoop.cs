using EntityStates;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier
{
    public class BaseBodyRollLoop : BaseSkillState
    {
        [SerializeField]
        public float maximumRollDuration;

        [SerializeField]
        public float minimumBaseDuration;

        [SerializeField]
        public float airControlOverride;

        [SerializeField]
        public float rollingTimeExtensionWhenAttacking;

        //Overlap attack related
        [SerializeField]
        public float baseOverlapAttackRate;

        [SerializeField]
        public float baseOverlapAttackCoefficient;

        [SerializeField]
        public GameObject hitEffectPrefab;

        [SerializeField]
        public string hitboxString;

        [SerializeField]
        private float overlapAttackDownwardsForce;

        [SerializeField]
        public float overlapAttackSmallHopVelocity;

        [SerializeField]
        [Tooltip("The frequency  (1/time) at which the overlap attack is reset. Higher values means more frequent ticks of damage.")]
        public float overlapAttackResetFrequency;

        //Hitlag
        [SerializeField]
        public float baseHitPauseDuration;

        [SerializeField]
        public string hitSoundString;

        [SerializeField]
        public float hitMaximumTargetsAtOnce;

        [SerializeField]
        public float hitRecoilAmplitude;

        private Animator animator;
        private OverlapAttack overlapAttack;
        private float totalStopwatch = 0f;

        private float previousAirControl;
        private HitStopCachedState hitStopCachedState;
        private float hitPauseTimer;
        private bool isInHitPause;

        private List<HurtBox> victims = new List<HurtBox>();
        private bool buttonReleased;
        private float resetStopwatch;
        private Vector3 idealDirection;
        private bool exitNextFrame;

        private float minimumDuration => minimumBaseDuration / this.attackSpeedStat;
        private float hitLagDuration => baseHitPauseDuration / this.attackSpeedStat;
        private float resetFrequency => overlapAttackResetFrequency * attackSpeedStat;

        public override void OnEnter()
        {
            base.OnEnter();
            ResetOverlap();
            PlayAnim();
            if (base.isAuthority)
            {
                characterMotor.onMovementHit += onMovementHit;
                if (base.inputBank)
                {
                    this.idealDirection = base.inputBank.aimDirection;
                    this.idealDirection.y = 0f;
                }
                this.UpdateDirection();
            }
            if (base.characterDirection)
            {
                base.characterDirection.forward = this.idealDirection;
            }
            this.animator = base.GetModelAnimator();
            //Very important to do
            resetStopwatch -= 1f / this.resetFrequency;
            previousAirControl = characterMotor.airControl;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.isSprinting = true;
            this.totalStopwatch += Time.fixedDeltaTime;
            this.resetStopwatch += Time.fixedDeltaTime;
            buttonReleased = !buttonReleased && !(base.inputBank && base.inputBank.skill1.down);
            if (base.isAuthority)
            {
                if (resetStopwatch >= 1f / resetFrequency)
                {
                    ResetOverlap();
                    resetStopwatch -= 1f / this.resetFrequency;
                }
                UpdateDirection();
                ModifyOverlapAttack(overlapAttack);
                if (overlapAttack.Fire(victims))
                {
                    base.SmallHop(characterMotor, overlapAttackSmallHopVelocity);
                    HitSuccessful(victims);
                }
                if (!isInHitPause)
                {
                    if (base.characterDirection)
                    {
                        base.characterDirection.moveVector = this.idealDirection;
                    }
                }
                else
                {
                    base.fixedAge -= Time.fixedDeltaTime;
                    this.hitPauseTimer -= Time.fixedDeltaTime;
                    this.resetStopwatch += Time.fixedDeltaTime;
                    this.totalStopwatch += Time.fixedDeltaTime;
                    base.characterMotor.velocity = Vector3.zero;
                    this.animator.SetFloat("UtilityRoll.playbackRate", 0f);
                }
                if (this.hitPauseTimer <= 0f && this.isInHitPause)
                {
                    base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                    base.characterMotor.Motor.ForceUnground();
                    this.isInHitPause = false;
                }
            }
            if ((totalStopwatch >= maximumRollDuration || base.fixedAge > this.minimumDuration) && (this.exitNextFrame || base.characterMotor.Motor.GroundingStatus.IsStableOnGround))
            {
                ReturnToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (isAuthority)
            {
                characterMotor.airControl = previousAirControl;
                characterMotor.onMovementHit -= onMovementHit;
            }
        }
        private void onMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            this.exitNextFrame = true;
        }

        public virtual void ModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            float damage = isInHitPause ? damageStat * baseOverlapAttackCoefficient : Mathf.Max(damageStat * baseOverlapAttackCoefficient, damageStat * (baseOverlapAttackCoefficient * GetDamageBoostFromSpeed()));
            overlapAttack.damage = damage;
            this.overlapAttack.forceVector = new Vector3(0, Mathf.Min(Vector3.down.y, characterBody.characterMotor.velocity.y), 0) * overlapAttackDownwardsForce;
        }

        public virtual void ResetOverlap()
        {
            this.overlapAttack = base.InitMeleeOverlap(damageStat * baseOverlapAttackCoefficient, hitEffectPrefab, base.GetModelTransform(), hitboxString);
        }

        public virtual void HitSuccessful(List<HurtBox> victims)
        {
            totalStopwatch -= rollingTimeExtensionWhenAttacking;
            characterMotor.airControl = airControlOverride;
            base.AddRecoil(-0.5f * hitRecoilAmplitude, -0.5f * hitRecoilAmplitude, -0.5f * hitRecoilAmplitude, 0.5f * hitRecoilAmplitude);
            Util.PlaySound(hitSoundString, base.gameObject);
            if (!this.isInHitPause)
            {
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "UtilityRoll.playbackRate");
                this.hitPauseTimer = hitLagDuration;
                this.isInHitPause = true;
            }
        }

        protected virtual void PlayAnim()
        {
            base.PlayCrossfade("FullBody, Override", "UtilityRoll", "UtilityRoll.playbackRate", attackSpeedStat, 0.1f);
        }

        private float GetDamageBoostFromSpeed()
        {
            return Mathf.Max(1f, base.characterBody.moveSpeed / base.characterBody.baseMoveSpeed);
        }

        private void UpdateDirection()
        {
            if (base.inputBank)
            {
                Vector2 vector = Util.Vector3XZToVector2XY(base.inputBank.moveVector);
                if (vector != Vector2.zero)
                {
                    vector.Normalize();
                    this.idealDirection = new Vector3(vector.x, 0f, vector.y).normalized;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (base.fixedAge <= minimumDuration && !buttonReleased)
            {
                return InterruptPriority.PrioritySkill;
            }
            return InterruptPriority.Any;
        }
        public virtual void ReturnToMain()
        {
            outer.SetNextStateToMain();
        }
    }
}
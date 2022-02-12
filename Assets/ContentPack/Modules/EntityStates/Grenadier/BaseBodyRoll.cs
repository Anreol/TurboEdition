using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier
{
    public class BaseBodyRoll : BaseSkillState
    {
        [SerializeField]
        public float maximumRollDuration;
        [SerializeField]
        public float minimumBaseDuration;
        [SerializeField]
        public float airControlOverride;

        [SerializeField]
        public float smallHopVelocity;
        [SerializeField]
        public float rollingTimeExtensionWhenAttacking;

        //Overlap attack related
        [SerializeField]
        public float baseOverlapAttackRefreshDelay;
        [SerializeField]
        public float baseOverlapAttackCoefficient;
        [SerializeField]
        public GameObject hitEffectPrefab;
        [SerializeField]
        public string hitboxString;
        [SerializeField]
        private float overlapAttackDownwardsForce;

        //Animation and Hitlag
        [SerializeField]
        public float baseAnimationPlaySpeed;
        [SerializeField]
        public float baseHitPauseDuration;

        [SerializeField]
        public string hitSoundString;

        private Animator animator;
        private OverlapAttack overlapAttack;
        private float rollingTime;

        private float previousAirControl;
        private HitStopCachedState hitStopCachedState;
        private float hitPauseTimer;
        private float overlapTimer;
        private bool isInHitPause;

        private List<HurtBox> victims = new List<HurtBox>();

        private float minimumDuration
        {
            get
            {
                return minimumBaseDuration / this.attackSpeedStat;
            }
        }
        private float overlapRefreshDelay
        {
            get
            {
                return baseOverlapAttackRefreshDelay / this.attackSpeedStat;
            }
        }
        private float animationPlaySpeed
        {
            get
            {
                return baseAnimationPlaySpeed / this.attackSpeedStat;
            }
        }

        private float hitLagDuration
        {
            get
            {
                return baseHitPauseDuration / this.attackSpeedStat;
            }
        }

        private float GetDamageBoostFromSpeed()
        {
            return Mathf.Max(1f, base.characterBody.moveSpeed / base.characterBody.baseMoveSpeed);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            ResetOverlap();
            if (base.characterDirection && base.inputBank)
            {
                base.characterDirection.forward = base.inputBank.aimDirection;
            }
            //Very important to do
            previousAirControl = characterMotor.airControl;
            PlayAnim();
        }

        protected virtual void PlayAnim()
        {
            base.PlayCrossfade("FullBody, Override", "UtilityRoll", "UtilityRoll.playbackRate", animationPlaySpeed, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //this.hitPauseTimer -= Time.fixedDeltaTime;
            this.rollingTime += Time.fixedDeltaTime;
            this.overlapTimer -= Time.fixedDeltaTime;
            if (base.isAuthority)
            {
                ModifyOverlapAttack(overlapAttack);
                if (overlapAttack.Fire(victims))
                {
                    overlapTimer = overlapRefreshDelay;
                    base.SmallHop(characterMotor, smallHopVelocity);
                    HitSuccessful(victims);
                }
                if (isInHitPause)
                {
                    base.fixedAge -= Time.fixedDeltaTime;
                    this.rollingTime += Time.fixedDeltaTime;
                    this.overlapTimer += Time.fixedDeltaTime;
                    this.hitPauseTimer -= Time.fixedDeltaTime;
                    base.characterMotor.velocity = Vector3.zero;
                    this.animator.SetFloat("UtilityRoll.playbackRate", 0f);
                }
                if (this.hitPauseTimer <= 0f && this.isInHitPause)
                {
                    base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                    base.characterMotor.Motor.ForceUnground();
                    this.isInHitPause = false;
                }
                if (rollingTime >= maximumRollDuration && fixedAge >= minimumDuration)
                {
                    this.outer.SetNextStateToMain();
                }
            }
        }

        public virtual void ModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            if (overlapTimer <= overlapRefreshDelay)
            {
                ResetOverlap();
            }

        }
        public virtual void ResetOverlap()
        {
            this.overlapAttack = base.InitMeleeOverlap(baseOverlapAttackCoefficient, hitEffectPrefab, base.GetModelTransform(), hitboxString);
            this.overlapAttack.forceVector = Vector3.down * overlapAttackDownwardsForce;
        }
        public virtual void HitSuccessful(List<HurtBox> victims)
        {
            rollingTime -= rollingTimeExtensionWhenAttacking;
            characterMotor.airControl = airControlOverride;
            Util.PlaySound(hitSoundString, base.gameObject);
            if (!this.isInHitPause)
            {
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "UtilityRoll.playbackRate");
                this.hitPauseTimer = hitLagDuration;
                this.isInHitPause = true;
            }
        }

        public override void OnExit()
        {
            characterMotor.airControl = previousAirControl;
            base.OnExit();
        }
    }
}
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Roll
{
    internal class UtilityRollLoopAerial : BaseBodyRollSingle
    {
        public static bool shouldUngroundIfGrounded;
        public static bool shouldApplyAimBoostIfAirborne;

        [Tooltip("Should it apply ungroundSmallHopForwardVel when entering this state if already airborne.")]
        public static bool shouldApplyForwardBoostIfAirborne;

        [Tooltip("Should it deal damage and then exit to the main state if touching the ground.")]
        public static bool shouldDamageAndExitIfGrounding;

        [Tooltip("Minimum Y velocity to apply whenever entering the state.")]
        public static float minimumYVel;

        [Tooltip("Velocity to apply ONLY forward, gets multiplied by movement speed.")]
        public static float forwardVel;

        [Tooltip("Velocity to apply from the Aim Vector.")]
        public static float aimVel;

        [Tooltip("Air control to set during this state.")]
        [SerializeField]
        public float newAirControl;

        [Tooltip("Air Control to be set after hitting an enemy.")]
        [SerializeField]
        public float improvedAirControl;

        [Tooltip("Evaluation ramp that will evaluate improvedAirControl.")]
        [SerializeField]
        public AnimationCurve improvedAirControlCurve;

        [SerializeField]
        [Tooltip("The frequency (1/time) at which the overlap attack is reset. Higher values means more frequent ticks of damage.")]
        public float overlapAttackResetFrequency;

        [SerializeField]
        [Tooltip("Should overlapAttackResetFrequency scale with attack speed.")]
        public bool overlapAttackScaleFrequencyAttackSpeed;

        [Tooltip("Time to freeze when a hit is performed.")]
        [SerializeField]
        public float baseHitPauseDuration;

        [Tooltip("How much each hit should they extend the duration of the state.")]
        [SerializeField]
        public float hitStateDurationExtention;

        private float hitPauseDuration => baseHitPauseDuration / this.attackSpeedStat;
        private float resetFrequency => overlapAttackScaleFrequencyAttackSpeed ? overlapAttackResetFrequency * attackSpeedStat : overlapAttackResetFrequency;

        internal override float calculatedDuration { get => baseDuration + (hitStateDurationExtention * overlapAttackTicks); }

        private Animator cachedAnimator;
        private float previousAirControl;

        private bool isInHitPause;
        private float hitPauseTimer;
        private HitStopCachedState hitStopCachedState;

        private float resetStopwatch;
        private float timeSinceLastHit;

        public override void OnEnter()
        {
            base.OnEnter();
            cachedAnimator = base.GetModelAnimator();

            Vector3 direction = base.GetAimRay().direction;
            if (isAuthority)
            {
                direction.y = Mathf.Max(direction.y, minimumYVel);
                Vector3 aimBoost = direction.normalized * aimVel;
                Vector3 forwardBoost = new Vector3(direction.x, 0f, direction.z).normalized * forwardVel * (this.moveSpeedStat / 4);

                if (base.characterMotor.Motor.GroundingStatus.IsStableOnGround && shouldUngroundIfGrounded)
                {
                    base.characterMotor.Motor.ForceUnground();
                    base.characterMotor.velocity = aimBoost + forwardBoost;
                }
                if (!characterMotor.Motor.GroundingStatus.IsStableOnGround)
                {
                    base.characterMotor.velocity = (shouldApplyForwardBoostIfAirborne ? forwardBoost : new Vector3(direction.x, 0f, direction.z).normalized) + (shouldApplyAimBoostIfAirborne ? aimBoost : new Vector3(0, direction.y, 0));
                }
                if (shouldDamageAndExitIfGrounding)
                {
                    characterMotor.onHitGroundAuthority += onHitGroundAuthority;
                }
                //Air control
                previousAirControl = characterMotor.airControl;
                characterMotor.airControl = newAirControl;
                base.characterMotor.disableAirControlUntilCollision = false;

                base.characterBody.isSprinting = true;
            }
            //Fall damage negation
            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
        }

        public override void OnExit()
        {
            base.OnExit();
            if (shouldDamageAndExitIfGrounding)
            {
                characterMotor.onHitGroundAuthority -= onHitGroundAuthority;
            }
            if (isAuthority)
            {
                characterMotor.airControl = previousAirControl;
            }

            //Fall damage recover
            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
        }

        public override void OnExitNextFrameAuthority()
        {
            if (isInHitPause)
            {
                base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.cachedAnimator);
                isInHitPause = false;
            }
            base.OnExitNextFrameAuthority();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
            {
                if (overlapAttackResetFrequency > 0f)
                {
                    this.resetStopwatch -= Time.fixedDeltaTime;
                    if (resetStopwatch <= 0f)
                    {
                        resetStopwatch = 1f / resetFrequency;
                        ResetOverlap();
                    }
                }

                if (isInHitPause)
                {
                    base.fireTimer += Time.fixedDeltaTime; //Don't decrease timer as we are frozen.
                    base.fixedAge -= Time.fixedDeltaTime; //Don't increase age because we're frozen.
                    this.resetStopwatch -= Time.fixedDeltaTime; //Don't increase attack reset time because we're frozen.
                    this.hitPauseTimer -= Time.fixedDeltaTime;
                    base.characterMotor.velocity = Vector3.zero;
                    this.cachedAnimator.SetFloat("UtilityRoll.playbackRate", 0f);
                }
                else
                {
                    timeSinceLastHit += Time.fixedDeltaTime;
                    if (overlapAttackTicks > 0)
                    {
                        float ac = improvedAirControl * improvedAirControlCurve.Evaluate(timeSinceLastHit);
                        characterMotor.airControl = ac > 0 ? ac : newAirControl;
                    }
                }

                if (this.hitPauseTimer <= 0f && this.isInHitPause)
                {
                    base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.cachedAnimator);
                    base.characterMotor.Motor.ForceUnground(); //Just in case, I guess.
                    this.isInHitPause = false;
                }
            }
        }

        private void onHitGroundAuthority(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            base.exitNextFrame = true;
            if (hitGroundInfo.position != null)
            {
                //Reset.
                ResetOverlap();
                //Do this instead of calling base modify attack, as the character speed might be already zero.
                overlapAttack.damage = Mathf.Max(damageStat * baseOverlapAttackCoefficient, damageStat * (baseOverlapAttackCoefficient * Mathf.Max(1f, hitGroundInfo.velocity.sqrMagnitude / base.characterBody.baseMoveSpeed)));
                if (overlapAttack.Fire(victims))
                {
                    HitSuccessful(victims);
                }
            }
        }

        public override void HitSuccessful(List<HurtBox> victims)
        {
            base.HitSuccessful(victims);
            if (!this.isInHitPause)
            {
                timeSinceLastHit = 0;
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.cachedAnimator, "UtilityRoll.playbackRate");
                this.hitPauseTimer = hitPauseDuration;
                this.isInHitPause = true;
            }
        }

        public override Vector3? UpdateDirection()
        {
            return isInHitPause ? null : base.UpdateDirection();
        }

        public override void OnLifetimeExpiredAuthority()
        {
            outer.SetNextStateToMain();

            /*UtilityRollLoopAerial utilityRollLoopAerial = new UtilityRollLoopAerial();

            //Append the currently hit victims to the next state, making them not get hit til next reset.
            foreach (var item in base.overlapAttack.ignoredHealthComponentList)
            {
                utilityRollLoopAerial.overlapAttack.ignoredHealthComponentList.Add(item);
            }
            //Hitstop stuff
            utilityRollLoopAerial.isInHitPause = isInHitPause;
            utilityRollLoopAerial.hitStopCachedState = hitStopCachedState;
            utilityRollLoopAerial.hitPauseTimer = hitPauseTimer;
            outer.SetNextState(utilityRollLoopAerial);*/
        }
    }
}
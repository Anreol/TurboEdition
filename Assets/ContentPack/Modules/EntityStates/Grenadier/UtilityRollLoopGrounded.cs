using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Roll
{
    internal class UtilityRollLoopGrounded : BaseBodyRollSingle
    {
        public bool shouldUngroundIfGrounded;

        [Tooltip("Should it apply ungroundSmallHopForwardVel when entering this state if already airborne.")]
        public bool shouldApplyForwardBoostIfAirborne;

        [Tooltip("Should it deal damage and then exit to the main state if touching the ground.")]
        public bool shouldDamageAndExitIfGrounding;

        [Tooltip("Vertical Velocity to apply when entering this state if ungrounding.")]
        [SerializeField]
        public float ungroundSmallHopYVel;

        [Tooltip("Forward Velocity to apply when entering this state if ungrounding.")]
        [SerializeField]
        public float ungroundSmallHopForwardVel;

        [Tooltip("Air control to set during this state.")]
        [SerializeField]
        public float newAirControl;

        [SerializeField]
        [Tooltip("The frequency (1/time) at which the overlap attack is reset. Higher values means more frequent ticks of damage.")]
        public float overlapAttackResetFrequency;

        [Tooltip("Time to freeze when a hit is performed.")]
        [SerializeField]
        public float baseHitPauseDuration;

        private float hitPauseDuration => baseHitPauseDuration / this.attackSpeedStat;
        private float resetFrequency => overlapAttackResetFrequency * attackSpeedStat;

        internal override float calculatedDuration { get => calculatedDuration * overlapAttackTicks; }

        private Animator cachedAnimator;
        private float previousAirControl;

        private bool isInHitPause;
        private float hitPauseTimer;
        private HitStopCachedState hitStopCachedState;

        private float resetStopwatch;

        public override void OnEnter()
        {
            base.OnEnter();
            cachedAnimator = base.GetModelAnimator();
            if (isAuthority)
            {
                if (base.characterMotor.Motor.GroundingStatus.IsStableOnGround && shouldUngroundIfGrounded)
                {
                    base.characterMotor.Motor.ForceUnground();
                    base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, Mathf.Max(base.characterMotor.velocity.y, ungroundSmallHopYVel), base.characterMotor.velocity.z) + (characterDirection.moveVector * ungroundSmallHopForwardVel);
                }
                else if (shouldApplyForwardBoostIfAirborne)
                {
                    base.characterMotor.ApplyForce(base.inputBank.moveVector * ungroundSmallHopForwardVel, true, false);
                }
                if (shouldDamageAndExitIfGrounding)
                {
                    base.characterMotor.onMovementHit += onMovementHit;
                    characterMotor.onHitGroundAuthority += onHitGroundAuthority;
                }
                //Air control
                previousAirControl = characterMotor.airControl;
                characterMotor.airControl = newAirControl;
                base.characterMotor.disableAirControlUntilCollision = false;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (shouldDamageAndExitIfGrounding)
            {
                base.characterMotor.onMovementHit -= onMovementHit;
                characterMotor.onHitGroundAuthority -= onHitGroundAuthority;
            }
            if (isAuthority)
            {
                characterMotor.airControl = previousAirControl;
            }
        }

        public override void ReturnToMain()
        {
            base.ReturnToMain();
            if (isInHitPause)
            {
                base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.cachedAnimator);
                isInHitPause = false;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
            {
                if (overlapAttackResetFrequency >= 0f)
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
                    base.fixedAge -= Time.fixedDeltaTime; //Don't increase age because we're frozen.
                    this.resetStopwatch -= Time.fixedDeltaTime; //Don't increase attack reset time because we're frozen.
                    this.hitPauseTimer -= Time.fixedDeltaTime;
                    base.characterMotor.velocity = Vector3.zero;
                    this.cachedAnimator.SetFloat("UtilityRoll.playbackRate", 0f);
                }

                if (this.hitPauseTimer <= 0f && this.isInHitPause)
                {
                    base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.cachedAnimator);
                    base.characterMotor.Motor.ForceUnground(); //Just in case, I guess.
                    this.isInHitPause = false;
                }
            }
        }

        private void onMovementHit(ref RoR2.CharacterMotor.MovementHitInfo movementHitInfo)
        {
            base.exitNextFrame = true;
            if (movementHitInfo.hitCollider)
            {
                //Reset.
                ResetOverlap();
                //Do this instead of calling base modify attack, as the character speed might be already zero.
                overlapAttack.damage = Mathf.Max(damageStat * baseOverlapAttackCoefficient, damageStat * (baseOverlapAttackCoefficient * Mathf.Max(1f, movementHitInfo.velocity.sqrMagnitude / base.characterBody.baseMoveSpeed)));
                if (overlapAttack.Fire(victims))
                {
                    HitSuccessful(victims);
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
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.cachedAnimator, "UtilityRoll.playbackRate");
                this.hitPauseTimer = hitPauseDuration;
                this.isInHitPause = true;
            }
        }

        public override Vector3? UpdateDirection()
        {
            return isInHitPause ? null : base.UpdateDirection();
        }

        public override BaseBodyRollSingle GetNextState()
        {
            UtilityRollLoopGrounded utilityRollLoopAerial = new UtilityRollLoopGrounded();
            //Init overlap attack.
            utilityRollLoopAerial.ResetOverlap();
            //Append the currently hit victims to the next state, making them not get hit til next reset.
            foreach (var item in base.overlapAttack.ignoredHealthComponentList)
            {
                utilityRollLoopAerial.overlapAttack.ignoredHealthComponentList.Add(item);
            }
            //Hitstop stuff
            utilityRollLoopAerial.isInHitPause = isInHitPause;
            utilityRollLoopAerial.hitStopCachedState = hitStopCachedState;
            utilityRollLoopAerial.hitPauseTimer = hitPauseTimer;
            return utilityRollLoopAerial;
        }
    }
}
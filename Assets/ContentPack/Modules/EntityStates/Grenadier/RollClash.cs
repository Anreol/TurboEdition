using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Roll
{
    internal class RollClash : BaseRoll
    {
        [Tooltip("Should the hitstop duration scale with damage or not.")]
        [SerializeField]
        public bool hitstopDamageScale = false;

        [Tooltip("The value which will be multiplied by the base damage, and will be represented as currentHitstopDuration.")]
        [SerializeField]
        public float hitstopDamageForMinStopCoeff = 2;

        [Tooltip("The value which will be multiplied by the base damage, and will be represented as hitstopMaxDuration.")]
        [SerializeField]
        public float hitstopDamageForMaxStopCoeff = 5;

        [Tooltip("The value which will be the maximum possible duration of the hitstop when damage scaling.")]
        [SerializeField]
        public float hitstopDamageMaxDuration = 2;

        [Tooltip("The frequency (1/time) at which the overlap attack is fired. Higher values means more frequent ticks of damage.")]
        public static float attackFireFrequency = 1;

        [Tooltip("The frequency (1/time) at which the overlap attack is reset and the character can hit again enemies already hit. Higher values means more resets.")]
        public static float attackResetFrequency = 1;

        [Tooltip("The push away force applied to the targets when hit.")]
        public static float attackPushAwayForce = 20;

        [Tooltip("Amount of enemies to hit with a single attack.")]
        public static int attackMaxTargetsAtOnce = 1;

        [Tooltip("VFX to use when the overlap attack hits.")]
        public static GameObject hitEffectPrefab;

        [Tooltip("Minimum amount of the aim direction's Y to use, to not let the user slam himself to the ground.")]
        public static float minimumAimDirectionY;

        [Tooltip("The value which the normalized aim direction will be multiplied by and applied to the character, only if they are moving.")]
        public static float aimDirectionVelocity;

        [Tooltip("The value which the normalized aim direction will be multiplied by and applied to the character, without the Y, only if they are moving.")]
        public static float aimDirectionBonusHorizontalVelocity;

        [Tooltip("Amount of upwards velocity to apply to the character on enter.")]
        public static float upwardVelocity;

        [Tooltip("Amount of forward velocity to apply to the character on enter, only if they are moving.")]
        public static float forwardVelocity;

        internal override float currentHitstopDuration => hitstopDamageScale ? Mathf.Max(Util.Remap(currentDamage, currentDamage * hitstopDamageForMinStopCoeff, currentDamage * hitstopDamageForMaxStopCoeff, hitstopDuration, hitstopDamageMaxDuration), hitstopDuration) : hitstopDuration;
        internal override float currentAirControlCurveEval => fixedAge / 4;

        //Attack stuff
        internal OverlapAttack overlapAttack = null;

        private bool hasMovementHit = false;
        private float fireTimer;
        private float resetTimer;

        /// <summary>
        /// The hit results get saved here. Just to don't create a new list constantly.
        /// </summary>
        internal List<HurtBox> victims = new List<HurtBox>();

        public override void OnEnter()
        {
            base.OnEnter();
            Vector3 direction = base.GetAimRay().direction;
            if (base.isAuthority)
            {
                characterMotor.onMovementHit += onMovementHit;
                base.characterBody.isSprinting = true;
                //Do only Y movement
                if (characterBody.characterMotor.moveDirection.x == 0f && characterBody.characterMotor.moveDirection.z == 0f)
                {
                    Vector3 b = Vector3.up * upwardVelocity;
                    base.characterMotor.Motor.ForceUnground();
                    base.characterMotor.velocity += b;
                }
                else
                {
                    //Do movement with all axis
                    //Avoid slamming the player into the ground if already grounded
                    if (characterMotor.isGrounded)
                    {
                        minimumAimDirectionY = 0.05f;
                    }
                    direction.y = Mathf.Max(direction.y, minimumAimDirectionY);
                    Vector3 a = ((direction.normalized * aimDirectionVelocity) + new Vector3(direction.normalized.x * aimDirectionBonusHorizontalVelocity, 0, direction.normalized.z * aimDirectionBonusHorizontalVelocity)) * this.moveSpeedStat / characterBody.baseMoveSpeed;
                    Vector3 b = Vector3.up * upwardVelocity;
                    Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * forwardVelocity;
                    base.characterMotor.Motor.ForceUnground();
                    base.characterMotor.velocity = a + b + b2;
                }
            }
            overlapAttack = base.InitMeleeOverlap(damageCoefficient, hitEffectPrefab, base.modelLocator.modelTransform, "RollHitbox");
            overlapAttack.pushAwayForce = attackPushAwayForce;
            overlapAttack.maximumOverlapTargets = attackMaxTargetsAtOnce;
        }

        public override void AuthorityFixedUpdate()
        {
            base.AuthorityFixedUpdate();

            if (overlapAttack != null)
            {
                //Reset attack before firing
                resetTimer -= Time.fixedDeltaTime;
                if (resetTimer <= 0f)
                {
                    resetTimer = 1f / attackResetFrequency;
                    ResetOverlap();
                }
                //Do attack, with a timer to dont lag the hell out of the game.
                fireTimer -= Time.fixedDeltaTime;
                if (fireTimer <= 0f || hasMovementHit)
                {
                    finishOnNextHitstopExit = hasMovementHit;
                    hasMovementHit = false;
                    fireTimer = 1f / attackFireFrequency;

                    //The damage might be outdated since the attack got initialized, update.
                    AuthorityModifyOverlapAttack(overlapAttack);
                    if (overlapAttack.Fire(victims))
                    {
                        AuthoritySuccessfulHit(victims);
                        return;
                    }
                }
            }
        }

        protected override void AuthorityOnFinish()
        {
            base.AuthorityOnFinish();
            characterMotor.onMovementHit -= onMovementHit;
        }

        public virtual void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            overlapAttack.damage = currentDamage * GetDamageBoostFromSpeed();
        }

        private void onMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            hasMovementHit = true;
        }

        public override void AuthoritySuccessfulHit(List<HurtBox> victims)
        {
            base.AuthoritySuccessfulHit(victims);
            finishOnNextHitstopExit = true;
        }

        /// <summary>
        /// Clears the already hit health components.
        /// </summary>
        public virtual void ResetOverlap()
        {
            overlapAttack?.ResetIgnoredHealthComponents();
            victims?.Clear();
        }
    }
}
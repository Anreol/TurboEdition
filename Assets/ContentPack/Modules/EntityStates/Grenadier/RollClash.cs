using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier
{
    internal class RollClash : BaseRoll
    {
        [Tooltip("Number to multiply the current damage by, and gets used for the hitstop duration. Set to zero or less to disable hitstop damage scaling.")]
        [SerializeField]
        public float hitstopDamageScaleCoeff = 1;

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

        [Tooltip("The frequency (1/time) at which the overlap attack is reset. Higher values means more resets.")]
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

        [Tooltip("Amount of upwards velocity to apply to the character on enter.")]
        public static float upwardVelocity;

        [Tooltip("Amount of forward velocity to apply to the character on enter, only if they are moving.")]
        public static float forwardVelocity;

        internal override float currentHitstopDuration => hitstopDamageScaleCoeff > 0 ? base.currentHitstopDuration : Util.Remap(currentDamage * hitstopDamageScaleCoeff, currentDamage * hitstopDamageForMinStopCoeff, currentDamage * hitstopDamageForMaxStopCoeff, base.currentHitstopDuration, hitstopDamageMaxDuration);

        //Attack stuff
        internal OverlapAttack overlapAttack = null;

        private bool hasClashedOrSuccessfullyAttacked = false;
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
                if (characterBody.characterMotor.moveDirection.x > 0 || characterBody.characterMotor.moveDirection.z > 0)
                {
                    direction.y = Mathf.Max(direction.y, minimumAimDirectionY);
                    Vector3 b = Vector3.up * upwardVelocity;
                    base.characterMotor.Motor.ForceUnground();
                    base.characterMotor.velocity += b;
                }
                else
                {
                    //Do movement with all axis
                    direction.y = Mathf.Max(direction.y, minimumAimDirectionY);
                    Vector3 a = direction.normalized * aimDirectionVelocity * this.moveSpeedStat / characterBody.baseMoveSpeed;
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


        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
            {
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
                    if (fireTimer <= 0f)
                    {
                        fireTimer = 1f / attackFireFrequency;

                        //The damage might be outdated since the attack got initialized, update.
                        ModifyOverlapAttack(overlapAttack);
                        if (overlapAttack.Fire(victims))
                        {
                            HandleSuccessfulHit(victims);
                        }
                    }
                }
            }
            if (hasClashedOrSuccessfullyAttacked)
            {
                outer.SetNextStateToMain();
            }
        }

        public virtual void ModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            overlapAttack.damage = Mathf.Max(currentDamage, currentDamage * GetDamageBoostFromSpeed());
        }
        private void onMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            hasClashedOrSuccessfullyAttacked = true;
            //Do an extra attack since we are exiting...?
            ModifyOverlapAttack(overlapAttack);
            if (overlapAttack.Fire(victims))
            {
                HandleSuccessfulHit(victims);
            }
        }
        public override void HandleSuccessfulHit(List<HurtBox> victims)
        {
            base.HandleSuccessfulHit(victims);
            hasClashedOrSuccessfullyAttacked = true;
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
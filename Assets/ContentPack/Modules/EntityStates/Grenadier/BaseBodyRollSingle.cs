using EntityStates;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier
{
    public abstract class BaseBodyRollSingle : BaseSkillState
    {
        [Tooltip("Base Duration, scale or not, depends on the states inheriting from base.")]
        [SerializeField]
        public float baseDuration;

        [Tooltip("Minimum Duration, scales with attack speed.")]
        [SerializeField]
        public float baseMinDuration = 0f;

        [Tooltip("Soundbank event to play when entering state.")]
        [SerializeField]
        public string enterSoundString;

        [Tooltip("The character's Base Damage will be multiplied by this. It also gets multiplied by the Speed Bonus, before multiplying by the Base Damage")]
        [SerializeField]
        public float baseOverlapAttackCoefficient;

        [Tooltip("Push Away Force of the overlap attack.")]
        [SerializeField]
        public float baseOverlapPushAwayForce;

        [Tooltip("The frequency (1/time) at which the overlap attack is fired. Higher values means more frequent ticks of damage.")]
        [SerializeField]
        public float baseOverlapAttackFireFrequency;

        [Tooltip("Should the fire frequency scale with attack speed.")]
        [SerializeField]
        public bool scaleFireFrequencyWithAttackSpeed;

        [Tooltip("Should the fire timer get reset whenever a succesful hit is done.")]
        [SerializeField]
        public bool hitResetOverlapFireTimer;

        [Tooltip("Effect prefab that will play when performing a hit.")]
        [SerializeField]
        public GameObject hitEffectPrefab;

        [Tooltip("Soundbank event to play when performing a hit.")]
        [SerializeField]
        public string hitSoundString;

        [Tooltip("Hitbox to search for for the overlap attack.")]
        [SerializeField]
        public string hitboxString;

        [Tooltip("Vertical Velocity to apply when performing a hit.")]
        [SerializeField]
        public float hitSmallHopYVel;

        [Tooltip("How many targets can be hit at once.")]
        [SerializeField]
        public int hitMaximumTargetsAtOnce;

        [Tooltip("Crosshair recoil to add when performing a hit.")]
        [SerializeField]
        public float hitRecoilAmplitude;

        internal OverlapAttack overlapAttack = null;

        /// <summary>
        /// The hit results get saved here. Just to don't create a new list constantly.
        /// </summary>
        internal List<HurtBox> victims;

        /// <summary>
        /// Should the skill be cancelled as soon as possible.
        /// </summary>
        internal bool exitNextFrame;

        internal abstract float calculatedDuration { get; }
        internal virtual float overlapAttackFireFrequency => scaleFireFrequencyWithAttackSpeed ? baseOverlapAttackFireFrequency * attackSpeedStat : baseOverlapAttackFireFrequency;
        private float minimumDuration => baseMinDuration / this.attackSpeedStat;

        internal int overlapAttackTicks;
        internal float fireTimer;

        public override void OnEnter()
        {
            base.OnEnter();
            //PlayAnim(); //Fucking crashes the game if there's no animation.
            //If it hasnt been initialized...
            if (overlapAttack == null)
            {
                overlapAttack = base.InitMeleeOverlap(damageStat * baseOverlapAttackCoefficient, hitEffectPrefab, base.GetModelTransform(), hitboxString);
                overlapAttack.pushAwayForce = baseOverlapPushAwayForce;
                overlapAttack.maximumOverlapTargets = hitMaximumTargetsAtOnce;
            }

            Util.PlaySound(enterSoundString, base.gameObject);
            if (base.characterDirection && isAuthority)
            {
                base.characterDirection.forward = UpdateDirection() ?? base.characterDirection.forward;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //base.characterMotor.moveDirection = base.inputBank.moveVector;

            if (isAuthority)
            {
                base.characterDirection.forward = UpdateDirection() ?? base.characterDirection.forward;
                //Do attack, with a timer to dont lag the hell out of the game.
                fireTimer -= Time.fixedDeltaTime;
                if (fireTimer <= 0f && overlapAttack != null)
                {
                    fireTimer = 1f / overlapAttackFireFrequency;

                    //The damage might be outdated since the attack got initialized, update.
                    ModifyOverlapAttack(overlapAttack);
                    if (overlapAttack.Fire(victims))
                    {
                        HitSuccessful(victims);
                    }
                }
                if (fixedAge >= calculatedDuration)
                {
                    BaseBodyRollSingle baseBodyRoll = GetNextState();
                    this.outer.SetNextState(baseBodyRoll);
                }
                if (base.fixedAge >= this.minimumDuration && this.exitNextFrame)
                {
                    ReturnToMain();
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public virtual void ModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            float damage = Mathf.Max(damageStat * baseOverlapAttackCoefficient, damageStat * (baseOverlapAttackCoefficient * GetDamageBoostFromSpeed()));
            overlapAttack.damage = damage;
        }

        /// <summary>
        /// Clears the already hit health components.
        /// </summary>
        public virtual void ResetOverlap()
        {
            this.overlapAttack?.ResetIgnoredHealthComponents();
        }

        /// <summary>
        /// Gets called ONCE every single overlap attack.
        /// </summary>
        /// <param name="victims"></param>
        public virtual void HitSuccessful(List<HurtBox> victims)
        {
            overlapAttackTicks++;
            if (hitResetOverlapFireTimer)
                fireTimer = 1f / overlapAttackFireFrequency;
            base.SmallHop(characterMotor, hitSmallHopYVel);
            base.AddRecoil(-0.5f * hitRecoilAmplitude, -0.5f * hitRecoilAmplitude, -0.5f * hitRecoilAmplitude, 0.5f * hitRecoilAmplitude);
            Util.PlaySound(hitSoundString, base.gameObject);
        }

        /// <summary>
        /// Plays the animation, can be overriden.
        /// </summary>
        protected virtual void PlayAnim()
        {
            base.PlayCrossfade("FullBody, Override", "UtilityRoll", "UtilityRoll.playbackRate", calculatedDuration, 0.1f);
        }

        /// <summary>
        /// Same as MUL-Ts Transport mode.
        /// </summary>
        /// <returns>Returns current move speed divided by the base movement speed or 1.</returns>
        internal virtual float GetDamageBoostFromSpeed()
        {
            return Mathf.Max(1f, base.characterBody.moveSpeed / base.characterBody.baseMoveSpeed);
        }

        /// <summary>
        /// Updates Ideal Direction to the current aim vector.
        /// </summary>
        public virtual Vector3? UpdateDirection()
        {
            if (base.inputBank)
            {
                Vector2 vector = Util.Vector3XZToVector2XY(base.inputBank.aimDirection);
                if (vector != Vector2.zero)
                {
                    vector.Normalize();
                    return new Vector3(vector.x, 0f, vector.y).normalized;
                }
            }
            return null;
        }

        /// <summary>
        /// Makes sure that the skill lasts the minimum duration before being able to get cancelled.
        /// </summary>
        /// <returns>Returns Priority Skill if the fixed age is lower than minimum duration, else any.</returns>
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (base.fixedAge <= minimumDuration)
            {
                return InterruptPriority.PrioritySkill;
            }
            return InterruptPriority.Any;
        }

        /// <summary>
        /// Returns to the default state. Can ve overriden.
        /// </summary>
        public virtual void ReturnToMain()
        {
            outer.SetNextStateToMain();
        }

        /// <summary>
        /// Called whenever a single hit is performed.
        /// </summary>
        /// <returns></returns>
        public abstract BaseBodyRollSingle GetNextState();
    }
}
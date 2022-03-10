using EntityStates;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier
{
    //A proper entity state would have both the loop and single states inherit from a base, but i cant be bothered to do that
    public abstract class BaseBodyRollSingle : BaseSkillState
    {
        [SerializeField]
        public float baseDuration;

        [SerializeField]
        public float baseMinDuration = 0f;

        [SerializeField]
        public float groundedSmallHopVelocity;

        [SerializeField]
        public float airFowardForceStrength;

        [SerializeField]
        public string enterSoundString;

        [SerializeField]
        public float baseOverlapAttackCoefficient;

        [SerializeField]
        public GameObject hitEffectPrefab;

        [SerializeField]
        public string hitboxString;

        [SerializeField]
        public float overlapAttackSmallHopVelocity;

        [SerializeField]
        public string hitSoundString;

        [SerializeField]
        public float hitMaximumTargetsAtOnce;

        [SerializeField]
        public float hitRecoilAmplitude;

        private bool buttonReleased;
        private OverlapAttack overlapAttack;
        private Vector3 idealDirection;
        private List<HurtBox> victims;
        private bool exitNextFrame;

        private float duration
        {
            get
            {
                return baseDuration / this.attackSpeedStat;
            }
        }

        private float minimumDuration
        {
            get
            {
                return baseMinDuration / this.attackSpeedStat;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnim();
            ResetOverlap();
            Util.PlaySound(enterSoundString, base.gameObject);
            if (base.characterMotor && isAuthority)
            {
                characterMotor.onMovementHit += onMovementHit;
                if (base.inputBank)
                {
                    this.idealDirection = base.inputBank.aimDirection;
                    this.idealDirection.y = 0f;
                }
                this.UpdateDirection();
                if (base.characterMotor.Motor.GroundingStatus.IsStableOnGround)
                {
                    base.characterMotor.Motor.ForceUnground();
                    base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, Mathf.Max(base.characterMotor.velocity.y, groundedSmallHopVelocity), base.characterMotor.velocity.z);
                }
                else
                {
                    base.characterMotor.ApplyForce(base.inputBank.moveVector * airFowardForceStrength, true, false);
                }
                base.characterMotor.disableAirControlUntilCollision = false;
            }
            if (base.characterDirection)
            {
                base.characterDirection.forward = this.idealDirection;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterMotor.moveDirection = base.inputBank.moveVector;
            buttonReleased = !buttonReleased && !(base.inputBank && base.inputBank.skill1.down);
            if (base.isAuthority)
            {
                UpdateDirection();
                ModifyOverlapAttack(overlapAttack);
                if (overlapAttack.Fire(victims))
                {
                    HitSuccessful(victims);
                    BaseBodyRollLoop baseBodyRoll = GetNextState();
                    baseBodyRoll.ResetOverlap();
                    foreach (var item in overlapAttack.ignoredHealthComponentList)
                    {
                        baseBodyRoll.overlapAttack.ignoredHealthComponentList.Add(item);
                    }

                    this.outer.SetNextState(baseBodyRoll);
                }
            }
            if (base.fixedAge > this.minimumDuration && (this.exitNextFrame || base.characterMotor.Motor.GroundingStatus.IsStableOnGround))
            {
                ReturnToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (isAuthority)
            {
                characterMotor.onMovementHit -= onMovementHit;
            }
        }
        private void onMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            this.exitNextFrame = true;
        }
        public virtual void ModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            float damage = Mathf.Max(damageStat * baseOverlapAttackCoefficient, damageStat * (baseOverlapAttackCoefficient * GetDamageBoostFromSpeed()));
            overlapAttack.damage = damage;
        }
        public virtual void ResetOverlap()
        {
            float damage = Mathf.Max(damageStat * baseOverlapAttackCoefficient, damageStat * (baseOverlapAttackCoefficient * GetDamageBoostFromSpeed()));
            this.overlapAttack = base.InitMeleeOverlap(damage, hitEffectPrefab, base.GetModelTransform(), hitboxString);
        }
        public virtual void HitSuccessful(List<HurtBox> victims)
        {
            base.SmallHop(characterMotor, overlapAttackSmallHopVelocity);
            base.AddRecoil(-0.5f * hitRecoilAmplitude, -0.5f * hitRecoilAmplitude, -0.5f * hitRecoilAmplitude, 0.5f * hitRecoilAmplitude);
            Util.PlaySound(hitSoundString, base.gameObject);
        }
        protected virtual void PlayAnim()
        {
            base.PlayCrossfade("FullBody, Override", "UtilityRoll", "UtilityRoll.playbackRate", duration, 0.1f);
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
        public abstract BaseBodyRollLoop GetNextState();
    }
}
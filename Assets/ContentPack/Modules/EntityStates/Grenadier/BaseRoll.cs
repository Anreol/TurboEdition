using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Roll
{
    internal abstract class BaseRoll : BasePushMoreState
    {
        [Tooltip("Value which will be multiplied by the current character damage.")]
        [SerializeField]
        public float damageCoefficient;

        [Tooltip("Duration of a hitstop when damage dealt, set to zero or less to disable.")]
        [SerializeField]
        public float hitstopDuration;

        [Tooltip("Should it grant iframes during hitstop.")]
        [SerializeField]
        public bool doIframesDuringHitstop;

        [Tooltip("The value which will be treated as 0.5 for the playback rate")]
        [SerializeField]
        public float animMinValue;

        [Tooltip("The value which will be treated as animMaxPlaybackSpeed.")]
        [SerializeField]
        public float animMaxValue;

        [Tooltip("Maximum speed the animation will play at.")]
        [SerializeField]
        public float animMaxPlaybackSpeed;

        internal virtual float currentHitstopDuration { get => hitstopDuration; }
        internal float currentDamage => damageStat * damageCoefficient;

        protected bool authorityIsInHitStop
        {
            get
            {
                return this.hitStopTimer > 0f;
            }
        }

        internal bool finishOnNextHitstopExit;

        private Animator animator;
        private int attacksSuccessful;
        private HitStopCachedState hitStopCachedState;
        private float hitStopTimer;

        private Transform rollVFXTransform;
        private AnimateShaderAlpha rollVFXAlpha;

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            characterMotor.disableAirControlUntilCollision = false;
            rollVFXTransform = base.FindModelChild("RollVFX");
            if (rollVFXTransform)
            {
                rollVFXTransform.gameObject.SetActive(true);
                rollVFXAlpha = rollVFXTransform.gameObject.GetComponent<AnimateShaderAlpha>();
            }
            base.PlayAnimation("Full Body, Override", "BodyRollLoop");
        }

        public override void Update()
        {
            base.Update();
            float playbackRate = Util.Remap(GetDamageBoostFromSpeed(), animMinValue, animMaxValue, 0.5f, animMaxPlaybackSpeed);
            if (rollVFXAlpha)
            {
                rollVFXAlpha.time = authorityIsInHitStop ? 0 : playbackRate;
            }
            if (!authorityIsInHitStop)
            {
                animator.SetFloat("BodyRollLoop.playbackRate", playbackRate, 0.85f, Time.deltaTime);
                return;
            }
        }

        public override void AuthorityFixedUpdate()
        {
            base.AuthorityFixedUpdate();

            if (authorityIsInHitStop)
            {
                this.hitStopTimer -= Time.fixedDeltaTime;
                if (characterMotor)
                {
                    base.characterMotor.velocity = Vector3.zero;
                }
                base.fixedAge -= Time.fixedDeltaTime; //Do not increase age as we are stopped
                if (!this.authorityIsInHitStop)
                {
                    this.AuthorityExitHitStop();
                    if (finishOnNextHitstopExit)
                    {
                        AuthorityOnFinish();
                    }
                }
            }
            else if (finishOnNextHitstopExit)
            {
                AuthorityOnFinish();
            }
        }

        internal virtual float GetDamageBoostFromSpeed()
        {
            return Mathf.Max(1f, base.characterBody.moveSpeed / base.characterBody.baseMoveSpeed);
        }

        public override void OnExit()
        {
            //Failsafe to don't mess up iframes or anything else
            if (authorityIsInHitStop)
            {
                AuthorityExitHitStop();
            }

            base.OnExit();

            rollVFXTransform?.gameObject.SetActive(false);
            base.PlayAnimation("Full Body, Override", "Empty");
            int layerIndex = animator.GetLayerIndex("Impact");
            if (layerIndex >= 0)
            {
                animator.SetLayerWeight(layerIndex, 2f);
                this.PlayAnimation("Impact", "LightImpact");
            }
        }

        public virtual void AuthoritySuccessfulHit(List<HurtBox> victims)
        {
            attacksSuccessful++;
            if (!this.authorityIsInHitStop && currentHitstopDuration > 0)
            {
                if (base.characterMotor)
                {
                    base.characterMotor.velocity = Vector3.zero;
                }
                if (animator)
                {
                    animator.speed = 0f;
                }

                hitStopCachedState = base.CreateHitStopCachedState(characterMotor, animator, "UtilityRoll.playbackRate");
                if (doIframesDuringHitstop)
                {
                    int hurtBoxesDeactivatorCounter = base.hurtboxGroup.hurtBoxesDeactivatorCounter + 1;
                    base.hurtboxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
                }
                this.hitStopTimer = currentHitstopDuration;
            }
        }

        public virtual void AuthorityExitHitStop()
        {
            hitStopTimer = 0;
            if (this.animator)
            {
                this.animator.speed = 1f;
            }
            base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, animator);
            if (doIframesDuringHitstop)
            {
                int hurtBoxesDeactivatorCounter = base.hurtboxGroup.hurtBoxesDeactivatorCounter - 1;
                base.hurtboxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }
        }

        protected virtual void AuthorityOnFinish()
        {
            this.outer.SetNextStateToMain();
        }
    }
}
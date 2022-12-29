using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier
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

        [Tooltip("The value which will be treated as 0 for the playback rate")]
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

        private int attacksSuccessful;
        private bool isInHitPause;
        private HitStopCachedState hitStopCachedState;
        private float hitPauseTimer;

        private Transform rollVFXTransform;
        private AnimateShaderAlpha rollVFXAlpha;

        public override void OnEnter()
        {
            base.OnEnter();
            characterMotor.disableAirControlUntilCollision = false;
            rollVFXTransform = base.FindModelChild("RollVFX");
            if (rollVFXTransform)
            {
                rollVFXTransform.gameObject.SetActive(true);
                rollVFXAlpha = rollVFXTransform.gameObject.GetComponent<AnimateShaderAlpha>();
            }
            base.PlayAnimation("Full Body, Override", "BodyRollLoop");
        }

        public override void UpdateAnimationParameters()
        {
            base.UpdateAnimationParameters();
            float playbackRate = Util.Remap(GetDamageBoostFromSpeed(), animMinValue, animMaxValue, 0.05f, animMaxPlaybackSpeed);
            if (rollVFXAlpha)
            {
                rollVFXAlpha.time = isInHitPause ? 0 : playbackRate;
            }
            if (!isInHitPause)
            {
                base.modelAnimator.SetFloat("BodyRollLoop.playbackRate", playbackRate, 0.1f, Time.deltaTime);
                return;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.hitPauseTimer -= Time.fixedDeltaTime;

            //Hitstop end
            if (this.hitPauseTimer <= 0f && this.isInHitPause)
            {
                this.isInHitPause = false;
                //Merc has hitstop consumption inside auth check
                if (isAuthority)
                {
                    base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, modelAnimator);
                }
                if (doIframesDuringHitstop)
                {
                    int hurtBoxesDeactivatorCounter = base.hurtboxGroup.hurtBoxesDeactivatorCounter - 1;
                    base.hurtboxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
                }
            }
        }

        internal virtual float GetDamageBoostFromSpeed()
        {
            return Mathf.Max(1f, base.characterBody.moveSpeed / base.characterBody.baseMoveSpeed);
        }

        public override void OnExit()
        {
            if (isInHitPause)
            {
                this.isInHitPause = false;
                //Merc has hitstop consumption inside auth check
                if (isAuthority)
                {
                    base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, modelAnimator);
                }
                if (doIframesDuringHitstop)
                {
                    int hurtBoxesDeactivatorCounter = base.hurtboxGroup.hurtBoxesDeactivatorCounter - 1;
                    base.hurtboxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
                }
            }

            base.OnExit();

            rollVFXTransform?.gameObject.SetActive(false);
            base.PlayAnimation("Full Body, Override", "Empty");
            int layerIndex = base.modelAnimator.GetLayerIndex("Impact");
            if (layerIndex >= 0)
            {
                base.modelAnimator.SetLayerWeight(layerIndex, 2f);
                this.PlayAnimation("Impact", "LightImpact");
            }
        }

        public virtual void HandleSuccessfulHit(List<HurtBox> victims)
        {
            attacksSuccessful++;
            if (!this.isInHitPause && currentHitstopDuration > 0)
            {
                this.isInHitPause = true;
                this.hitPauseTimer = currentHitstopDuration;
                //Merc has hitstop creation inside auth check
                if (isAuthority)
                {
                    hitStopCachedState = base.CreateHitStopCachedState(characterMotor, modelAnimator, "UtilityRoll.playbackRate");
                }
                if (doIframesDuringHitstop)
                {
                    int hurtBoxesDeactivatorCounter = base.hurtboxGroup.hurtBoxesDeactivatorCounter + 1;
                    base.hurtboxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
                }
            }
        }
    }
}
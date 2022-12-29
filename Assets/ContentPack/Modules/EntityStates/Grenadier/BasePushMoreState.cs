using EntityStates;
using RoR2;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier
{
    internal abstract class BasePushMoreState : BaseCharacterMain
    {

        [Tooltip("The minimum amount of seconds in which this state will have skill priority, scales with attack speed.")]
        [SerializeField]
        public float baseMinDuration;

        [Tooltip("Should the body flags change to ignore fall damage.")]
        [SerializeField]
        public bool setIgnoreFallDamage;

        [Tooltip("Ramp that can be used to change the air control during the state.")]
        [SerializeField]
        public AnimationCurve airControlOverrideCurve;

        private float minimumDuration => baseMinDuration / this.attackSpeedStat;
        internal virtual float currentAirControlCurveEval { get => 0; }

        private float previousAirControl;

        internal HurtBoxGroup hurtboxGroup;

        public override void OnEnter()
        {
            base.OnEnter();
            this.previousAirControl = base.characterMotor.airControl;
            base.characterBody.bodyFlags |= setIgnoreFallDamage ? CharacterBody.BodyFlags.IgnoreFallDamage : CharacterBody.BodyFlags.None;
            if (characterBody.modelLocator)
            {
                hurtboxGroup = characterBody.modelLocator.modelTransform.GetComponent<HurtBoxGroup>();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (airControlOverrideCurve != null)
            {
                base.characterMotor.airControl = airControlOverrideCurve.Evaluate(Mathf.Clamp01(currentAirControlCurveEval));
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (base.fixedAge <= minimumDuration)
            {
                return InterruptPriority.PrioritySkill;
            }
            return InterruptPriority.Any;
        }

        public override void OnExit()
        {
            base.OnExit();
            base.characterMotor.airControl = previousAirControl;
            base.characterBody.bodyFlags &= setIgnoreFallDamage ? CharacterBody.BodyFlags.IgnoreFallDamage : CharacterBody.BodyFlags.None;
        }
    }
}
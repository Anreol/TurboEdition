using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier
{
	public abstract class BaseBodyRollEnter : BaseSkillState
	{
		[SerializeField]
		public float baseDuration;
		[SerializeField]
		public float baseMinDuration = 0f;
		[SerializeField]
        public float smallHopVelocity;
		[SerializeField]
		public float fowardForceStrength;
		[SerializeField]
		public string enterSoundString;
        private bool buttonReleased;

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
			DisableSkillSlots();
			base.PlayAnimation("Body", "PreGroundSlam", "GroundSlam.playbackRate", this.duration);
			Util.PlaySound(enterSoundString, base.gameObject);
			if (base.characterMotor)
            {
                base.characterMotor.Motor.ForceUnground();
				base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, Mathf.Max(base.characterMotor.velocity.y, smallHopVelocity), base.characterMotor.velocity.z);
				base.characterMotor.ApplyForce(base.inputBank.moveVector * fowardForceStrength, true, false);
				base.characterMotor.disableAirControlUntilCollision = false;
			}
		}
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			base.characterMotor.moveDirection = base.inputBank.moveVector;
			buttonReleased = !buttonReleased && !(base.inputBank && base.inputBank.skill1.down);
			if (base.fixedAge > this.duration)
			{
				EnableSkillSlots();
				BaseBodyRoll baseBodyRoll = GetNextState();
				this.outer.SetNextState(baseBodyRoll);
			}
		}
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			if (base.fixedAge <= minimumDuration || !buttonReleased)
			{
				return InterruptPriority.PrioritySkill;
			}
			return InterruptPriority.Any;
		}
		public abstract BaseBodyRoll GetNextState();
		public virtual void DisableSkillSlots(){}
		public virtual void EnableSkillSlots() { }
	}
}

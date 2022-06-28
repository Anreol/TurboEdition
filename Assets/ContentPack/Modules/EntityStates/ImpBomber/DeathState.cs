using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.EntityStates.ImpBomber
{
    class DeathState : GenericCharacterDeath
    {
        public static GameObject initialEffect;
        public static GameObject deathEffect;
        public static float duration;

        private Animator animator;
        private float stopwatch;
        private bool hasPlayedDeathEffect;

        public override void OnEnter()
		{
			base.OnEnter();
			this.animator = base.GetModelAnimator();
            if (animator)
            {
				animator.SetBool("BombHolding.active", false);
			}
			if (base.characterMotor)
			{
				base.characterMotor.enabled = false;
			}
			if (base.modelLocator && base.modelLocator.modelTransform.GetComponent<ChildLocator>() && DeathState.initialEffect)
			{
				EffectManager.SimpleMuzzleFlash(DeathState.initialEffect, base.gameObject, "Root", false);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.animator)
			{
				this.stopwatch += Time.fixedDeltaTime;
				if (!this.hasPlayedDeathEffect && stopwatch > 1)
				{
					this.hasPlayedDeathEffect = true;
					EffectManager.SimpleMuzzleFlash(DeathState.deathEffect, base.gameObject, "Base", false);
				}
				if (this.stopwatch >= DeathState.duration)
				{
					EntityState.Destroy(base.gameObject);
				}
			}
		}
	}
}

using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.EntityStates.CrabChest.Weapon
{
    class DoubleMinigunSpindown : DoubleMinigunState
    {
		public static float baseDuration;
		public static string exitPlaySoundEvent;

		private float duration;
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = DoubleMinigunSpindown.baseDuration / this.attackSpeedStat;
			Util.PlayAttackSpeedSound(DoubleMinigunSpindown.exitPlaySoundEvent, base.gameObject, this.attackSpeedStat);
			base.GetModelAnimator().SetBool("WeaponIsReady", false);
		}
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
			}
		}
	}
}

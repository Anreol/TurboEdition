using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class EnterReload : BaseState
    {
        public static float baseDuration;
		public static string enterSoundString;
		private float duration
		{
			get
			{
				return EnterReload.baseDuration / this.attackSpeedStat;
			}
		}
		public override void OnEnter()
		{
			base.OnEnter();
			base.PlayCrossfade("Gesture, Additive", "EnterReload", "Reload.playbackRate", this.duration, 0.1f);
			Util.PlaySound(EnterReload.enterSoundString, base.gameObject);
		}
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority && base.fixedAge > this.duration)
			{
				this.outer.SetNextState(new Reload());
			}
		}
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}
}

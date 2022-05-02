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
    class SpawnState : BaseState
    {
        public static float duration = 4f;
        public static string spawnSoundString;
        public static GameObject spawnEffectPrefab;

        private float stopwatch;

        public override void OnEnter()
		{
			base.OnEnter();
            if (skillLocator && skillLocator.primary)
            {
				skillLocator.primary.DeductStock(skillLocator.primary.stock);
            }
			base.PlayAnimation("Body", "Spawn", "Spawn.playbackRate", SpawnState.duration);
			Util.PlaySound(SpawnState.spawnSoundString, base.gameObject);
			if (SpawnState.spawnEffectPrefab)
			{
				EffectManager.SimpleMuzzleFlash(SpawnState.spawnEffectPrefab, base.gameObject, "Root", false);
			}
		}
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.stopwatch += Time.fixedDeltaTime;
			if (this.stopwatch >= SpawnState.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
			}
		}
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Death;
		}
	}
}

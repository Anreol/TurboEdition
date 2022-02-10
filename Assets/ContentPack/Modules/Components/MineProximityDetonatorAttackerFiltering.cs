using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    class MineProximityDetonatorAttackerFiltering : MonoBehaviour
    {
		public TeamFilter myTeamFilter;
		public ProjectileDotZone projectileDotZone;
		public ProjectileExplosion projectileExplosion;
		public OverlapAttack overlapAttack;
		public UnityEvent triggerEvents;

		private AttackerFiltering attackerFiltering;
        private void OnEnable()
        {
            if (projectileDotZone != null)
				attackerFiltering = projectileDotZone.attackerFiltering;
			if (projectileExplosion != null)
				attackerFiltering = projectileExplosion.blastAttackerFiltering;
			if (overlapAttack != null)
				attackerFiltering = overlapAttack.attackerFiltering;
		}
        public void OnTriggerEnter(Collider collider)
		{
			if (NetworkServer.active)
			{
				if (collider)
				{
					HurtBox component = collider.GetComponent<HurtBox>();
					if (component)
					{
						HealthComponent healthComponent = component.healthComponent;
						if (healthComponent)
						{
							TeamComponent component2 = healthComponent.GetComponent<TeamComponent>();
							GameObject attacker = (this.gameObject.GetComponent<ProjectileController>()?.owner ? this.gameObject.GetComponent<ProjectileController>().owner.gameObject : null);
							bool dontHit = true;
							if (healthComponent.gameObject == attacker && this.attackerFiltering == AttackerFiltering.AlwaysHit)
                            {
								dontHit = false;
                            }
							if (dontHit && component2 && component2.teamIndex == this.myTeamFilter.teamIndex)
							{
								return;
							}
							UnityEvent unityEvent = this.triggerEvents;
							if (unityEvent == null)
							{
								return;
							}
							unityEvent.Invoke();
						}
					}
				}
				return;
			}
		}
	}
}

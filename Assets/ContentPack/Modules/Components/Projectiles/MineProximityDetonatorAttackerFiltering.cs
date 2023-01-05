using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    internal class MineProximityDetonatorAttackerFiltering : MonoBehaviour
    {
        public TeamFilter projectileTeamFilter;
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
                    HurtBox hurtboxHit = collider.GetComponent<HurtBox>();
                    if (hurtboxHit)
                    {
                        HealthComponent healthComponent = hurtboxHit.healthComponent;
                        if (healthComponent && healthComponent.body)
                        {
                            TeamComponent teamComponentHit = healthComponent.body.teamComponent;
                            GameObject attacker = (this.gameObject.GetComponent<ProjectileController>()?.owner ? this.gameObject.GetComponent<ProjectileController>().owner.gameObject : null);
                            bool dontHit = true;
                            if (healthComponent.gameObject == attacker && this.attackerFiltering == AttackerFiltering.AlwaysHit)
                            {
                                dontHit = false;
                            }
                            if (dontHit && teamComponentHit && teamComponentHit.teamIndex == this.projectileTeamFilter.teamIndex)
                            {
                                return;
                            }
                            this.triggerEvents?.Invoke();
                        }
                    }
                }
                return;
            }
        }
    }
}
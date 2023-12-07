using EntityStates;
using HG;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.EntityStates.Projectiles
{
    /// <summary>
    /// State for <see cref="Equipments.Hellchain"/>. Mpves to <see cref="HellChainCasting"/> after a few seconds.
    /// </summary>
    public class HellChainPreCasting : EntityState
    {
        [SerializeField] public float detachForce;
        [SerializeField] public float torqueForce = 200f;
        [SerializeField] public bool shouldStick;
        [SerializeField] public float durationBeforeDetach;
        [SerializeField] public float durationBeforeCast;

        private bool hasDettached = false;
        private ProjectileStickOnImpact projectileStickOnImpact;
        public override void OnEnter()
        {
            base.OnEnter();
            projectileStickOnImpact = base.GetComponent<ProjectileStickOnImpact>();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (durationBeforeDetach <= fixedAge && !hasDettached)
            {
                hasDettached = true;
                if (projectileStickOnImpact.enabled != shouldStick)
                {
                    projectileStickOnImpact.enabled = shouldStick;
                }
                rigidbody.AddForce(transform.forward * detachForce);
                rigidbody.AddTorque(Random.onUnitSphere * torqueForce);
            }
            if (NetworkServer.active && durationBeforeCast <= fixedAge && hasDettached)
            {
                outer.SetNextState(new HellChainCasting());
            }
        }
    }
}
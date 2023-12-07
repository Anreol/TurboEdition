using EntityStates;
using HG;
using RoR2;
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
        [SerializeField] public float durationBeforeDetach;
        [SerializeField] public float durationBeforeCast;

        private bool hasDettached = false;
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (durationBeforeDetach <= fixedAge && !hasDettached)
            {
                hasDettached = true;
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
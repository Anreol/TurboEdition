using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.Components
{
    public class PainArtifactServerListener : MonoBehaviour, IOnIncomingDamageServerReceiver
    {
        public CharacterBody body;

        private void OnEnable()
        {
            InstanceTracker.Add<PainArtifactServerListener>(this);
        }

        private void OnDisable()
        {
            InstanceTracker.Remove<PainArtifactServerListener>(this);
        }

        public void OnIncomingDamageServer(DamageInfo damageInfo)
        {
            if (damageInfo.rejected || damageInfo.dotIndex != DotController.DotIndex.None || damageInfo.damageType == DamageType.FallDamage || damageInfo.attacker == null)
                return;
            float fraction = damageInfo.damage / PlayerCharacterMasterController.instances.Count;
            damageInfo.damage -= fraction;

            DamageInfo newDamage = new DamageInfo
            {
                attacker = null,
                crit = false,
                damage = fraction,
                damageColorIndex = DamageColorIndex.DeathMark,
                damageType = damageInfo.damageType,
                dotIndex = DotController.DotIndex.None,
                force = new Vector3(0, 0, 0),
                inflictor = body.gameObject,
                procChainMask = damageInfo.procChainMask,
                procCoefficient = damageInfo.procCoefficient
            };
            List<PainArtifactServerListener> painArtifactServerListeners = InstanceTracker.GetInstancesList<PainArtifactServerListener>();
            foreach (PainArtifactServerListener item in painArtifactServerListeners)
            {
                if (item == this) continue;
                item.body.healthComponent.TakeDamage(newDamage);
            }
        }
    }
}
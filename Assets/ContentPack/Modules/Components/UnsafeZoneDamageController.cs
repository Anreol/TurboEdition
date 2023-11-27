using RoR2;
using System.Collections.Generic;
using TurboEdition.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    /// <summary>
    /// <see cref="MonoBehaviour"/> wrapper for <see cref="UnsafeZoneDamageManager"/>
    /// </summary>
    public class UnsafeZoneDamageController : MonoBehaviour
    {
        [Tooltip("An initial list of unsafe zones where bodies will be dealt damage in.")]
        [SerializeField] private BaseZoneBehavior[] initialUnsafeZones;

        [Header("Team stuff")]
        [Tooltip("Used to control which teams TO DAMAGE.")]
        [SerializeField] private TeamMask teamMask;

        [Header("Damage things")]
        [SerializeField] private DamageType damageType;

        [SerializeField] private DamageColorIndex damageColorIndex;

        [Header("Tick things")]
        [Tooltip("The period in seconds in between each tick")]
        [SerializeField] private float tickPeriodSeconds;

        [Range(0f, 1f)]
        [Tooltip("The fraction of combined health to deduct per tick. Note that damage is actually applied per tick, not per second.")]
        [SerializeField] private float healthFractionPerSecond;

        [Tooltip("The coefficient to increase the damage by, for every tick they take inside the zones.")]
        [SerializeField] private float healthFractionRampCoefficientPerSecond;

        public UnsafeZoneDamageManager unsafeZoneDamageManager;

        private void OnEnable()
        {
            if(unsafeZoneDamageManager == null)
            {
                unsafeZoneDamageManager = new UnsafeZoneDamageManager(initialUnsafeZones)
                {
                    teamMask = teamMask,
                    damageType = damageType,
                    damageColorIndex = damageColorIndex,
                    tickPeriodSeconds = tickPeriodSeconds,
                    healthFractionEachSecond = healthFractionPerSecond,
                    healthFractionRampCoefficientPerSecond = healthFractionRampCoefficientPerSecond
                };
            }
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active && unsafeZoneDamageManager != null)
            {
                unsafeZoneDamageManager.ServerFixedUpdate(Time.fixedDeltaTime);
            }
        }
    }
}
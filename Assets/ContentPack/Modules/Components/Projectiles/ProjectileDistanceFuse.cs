using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace TurboEdition.Components.Projectiles
{
    [RequireComponent(typeof(ProjectileController))]
    internal class ProjectileDistanceFuse : MonoBehaviour
    {
        [Tooltip("How many meters in advance for the first check to pass should it have when it starts.")]
        public float startingDistance;

        [Tooltip("Every how many meters from the last position should the event get invoked.")]
        public float meterInterval;

        [Tooltip("Maximum times to invoke the event.")]
        public int maxTimesToInvoke;

        public UnityEvent onFirstFuse;
        public UnityEvent onEveryFuse;
        public UnityEvent onLastFuse;

        private float lastPosSqr;
        private int timesInvoked;

        private void Awake()
        {
            if (!NetworkServer.active)
                enabled = false;
            lastPosSqr = gameObject.transform.position.sqrMagnitude + startingDistance * startingDistance;
        }

        private void FixedUpdate()
        {
            if (gameObject.transform.position.sqrMagnitude - lastPosSqr >= meterInterval * meterInterval)
            {
                lastPosSqr = gameObject.transform.position.sqrMagnitude;
                if (timesInvoked == 0)
                {
                    onFirstFuse?.Invoke();
                }
                onEveryFuse?.Invoke();
                timesInvoked++;
                if (timesInvoked >= maxTimesToInvoke)
                {
                    onLastFuse?.Invoke();
                    enabled = false;
                }
            }
        }
    }
}
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    public class VerticalCylinderZone : BaseZoneBehavior, IZone
    {
        [Tooltip("The radius of effect.")]
        [SyncVar] public float radius;

        [Tooltip("The radius of effect.")]
        [SyncVar] public float height;

        [Tooltip("The child range indicator object. Will be scaled to the radius and height.")]
        public Transform rangeIndicator;

        [Tooltip("The time it takes the range indicator to update")]
        public float indicatorSmoothTime = 0.2f;

        private float rangeIndicatorScaleVelocityX;
        private float rangeIndicatorScaleVelocityY;
        private void OnEnable()
        {
            if (rangeIndicator)
            {
                rangeIndicator.gameObject.SetActive(true);
            }
        }
        private void OnDisable()
        {
            if (rangeIndicator)
            {
                rangeIndicator.gameObject.SetActive(false);
            }
        }
        private void Update()
        {
            if (rangeIndicator)
            {
                float num = Mathf.SmoothDamp(rangeIndicator.localScale.x, radius, ref rangeIndicatorScaleVelocityX, indicatorSmoothTime);
                float numy = Mathf.SmoothDamp(rangeIndicator.localScale.y, height, ref rangeIndicatorScaleVelocityY, indicatorSmoothTime);
                rangeIndicator.localScale = new Vector3(num, numy, num);
            }
        }

        public override bool IsInBounds(Vector3 position)
        {
            Vector3 relativePos = position - transform.position;
            relativePos.y = 0f;
            float absHeight = Mathf.Abs(position.y - transform.position.y);
            return relativePos.sqrMagnitude <= radius * radius && absHeight <= height;
        }
    }
}
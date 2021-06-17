using RoR2;
using UnityEngine;

namespace TurboEdition
{
    internal class TeleporterRadiusController : MonoBehaviour
    {
        private HoldoutZoneController holdoutZoneController;

        private float currentCalculatedRadius = 0f;
        private float currentRadius;

        private Run.FixedTimeStamp enabledTime;

        private float radiusIncreaseUnique = 8f;
        private float radiusIncreaseExtra = 4f;

        private float startupDelay = 3f;
        private float rampUpTime = 5f;

        private static readonly AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        private static Color newMaterialColor = new Color(5f, 2.5f, 0f, 1f);
        private static int nTimes;

        private void Awake()
        {
            this.holdoutZoneController = base.GetComponent<HoldoutZoneController>();
        }

        private void OnEnable()
        {
            this.enabledTime = Run.FixedTimeStamp.now;
            if (currentCalculatedRadius > 0)
            {
                this.holdoutZoneController.calcRadius += this.ApplyRadius;
                this.holdoutZoneController.calcColor += this.ApplyColor;
            }
        }

        private void OnDisable()
        {
            this.holdoutZoneController.calcColor -= this.ApplyColor;
            this.holdoutZoneController.calcRadius -= this.ApplyRadius;
        }

        private void ApplyRadius(ref float radius)
        {
            radius += currentCalculatedRadius;
        }

        private void ApplyColor(ref Color color)
        {
            color = Color.Lerp(color, newMaterialColor, colorCurve.Evaluate(this.currentRadius));
        }

        private void FixedUpdate()
        {
            var chargingTeam = TeamComponent.GetTeamMembers(this.holdoutZoneController.chargingTeam);
            if (this.enabledTime.timeSince > startupDelay)
            {
                foreach (var teamMember in chargingTeam)
                {
                    if (teamMember.body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("AddTeleporterRadius")) > 0)
                    {
                        this.currentCalculatedRadius += ((teamMember.body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("AddTeleporterRadius")) - 1) * radiusIncreaseExtra) + radiusIncreaseUnique;
                    }
                }
            }

            //this.currentCalculatedRadius = Mathf.Min(this.currentCalculatedRadius, itemCap);
            float target = (this.currentCalculatedRadius > 0f) ? 1f : 0f;
            float prevValue = Mathf.MoveTowards(this.currentRadius, target, rampUpTime * Time.fixedDeltaTime);
            if (this.currentRadius <= 0f && prevValue > 0f)
            {
                Util.PlaySound("Play_item_lunar_focusedConvergence", base.gameObject);
            }
            this.currentRadius = prevValue;

            //Check if using % == 0 might work
            if ((holdoutZoneController.currentRadius > (nTimes * (holdoutZoneController.baseRadius / 4))))
            {
                nTimes++;
                CalculateTeleporterColor(-1.2f);
            }
            else if ((holdoutZoneController.currentRadius < (nTimes - 1 * (holdoutZoneController.baseRadius / 4))))
            {
                nTimes--;
                CalculateTeleporterColor(1.2f);
            }
        }

        private void CalculateTeleporterColor(float colorCycle)
        {
            float rMove = -1, gMove = -1, bMove = -1;
            if (newMaterialColor.r >= 5.99f || newMaterialColor.r <= 0f)
            {
                rMove *= colorCycle;
            }
            if (newMaterialColor.g >= 5.99f || newMaterialColor.g <= 0f)
            {
                gMove *= colorCycle;
            }
            if (newMaterialColor.b >= 5.99f || newMaterialColor.b <= 0f)
            {
                bMove *= colorCycle;
            }
            newMaterialColor.r += rMove;
            newMaterialColor.g += gMove;
            newMaterialColor.b += bMove;
        }
    }
}
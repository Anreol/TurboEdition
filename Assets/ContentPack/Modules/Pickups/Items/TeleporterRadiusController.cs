using RoR2;
using UnityEngine;

namespace TurboEdition.Items
{
    public class TeleporterRadius : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("AddTeleporterRadius");

        public override void Initialize()
        {
            On.RoR2.HoldoutZoneController.Start += HoldoutZoneController_Start;
        }

        private void HoldoutZoneController_Start(On.RoR2.HoldoutZoneController.orig_Start orig, HoldoutZoneController self)
        {
            if (self.applyFocusConvergence) //If zone can get shrunk, it can grow too
            {
                self.gameObject.AddComponent<TeleporterRadiusController>();
            }
        }

        internal class TeleporterRadiusController : MonoBehaviour
        {
            private HoldoutZoneController holdoutZoneController;

            private float currentValue;

            private Run.FixedTimeStamp enabledTime;

            private float radiusIncreaseUnique = 8f;
            private float radiusIncreaseExtra = 4f;

            private float startupDelay = 3f;
            private float rampUpTime = 5f;

            private static readonly AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            private static Color newMaterialColor; // = new Color(2.5f, 1.5f, 0.5f, 1f);
            private static float newR, newG, newB; //I have no idea why i should do this
            private float rMove = -1, gMove = -1, bMove = -1;
            private static int nTimes;

            private int itemCount;
            private int uniqueItemCount;

            private void Awake()
            {
                this.holdoutZoneController = base.GetComponent<HoldoutZoneController>();
                newMaterialColor = this.holdoutZoneController.baseIndicatorColor;
            }

            private void OnEnable()
            {
                this.enabledTime = Run.FixedTimeStamp.now;
                this.holdoutZoneController.calcRadius += this.ApplyRadius;
                this.holdoutZoneController.calcColor += this.ApplyColor;
            }

            private void OnDisable()
            {
                this.holdoutZoneController.calcColor -= this.ApplyColor;
                this.holdoutZoneController.calcRadius -= this.ApplyRadius;
            }

            private void ApplyRadius(ref float radius)
            {
                if (this.enabledTime.timeSince > startupDelay)
                {
                    radius += (uniqueItemCount * radiusIncreaseUnique) + ((itemCount - uniqueItemCount) * radiusIncreaseExtra);
                }
            }

            private void ApplyColor(ref Color color)
            {
                color = Color.Lerp(color, newMaterialColor, colorCurve.Evaluate(this.currentValue));
            }

            private void FixedUpdate()
            {
                //this.currentCalculatedRadius = Mathf.Min(this.currentCalculatedRadius, itemCap);
                var chargingTeam = TeamComponent.GetTeamMembers(this.holdoutZoneController.chargingTeam);
                this.uniqueItemCount = 0;
                foreach (var teamMember in chargingTeam)
                {
                    if (teamMember.body.healthComponent.alive && teamMember.body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("AddTeleporterRadius")) > 0)
                    {
                        this.uniqueItemCount++; //Basically count that tracks bodies that are alive and have at least one item
                    }
                }
                this.itemCount = Util.GetItemCountForTeam(this.holdoutZoneController.chargingTeam, Assets.mainAssetBundle.LoadAsset<ItemDef>("AddTeleporterRadius").itemIndex, true, false);
                if (this.enabledTime.timeSince < this.startupDelay)
                {
                    this.uniqueItemCount = 0;
                    this.itemCount = 0;
                    return; //we do not want to do anything else as long as the start up delay hasnt passed
                }
                float target = (this.itemCount > 0f) ? 1f : 0f; //FIXME: Wont change if players pick up new items / lose em, meaning color changes will be sharp and sound wont play
                float prevValue = Mathf.MoveTowards(this.currentValue, target, rampUpTime * Time.fixedDeltaTime);
                if (currentValue <= 0f && prevValue > 0f)
                {
                    Util.PlaySound("Play_item_lunar_focusedConvergence", base.gameObject);
                }
                this.currentValue = prevValue;

                //Check if using % == 0 might work
                if ((holdoutZoneController.currentRadius > (nTimes * (holdoutZoneController.baseRadius / 4))))
                {
                    nTimes++;
                    CalculateTeleporterColor(-1.3f);
                }
                else if ((holdoutZoneController.currentRadius < (nTimes - 1 * (holdoutZoneController.baseRadius / 4))))
                {
                    nTimes--;
                    CalculateTeleporterColor(1.3f);
                }
            }

            private void CalculateTeleporterColor(float colorCycle)
            {
                newR += rMove;
                newG += gMove;
                newB += bMove;

                if (newR >= 4.99f || newR <= 1f)
                {
                    rMove *= colorCycle;
                }
                if (newG >= 4.99f || newG <= 1f)
                {
                    gMove *= colorCycle;
                }
                if (newB >= 4.99f || newB <= 1f)
                {
                    bMove *= colorCycle;
                }
                newMaterialColor.r = newR;
                newMaterialColor.g = newG;
                newMaterialColor.b = newB;
                newMaterialColor.a = 1f; //Reassigning just in case
            }
        }
    }
}
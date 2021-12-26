using RoR2;
using TurboEdition.UI;
using UnityEngine;

namespace TurboEdition.Items
{
    public class StandBonus : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("StandBonus");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<Sandbag>(stack);
        }

        internal class Sandbag : CharacterBody.ItemBehavior, IStatItemBehavior, IOnTakeDamageServerReceiver, IStatBarProvider
        {
            private CharacterMotor motor; //brrrrum brrum
            private bool provideBuffs;
            private bool reset;
            private float accumulatedDamage;
            private float lerp = 0.0f;

            private void Start() //On Start since we need to subscribe to the body, ANYTHING THAT HAS TO DO WITH BODIES, CANNOT BE ON AWAKE() OR ONENABLE()
            {
                if (!body)
                {
                    TELog.LogE("Body not available or does not exist.");
                    return;
                }
                motor = base.GetComponent<CharacterMotor>();
            }

            private void FixedUpdate()
            {
                if (!body.GetNotMoving())
                {
                    accumulatedDamage = 0f;
                    lerp = 0f;
                    return;
                }
                lerp = Mathf.InverseLerp((stack / 4f) * body.healthComponent.fullCombinedHealth, 0f, accumulatedDamage);
                provideBuffs = body.GetNotMoving() && stack > 0;
            }

            public void RecalculateStatsEnd()
            {
                if (!provideBuffs) return;
                if (motor)
                {
                    motor.mass += (10 + ((stack - 1) * 5)); //[body] IS FAT
                }
                body.armor += Mathf.Round((500f * lerp) * 100) / 100;
            }

            public void RecalculateStatsStart()
            {
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (!damageReport.damageInfo.rejected)
                {
                    this.accumulatedDamage += damageReport.damageDealt;
                }
            }

            public StatBarData GetStatBarData()
            {
                float currentData = Mathf.Round((500f * lerp) * 100) / 100;
                string overString = (currentData <= 0) ? "TOOLTIP_ITEM_NOBUFF_DESCRIPTION" : "";
                return new StatBarData
                {
                    fillBarColor = new Color(0.5f, 0.7f, 0.5f, 1f),
                    maxData = 500f,
                    currentData = currentData,
                    sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StandBonus")).pickupIconSprite,
                    tooltipContent = new RoR2.UI.TooltipContent
                    {
                        titleColor = ColorCatalog.GetColor(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StandBonus")).darkColorIndex),
                        titleToken = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StandBonus")).nameToken,
                        bodyToken = "TOOLTIP_ITEM_STANDBONUS_DESCRIPTION",
                        overrideBodyText = Language.GetString(overString),
                    }
                };
            }
        }
    }
}
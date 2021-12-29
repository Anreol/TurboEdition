using RoR2;
using TurboEdition.UI;
using UnityEngine;
using UnityEngine.Networking;

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
            private CharacterMotor _motor; //brrrrum brrum
            private bool _provideBuffs;
            private float _accumulatedDamage;
            private float _lerp = 0.0f;

            private void Start() //On Start since we need to subscribe to the body, ANYTHING THAT HAS TO DO WITH BODIES, CANNOT BE ON AWAKE() OR ONENABLE()
            {
                if (!body)
                {
                    TELog.LogE("Body not available or does not exist.");
                    return;
                }
                _motor = base.GetComponent<CharacterMotor>();
            }

            private void FixedUpdate()
            {
                if (!NetworkServer.active) return;
                if (!body.GetNotMoving())
                {
                    _accumulatedDamage = 0f;
                    _lerp = 0f;
                    return;
                }
                _lerp = Mathf.InverseLerp((stack / 4f) * body.healthComponent.fullCombinedHealth, 0f, _accumulatedDamage);
                _provideBuffs = body.GetNotMoving() && stack > 0;
            }

            public void RecalculateStatsEnd()
            {
                if (!_provideBuffs) return;
                if (_motor)
                {
                    _motor.mass += (10 + ((stack - 1) * 5)); //[body] IS FAT
                }
                body.armor += Mathf.RoundToInt(500f * this._lerp);
            }

            public void RecalculateStatsStart()
            {
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (!damageReport.damageInfo.rejected)
                {
                    this._accumulatedDamage += damageReport.damageDealt;
                }
            }

            public StatBarData GetStatBarData()
            {
                float currentData = Mathf.RoundToInt(500f * this._lerp);
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
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurboEdition.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    class GetDamageFromHurtVictim : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("SoulDevourer");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<GetDamageFromHurtVictimBehavior>(stack);
        }
        public override void Initialize()
        {
            GlobalEventManager.onServerCharacterExecuted += onServerCharacterExecuted;
        }

        private void onServerCharacterExecuted(DamageReport arg1, float arg2)
        {
            if (arg1.attackerBody.inventory.GetItemCount(ItemCatalog.FindItemIndex("SoulDevourer")) > 0)
            {
                arg1.attackerBody.GetComponent<GetDamageFromHurtVictimBehavior>()?.onServerCharacterExecuted(arg1, arg2);
            }
        }

        internal class GetDamageFromHurtVictimBehavior : CharacterBody.ItemBehavior, IStatItemBehavior, IOnDamageDealtServerReceiver, IOnTakeDamageServerReceiver, IStatBarProvider
        {
            private float accumulatedDamage;
            public void onServerCharacterExecuted(DamageReport arg1, float executionHealthLost)
            {
                if (arg1.attackerBody == body && accumulatedDamage <= (100 + ((stack - 1) * 50))) 
                    accumulatedDamage += 25f;
                accumulatedDamage = Mathf.Min(accumulatedDamage, 100 + ((stack - 1) * 50));
            }

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (accumulatedDamage > (100 + ((stack - 1) * 50)) || damageReport.victim.isHealthLow)
                    return;
                if ( damageReport.combinedHealthBeforeDamage >= damageReport.victim.fullCombinedHealth && !damageReport.victim.alive)
                    accumulatedDamage += 25f;
                if (damageReport.victim.alive)
                    accumulatedDamage += damageReport.damageInfo.procCoefficient;
                accumulatedDamage = Mathf.Min(accumulatedDamage, 100 + ((stack - 1) * 50)); //it's 3am and i suck at math lol dont mind me
            }
            public void RecalculateStatsEnd()
            {
                body.damage += accumulatedDamage;
            }

            public void RecalculateStatsStart()
            {
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (!damageReport.damageInfo.rejected || !damageReport.isFallDamage || (damageReport.dotType == DotController.DotIndex.None)) //Rejects shit with dot because if you are getting dotted, you got it from being damaged
                    this.accumulatedDamage = 0;
            }

            public StatBarData GetStatBarData()
            {
                string overString = (accumulatedDamage <= 0) ? "TOOLTIP_ITEM_NOBUFF_DESCRIPTION" : "";
                return new StatBarData
                {
                    fillBarColor = new Color(0.3f, 1f, 0.8f, 1f),
                    maxData = (100 + ((stack - 1) * 50)),
                    currentData = accumulatedDamage,
                    offData = "TOOLTIP_ITEM_NOBUFF_DESCRIPTION",
                    sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("SoulDevourer")).pickupIconSprite,
                    tooltipContent = new RoR2.UI.TooltipContent
                    {
                        titleColor = ColorCatalog.GetColor(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("SoulDevourer")).darkColorIndex),
                        titleToken = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("SoulDevourer")).nameToken,
                        bodyToken = "TOOLTIP_ITEM_SOULDEVOURER_DESCRIPTION",
                        overrideBodyText = Language.GetString(overString),
                    }
                };
            }


        }

    }
}

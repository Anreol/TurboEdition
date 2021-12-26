using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.Items
{
    class BloodMoney : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("BloodEconomy");
        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<BloodMoneyBehavior>(stack);
        }

        internal class BloodMoneyBehavior : CharacterBody.ItemBehavior, IOnTakeDamageServerReceiver
        {
            private int currentMl = 0;
            private bool shouldScale = true;
            private void FixedUpdate()
            {
                if (currentMl >= 600)
                {
                    int moneyReward = 25 + ((stack - 1) * 15);
                    moneyReward = shouldScale ? Run.instance.GetDifficultyScaledCost(moneyReward) : moneyReward;
                    body.master.GiveMoney((uint)moneyReward);
                    currentMl =- 600;
                }
            }
            public void OnTakeDamageServer(DamageReport damageReport)
            {
                switch (damageReport.dotType)
                {
                    case DotController.DotIndex.Bleed:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt);
                        break;
                    case DotController.DotIndex.SuperBleed:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt);
                        break;
                }
                if (body.healthComponent.isHealthLow)
                {
                    currentMl += Mathf.RoundToInt(damageReport.damageDealt / 4);
                }
            }
        }
    }
}

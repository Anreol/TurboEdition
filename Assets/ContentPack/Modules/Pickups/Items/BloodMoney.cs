﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    class BloodMoney : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("BloodEconomy");
        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            if (NetworkServer.active)
                body.AddItemBehavior<BloodMoneyBehaviorServer>(stack);
        }

        internal class BloodMoneyBehaviorServer : CharacterBody.ItemBehavior, IOnTakeDamageServerReceiver
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
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt * 1.5f);
                        break;
                    case DotController.DotIndex.Burn:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt);
                        break;
                    case DotController.DotIndex.Helfire:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt / 2);
                        break;
                    case DotController.DotIndex.PercentBurn:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt);
                        break;
                    case DotController.DotIndex.Poison:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt);
                        break;
                    case DotController.DotIndex.Blight:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt);
                        break;
                    case DotController.DotIndex.SuperBleed:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt * 1.5f);
                        break;
                    default:
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
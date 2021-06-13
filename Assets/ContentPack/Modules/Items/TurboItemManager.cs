using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace TurboEdition
{
    public class TurboItemManager : CharacterBody.ItemBehavior
    {
        //The item behavior that manages item behaviors
        private void Awake()
        {
            body.onInventoryChanged += AddBehaviors;
        }

        private void AddBehaviors()
        {
            if (NetworkServer.active)
            {
                body.AddItemBehavior<HitlagBehavior>(body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("Hitlag")));
                body.AddItemBehavior<EnvBonusBehavior>(body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("EnvRadio")));
            }
        }
    }
}

namespace TurboEdition
{
    public class ExternalControllers
    {
        //For stuff that doesnt use itemBehaviors or that doesn't require bodies
        private void HoldoutZoneControllers()
        {
            var holdoutZoneControllers = InstanceTracker.GetInstancesList<HoldoutZoneController>();
            foreach (var item in holdoutZoneControllers)
            {
                item.gameObject.AddComponent<TeleporterRadiusController>();
            }
        }
    }
}

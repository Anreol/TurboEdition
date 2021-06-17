using HG;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition
{
    public class TurboItemManager : CharacterBody.ItemBehavior
    {
        //The item behavior that manages item behaviors
        private void Awake()
        {
            body.onInventoryChanged += AddBehaviors;
            MasterSummon.onServerMasterSummonGlobal += AnythingSummoned;
        }

        private void AddBehaviors()
        {
            if (NetworkServer.active)
            {
                body.AddItemBehavior<HitlagBehavior>(body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("Hitlag")));
                body.AddItemBehavior<MeleeArmorBehavior>(body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("MeleeArmor")));
                body.AddItemBehavior<EnvBonusBehavior>(body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("EnvRadio")));
            }
        }

        private void AnythingSummoned(MasterSummon.MasterSummonReport msr)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            #region ItemDeploys

            int itemDeployCount = body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("ItemDeploys"));
            if (itemDeployCount > 0)
            {
                List<RoR2.Orbs.ItemTransferOrb> inFlightOrbs = new List<RoR2.Orbs.ItemTransferOrb>();
                int maxItemsToGive = Mathf.Min(itemDeployCount + 1, body.inventory.itemAcquisitionOrder.Count);
                for (int i = maxItemsToGive - 1; i >= 0; i--)
                {
                    ItemIndex itemIndex = body.inventory.itemAcquisitionOrder[i];
                    int maxStackToGive = Mathf.Min(itemDeployCount * 2, body.inventory.GetItemCount(itemIndex));
                    if (ItemCatalog.GetItemDef(itemIndex).DoesNotContainTag(ItemTag.AIBlacklist) || ItemCatalog.GetItemDef(itemIndex).DoesNotContainTag(ItemTag.CannotCopy) || itemIndex != Assets.mainAssetBundle.LoadAsset<ItemDef>("ItemDeploys").itemIndex)
                    {
                        ItemDeploysManager(itemIndex, maxStackToGive, msr, inFlightOrbs);
                    }
                }
            }

            #endregion ItemDeploys
        }

        //One time use methods go here
        private void ItemDeploysManager(ItemIndex indexGive, int numGive, MasterSummon.MasterSummonReport msr, List<ItemTransferOrb> inFlightOrbs)
        {
            if ((msr.summonMasterInstance.hasBody && (msr.summonMasterInstance.GetBody().bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None))
            {
                ItemTransferOrb item = ItemTransferOrb.DispatchItemTransferOrb(msr.leaderMasterInstance.GetBody().transform.position, msr.summonMasterInstance.inventory, indexGive, numGive, delegate (ItemTransferOrb orb)
                {
                    msr.summonMasterInstance.inventory.GiveItem(orb.itemIndex, orb.stack);
                    inFlightOrbs.Remove(orb);
                }, default(Either<NetworkIdentity, HurtBox>));
                inFlightOrbs.Add(item);
            }
            else
            {
                msr.summonMasterInstance.inventory.GiveItem(indexGive, numGive);
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
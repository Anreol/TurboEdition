using HG;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class ItemDeployer : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("ItemDeployer");
        ItemDeployerController itemDeployerController;
        public override void Initialize()
        {
            MasterSummon.onServerMasterSummonGlobal += MasterSummon_onServerMasterSummonGlobal;
        }

        private void MasterSummon_onServerMasterSummonGlobal(MasterSummon.MasterSummonReport obj)
        {
            itemDeployerController = new ItemDeployerController();
            itemDeployerController.Activate(obj);
        }

        public class ItemDeployerController
        {
            public void Activate(MasterSummon.MasterSummonReport msr)
            {
                int itemDeployCount = msr.leaderMasterInstance.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("ItemDeployer"));
                if (itemDeployCount > 0)
                {
                    List<RoR2.Orbs.ItemTransferOrb> inFlightOrbs = new List<RoR2.Orbs.ItemTransferOrb>();
                    int maxItemsToGive = Mathf.Min(itemDeployCount + 1, msr.leaderMasterInstance.inventory.itemAcquisitionOrder.Count);
                    for (int i = maxItemsToGive - 1; i >= 0; i--)
                    {
                        ItemIndex itemIndex = msr.leaderMasterInstance.inventory.itemAcquisitionOrder[i];
                        int maxStackToGive = Mathf.Min(itemDeployCount * 2, msr.leaderMasterInstance.inventory.GetItemCount(itemIndex));
                        if (ItemCatalog.GetItemDef(itemIndex).DoesNotContainTag(ItemTag.AIBlacklist) || ItemCatalog.GetItemDef(itemIndex).DoesNotContainTag(ItemTag.CannotCopy) || itemIndex != Assets.mainAssetBundle.LoadAsset<ItemDef>("ItemDeploys").itemIndex)
                        {
                            ItemDeploysManager(itemIndex, maxStackToGive, msr, inFlightOrbs);
                        }
                    }
                }
            }

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
}
using HG;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class ItemDeployer : Item //Do we need anything of the item class here? We dont. Look at making this independent. It should be added to the dictionary, yknow
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("ItemDeployer");

        //public static ItemDeployerController itemDeployerController = new ItemDeployerController();
        public override void Initialize()
        {
            MasterSummon.onServerMasterSummonGlobal += MasterSummon_onServerMasterSummonGlobal;
        }

        private void MasterSummon_onServerMasterSummonGlobal(MasterSummon.MasterSummonReport obj)
        {
            ItemDeployerController.Activate(obj);
        }

        public static class ItemDeployerController
        {
            public static void Activate(MasterSummon.MasterSummonReport msr)
            {
                if (!NetworkServer.active || !msr.leaderMasterInstance || !msr.summonMasterInstance) //needs a master else it will NRE director spawns
                {
                    return;
                }
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

            private static void ItemDeploysManager(ItemIndex indexGive, int numGive, MasterSummon.MasterSummonReport msr, List<ItemTransferOrb> inFlightOrbs)
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
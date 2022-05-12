using HG;
using RoR2;
using RoR2.Audio;
using RoR2.Orbs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public static class ItemDeployerManager
    {
        private static NetworkSoundEventDef networkSound = Assets.mainAssetBundle.LoadAsset<NetworkSoundEventDef>("nseItemProcItemDeployer");

        [SystemInitializer(typeof(PickupCatalog))]
        public static void Initialize()
        {
            MasterSummon.onServerMasterSummonGlobal += onServerMasterSummonGlobal;
        }

        private static void onServerMasterSummonGlobal(MasterSummon.MasterSummonReport obj)
        {
            if (!obj.leaderMasterInstance || !obj.summonMasterInstance) return; //needs a master else it will NRE director spawns

            foreach (var item in bannedBodies)
            {
                if (obj.summonMasterInstance.GetBody().bodyIndex == BodyCatalog.FindBodyIndex(item))
                    return;
            }
            Activate(obj);
        }

        public static void Activate(MasterSummon.MasterSummonReport msr)
        {
            int itemDeployCount = msr.leaderMasterInstance.inventory.GetItemCount(TEContent.Items.ItemDeployer);
            if (itemDeployCount > 0)
            {
                EntitySoundManager.EmitSoundServer(networkSound.index, msr.summonMasterInstance.GetBody()?.gameObject);

                List<RoR2.Orbs.ItemTransferOrb> inFlightOrbs = new List<RoR2.Orbs.ItemTransferOrb>();
                int maxItemsToGive = Mathf.Min(itemDeployCount + 1, msr.leaderMasterInstance.inventory.itemAcquisitionOrder.Count);
                for (int i = maxItemsToGive - 1; i >= 0; i--)
                {
                    ItemIndex itemIndex = msr.leaderMasterInstance.inventory.itemAcquisitionOrder[i];
                    int maxStackToGive = Mathf.Min(itemDeployCount * 2, msr.leaderMasterInstance.inventory.GetItemCount(itemIndex));
                    ItemDef itemdef = ItemCatalog.GetItemDef(itemIndex);
                    if ((itemdef.ContainsTag(ItemTag.AIBlacklist) || itemdef.ContainsTag(ItemTag.CannotCopy)) || itemdef.hidden || itemdef == TEContent.Items.ItemDeployer)
                        continue;
                    ItemDeploysManager(itemIndex, maxStackToGive, msr, inFlightOrbs);
                }
            }
        }

        private static void ItemDeploysManager(ItemIndex indexGive, int numGive, MasterSummon.MasterSummonReport msr, List<ItemTransferOrb> inFlightOrbs)
        {
            if ((msr.summonMasterInstance.hasBody))
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

        private static string[] bannedBodies = new string[]
        {
                "EngiTurretBody",
                "EngiWalkerTurretBody"
        };
    }
}
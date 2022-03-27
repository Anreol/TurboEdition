using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace TurboEdition.EntityStates.CrabChest.ItemScanner
{
    internal class BaseItemStealState : BaseState
    {
        protected ItemStealController itemStealController;

        public override void OnEnter()
        {
            base.OnEnter();
            this.FindItemStealer();
            if (NetworkServer.active && !this.itemStealController)
            {
                this.InitItemStealer();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!this.itemStealController)
                this.FindItemStealer();
        }

        private void FindItemStealer()
        {
            List<NetworkedBodyAttachment> list = new List<NetworkedBodyAttachment>();
            NetworkedBodyAttachment.FindBodyAttachments(base.characterBody, list);
            foreach (NetworkedBodyAttachment networkedBodyAttachment in list)
            {
                this.itemStealController = networkedBodyAttachment.GetComponent<ItemStealController>();
                if (this.itemStealController)
                {
                    break;
                }
            }
        }

        private void InitItemStealer()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            if (this.itemStealController == null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/ItemStealController.prefab").WaitForCompletion(), base.transform.position, Quaternion.identity);
                this.itemStealController = gameObject.GetComponent<ItemStealController>();
                this.itemStealController.itemLendFilter = new Func<ItemIndex, bool>(ItemStealController.AIItemFilter);
                gameObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject, null);
                base.gameObject.GetComponent<ReturnStolenItemsOnGettingHit>().itemStealController = this.itemStealController;
                NetworkServer.Spawn(gameObject);
            }
        }

        public bool PlayerFilter(CharacterMaster characterMaster)
        {
            return true;
        }
    }
}
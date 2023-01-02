using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.EntityStates.CrabChest.ItemScanner
{
    internal class ItemScanningExit : BaseItemStealState
    {
        public GameObject finishScanningEffectPrefab;
        public float lendInterval;

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active && this.itemStealController)
            {
                this.itemStealController.stealInterval = lendInterval;
            }
            if (finishScanningEffectPrefab)
            {
                EffectManager.SimpleEffect(finishScanningEffectPrefab, base.gameObject.transform.position, base.gameObject.transform.rotation, false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!itemStealController.inItemSteal)
            {
                //this.itemStealController.LendImmediately(base.characterBody.inventory);
                this.outer.SetNextStateToMain();
            }
        }
    }
}
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class DropletDupeBodyBehavior : BaseItemBodyBehavior, IOnTakeDamageServerReceiver
    {
        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.DropletDupe;
        }

        private float dropUpStrength = 20f; //Default is 20 in chests but seems that its too strong

        private const float baseDupeDelay = 15f;
        private float dupeDelay;
        private float dropForwardStrength = 2f; //Default is 2
        public bool suicideReady = false;
        private MasterSuicideOnTimer masterSuicideOnTimer;

        private void Start()
        {
            PickupDropletController.onDropletHitGroundServer += onDropletHitGround;
            PurchaseInteraction.onItemSpentOnPurchase += onItemSpentOnPurchase;
            if (body.healthComponent)
                HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onTakeDamageReceivers, this);
        }

        private void onItemSpentOnPurchase(PurchaseInteraction arg1, Interactor arg2)
        {
            DisableDupingFor(2.5f, true); //The more you print the less items you get lololo
        }

        private void FixedUpdate() //I hate fixed updates but i want to nerf the item so be it
        {
            this.dupeDelay -= Time.fixedDeltaTime;
        }

        public void OnTakeDamageServer(DamageReport damageReport)
        {
            if (!NetworkServer.active) return;
            if (damageReport.isFallDamage || damageReport.dotType != DotController.DotIndex.None) return;
            if (Util.CheckRoll(3f * stack, -body.master.luck) && body.healthComponent && !suicideReady)
            {
                TELog.LogW("Rolled for death");
                suicideReady = true;
                if (body.master && masterSuicideOnTimer == null)
                {
                    masterSuicideOnTimer = (body.master.gameObject.AddComponent<MasterSuicideOnTimer>());
                    masterSuicideOnTimer.lifeTimer = 10 + UnityEngine.Random.Range(0f, 10f);
                }
                else
                    body.healthComponent.Suicide(damageReport.attacker, damageReport.attacker, DamageType.Generic);
            }
        }

        private void onDropletHitGround(ref GenericPickupController.CreatePickupInfo createPickupInfo, ref bool shouldSpawn)
        {
            if (dupeDelay >= 0) return;
            if (!createPickupInfo.pickupIndex.isValid || !shouldSpawn || createPickupInfo.pickupIndex.pickupDef.itemIndex == RoR2Content.Items.ScrapWhite.itemIndex || createPickupInfo.pickupIndex.pickupDef.itemIndex == RoR2Content.Items.ScrapGreen.itemIndex || createPickupInfo.pickupIndex.pickupDef.itemIndex == RoR2Content.Items.ScrapRed.itemIndex || createPickupInfo.pickupIndex.pickupDef.itemIndex == RoR2Content.Items.ScrapYellow.itemIndex) return;
            if (createPickupInfo.pickupIndex.pickupDef.isBoss || createPickupInfo.pickupIndex.pickupDef.isLunar || createPickupInfo.pickupIndex.pickupDef.artifactIndex != ArtifactIndex.None || createPickupInfo.pickupIndex.pickupDef.equipmentIndex != EquipmentIndex.None) return;
            if (Util.CheckRoll(8f + ((stack - 1) * 2.5f)))
            {
                EffectData effectData = new EffectData
                {
                    origin = createPickupInfo.position,
                    //networkSoundEventIndex =
                };
                //EffectManager.SpawnEffect(effectPrefab, effectData, true);
                PickupDropletController.CreatePickupDroplet(createPickupInfo.pickupIndex, createPickupInfo.position + Vector3.up * dropForwardStrength, Vector3.up * dropUpStrength);
                dupeDelay = baseDupeDelay;
            }
        }

        public void DisableDupingFor(float time, bool additive = false)
        {
            if (time > 0)
            {
                if (additive)
                {
                    dupeDelay += time;
                    return;
                }
                dupeDelay = time;
            }
        }

        private void OnDestroy()
        {
            PickupDropletController.onDropletHitGroundServer -= onDropletHitGround;
            PurchaseInteraction.onItemSpentOnPurchase -= onItemSpentOnPurchase;
            if (body && body.master && masterSuicideOnTimer)
            {
                Object.Destroy(masterSuicideOnTimer);
            }
            if (body.healthComponent)
            {
                int i = System.Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                if (i > -1)
                    HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onIncomingDamageReceivers, body.healthComponent.onIncomingDamageReceivers.Length, i);
            }
        }
    }
}
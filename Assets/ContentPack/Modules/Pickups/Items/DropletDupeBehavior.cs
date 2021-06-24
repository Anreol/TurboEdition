using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    internal class DropletDupeBehavior : CharacterBody.ItemBehavior, IOnTakeDamageServerReceiver
    {
        //If anybody else gets this item it will subscribe as well, letting the % stack in one way or another
        //However this can lead to a single item being duplicated more than once
        private float dropUpStrength = 20f;

        private float dropForwardStrength = 2f;

        private void Awake()
        {
            PickupDropletController.onDropletHitGroundServer += PickupDropletController_onDropletHitGroundServer;
        }

        public void OnTakeDamageServer(DamageReport damageReport)
        {
            if (!NetworkServer.active) return;
            if (damageReport.isFallDamage || damageReport.dotType != DotController.DotIndex.None)
            {
                return;
            }
            if (Util.CheckRoll(3f * stack, -body.master.luck) && body.healthComponent)
            {
                body.healthComponent.Suicide(damageReport.attacker, damageReport.attacker, DamageType.VoidDeath);
            }
        }

        private void PickupDropletController_onDropletHitGroundServer(ref GenericPickupController.CreatePickupInfo createPickupInfo, ref bool shouldSpawn)
        {
            if (!NetworkServer.active) return;
            if (!createPickupInfo.pickupIndex.isValid || !shouldSpawn)
            {
                return;
            }
            if (createPickupInfo.pickupIndex.pickupDef.isBoss || createPickupInfo.pickupIndex.pickupDef.isLunar || createPickupInfo.pickupIndex.pickupDef.artifactIndex != ArtifactIndex.None || createPickupInfo.pickupIndex.pickupDef.equipmentIndex != EquipmentIndex.None)
            {
                return;
            }
            if (Util.CheckRoll(8f + ((stack - 1) * 2.5f)))
            {
                PickupDropletController.CreatePickupDroplet(createPickupInfo.pickupIndex, createPickupInfo.position + Vector3.up * 1.5f, Vector3.up * dropUpStrength + createPickupInfo.position * dropForwardStrength);
            }
        }
    }
}
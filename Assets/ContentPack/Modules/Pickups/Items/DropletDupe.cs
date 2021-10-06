using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class DropletDupe : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("DropletDupe");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<DropletDupeBehavior>(stack);
        }

        internal class DropletDupeBehavior : CharacterBody.ItemBehavior, IOnTakeDamageServerReceiver
        {
            //If anybody else gets this item it will subscribe as well, letting the % stack in one way or another
            //However this can lead to a single item being duplicated more than once
            private float dropUpStrength = 20f; //Default is 20 in chests but seems that its too strong

            private float dupeDelay = 30f;
            private float dropForwardStrength = 2f; //Default is 2
            private bool dupeReady = true;
            public bool suicideReady = false;

            private void Start()
            {
                if (body.healthComponent)
                    HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onTakeDamageReceivers, this);
            }

            private void OnEnable()
            {
                PickupDropletController.onDropletHitGroundServer += PickupDropletController_onDropletHitGroundServer;
                PurchaseInteraction.onItemSpentOnPurchase += PurchaseInteraction_onItemSpentOnPurchase;
            }

            private void PurchaseInteraction_onItemSpentOnPurchase(PurchaseInteraction arg1, Interactor arg2)
            {
                DisableDupingFor(2.5f, true); //The more you print the less items you get lololo
            }

            private void FixedUpdate() //I hate fixed updates but i want to nerf the item so be it
            {
                this.dupeDelay -= Time.fixedDeltaTime;
                if (this.dupeDelay <= 0)
                {
                    dupeReady = true;
                    this.dupeDelay = 30f;
                }
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (!NetworkServer.active) return;
                if (damageReport.isFallDamage || damageReport.dotType != DotController.DotIndex.None) return;
                if (Util.CheckRoll(3f * stack, -body.master.luck) && body.healthComponent && !suicideReady)
                {
                    TELog.LogW("Rolled for death");
                    suicideReady = true;
                    if (body.master)
                    {
                        body.master.gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = 10 + UnityEngine.Random.Range(0f, 8f);
                    }
                    else
                        body.healthComponent.Suicide(damageReport.attacker, damageReport.attacker, DamageType.VoidDeath);
                }
            }

            private void PickupDropletController_onDropletHitGroundServer(ref GenericPickupController.CreatePickupInfo createPickupInfo, ref bool shouldSpawn)
            {
                if (!NetworkServer.active || !dupeReady) return;
                if (!createPickupInfo.pickupIndex.isValid || !shouldSpawn || createPickupInfo.pickupIndex.pickupDef.itemIndex == RoR2Content.Items.ScrapWhite.itemIndex || createPickupInfo.pickupIndex.pickupDef.itemIndex == RoR2Content.Items.ScrapGreen.itemIndex || createPickupInfo.pickupIndex.pickupDef.itemIndex == RoR2Content.Items.ScrapRed.itemIndex || createPickupInfo.pickupIndex.pickupDef.itemIndex == RoR2Content.Items.ScrapYellow.itemIndex) return;
                if (createPickupInfo.pickupIndex.pickupDef.isBoss || createPickupInfo.pickupIndex.pickupDef.isLunar || createPickupInfo.pickupIndex.pickupDef.artifactIndex != ArtifactIndex.None || createPickupInfo.pickupIndex.pickupDef.equipmentIndex != EquipmentIndex.None) return;
                if (Util.CheckRoll(8f + ((stack - 1) * 2.5f)))
                {
                    dupeReady = false;
                    EffectData effectData = new EffectData
                    {
                        origin = createPickupInfo.position,
                        //networkSoundEventIndex =
                    };
                    //EffectManager.SpawnEffect(effectPrefab, effectData, true);
                    PickupDropletController.CreatePickupDroplet(createPickupInfo.pickupIndex, createPickupInfo.position + Vector3.up * dropForwardStrength, Vector3.up * dropUpStrength);
                }
            }

            public void DisableDupingFor(float time, bool additive = false)
            {
                if (time > 0)
                {
                    dupeReady = false;
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
                //This SHOULDNT cause any errors because nothing should be fucking with the order of things in this list... I hope.
                if (body.healthComponent)
                {
                    int i = System.Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                    if (i > -1)
                        HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onIncomingDamageReceivers, body.healthComponent.onIncomingDamageReceivers.Length, i);
                }
            }
        }
    }
}
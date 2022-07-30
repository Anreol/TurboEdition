using RoR2;
using RoR2.Items;
using System;
using TurboEdition.Components;
using TurboEdition.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    internal class GetDamageFromHurtVictimBodyBehavior : BaseItemBodyBehavior, IStatItemBehavior, IStatBarProvider
    {
        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.SoulDevourer;
        }

        [SystemInitializer(new Type[] { typeof(PickupCatalog) })]
        public static void Initialize()
        {
            GlobalEventManager.onServerCharacterExecuted += onServerCharacterExecuted;
        }

        private static void onServerCharacterExecuted(DamageReport arg1, float arg2)
        {
            if (arg1.attackerBody.inventory.GetItemCount(TEContent.Items.SoulDevourer) > 0)
            {
                arg1.attackerBody.GetComponent<ServerListener>()?.onServerCharacterExecuted(arg1, arg2);
            }
        }

        private NetworkedBodyAttachment attachment;
        public GetDamageFromHurtVictimBodyAttachment getDamageFromHurtVictimBodyAttachment;
        internal const int stackFirst = 10;
        internal const int stackLater = 10;
        private void Start()
        {
            if (NetworkServer.active)
            {
                this.attachment = UnityEngine.Object.Instantiate<GameObject>(Assets.mainAssetBundle.LoadAsset<GameObject>("SoulDevourerBodyAttachment")).GetComponent<NetworkedBodyAttachment>();
                this.attachment.AttachToGameObjectAndSpawn(this.body.gameObject);
                this.getDamageFromHurtVictimBodyAttachment = this.attachment.GetComponent<GetDamageFromHurtVictimBodyAttachment>();
            }
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                if (!this.body.healthComponent.alive)
                {
                    UnityEngine.Object.Destroy(this);
                }
            }
            if (this.getDamageFromHurtVictimBodyAttachment)
            {
                this.getDamageFromHurtVictimBodyAttachment.stack = stack;
            }
        }

        private void OnDestroy()
        {
            if (NetworkServer.active)
            {
                if (this.attachment)
                {
                    UnityEngine.Object.Destroy(this.attachment.gameObject);
                    this.attachment = null;
                }
            }
        }

        public void RecalculateStatsEnd()
        {
            if (getDamageFromHurtVictimBodyAttachment)
                getDamageFromHurtVictimBodyAttachment.RecalculateStatsEnd();
        }

        public void RecalculateStatsStart()
        {
        }

        public StatBarData GetStatBarData()
        {
            if (getDamageFromHurtVictimBodyAttachment)
            {
                return getDamageFromHurtVictimBodyAttachment.GetStatBarData();
            }
            return new StatBarData
            {
                fillBarColor = new Color(0.3f, 1f, 0.8f, 1f),
                maxData = 0,
                currentData = 0,
                offData = "TOOLTIP_ITEM_INIT",
                sprite = TEContent.Items.SoulDevourer.pickupIconSprite,
                tooltipContent = new RoR2.UI.TooltipContent
                {
                    titleColor = ColorCatalog.GetColor(ItemTierCatalog.GetItemTierDef(TEContent.Items.SoulDevourer.tier).darkColorIndex),
                    titleToken = TEContent.Items.SoulDevourer.nameToken,
                    bodyToken = "TOOLTIP_ITEM_INIT"
                }
            };
        }

        internal class ServerListener : MonoBehaviour, IOnDamageDealtServerReceiver, IOnTakeDamageServerReceiver
        {
            public GetDamageFromHurtVictimBodyAttachment attachment;

            public void onServerCharacterExecuted(DamageReport arg1, float executionHealthLost)
            {
                if (!attachment) return;
                if (arg1.attackerBody == attachment.nba.attachedBody && attachment.currentDamage < (stackFirst + ((attachment.stack - 1) * stackLater)))
                    attachment.currentDamage += 15f;
                attachment.currentDamage = Mathf.Min(attachment.currentDamage, stackFirst + ((attachment.stack - 1) * stackLater));
            }

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (!attachment) return;
                if (attachment.currentDamage >= (stackFirst + ((attachment.stack - 1) * stackLater)) || !damageReport.victim.isHealthLow)
                    return;
                if (damageReport.combinedHealthBeforeDamage >= damageReport.victim.fullCombinedHealth && !damageReport.victim.alive)
                    attachment.currentDamage += 15f;
                if (damageReport.victim.alive)
                    attachment.currentDamage += damageReport.damageInfo.procCoefficient;
                attachment.currentDamage = Mathf.Min(attachment.currentDamage, stackFirst + ((attachment.stack - 1) * stackLater)); //it's 3am and i suck at math lol dont mind me
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (!attachment) return;
                if (!damageReport.damageInfo.rejected || !damageReport.isFallDamage || (damageReport.dotType == DotController.DotIndex.None)) //Rejects shit with dot because if you are getting dotted, you got it from being damaged
                    this.attachment.currentDamage = 0;
            }
        }
    }
}
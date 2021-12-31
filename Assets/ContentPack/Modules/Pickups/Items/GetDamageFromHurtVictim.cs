using RoR2;
using TurboEdition.Components;
using TurboEdition.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    internal class GetDamageFromHurtVictim : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("SoulDevourer");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<GetDamageFromHurtVictimBehavior>(stack);
        }

        public override void Initialize()
        {
            GlobalEventManager.onServerCharacterExecuted += onServerCharacterExecuted;
        }

        private void onServerCharacterExecuted(DamageReport arg1, float arg2)
        {
            if (arg1.attackerBody.inventory.GetItemCount(ItemCatalog.FindItemIndex("SoulDevourer")) > 0)
            {
                arg1.attackerBody.GetComponent<ServerListener>()?.onServerCharacterExecuted(arg1, arg2);
            }
        }

        internal class GetDamageFromHurtVictimBehavior : CharacterBody.ItemBehavior, IStatItemBehavior, IStatBarProvider
        {
            private NetworkedBodyAttachment attachment;
            public GetDamageFromHurtVictimBodyAttachment getDamageFromHurtVictimBodyAttachment;

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
                    sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("SoulDevourer")).pickupIconSprite,
                    tooltipContent = new RoR2.UI.TooltipContent
                    {
                        titleColor = ColorCatalog.GetColor(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("SoulDevourer")).darkColorIndex),
                        titleToken = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("SoulDevourer")).nameToken,
                        bodyToken = "TOOLTIP_ITEM_INIT"
                    }
                };
            }
        }

        internal class ServerListener : MonoBehaviour, IOnDamageDealtServerReceiver, IOnTakeDamageServerReceiver
        {
            public GetDamageFromHurtVictimBodyAttachment attachment;

            public void onServerCharacterExecuted(DamageReport arg1, float executionHealthLost)
            {
                if (!attachment) return;
                if (arg1.attackerBody == attachment.nba.attachedBody && attachment.currentDamage < (50 + ((attachment.stack - 1) * 25)))
                    attachment.currentDamage += 15f;
                attachment.currentDamage = Mathf.Min(attachment.currentDamage, 50 + ((attachment.stack - 1) * 25));
            }

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (!attachment) return;
                if (attachment.currentDamage >= (50 + ((attachment.stack - 1) * 25)) || !damageReport.victim.isHealthLow)
                    return;
                if (damageReport.combinedHealthBeforeDamage >= damageReport.victim.fullCombinedHealth && !damageReport.victim.alive)
                    attachment.currentDamage += 15f;
                if (damageReport.victim.alive)
                    attachment.currentDamage += damageReport.damageInfo.procCoefficient;
                attachment.currentDamage = Mathf.Min(attachment.currentDamage, 50 + ((attachment.stack - 1) * 25)); //it's 3am and i suck at math lol dont mind me
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
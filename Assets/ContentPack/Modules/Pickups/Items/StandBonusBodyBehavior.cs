using RoR2;
using RoR2.Items;
using TurboEdition.Components;
using TurboEdition.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    //As of July 4, I changed the code, I don't fucking recall what this shit was before but:
    //This runs in the server. This adds a attachment to the player, spawns over the net, the client gets it. The client has authority over it so it can destroy it.
    //On spawning on the body, in the server, it spawns a listener, it does the logic, sends it back to the attachment (server) which gets networked to the client.
    public class StandBonusBodyBehavior : BaseItemBodyBehavior, IStatBarProvider
    {
        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.StandBonus;
        }

        private NetworkedBodyAttachment attachment;
        public SandbagBodyAttachment sandbagBodyAttachment;

        private void Start()
        {
            if (NetworkServer.active)
            {
                this.attachment = UnityEngine.Object.Instantiate<GameObject>(Assets.mainAssetBundle.LoadAsset<GameObject>("SandbagBodyAttachment")).GetComponent<NetworkedBodyAttachment>();
                this.attachment.AttachToGameObjectAndSpawn(this.body.gameObject);
                this.sandbagBodyAttachment = this.attachment.GetComponent<SandbagBodyAttachment>();
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
                if (this.sandbagBodyAttachment)
                {
                    this.sandbagBodyAttachment.stack = stack;
                }
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

        public StatBarData GetStatBarData()
        {
            if (sandbagBodyAttachment)
            {
                return sandbagBodyAttachment.GetStatBarData();
            }
            return new StatBarData
            {
                fillBarColor = new Color(0.5f, 0.7f, 0.5f, 1f),
                maxData = 0,
                currentData = 0,
                offData = "TOOLTIP_ITEM_INIT",
                sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StandBonus")).pickupIconSprite,
                tooltipContent = new RoR2.UI.TooltipContent
                {
                    titleColor = ColorCatalog.GetColor(TEContent.Items.StandBonus.darkColorIndex),
                    titleToken = TEContent.Items.StandBonus.nameToken,
                    bodyToken = "TOOLTIP_ITEM_INIT"
                }
            };
        }

        internal class ServerListener : MonoBehaviour, IOnIncomingDamageServerReceiver
        {
            internal SandbagBodyAttachment attachment;

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (!damageInfo.rejected)
                {
                    attachment.accumulatedDamage += damageInfo.damage;
                    if (attachment.syncLerp <= 0) return;
                    damageInfo.damage /= 2 * attachment.syncLerp;
                    damageInfo.force *= 0;
                }
            }

            private void FixedUpdate()
            {
                if (!attachment.nba.attachedBody.GetNotMoving())
                {
                    attachment.accumulatedDamage = 0f;
                    attachment.syncLerp = 0f;
                    return;
                }
                attachment.syncLerp = Mathf.InverseLerp(((float)attachment.stack / 2f) * (float)attachment.nba.attachedBody.healthComponent.fullCombinedHealth, 0f, (float)attachment.accumulatedDamage);
            }
        }
    }
}
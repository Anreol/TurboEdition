using RoR2;
using TurboEdition.Items;
using TurboEdition.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    //This will be in the client and server
    [RequireComponent(typeof(NetworkedBodyAttachment))]
    public class GetDamageFromHurtVictimBodyAttachment : NetworkBehaviour, INetworkedBodyAttachmentListener
    {
        internal NetworkedBodyAttachment nba;

        private GetDamageFromHurtVictimBodyBehavior.ServerListener serverListener;

        internal int stack;

        [SyncVar]
        public float currentDamage;

        private void Awake()
        {
            this.nba = base.GetComponent<NetworkedBodyAttachment>();
        }

        public void OnAttachedBodyDiscovered(NetworkedBodyAttachment networkedBodyAttachment, CharacterBody attachedBody)
        {
            if (NetworkServer.active)
            {
                this.serverListener = attachedBody.gameObject.AddComponent<GetDamageFromHurtVictimBodyBehavior.ServerListener>();
                this.serverListener.attachment = this;
            }
            if (attachedBody.hasEffectiveAuthority)
            {
                attachedBody.GetComponent<GetDamageFromHurtVictimBodyBehavior>().getDamageFromHurtVictimBodyAttachment = this;
            }
        }
        public void OnDestroy()
        {
            if (NetworkServer.active && serverListener)
            {
                Object.Destroy(serverListener);
            }
        }
        public void RecalculateStatsEnd()
        {
            nba.attachedBody.damage += currentDamage;
        }

        public StatBarData GetStatBarData()
        {
            string overString = (currentDamage <= 0) ? "TOOLTIP_ITEM_NOBUFF_DESCRIPTION" : "";
            return new StatBarData
            {
                fillBarColor = new Color(0.3f, 1f, 0.8f, 1f),
                maxData = (GetDamageFromHurtVictimBodyBehavior.stackFirst + ((stack - 1) * GetDamageFromHurtVictimBodyBehavior.stackLater)),
                currentData = currentDamage,
                offData = "TOOLTIP_ITEM_NOBUFF_DESCRIPTION",
                sprite = TEContent.Items.SoulDevourer.pickupIconSprite,
                tooltipContent = new RoR2.UI.TooltipContent
                {
                    titleColor = ColorCatalog.GetColor(ItemTierCatalog.GetItemTierDef(TEContent.Items.SoulDevourer.tier).colorIndex),
                    titleToken = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("SoulDevourer")).nameToken,
                    bodyToken = "TOOLTIP_ITEM_SOULDEVOURER_DESCRIPTION",
                    overrideBodyText = Language.GetString(overString),
                }
            };
        }
    }
}
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

        [HideInInspector]
        public NetworkedBodyAttachment nba;

        private GetDamageFromHurtVictim.ServerListener serverListener;
        private GetDamageFromHurtVictim.GetDamageFromHurtVictimBehavior clientAuthorityBehavior;

        public int stack;

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
                this.serverListener = attachedBody.gameObject.AddComponent<GetDamageFromHurtVictim.ServerListener>();
                this.serverListener.attachment = this;
            }
            if (attachedBody.hasEffectiveAuthority)
            {
                attachedBody.GetComponent<GetDamageFromHurtVictim.GetDamageFromHurtVictimBehavior>().getDamageFromHurtVictimBodyAttachment = this; ;
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
                maxData = (50 + ((stack - 1) * 25)),
                currentData = currentDamage,
                offData = "TOOLTIP_ITEM_NOBUFF_DESCRIPTION",
                sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("SoulDevourer")).pickupIconSprite,
                tooltipContent = new RoR2.UI.TooltipContent
                {
                    titleColor = ColorCatalog.GetColor(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("SoulDevourer")).darkColorIndex),
                    titleToken = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("SoulDevourer")).nameToken,
                    bodyToken = "TOOLTIP_ITEM_SOULDEVOURER_DESCRIPTION",
                    overrideBodyText = Language.GetString(overString),
                }
            };
        }
    }
}
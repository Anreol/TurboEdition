using RoR2;
using TurboEdition.Items;
using TurboEdition.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    //This will be in the client and server
    [RequireComponent(typeof(NetworkedBodyAttachment))]
    public class SandbagBodyAttachment : NetworkBehaviour, INetworkedBodyAttachmentListener
    {
        [HideInInspector]
        public NetworkedBodyAttachment nba;

        private StandBonus.ServerListener serverListener;

        [HideInInspector]
        public int stack;

        public float accumulatedDamage;

        [SyncVar]
        public float syncLerp;

        private void Start()
        {
            this.nba = base.GetComponent<NetworkedBodyAttachment>();
        }

        public void OnAttachedBodyDiscovered(NetworkedBodyAttachment networkedBodyAttachment, CharacterBody attachedBody)
        {
            if (NetworkServer.active)
            {
                this.serverListener = attachedBody.gameObject.AddComponent<StandBonus.ServerListener>();
                this.serverListener.attachment = this;
            }
            if (attachedBody.hasEffectiveAuthority)
            {
                attachedBody.GetComponent<StandBonus.Sandbag>().sandbagBodyAttachment = this;
            }
        }

        public StatBarData GetStatBarData()
        {
            float currentData = Mathf.RoundToInt(500f * this.syncLerp);
            string overString = (currentData <= 0) ? "TOOLTIP_ITEM_NOBUFF_DESCRIPTION" : "";
            return new StatBarData
            {
                fillBarColor = new Color(0.5f, 0.7f, 0.5f, 1f),
                maxData = 500f,
                currentData = currentData,
                sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StandBonus")).pickupIconSprite,
                tooltipContent = new RoR2.UI.TooltipContent
                {
                    titleColor = ColorCatalog.GetColor(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StandBonus")).darkColorIndex),
                    titleToken = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StandBonus")).nameToken,
                    bodyToken = "TOOLTIP_ITEM_STANDBONUS_DESCRIPTION",
                    overrideBodyText = Language.GetString(overString),
                }
            };
        }
    }
}
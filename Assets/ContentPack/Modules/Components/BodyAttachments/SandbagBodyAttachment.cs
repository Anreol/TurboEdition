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
        internal NetworkedBodyAttachment nba;

        private StandBonusBodyBehavior.ServerListener serverListener;

        internal int stack;

        internal float accumulatedDamage; //Why the fuck does this need to be here if it gets only read and written in the server listener? Change it.

        [SyncVar]
        public float syncLerp;

        private void Start()
        {
            this.nba = base.GetComponent<NetworkedBodyAttachment>();
        }
        private void OnDestroy()
        {
            if (NetworkServer.active && serverListener)
            {
                Destroy(serverListener);
            }
        }
        public void OnAttachedBodyDiscovered(NetworkedBodyAttachment networkedBodyAttachment, CharacterBody attachedBody)
        {
            if (NetworkServer.active)
            {
                this.serverListener = attachedBody.gameObject.AddComponent<StandBonusBodyBehavior.ServerListener>();
                this.serverListener.attachment = this;
            }
            if (attachedBody.hasEffectiveAuthority)
            {
                attachedBody.GetComponent<StandBonusBodyBehavior>().sandbagBodyAttachment = this;
            }
        }

        public StatBarData GetStatBarData()
        {
            float currentData = Mathf.RoundToInt(50f * this.syncLerp);
            string overString = (currentData <= 0) ? "TOOLTIP_ITEM_NOBUFF_DESCRIPTION" : "";
            return new StatBarData
            {
                fillBarColor = new Color(0.5f, 0.7f, 0.5f, 1f),
                maxData = 50f,
                currentData = currentData,
                sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StandBonus")).pickupIconSprite,
                tooltipContent = new RoR2.UI.TooltipContent
                {
                    titleColor = ColorCatalog.GetColor(TEContent.Items.StandBonus.darkColorIndex),
                    titleToken = TEContent.Items.StandBonus.nameToken,
                    bodyToken = "TOOLTIP_ITEM_STANDBONUS_DESCRIPTION",
                    overrideBodyText = Language.GetString(overString),
                }
            };
        }
    }
}
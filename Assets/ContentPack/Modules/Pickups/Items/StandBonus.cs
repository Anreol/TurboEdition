using RoR2;
using TurboEdition.Components;
using TurboEdition.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    //Networking can suck my dick so im copying and pasting my soul devourer solution, cant be bothered to mess with new stuff now
    //i find it a bad practice due to the amount of components (2 in go, 1 in client, 1 in server) it uses to function
    //This wouldnt be such a pain in the ass if RecalcStats was easier to use
    public class StandBonus : Item
    {
        public override ItemDef itemDef { get; set; } = TEContent.Items.StandBonus;

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<Sandbag>(stack);
        }

        internal class Sandbag : CharacterBody.ItemBehavior, IStatBarProvider
        {
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
                }
                if (this.sandbagBodyAttachment)
                {
                    this.sandbagBodyAttachment.stack = stack;
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
                        titleColor = ColorCatalog.GetColor(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StandBonus")).darkColorIndex),
                        titleToken = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StandBonus")).nameToken,
                        bodyToken = "TOOLTIP_ITEM_INIT"
                    }
                };
            }
        }

        internal class ServerListener : MonoBehaviour, IOnIncomingDamageServerReceiver
        {
            public SandbagBodyAttachment attachment;

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
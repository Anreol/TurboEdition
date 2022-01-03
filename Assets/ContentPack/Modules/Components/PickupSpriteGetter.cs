using RoR2;
using RoR2.UI.LogBook;
using UnityEngine;

namespace TurboEdition.Components
{
    internal class PickupSpriteGetter : MonoBehaviour
    {
        public SpriteRenderer spriteComponent;
        public bool searchIfNull = true;

        private void Start()
        {
            if (spriteComponent == null && searchIfNull)
                spriteComponent = transform.gameObject.GetComponentInChildren<SpriteRenderer>();
            if (spriteComponent == null)
            {
                return;
            }
            if (SceneCatalog.mostRecentSceneDef.baseSceneName == "logbook")
            {
                GameObject panel = GameObject.Find("LogbookEntryPanel (JUICED)");
                LogBookPage page = panel.GetComponent<LogBookPage>();
                if (page)
                {
                    if (page.pageBuilder != null && page.pageBuilder.entry != null)
                    {
                        //spriteComponent.sprite.texture = (Texture2D)page.pageBuilder.entry.iconTexture;
                        if (page.pageBuilder.entry.extraData.GetType() == typeof(PickupIndex))
                        {
                            if (AssignFromPickupDef(PickupCatalog.GetPickupDef((PickupIndex)page.pageBuilder.entry.extraData)))
                                Destroy(this);
                        }
                    }
                }
            }
            GenericPickupController genericPickupController = GetComponentInParent<GenericPickupController>();
            if (genericPickupController)
            {
                if (AssignFromPickupDef(PickupCatalog.GetPickupDef(genericPickupController.pickupIndex)))
                {
                    Destroy(this);
                }
            }

            ShopTerminalBehavior shopTerminalBehavior = GetComponentInParent<ShopTerminalBehavior>();
            if (shopTerminalBehavior)
            {
                if (AssignFromPickupDef(PickupCatalog.GetPickupDef(shopTerminalBehavior.pickupIndex)))
                {
                    Destroy(this);
                }
            }
        }

        private bool AssignFromPickupDef(PickupDef pickupDef)
        {
            ItemDef itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
            if (itemDef != null)
            {
                //Sprite sprite = Sprite.Create(itemDef.pickupIconSprite.texture, spriteComponent.sprite.rect, spriteComponent.sprite.pivot);
                spriteComponent.sprite = itemDef.pickupIconSprite;
                return true;
            }
            EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex);
            if (equipmentDef != null)
            {
                //Sprite sprite = Sprite.Create(equipmentDef.pickupIconSprite.texture, spriteComponent.sprite.rect, spriteComponent.sprite.pivot);
                spriteComponent.sprite = equipmentDef.pickupIconSprite;
                return true;
            }
            return false;
        }
    }
}
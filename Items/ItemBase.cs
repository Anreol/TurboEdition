using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;

namespace TurboEdition.Items
{
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }

        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; set; } = new ItemTag[] { };

        public abstract string ItemModelPath { get; }
        public abstract string ItemIconPath { get; }

        public ItemIndex Index;

        public virtual bool CanRemove { get; } = true;

        public virtual bool AIBlacklisted { get; set; } = false;

        protected abstract void Initialization();

        /// <summary>
        /// Only override when you know what you are doing, or call base.Init()!
        /// </summary>
        /// <param name="config"></param>
        internal virtual void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Initialization();
            Hooks();
        }

        public virtual void CreateConfig(ConfigFile config) { }

        protected virtual void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
        protected void CreateItem()
        {
            if (AIBlacklisted)
            {
                ItemTags = new List<ItemTag>(ItemTags) { ItemTag.AIBlacklist }.ToArray();
            }
            ItemDef itemDef = new RoR2.ItemDef()
            {
                name = "ITEM_" + ItemLangTokenName,
                nameToken = "ITEM_" + ItemLangTokenName + "_NAME",
                pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP",
                descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION",
                loreToken = "ITEM_" + ItemLangTokenName + "_LORE",
                pickupModelPath = ItemModelPath,
                pickupIconPath = ItemIconPath,
                hidden = false,
                canRemove = CanRemove,
                tier = Tier
            };
            if (ItemTags.Length > 0)
            {
                itemDef.tags = ItemTags;
            }
            var itemDisplayRules = CreateItemDisplayRules();
            Index = ItemAPI.Add(new CustomItem(itemDef, itemDisplayRules));
        }

        public virtual void Hooks() { }

        //Based on ThinkInvis' methods
        public int GetCount(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(Index);
        }

        public int GetCount(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCount(Index);
        }

        public int GetCountSpecific(CharacterBody body, ItemIndex itemIndex)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(itemIndex);
        }
    }
}
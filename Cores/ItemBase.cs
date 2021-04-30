using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

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

        public abstract GameObject ItemModel { get; }
        public abstract Sprite ItemIcon { get; }

        public ItemDef ItemDef;

        public virtual bool CanRemove { get; } = true;

        public virtual bool AIBlacklisted { get; set; } = false;
        public virtual bool BrotherBlacklisted { get; set; } = false;

        protected abstract void Initialization();

        /// <summary>
        /// This method structures your code execution of this class. An example implementation inside of it would be:
        /// <para>CreateConfig(config);</para>
        /// <para>CreateLang();</para>
        /// <para>CreateItem();</para>
        /// <para>Hooks();</para>
        /// <para>This ensures that these execute in this order, one after another, and is useful for having things available to be used in later methods.</para>
        /// <para>P.S. CreateItemDisplayRules(); does not have to be called in this, as it already gets called in CreateItem();</para>
        /// </summary>
        /// <param name="config">The config file that will be passed into this from the main class.</param>
        internal virtual void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Initialization();
            Hooks();
        }

        protected virtual void CreateConfig(ConfigFile config)
        {
        }

        //Change this to include ModInitals when I figure out how to make it so this file can access it
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
            if (BrotherBlacklisted)
            {
                ItemTags = new List<ItemTag>(ItemTags) { ItemTag.BrotherBlacklist }.ToArray();
            }

            ItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ItemDef.name = "ITEM_" + ItemLangTokenName;
            ItemDef.nameToken = "ITEM_" + ItemLangTokenName + "_NAME";
            ItemDef.pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP";
            ItemDef.descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION";
            ItemDef.loreToken = "ITEM_" + ItemLangTokenName + "_LORE";
            ItemDef.pickupModelPrefab = ItemModel;
            ItemDef.pickupIconSprite = ItemIcon;
            ItemDef.hidden = false;
            ItemDef.canRemove = CanRemove;
            ItemDef.tier = Tier;

            if (ItemTags.Length > 0) { ItemDef.tags = ItemTags; }

            ItemAPI.Add(new CustomItem(ItemDef, CreateItemDisplayRules()));
        }

        public virtual void Hooks()
        {
        }

        //I sometimes think to myself, is there something as "too many methods?"
        public static int GetUniqueCountFromPlayers(ItemDef itemDef, bool requiresAlive = false)
        {
            var playerUnique = PlayerCharacterMasterController.instances; //Remove this and replace it with ANYTHING master, not just players. Add parameter for a teamindex
            int stackCount = 0;
            for (int i = 0; i < playerUnique.Count; i++)
            {
                if (playerUnique[i].master.inventory.GetItemCount(itemDef) > 0 && (!requiresAlive || !playerUnique[i].master.IsDeadAndOutOfLivesServer()))
                {
                    stackCount++;
                }
            }
            return stackCount;
        }

        public static int GetCountFromPlayers(ItemDef itemDef, bool requiresAlive = false)
        {
            var playerTotal = PlayerCharacterMasterController.instances; //Remove this and replace it with ANYTHING master, not just players
            int totalCount = 0;
            for (int i = 0; i < playerTotal.Count; i++)
            {
                if (playerTotal[i].master.inventory.GetItemCount(itemDef) > 0 && (!requiresAlive || !playerTotal[i].master.IsDeadAndOutOfLivesServer()))
                {
                    totalCount += playerTotal[i].master.inventory.GetItemCount(itemDef);
                }
            }
            return totalCount;
        }

        public static int GetCountHighestFromPlayers(ItemDef itemDef, bool requiresAlive = false)
        {
            var playerTotal = PlayerCharacterMasterController.instances;
            int highestCount = 0;
            for (int i = 0; i < playerTotal.Count; i++)
            {
                if (playerTotal[i].master.inventory.GetItemCount(itemDef) > 0 && (!requiresAlive || !playerTotal[i].master.IsDeadAndOutOfLivesServer()))
                {
                    highestCount = Mathf.Max(highestCount, playerTotal[i].master.inventory.GetItemCount(itemDef));
                }
            }
            return highestCount;
        }

        //Based on ThinkInvis' methods
        public int GetCount(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(ItemDef);
        }

        public int GetCount(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCount(ItemDef);
        }

        public static int GetCountSpecific(CharacterBody body, ItemDef itemDef)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(itemDef);
        }

        /*
        public static int GetCountOnDeployables(CharacterMaster master)
        {
            if(master == null) return 0;
            var dplist = master.deployablesList;
            if(dplist == null) return 0;
            int count = 0;
            foreach(DeployableInfo d in dplist) {
                count += GetCount(d.deployable.gameObject.GetComponent<Inventory>());
            }
            return count;
        }
        */
    }
}

public abstract class ItemBase<T> : TurboEdition.Items.ItemBase where T : ItemBase<T>
{
    public static T Instance { get; private set; }

    public ItemBase()
    {
        Instance = this as T;
    }
}
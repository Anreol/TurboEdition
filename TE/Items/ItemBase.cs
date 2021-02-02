using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;

namespace TurboEdition.Items
{
    //This is also based on TILER, thank you TI, very cool!
    public abstract class ItemBase<T>:ItemBase where T : ItemBase<T> 
    {
        public static T instance {get;private set;}
        public ItemBase() 
        {
                if(instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBoilerplate/Item was instantiated twice");
                instance = this as T;
        }
    }

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

        public static ItemIndex Index;

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

        public static int GetUniqueItemCountForTeam(TeamIndex teamIndex, ItemIndex itemIndex, bool requiresAlive, bool requiresConnected = true)
        {
            int num = 0;
            for (int i = 0; i < CharacterMaster.readOnlyInstancesList.Count; i++)
            {
                CharacterMaster characterMaster = CharacterMaster.readOnlyInstancesList[i];
                if (characterMaster.teamIndex == teamIndex && (!requiresAlive || characterMaster.hasBody) && (!requiresConnected || !characterMaster.playerCharacterMasterController || characterMaster.playerCharacterMasterController.isConnected) && characterMaster.inventory.GetItemCount(itemIndex) >= 1)
                {
                    num ++;
                }
            }
            return num;
        }

        //Based on ThinkInvis' methods
        public static int GetCount(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(Index);
        }

        public static int GetCount(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCount(Index);
        }

        public static int GetCountSpecific(CharacterBody body, ItemIndex itemIndex)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(itemIndex);
        }

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
    }
}
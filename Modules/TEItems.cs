using BepInEx;
using BepInEx.Logging;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TurboEdition.Artifacts;
using TurboEdition.Equipment;
using TurboEdition.Items;
using UnityEngine;

namespace TurboEdition.Modules
{
    class TEItems
    {
        public static TEItems instance;
        public List<ItemDef> ItemDefs = new List<ItemDef>();
        public List<ItemBase> ItemList = new List<ItemBase>();

        public TEItems()
        {
            instance = this;
        }

        public void InitItems()
        {
            var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

            foreach (var itemType in ItemTypes)
            {
                ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
                if (ValidateItem(item, ItemList))
                {
                    item.Init(TurboEdition.instance.Config);
                }
            }
        }

        /// <summary>
        /// A helper to easily set up and initialize an item from your item classes if the user has it enabled in their configuration files.
        /// <para>Additionally, it generates a configuration for each item to allow blacklisting it from AI.</para>
        /// </summary>
        /// <param name="item">A new instance of an ItemBase class. e.g. "new ExampleItem()"</param>
        /// <param name="itemList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateItem(ItemBase item, List<ItemBase> itemList)
        {
            var enabled = TurboEdition.instance.Config.Bind<bool>("Item: " + item.ItemName, "Enable Item?", true, "Should this item appear in runs?").Value;
            var aiBlacklist = TurboEdition.instance.Config.Bind<bool>("Item: " + item.ItemName, "Blacklist Item from AI Use?", item.AIBlacklisted, "Should the AI not be able to obtain this item?").Value;
            var brotherBlacklist = TurboEdition.instance.Config.Bind<bool>("Item: " + item.ItemName, "Blacklist Item from Mithrix Use?", item.BrotherBlacklisted, "Should Mithrix not be able to obtain this item?").Value;
            if (enabled)
            {
                itemList.Add(item);
                if (aiBlacklist)
                {
                    item.AIBlacklisted = true;
                }
                if (brotherBlacklist)
                {
                    item.BrotherBlacklisted = true;
                }
            }
            return enabled;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using RoR2;
using R2API;
using R2API.Utils;
using UnityEngine;
using System.Linq;
using TurboEdition.Equipment;
using TurboEdition.Items;
using System.Reflection;

//Dumbfuck's first (not really) ror2 mod
//Programming is fun!
namespace TurboEdition
{
    [R2APISubmoduleDependency("ResourcesAPI")]
    [R2APISubmoduleDependency("AssetAPI")]
    [R2APISubmoduleDependency("ItemAPI")]
    [R2APISubmoduleDependency("ItemDropAPI")]
    [R2APISubmoduleDependency("LanguageAPI")]
    [R2APISubmoduleDependency("BuffAPI")]
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    [BepInDependency("com.bepis.r2api")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class TurboEdition : BaseUnityPlugin
    {
        //Lets get cool mod info.
        public const string ModVer =
        #if DEBUG
            "000." +
        #endif
            "0.0.1";

        public const string ModName = "TurboEdition";
        public const string ModInitals = "TE";
        public const string ModGuid = "com.Anreol." + ModName;

        private const string assetProvider = "@TurboEdition";
        internal const string assetPrefix = assetProvider + ":";
        internal static ManualLogSource _logger; // allow access to the logger across the plugin classes


        public static AssetBundle MainAssets;

        public List<ItemBase> Items = new List<ItemBase>();
        public List<EquipmentBase> Equipments = new List<EquipmentBase>();


        //Buffs
        public static BuffIndex fortifiedBuff { get; private set; }
        public static BuffIndex tauntingBuff { get; private set; }
        public static BuffIndex heatedBuff { get; private set; }
        //Debuffs
        public static BuffIndex shockedBuff { get; private set; }
        public static BuffIndex buzzedBuff { get; private set; }
        public static BuffIndex transformedBuff { get; private set; }
        public static BuffIndex disablelunarBuff { get; private set; }
        public static BuffIndex oiledBuff { get; private set; }


        public void Awake()
        {
            _logger = Logger;
            CommandHelper.AddToConsoleWhenReady();
            #if DEBUG
            Logger.LogWarning("Running TurboEdition DEBUG build.");
            #endif

            // Don't know how to create/use an asset bundle, or don't have a unity project set up?
            // Look here for info on how to set these up: https://github.com/KomradeSpectre/AetheriumMod/blob/rewrite-master/Tutorials/Item%20Mod%20Creation.md#unity-project

            Logger.LogWarning("Getting assets...");
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TurboEdition.turboedition_assets"))
            {
                var MainAssets = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider($"@{ModName}", MainAssets);
                ResourcesAPI.AddProvider(provider);
            }

            //This section automatically scans the project for all items
            Logger.LogWarning("Adding items...");
            var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

            foreach (var itemType in ItemTypes)
            {
                ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
                if (ValidateItem(item, Items))
                {
                    item.Init(Config);
                }
            }

            //this section automatically scans the project for all equipment
            Logger.LogWarning("Adding equipment...");
            var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));

            foreach (var equipmentType in EquipmentTypes)
            {
                EquipmentBase equipment = (EquipmentBase)System.Activator.CreateInstance(equipmentType);
                if (ValidateEquipment(equipment, Equipments))
                {
                    equipment.Init(Config);
                }
            }
        }

        //Debugging
        #if DEBUG
        bool DEBUGcheckingInput = false;
        public void Update()
        {
            var i3 = Input.GetKeyDown(KeyCode.Keypad3);
            var i4 = Input.GetKeyDown(KeyCode.Keypad4);
            var i5 = Input.GetKeyDown(KeyCode.Keypad5);
            var i6 = Input.GetKeyDown(KeyCode.Keypad6);
            var i7 = Input.GetKeyDown(KeyCode.Keypad7);
            var i8 = Input.GetKeyDown(KeyCode.Keypad8);
            if (i3 || i4 || i5 || i6 || i7 || i8)
            {
                var trans = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                List<PickupIndex> spawnList;
                if (i3) spawnList = Run.instance.availableTier1DropList;
                else if (i4) spawnList = Run.instance.availableTier2DropList;
                else if (i5) spawnList = Run.instance.availableTier3DropList;
                else if (i6) spawnList = Run.instance.availableEquipmentDropList;
                //else if (i7) spawnList = Run.instance.availableTierSynergyDropList;
                else spawnList = Run.instance.availableLunarDropList;

                PickupDropletController.CreatePickupDroplet(spawnList[Run.instance.spawnRng.RangeInt(0, spawnList.Count)], trans.position, new Vector3(0f, -5f, 0f));
            }
        }
        #endif

        [ConCommand(commandName ="itemsonteam",flags=ConVarFlags.None,helpText ="dumsp the amount of the item on the team")]
        public static void ccItemsOnTeam(ConCommandArgs args)
        {
            Debug.Log("Count:"+ ItemBase.GetCountFromPlayers((ItemIndex)args.GetArgInt(0), false));
            Debug.Log("uCount:"+ItemBase.GetUniqueCountFromPlayers((ItemIndex)args.GetArgInt(0), false));
        }


        /// <summary>
        /// A helper to easily set up and initialize an item from your item classes if the user has it enabled in their configuration files.
        /// <para>Additionally, it generates a configuration for each item to allow blacklisting it from AI.</para>
        /// </summary>
        /// <param name="item">A new instance of an ItemBase class. e.g. "new ExampleItem()"</param>
        /// <param name="itemList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateItem(ItemBase item, List<ItemBase> itemList)
        {
            var enabled = Config.Bind<bool>("Item: " + item.ItemName, "Enable Item?", true, "Should this item appear in runs?").Value;
            var aiBlacklist = Config.Bind<bool>("Item: " + item.ItemName, "Blacklist Item from AI Use?", false, "Should the AI not be able to obtain this item?").Value;
            if (enabled)
            {
                itemList.Add(item);
                if (aiBlacklist)
                {
                    item.AIBlacklisted = true;
                }
            }
            return enabled;
        }

        /// <summary>
        /// A helper to easily set up and initialize an equipment from your equipment classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="equipment">A new instance of an EquipmentBase class. E.g. "new ExampleEquipment()"</param>
        /// <param name="equipmentList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateEquipment(EquipmentBase equipment, List<EquipmentBase> equipmentList)
        {
            if (Config.Bind<bool>("Equipment: " + equipment.EquipmentName, "Enable Equipment?", true, "Should this equipment appear in runs?").Value)
            {
                equipmentList.Add(equipment);
                return true;
            }
            return false;
        }
    }
}
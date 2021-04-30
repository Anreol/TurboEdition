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

//Dumbfuck's first (not really) ror2 mod
//Programming is fun!
//le frankestein monster born from komrade's item boiler plate and how ss2 is structured

//TODOS:
//Adding this in here because its about the mod overall.
//Get yer shit together and standarize the chances of items and stuff,
//i.e for 5%, some items have it as 5f, and others as 0.05f, and I'm losing my mind over it
//Get a component helper or something, like SS2, really streamlines stuff and standarizes how everything is done
//Im sick of having to do null checks everytime i want to see if a component exists to see if i need to delete it or to add it

//Uhhhh im manually setting up if an item is AI blacklisted / Mithrix Blacklisted, but by default they are always false
//Fix that so we dont give the user a headache when Mithrix suddenly gets all your broken cables and you cannot hit him

//When game update drops / R2API updates:
//create a lang file, find out if r2api automatically loads different lang files depending on your system or i have to setup the spanish language myself
//Add to boilerplate the shader setup which is essentially going item by item (models) and giving them hopoo's shaders
//Find out what the hell is "Item behaviour", seems like a standarized way to add item components to a body

namespace TurboEdition
{
    [R2APISubmoduleDependency("AssetAPI")]
    [R2APISubmoduleDependency("ItemAPI")]
    [R2APISubmoduleDependency("ItemDropAPI")]
    [R2APISubmoduleDependency("LanguageAPI")]
    [R2APISubmoduleDependency("BuffAPI")]
    [R2APISubmoduleDependency("PrefabAPI")]
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    [BepInDependency("com.bepis.r2api")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(ModGuid, ModName, ModVer)]

    //SS2 soft dependency
    [BepInDependency("com.TeamMoonstorm.Starstorm2", BepInDependency.DependencyFlags.SoftDependency)]
    //[BepInDependency("com.niwith.DropInMultiplayer", BepInDependency.DependencyFlags.SoftDependency)]
    //[BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]

    public class TurboEdition : BaseUnityPlugin
    {
        internal const string ModVer =
#if DEBUG
            "2060." +
#endif
            "0.0.1";

        internal const string ModName = "TurboEdition";
        internal const string ModInitals = "TE";
        internal const string ModGuid = "com.Anreol." + ModName;

        public static TurboEdition instance;

        private const string assetProvider = "@TurboEdition";
        internal const string assetPrefix = assetProvider + ":";
        internal static ManualLogSource _logger; // allow access to the logger across the plugin classes

        public static AssetBundle MainAssets;

        public List<ItemBase> Items = new List<ItemBase>();
        public List<EquipmentBase> Equipments = new List<EquipmentBase>();
        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();

        public static bool starstormInstalled = false;

        public void Awake()
        {
#if DEBUG
            Logger.LogWarning("Running TurboEdition DEBUG build. PANIC!");
#endif
            instance = this;
            _logger = Logger;

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TeamMoonstorm.Starstorm2")) { starstormInstalled = true; };
            Initialize();
            CommandHelper.AddToConsoleWhenReady();

            //new Modules.ContentPacks().Initialize();
        }

        private void Initialize()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TurboEdition.turboedition_assets"))
            {
                var MainAssets = AssetBundle.LoadFromStream(stream);
            }

            var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));
            foreach (var itemType in ItemTypes)
            {
                ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
                if (ValidateItem(item, Items))
                {
                    item.Init(Config);
                }
            }

            var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));
            foreach (var equipmentType in EquipmentTypes)
            {
                EquipmentBase equipment = (EquipmentBase)System.Activator.CreateInstance(equipmentType);
                if (ValidateEquipment(equipment, Equipments))
                {
                    equipment.Init(Config);
                }
            }

            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));
            foreach (var artifactType in ArtifactTypes)
            {
#if DEBUG
                Logger.LogWarning("Initializing artifacts, adding: " + artifactType + " in a ArtifactTypes of count " + ArtifactTypes.Count());
#endif
                ArtifactBase artifact = (ArtifactBase)System.Activator.CreateInstance(artifactType);
                if (ValidateArtifact(artifact, Artifacts))
                {
#if DEBUG
                    Logger.LogWarning("Validated artifact, and intializing config of " + artifact);
#endif
                    artifact.Init(Config);
                }
            }
        }

        [ConCommand(commandName = "ccInspectArtifactCat", flags = ConVarFlags.None, helpText = "Dumps info from the game's Artifact Catalog")]
        public static void ccInspectArtifactCat(ConCommandArgs args)
        {
            Debug.Log("Count:" + ArtifactCatalog.artifactCount);
            Debug.Log("Found artifact def:" + ArtifactCatalog.FindArtifactDef(args.ToString()));
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
            var aiBlacklist = Config.Bind<bool>("Item: " + item.ItemName, "Blacklist Item from AI Use?", item.AIBlacklisted, "Should the AI not be able to obtain this item?").Value;
            var brotherBlacklist = Config.Bind<bool>("Item: " + item.ItemName, "Blacklist Item from Mithrix Use?", item.BrotherBlacklisted, "Should Mithrix not be able to obtain this item?").Value;
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

        /// <summary>
        /// A helper to easily set up and initialize an artifact from your artifact classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="artifact">A new instance of an ArtifactBase class. E.g. "new ExampleArtifact()"</param>
        /// <param name="artifactList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            if (Config.Bind<bool>("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true, "Should this artifact be enabled in game?").Value)
            {
#if DEBUG
                Logger.LogWarning("Validating artifact " + artifact + ", user has it set to enabled, so adding it to " + artifactList);
#endif
                artifactList.Add(artifact);
                return true;
            }
            return false;
        }
    }
}
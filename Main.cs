using BepInEx;
using BepInEx.Logging;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TurboEdition.Modules;
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

//Get a way to parse the config file into the modules correctly so we dont have to do TurboEdition.instance.Config in all of them.
//See if we can concentrate all of them into Modules/ModConfig

namespace TurboEdition
{
    [R2APISubmoduleDependency("AssetAPI")]
    [R2APISubmoduleDependency("ItemAPI")]
    [R2APISubmoduleDependency("ItemDropAPI")]
    [R2APISubmoduleDependency("LanguageAPI")]
    [R2APISubmoduleDependency("BuffAPI")]
    [R2APISubmoduleDependency("PrefabAPI")]
    [R2APISubmoduleDependency("ArtifactAPI")]
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
            "9999." +
#endif
            "0.0.1";

        internal const string ModName = "TurboEdition";
        internal const string ModInitals = "TE";
        internal const string ModGuid = "com.Anreol." + ModName;

        public static TurboEdition instance;
        internal static ManualLogSource _logger; // allow access to the logger across the plugin classes
        public static AssetBundle MainAssets;

        private static bool starstormInstalled = false;

        public void Awake()
        {
            #if DEBUG
            Logger.LogWarning("Running TurboEdition DEBUG build. PANIC!");
            #endif
            instance = this;
            _logger = Logger;

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TeamMoonstorm.Starstorm2")) { starstormInstalled = true; };
            BootUp();
            

            CommandHelper.AddToConsoleWhenReady();
        }

        private void BootUp()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TurboEdition.turboedition_assets"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }
            CreateContentPack();
        }

        private void CreateContentPack()
        {
            new TEItems().InitItems();
            new TEEquipments().InitEquips();
            new TEArtifacts().InitArtfs();
            new ContentPacks().Initialize();
        }

            [ConCommand(commandName = "ccInspectArtifactCat", flags = ConVarFlags.None, helpText = "Dumps info from the game's Artifact Catalog")]
            public static void ccInspectArtifactCat(ConCommandArgs args)
            {
                Debug.Log("Count:" + ArtifactCatalog.artifactCount);
                Debug.Log("Found artifact def:" + ArtifactCatalog.FindArtifactDef(args.ToString()));
            }
    }
}
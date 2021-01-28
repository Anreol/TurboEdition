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

//Dumbfuck's first (not really) ror2 mod
//Programming is fun!
namespace Anreol.TurboEdition
{
    [R2APISubmoduleDependency("ResourcesAPI")]
    [R2APISubmoduleDependency("AssetAPI")]
    [R2APISubmoduleDependency("ItemAPI")]
    [R2APISubmoduleDependency("ItemDropAPI")]
    [R2APISubmoduleDependency("LanguageAPI")]
    [R2APISubmoduleDependency("BuffAPI")]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class TurboEdition : BaseUnityPlugin
    {
        //Lets get cool mod info.
        public const string ModVer =
                "0.0.1"
        #if DEBUG
            + ".0" //Did you know you cannot add text here, despite Version being a string? LAME!
        #endif
            ;
        public const string ModName = "TurboEdition";
        public const string ModGuid = "com.Anreol." + ModName;

        private const string assetProvider = "@TurboEdition";
        internal const string assetPrefix = assetProvider + ":";
        internal static ManualLogSource _logger; // allow access to the logger across the plugin classes


        //Buffs
        public static BuffIndex meleearmorBuff { get; private set; }
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

            #if DEBUG
            Logger.LogWarning("Running TurboEdition DEBUG build.");
            #endif

            //These get used in item names and others
            //Its better than using above's ModName and other because I dont have to indicate TurboEdition.
            //lol
            new ModInfo
            {
                displayName = "Turbo Edition",
                longIdentifier = "TurboEdition",
                shortIdentifier = "TE"
            };

            //AssetBundleResourcesProvider provider = new AssetBundleResourcesProvider(assetProvider, Assets.turboeditionAssetBundle);
            //ResourcesAPI.AddProvider(provider);

            Logger.LogWarning("Adding buffs...");
            var meleearmorBuffDef = new CustomBuff(new BuffDef
            {
                buffColor = Color.cyan,
                canStack = false,
                isDebuff = true,
                name = "TEmeleearmor",
                iconPath = assetPrefix + "Assets/TurboEdition/icons/meleearmor_icon.png"
            });
            meleearmorBuff = BuffAPI.Add(meleearmorBuffDef);

            Logger.LogWarning("Adding items...");
            MeleeArmor.Init();

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

        private void MakeCombo()
        {

        }
    }

    public struct ModInfo
    {
        public string displayName;
        public string longIdentifier;
        public string shortIdentifier;
    }
    //This is from TILER2 lmao

}
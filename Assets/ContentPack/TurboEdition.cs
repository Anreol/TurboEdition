﻿using BepInEx;
using UnityEngine;

//Dumbfuck's first (not really) ror2 mod
//Programming is fun!
//Now in Thunderkit!

namespace TurboEdition
{
    [BepInDependency("com.bepis.r2api")]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(ModGuid, ModIdentifier, ModVer)]

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

        internal const string ModIdentifier = "TurboEdition";
        internal const string ModGuid = "com.Anreol." + ModIdentifier;

        public void Awake()
        {
#if DEBUG
            Debug.LogWarning("Running TurboEdition DEBUG build. PANIC!");
#endif
            Assets.Initialize();
            InitPickups.Initialize();
            InitBuffs.Initialize();
            ApplyShaders();
            ContentPackProvider.Initialize();
#if DEBUG
            //Components.MaterialControllerComponents.AttachControllerFinderToObjects(Assets.mainAssetBundle);
#endif
        }

        public static void ApplyShaders()
        {
            var materials = Assets.mainAssetBundle.LoadAllAssets<Material>();
            foreach (Material material in materials)
                if (material.shader.name.StartsWith("StubbedShader"))
                    material.shader = Resources.Load<Shader>("shaders" + material.shader.name.Substring(13));
        }
    }

    /* Apparently this is all unused because Ghor didn't finish it....
     * leaving it just in case tho...
     * im sad...

    public class TurboRoR2Mod : RoR2Mod
    {
        public RoR2.ContentManagement.IContentPackProvider contentPackProvider = ???;
        public TurboRoR2Mod() : base (Mod.FromJsonFile("TurboEdition", pathToFile))
        {
        }
    }*/
}
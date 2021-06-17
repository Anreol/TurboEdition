using BepInEx;
using BepInEx.Logging;
using RoR2;

//Dumbfuck's first (not really) ror2 mod
//Programming is fun!
//le frankestein monster born from komrade's item boiler plate and how ss2 is structured

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

        internal static ManualLogSource _logger; // allow access to the logger across the plugin classes

        public void Awake()
        {
#if DEBUG
            _logger.LogWarning("Running TurboEdition DEBUG build. PANIC!");
#endif
            //Le items
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody cb)
        {
            if (cb.inventory)
            {
                cb.AddItemBehavior<TurboItemManager>(1); //ItemBehavior that manages ItemBehaviors
            }
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
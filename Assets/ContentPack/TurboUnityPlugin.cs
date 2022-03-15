using BepInEx;
using RoR2.ContentManagement;
using UnityEngine;

//Dumbfuck's first (not really) ror2 mod
//Programming is fun!
//Now in Thunderkit!
//Mod wouldnt be possible without Kevin and his contributions to SS2!

namespace TurboEdition
{
    [BepInPlugin(ModGuid, ModIdentifier, ModVer)]
    public class TurboUnityPlugin : BaseUnityPlugin
    {
        internal const string ModVer =
#if DEBUG
            "9999." +
#endif
            "0.0.6";

        internal const string ModIdentifier = "TurboEdition";
        internal const string ModGuid = "com.Anreol." + ModIdentifier;

        public static TurboUnityPlugin instance;
        public static PluginInfo pluginInfo;

        public string identifier
        {
            get
            {
                return ModGuid;
            }
        }

        public void Awake()
        {
            Debug.Log("Running Turbo Edition!");
#if DEBUG
            TELog.logger = Logger;
            TELog.LogW("Running TurboEdition DEBUG build. PANIC!");
#endif
            pluginInfo = Info;
            instance = this;

            ContentManager.collectContentPackProviders += (addContentPackProvider) => addContentPackProvider(new TEContent());
#if DEBUG
            //Components.MaterialControllerComponents.AttachControllerFinderToObjects(Assets.mainAssetBundle);
#endif
        }
    }
}
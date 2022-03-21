using BepInEx;
using RoR2;
using RoR2.ContentManagement;
using System.Linq;
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

        public void Awake()
        {
            Debug.Log("Running Turbo Edition!");
#if DEBUG
            TELog.logger = Logger;
            TELog.LogW("Running TurboEdition DEBUG build. PANIC!");
#endif
            pluginInfo = Info;
            instance = this;

            On.RoR2.Language.SetFolders += StinkyShit;
            ContentManager.collectContentPackProviders += (addContentPackProvider) => addContentPackProvider(new TEContent());
#if DEBUG
            //Components.MaterialControllerComponents.AttachControllerFinderToObjects(Assets.mainAssetBundle);
#endif
        }

        private void StinkyShit(On.RoR2.Language.orig_SetFolders orig, Language self, System.Collections.Generic.IEnumerable<string> newFolders)
        {
            if (System.IO.Directory.Exists(Assets.languageRoot))
            {
                /*var allLanguageFolders = System.IO.Directory.EnumerateDirectories(Assets.languageRoot);
                foreach (var singleFolder in allLanguageFolders)
                {
                    int languageName = singleFolder.LastIndexOf("\\") + 1;
                    if ((newFolders.First().Contains(singleFolder.Substring(languageName, singleFolder.Length - languageName))))
                    {
                        newFolders = newFolders.Concat(new[] { singleFolder });
                    }
                }*/
                var dirs = System.IO.Directory.EnumerateDirectories(System.IO.Path.Combine(Assets.languageRoot), self.name);
                orig(self, newFolders.Union(dirs));
                return;
            }
            orig(self, newFolders);
        }
    }
}
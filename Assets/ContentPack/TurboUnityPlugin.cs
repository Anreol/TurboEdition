using BepInEx;
using RoR2;
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

        public void Awake()
        {
            TELog.logger = Logger;
            TELog.LogI("Running Turbo Edition!", true);
#if DEBUG
            TELog.outputAlways = true;
            TELog.LogW("Running TurboEdition DEBUG build. PANIC!");
#endif
            pluginInfo = Info;
            instance = this;
            ContentManager.collectContentPackProviders += (addContentPackProvider) => addContentPackProvider(new TEContent());
#if DEBUG
            //Components.MaterialControllerComponents.AttachControllerFinderToObjects(Assets.mainAssetBundle);
#endif

            //RoR2Application.onFixedUpdate += onFixedUpdate;
            //UnityEngine.SceneManagement.SceneManager.activeSceneChanged += activeSceneChanged;
        }

        private void activeSceneChanged(UnityEngine.SceneManagement.Scene current, UnityEngine.SceneManagement.Scene next)
        {
            if (next.isLoaded && next.name == "renderitem")
            {
                GameObject parent = GameObject.Find("ITEM GOES HERE (can offset from here)");
                GameObject light = null; //Thing is disabled, wont be found by GameObject.Find
                foreach (var item in next.GetRootGameObjects())
                {
                    if (item.name == "move me for pleasant local lighting details")
                    {
                        light = item;
                    }
                }

                if (light != null)
                {
                    GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    primitive.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                    primitive.transform.SetParent(light.transform);
                }
                foreach (System.Reflection.FieldInfo itemdef in typeof(TEContent.Items).GetFields())
                {
                    ItemDef itemDef = (ItemDef)itemdef.GetValue(null);
                    if (!itemDef.pickupModelPrefab || itemDef.pickupModelPrefab.name.Contains("Placeholder"))
                        continue;
                    if (parent)
                    {
                        UnityEngine.Object.Instantiate(itemDef.pickupModelPrefab, parent.transform).SetActive(false);
                    }
                    else
                    {
                        UnityEngine.Object.Instantiate(itemDef.pickupModelPrefab, new Vector3(-0.06f, 0.04f, 2.07f), Quaternion.Euler(0f, 210.6264f, 0f)).SetActive(false);
                    }
                }
            }
        }

        /*private void onFixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                UnityEngine.AddressableAssets.Addressables.LoadSceneAsync("RoR2/Dev/renderitem/renderitem.unity", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }*/
    }
}
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
            "0.1.6";

        internal const string ModIdentifier = "TurboEdition";
        public const string ModGuid = "com.Anreol." + ModIdentifier;

        public static TurboUnityPlugin instance;
        public static PluginInfo pluginInfo;
        public static uint playMusicSystemID;

        public void Awake()
        {
            TELog.logger = Logger;
            //TELog.LogI("Running Turbo Edition for PLAYTESTING!", true);
            //TELog.LogI("Whenever a run ends, a log message will appear with a link to a form for feedback. Fill it if you want!", true);
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

            // RoR2Application.onFixedUpdate += onFixedUpdate;
            //UnityEngine.SceneManagement.SceneManager.activeSceneChanged += activeSceneChanged;
            //RoR2.Run.onClientGameOverGlobal += FeedbackLog;
        }

        private void FeedbackLog(Run arg1, RunReport arg2)
        {
            TELog.LogW("That was a good run, right? If you used Turbo Edition content, please fill up this form below!", true);
            TELog.LogW("https://forms.gle/6kEEJdguHPrKzHNo9", true);
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
                foreach (System.Reflection.FieldInfo survivor in typeof(TEContent.Survivors).GetFields())
                {
                    SurvivorDef survivorDef = (SurvivorDef)survivor.GetValue(null);
                    if (!survivorDef.displayPrefab)
                        continue;

                    UnityEngine.Object.Instantiate(survivorDef.displayPrefab, new Vector3(0.04f, -0.09f, 2.17f), Quaternion.Euler(0f, 210.6264f, 0f)).SetActive(false);
                }
            }
        }

        private void onFixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                UnityEngine.AddressableAssets.Addressables.LoadSceneAsync("RoR2/Dev/renderitem/renderitem.unity", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
    }
}
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.Misc
{
    class PropDecor
    {
        [SystemInitializer]
        private static void Init()
        {
            if (RoR2.RoR2Application.isDedicatedServer || Application.isBatchMode) //We dont need graphics
                return;
            SceneCatalog.onMostRecentSceneDefChanged += onMostRecentSceneChanged;
        }

        private static void onMostRecentSceneChanged(SceneDef obj)
        {
            if (obj == SceneCatalog.GetSceneDefFromSceneName("title"))
            {
                TurboUnityPlugin.instance.StartCoroutine(AwaitForMainMenuToComplete());
            }
        }
        private static void AddDecorToMM()
        {
            GameObject itemHolder = RoR2.UI.MainMenu.MultiplayerMenuController.instance.transform.parent.GetChild(0).GetChild(1).GetChild(1).GetChild(26).GetChild(4).gameObject;
            LocalUser firstLocalUser = LocalUserManager.GetFirstLocalUser();
            if (firstLocalUser.userProfile.HasDiscoveredPickup(PickupCatalog.itemIndexToPickupIndex[(int)TEContent.Items.StandBonus.itemIndex]))
            {
                GameObject sandBag = UnityEngine.Object.Instantiate(Assets.mainAssetBundle.LoadAsset<GameObject>("PickupSandBag"), new Vector3(-0.25f, -2.2f, 2.3f), Quaternion.Euler(0f, 152.6924f, 0), itemHolder.transform);
                sandBag.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                sandBag.SetActive(true); //Just in case.
            }
        }
        private static IEnumerator AwaitForMainMenuToComplete()
        {
            yield return new WaitForEndOfFrame();
            //if (RoR2.UI.MainMenu.MultiplayerMenuController.instance)
            {
                //AddDecorToMM();
            }
        }
    }
}

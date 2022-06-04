using LeTai.Asset.TranslucentImage;
using RoR2;
using RoR2.UI;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TurboEdition.Misc
{
    internal class UIExtras
    {
        internal static GameObject mainMenuObject; //WHAT IS ENCAPSULATION AMIRITE

        [SystemInitializer]
        private static void Init()
        {
            if (RoR2.RoR2Application.isDedicatedServer || Application.isBatchMode) //We dont need graphics
                return;
            CameraRigController.onCameraEnableGlobal += onCameraEnabledGlobal;
            SceneCatalog.onMostRecentSceneDefChanged += onMostRecentSceneDefChanged;
            PauseManager.onPauseStartGlobal += new Action(delegate ()
            {
                PauseAction();
            });
        }

        public static GameObject panel = Assets.mainAssetBundle.LoadAsset<GameObject>("SettingsSubPanel, Turbo");
        public static GameObject headerButton = Assets.mainAssetBundle.LoadAsset<GameObject>("GenericHeaderButton (Turbo)");

        //public static GameObject statBarContainer = Assets.mainAssetBundle.LoadAsset<GameObject>("StatBarsContainer");
        public static GameObject scoreboardLeftSidePanel = Assets.mainAssetBundle.LoadAsset<GameObject>("ScoreboardLeftSidePanel");

        private static void onCameraEnabledGlobal(CameraRigController obj)
        {
            if (obj)
            {
                //TurboEdition.instance.StartCoroutine(AwaitForHUDCreationAndAppend(obj, statBarContainer, "MainContainer/MainUIArea/SpringCanvas/LeftCluster"));
                // TODO: FIX FIX FIX FIX FIX TurboUnityPlugin.instance.StartCoroutine(AwaitForHUDCreationAndAppend(obj, scoreboardLeftSidePanel, "MainContainer/MainUIArea/SpringCanvas"));
            }
        }

        private static void onMostRecentSceneDefChanged(SceneDef obj)
        {
            if (obj == SceneCatalog.GetSceneDefFromSceneName("title"))
            {
                mainMenuObject = GameObject.Find("MainMenu");
                RoR2.UI.MainMenu.MainMenuController mmc = mainMenuObject.GetComponent<RoR2.UI.MainMenu.MainMenuController>();
                //RoR2.UI.MainMenu.MainMenuController mmc = RoR2.UI.MainMenu.MainMenuController.instance; THIS FAILS. GOD DAMNIT HOPOO
                if (mmc.settingsMenuScreen)
                {
                    mmc.settingsMenuScreen.onEnter.AddListener(new UnityAction(delegate ()
                    {
                        TurboUnityPlugin.instance.StartCoroutine(AddSettingsMenuCoroutine(mmc.settingsMenuScreen.GetComponent<RoR2.UI.MainMenu.SubmenuMainMenuScreen>()));
                    }));
                }
                if (mmc.multiplayerMenuScreen)
                {
                    GameObject itemHolder = mmc.multiplayerMenuScreen.transform.parent.GetChild(0).GetChild(1).GetChild(1).GetChild(26).GetChild(4).gameObject;
                    LocalUser firstLocalUser = LocalUserManager.GetFirstLocalUser();
                    if (firstLocalUser.userProfile.HasDiscoveredPickup(PickupCatalog.itemIndexToPickupIndex[(int)TEContent.Items.StandBonus.itemIndex]))
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate(Assets.mainAssetBundle.LoadAsset<GameObject>("PickupSandBag"), new Vector3(-5.4547f, 597.8f, -430.8293f), Quaternion.Euler(0f, 152.6924f, 0), itemHolder.transform);
                        gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                        gameObject.SetActive(true); //Just in case.
                    }
                    if (firstLocalUser.userProfile.HasDiscoveredPickup(PickupCatalog.itemIndexToPickupIndex[(int)TEContent.Items.DropletDupe.itemIndex]))
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate(Assets.mainAssetBundle.LoadAsset<GameObject>("DisplayDropletDupe"), new Vector3(-5.06f, 598.34f, -433.3199f), Quaternion.Euler(270f, 328f, 0), itemHolder.transform);
                        gameObject.SetActive(true); //Just in case.
                    }
                }
            }
        }

        private static void PauseAction()
        {
            if (!PauseManager.isPaused || SceneCatalog.mostRecentSceneDef.isOfflineScene)
                return;
            PauseManager.pauseScreenInstance.transform.Find("Blur + Background Panel/ValidScreenspacePanel/MainPanel/OptionsPanel (JUICED)/GenericMenuButton (Settings)").GetComponent<RoR2.UI.HGButton>().onClick.AddListener(delegate ()
            {
                TurboUnityPlugin.instance.StartCoroutine(AddSettingsMenuCoroutine(PauseManager.pauseScreenInstance.GetComponent<PauseScreenController>()));
            });
            return;
        }

        private static void AssignHUDElement(HUD newHud, GameObject panel, string transform)
        {
            if (!newHud.transform.Find(transform))
                return;
            Transform parent = newHud.transform.Find(transform).transform;
            UnityEngine.Object.Instantiate(panel, parent).SetActive(true);
        }

        private static void AssignMenu(GameObject submenuPanelInstance)
        {
            Transform header = submenuPanelInstance.transform.Find("SafeArea/HeaderContainer/Header (JUICED)");
            Transform subPanelArea = submenuPanelInstance.transform.Find("SafeArea/SubPanelArea");

            GameObject headerInstance = UnityEngine.Object.Instantiate(headerButton, header);
            GameObject panelInstance = UnityEngine.Object.Instantiate(panel, subPanelArea);

            FixBlurShader(panelInstance.transform.Find("Scroll View/BlurPanel").gameObject);
            panelInstance.SetActive(false);
            RoR2.UI.HGButton leButton = headerInstance.GetComponent<RoR2.UI.HGButton>();
            RoR2.UI.HGHeaderNavigationController leHeader = submenuPanelInstance.GetComponent<RoR2.UI.HGHeaderNavigationController>();
            leButton.onClick.AddListener((delegate ()
            {
                leHeader.ChooseHeaderByButton(leButton);
            }));

            if (panelInstance.GetComponent<RoR2.UI.SettingsPanelController>())
                panelInstance.GetComponent<RoR2.UI.SettingsPanelController>().revertButton = submenuPanelInstance.transform.Find("SafeArea/FooterContainer/FooterPanel, M&KB/RevertAndBack (JUICED)/NakedButton (Revert)").GetComponent<RoR2.UI.HGButton>();

            if (submenuPanelInstance.GetComponent<RoR2.UI.UILayerKey>())
            {
                if (panelInstance.GetComponent<RoR2.UI.HGButtonHistory>())
                    panelInstance.GetComponent<RoR2.UI.HGButtonHistory>().requiredTopLayer = submenuPanelInstance.GetComponent<RoR2.UI.UILayerKey>();
                if (panelInstance.GetComponent<RoR2.UI.HGScrollRectHelper>())
                    panelInstance.GetComponent<RoR2.UI.HGScrollRectHelper>().requiredTopLayer = submenuPanelInstance.GetComponentInChildren<RoR2.UI.UILayerKey>();
            }

            RoR2.UI.HGHeaderNavigationController hnc = submenuPanelInstance.GetComponent<RoR2.UI.HGHeaderNavigationController>();
            RoR2.UI.HGHeaderNavigationController.Header sloppytoppy = new RoR2.UI.HGHeaderNavigationController.Header
            {
                headerButton = headerInstance.GetComponent<RoR2.UI.HGButton>(),
                headerName = "Turbo",
                tmpHeaderText = headerInstance.GetComponentInChildren<TextMeshProUGUI>(),
                headerRoot = panelInstance
            };
            HG.ArrayUtils.ArrayAppend(ref hnc.headers, sloppytoppy);

            int lastOption = header.Find("GenericHeaderButton (Graphics)").GetSiblingIndex() + 1;
            headerInstance.transform.SetSiblingIndex(lastOption);
            header.Find("GenericGlyph (Right)").SetSiblingIndex(headerInstance.transform.GetSiblingIndex() + 1);
        }

        private static void FixBlurShader(GameObject panel)
        {
            panel.GetComponent<LeTai.Asset.TranslucentImage.TranslucentImage>().material.shader = Shader.Find("UI/TranslucentImage");
        }

        private void GetGapBetweenPanels(RectTransform leftPanel, RectTransform rightPanel)
        {
            Vector2 panel2UpperLeftCorner = new Vector2((rightPanel.anchorMax.x - rightPanel.rect.width), (rightPanel.anchorMax.y - rightPanel.rect.height));
            Vector2.Distance(leftPanel.anchorMax, panel2UpperLeftCorner);
        }

        private static IEnumerator AwaitForHUDCreationAndAppend(CameraRigController camera, GameObject objectToInstantiate = null, string parent = null) //Me getting trolled by one single line of code
        {
            yield return new WaitForEndOfFrame();
            if (SceneCatalog.mostRecentSceneDef.baseSceneName == "lobby")
                yield break;
            if (camera.hud == null && !SceneCatalog.mostRecentSceneDef.isOfflineScene)
                TELog.LogW("Something went wrong when awaiting for the Camera's HUD creation on a Non-Offline Scene.");
            else if (!SceneCatalog.mostRecentSceneDef.isOfflineScene)
                AssignHUDElement(camera.hud, objectToInstantiate, parent);
        }

        private static IEnumerator AddSettingsMenuCoroutine(RoR2.UI.MainMenu.SubmenuMainMenuScreen trolled) //Me getting trolled by one single line of code
        {
            yield return new WaitForEndOfFrame();
            if (trolled == null)
                TELog.LogW("Something went wrong when getting SubmenuMainMenuScreen");
            else
                AssignMenu(trolled.submenuPanelInstance);
        }

        private static IEnumerator AddSettingsMenuCoroutine(RoR2.UI.PauseScreenController trolled) //Me getting trolled by one single line of code
        {
            yield return new WaitForEndOfFrame();
            if (trolled == null)
                TELog.LogW("Something went wrong when getting PauseScreenController");
            else
                AssignMenu(trolled.submenuObject);
        }
    }
}
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
        [SystemInitializer(new Type[]
        {
        })]
        private static void Init()
        {
            if (RoR2.RoR2Application.isDedicatedServer || Application.isBatchMode) //We dont need graphics
                return;
            CameraRigController.onCameraEnableGlobal += onCameraEnabledGlobal;
            SceneCatalog.onMostRecentSceneDefChanged += SceneCatalog_onMostRecentSceneDefChanged;
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

        private static void SceneCatalog_onMostRecentSceneDefChanged(SceneDef obj)
        {
            if (obj == SceneCatalog.GetSceneDefFromSceneName("title"))
            {
                GameObject mm = GameObject.Find("MainMenu");
                RoR2.UI.MainMenu.MainMenuController mmc = mm.GetComponent<RoR2.UI.MainMenu.MainMenuController>();
                if (mmc.settingsMenuScreen)
                {
                    mmc.settingsMenuScreen.onEnter.AddListener(new UnityAction(delegate ()
                    {
                        TurboUnityPlugin.instance.StartCoroutine(AddSettingsMenuCoroutine(mmc.settingsMenuScreen.GetComponent<RoR2.UI.MainMenu.SubmenuMainMenuScreen>()));
                    }));
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
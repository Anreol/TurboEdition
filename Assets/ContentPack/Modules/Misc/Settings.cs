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
    internal class Settings
    {
        [SystemInitializer(new Type[]
        {
        })]
        private static void Init()
        {
            SceneCatalog.onMostRecentSceneDefChanged += SceneCatalog_onMostRecentSceneDefChanged;
            PauseManager.onPauseStartGlobal += new Action(delegate ()
            {
                PauseAction();
            });
        }

        public static GameObject panel = Assets.mainAssetBundle.LoadAsset<GameObject>("SettingsSubPanel, Turbo");
        public static GameObject headerButton = Assets.mainAssetBundle.LoadAsset<GameObject>("GenericHeaderButton (Turbo)");

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
                        TurboEdition.instance.StartCoroutine(Trolltine(mmc.settingsMenuScreen.GetComponent<RoR2.UI.MainMenu.SubmenuMainMenuScreen>()));
                    }));
                }
            }
        }

        private static void PauseAction()
        {
            if (!PauseManager.isPaused || SceneCatalog.mostRecentSceneDef == SceneCatalog.GetSceneDefFromSceneName("title"))
                return;
            PauseManager.pauseScreenInstance.transform.Find("Blur + Background Panel/ValidScreenspacePanel/MainPanel/OptionsPanel (JUICED)/GenericMenuButton (Settings)").GetComponent<RoR2.UI.HGButton>().onClick.AddListener(delegate ()
            {
                TurboEdition.instance.StartCoroutine(Trolltine(PauseManager.pauseScreenInstance.GetComponent<PauseScreenController>()));
            });
            return;
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
            panel.GetComponent<TranslucentImage>().material.shader = Shader.Find("UI/TranslucentImage");
        }
        private static IEnumerator Trolltine(RoR2.UI.MainMenu.SubmenuMainMenuScreen trolled) //Me getting trolled by one single line of code
        {
            yield return new WaitForEndOfFrame();
            if (trolled == null)
                TELog.logger.LogWarning("Something went wrong when getting SubmenuMainMenuScreen");
            else
                AssignMenu(trolled.submenuPanelInstance);
        }

        private static IEnumerator Trolltine(RoR2.UI.PauseScreenController trolled) //Me getting trolled by one single line of code
        {
            yield return new WaitForEndOfFrame();
            if (trolled == null)
                TELog.logger.LogWarning("Something went wrong when getting PauseScreenController");
            else
                AssignMenu(trolled.submenuObject);
        }
    }
}
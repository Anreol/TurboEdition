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
            PauseManager.onPauseStartGlobal += new Action (delegate() {
                PauseAction();
            });
        }

        public static GameObject panel = Assets.mainAssetBundle.LoadAsset<GameObject>("SettingsSubPanel, Turbo");
        public static GameObject headerButton = Assets.mainAssetBundle.LoadAsset<GameObject>("GenericHeaderButton (Turbo)");
        public static RoR2.UI.MainMenu.SubmenuMainMenuScreen currentSmms = null;

        private static void SceneCatalog_onMostRecentSceneDefChanged(SceneDef obj)
        {
            if (obj == SceneCatalog.GetSceneDefFromSceneName("title"))
            {
                GameObject mm = GameObject.Find("MainMenu");
                RoR2.UI.MainMenu.MainMenuController mmc = mm.GetComponent<RoR2.UI.MainMenu.MainMenuController>();
                if (mmc.settingsMenuScreen)
                {
                    mmc.settingsMenuScreen.onEnter.AddListener(new UnityAction(MainMenuSettings));
                    currentSmms = mmc.settingsMenuScreen.GetComponent<RoR2.UI.MainMenu.SubmenuMainMenuScreen>();
                    if (currentSmms != null)
                    {
                        TELog.logger.LogWarning("Adding listener to currentSmms");
                        //currentSmms.onEnter.AddListener(new UnityAction(MainMenuSettings));
                    }
                }
            }
        }

        private static void PauseAction()
        {
            if (!PauseManager.isPaused || SceneCatalog.mostRecentSceneDef == SceneCatalog.GetSceneDefFromSceneName("title"))
                return;
            PauseManager.pauseScreenInstance.transform.Find("Blur + Background Panel/ValidScreenspacePanel/MainPanel/OptionsPanel (JUICED)/GenericMenuButton (Settings)").GetComponent<RoR2.UI.HGButton>().onClick.AddListener(new UnityAction(PauseMenuSettings));
            return;
        }
        private static void MainMenuSettings()
        {
            TurboEdition.instance.StartCoroutine(Trolltine(currentSmms));
            /*if (currentSmms.submenuPanelInstance)
            {
                TELog.logger.LogWarning("Assigning to settings menu.");
                AssignMenu(currentSmms.submenuPanelInstance);
            }*/
        }
        private static void PauseMenuSettings()
        {
            TELog.logger.LogWarning(PauseManager.pauseScreenInstance.GetComponent<RoR2.UI.PauseScreenController>().submenuObject);
            //TODO TurboEdition.instance.StartCoroutine(Trolltine(PauseManager.pauseScreenInstance.GetComponent<PauseScreenController>().submenuObject));
            /*if (PauseManager.pauseScreenInstance.GetComponent<RoR2.UI.PauseScreenController>().submenuObject)
            {
                AssignMenu(PauseManager.pauseScreenInstance.GetComponent<RoR2.UI.PauseScreenController>().submenuObject);
            }*/
        }
        private static void AssignMenu(GameObject submenuPanelInstance)
        {
            Transform header = submenuPanelInstance.transform.Find("SafeArea/HeaderContainer/Header (JUICED)");
            Transform subPanelArea = submenuPanelInstance.transform.Find("SafeArea/SubPanelArea");

            GameObject headerInstance = UnityEngine.Object.Instantiate(headerButton, header);
            GameObject panelInstance = UnityEngine.Object.Instantiate(panel, subPanelArea);

            RoR2.UI.HGButton leButton = headerInstance.GetComponent<RoR2.UI.HGButton>();
            RoR2.UI.HGHeaderNavigationController leHeader = submenuPanelInstance.GetComponent<RoR2.UI.HGHeaderNavigationController>();
            //leButton.onClick.AddListener(new UnityAction (ChooseHeaderByButton())); //TODO ADD LISTENER TO HEADER ChooseHeaderByButton()

            panelInstance.GetComponent<RoR2.UI.SettingsPanelController>().revertButton = submenuPanelInstance.transform.Find("FooterContainer/FooterPanel, M&KB/RevertAndBack (JUICED)/NakedButton (Revert)").GetComponent<RoR2.UI.MPButton>();
            
            //panelInstance.GetComponent<RoR2.UI.HGButtonHistory>().requiredTopLayer = submenuPanelInstance.GetComponentInChildren<RoR2.UI.UILayerKey>();
            //panelInstance.GetComponent<RoR2.UI.HGScrollRectHelper>().requiredTopLayer = submenuPanelInstance.GetComponentInChildren<RoR2.UI.UILayerKey>();

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

        private static IEnumerator Trolltine(RoR2.UI.MainMenu.SubmenuMainMenuScreen trolled) //Me getting trolled by one single line of code
        {
            yield return new WaitForEndOfFrame();
            TELog.logger.LogWarning("Waited til the end of frame");
            AssignMenu(trolled.submenuPanelInstance);
        }
    }
}
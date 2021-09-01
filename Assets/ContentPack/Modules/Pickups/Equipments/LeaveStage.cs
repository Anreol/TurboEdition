using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Equipments
{
    public class LeaveStage : Equipment
    {
        public override EquipmentDef equipmentDef { get; set; } = Assets.mainAssetBundle.LoadAsset<EquipmentDef>("LeaveStage");

        private static bool canLeave = true;

        public override void Initialize()
        {
            Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            SceneExitController.onBeginExit += ListenToSceneExitController;
        }

        private void Stage_onStageStartGlobal(Stage obj)
        {
            if (!NetworkServer.active) return;
            canLeave = true;
        }

        public override bool FireAction(EquipmentSlot slot)
        {
            if (canLeave)
            {
                return UseThingie();
            }
            return false;
        }

        public bool UseThingie()
        {
            if (SceneCatalog.mostRecentSceneDef == SceneCatalog.GetSceneDefFromSceneName("moon2") && MoonBatteryMissionController.instance.numChargedBatteries >= MoonBatteryMissionController.instance.numRequiredBatteries) //Is anniversary moon. Do not know how to get it in a better way.
            {
                SceneExitController sceneExitController = CreateSceneExit();
                sceneExitController.useRunNextStageScene = false;
                sceneExitController.destinationScene = SceneCatalog.GetSceneDefFromSceneName("moon");
                sceneExitController.Begin();
                return true;
            }
            if (SceneCatalog.mostRecentSceneDef.sceneType != SceneType.Stage || SceneCatalog.mostRecentSceneDef.isFinalStage) //Not a stage trololo
            {
                return false;
            }
            if (TeleporterInteraction.instance.chargeFraction >= 0.99 || TeleporterInteraction.instance.monstersCleared)
            {
                CreateSceneExit().Begin();
                return true;
            }
            return false;
        }

        public SceneExitController CreateSceneExit()
        {
            GameObject sceneExitGo = new GameObject("SceneExitLeaveStageEquip");
            sceneExitGo.AddComponent<SceneExitController>();
            SceneExitController sceneExitController = sceneExitGo.GetComponent<SceneExitController>();
            //InstanceTracker.Add(sceneExitController); //Add it for the sake of consistency EDIT: It adds itself on Enable!!
            sceneExitController.useRunNextStageScene = true; //True by default
            sceneExitController.SetState(SceneExitController.ExitState.Idle); //Idle by default
            return sceneExitController;
        }

        private void ListenToSceneExitController(SceneExitController obj)
        {
            if (!NetworkServer.active) return;
            canLeave = false;
        }
    }
}
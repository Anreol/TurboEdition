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
            Stage.onServerStageBegin += ServerStageStart;
            SceneExitController.onBeginExit += ListenToSceneExitController;
        }

        private void ServerStageStart(Stage obj)
        {
            canLeave = true;
        }
        private void ListenToSceneExitController(SceneExitController obj)
        {
            canLeave = false;
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
                SceneExitController sceneExitController;
                GameObject sceneExitGO = CreateSceneExitGameObject(out sceneExitController);
                sceneExitController.useRunNextStageScene = false;
                sceneExitController.destinationScene = SceneCatalog.GetSceneDefFromSceneName("moon");
                NetworkServer.Spawn(sceneExitGO);
                sceneExitController.Begin();
                return true;
            }
            if (SceneCatalog.mostRecentSceneDef.sceneType != SceneType.Stage || SceneCatalog.mostRecentSceneDef.isFinalStage) //Not a stage trololo
            {
                return false;
            }
            if (TeleporterInteraction.instance.chargeFraction >= 0.99f || TeleporterInteraction.instance.monstersCleared || (TeleporterInteraction.instance.currentState is TeleporterInteraction.ChargedState && TeleporterInteraction.instance.monstersCleared))
            {
                SceneExitController sceneExitController;
                GameObject sceneExitGO = CreateSceneExitGameObject(out sceneExitController);
                NetworkServer.Spawn(sceneExitGO);
                sceneExitController.Begin();
                return true;
            }
            return false;
        }

        public GameObject CreateSceneExitGameObject(out SceneExitController sceneExitController)
        {
            GameObject sceneExitGo = new GameObject("SceneExitLeaveStageEquip");
            sceneExitGo.AddComponent<SceneExitController>();
            sceneExitController = sceneExitGo.GetComponent<SceneExitController>();
            //InstanceTracker.Add(sceneExitController); //Add it for the sake of consistency EDIT: It adds itself on Enable!!
            sceneExitController.useRunNextStageScene = true; //True by default
            sceneExitController.SetState(SceneExitController.ExitState.Idle); //Idle by default
            return sceneExitGo;
        }


    }
}
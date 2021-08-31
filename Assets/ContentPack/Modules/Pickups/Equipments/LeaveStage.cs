using RoR2;
using UnityEngine.Networking;
using UnityEngine;

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
            GameObject sceneExitGo = new GameObject("SceneExitLeaveStageEquip");
            sceneExitGo.AddComponent<SceneExitController>();
            SceneExitController sceneExitController = UnityEngine.Object.Instantiate<GameObject>(sceneExitGo).GetComponent<SceneExitController>();
            InstanceTracker.Add(sceneExitController); //Add it for the sake of consistency
            sceneExitController.useRunNextStageScene = true; //True by default
            if (SceneCatalog.mostRecentSceneDef == SceneCatalog.GetSceneDefFromSceneName("moon2") && MoonBatteryMissionController.instance.numChargedBatteries >= MoonBatteryMissionController.instance.numRequiredBatteries) //Is anniversary moon. Do not know how to get it in a better way.
            {
                sceneExitController.useRunNextStageScene = false;
                sceneExitController.destinationScene = SceneCatalog.GetSceneDefFromSceneName("moon");
                sceneExitController.SetState(SceneExitController.ExitState.Idle);
                return true;
            }
            if (SceneCatalog.mostRecentSceneDef.sceneType != SceneType.Stage || SceneCatalog.mostRecentSceneDef.isFinalStage) //Not a stage trololo
            {
                return false;
            }
            if (TeleporterInteraction.instance.chargeFraction >= 0.99 || TeleporterInteraction.instance.monstersCleared)
            {
                sceneExitController.SetState(SceneExitController.ExitState.Idle);
                return true;
            }
            return false;
        }

        private void ListenToSceneExitController(SceneExitController obj)
        {
            if (!NetworkServer.active) return;
            canLeave = false;
        }
    }
}
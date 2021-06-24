using RoR2;

namespace TurboEdition.Equipments
{
    public class LeaveStage : Equipment
    {
        public override EquipmentDef equipmentDef { get; set; } = Assets.mainAssetBundle.LoadAsset<EquipmentDef>("LeaveStage");

        private bool canLeave = true;
        public override bool FireAction(EquipmentSlot slot)
        {
            SceneExitController.onBeginExit += SceneExitController_onBeginExit;
            if (canLeave)
            {
                SceneExitController.onBeginExit -= SceneExitController_onBeginExit; //I dunno bout this. Meant to unsubscribe after equipment use because i do not know if they stack per use.
                return UseThingie();
            }
            return false;
        }
        public override void Initialize()
        {
            base.Initialize();
        }
        public bool UseThingie()
        {
            SceneExitController sceneExitController = new SceneExitController();
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

        private void SceneExitController_onBeginExit(SceneExitController obj)
        {
            SceneExitController.onBeginExit -= SceneExitController_onBeginExit;
            canLeave = false;
        }
    }
}
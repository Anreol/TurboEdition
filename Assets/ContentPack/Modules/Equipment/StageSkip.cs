using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

//TODO Fix this fucking thing not working in HR and Moon!
//I dont give a fuck if moon doesn't have a next stage or whatever the functionality has to be there!
//Then we just make it reject it lol
namespace TurboEdition.Equipment
{
    public class StageSkip : EquipmentBase<StageSkip>
    {
        public override string EquipmentName => "Emergency Button";
        public override string EquipmentLangTokenName => "STAGESKIP";
        public override string EquipmentPickupDesc => "Advances to next stage if teleporter is fully charged or boss is defeated.";
        public override string EquipmentFullDescription => "";
        public override string EquipmentLore => "";

        public override GameObject EquipmentModel => TurboEdition.MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Default.prefab");

        public override Sprite EquipmentIcon => TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Items/Equipment.png");
        public override float Cooldown => equipmentRecharge;

        public float equipmentRecharge;

        public SceneExitController explicitSceneExitController { get; }

        protected override void CreateConfig(ConfigFile config)
        {
            equipmentRecharge = config.Bind<float>("Equipment : " + EquipmentName, "Recharge time", 90f, "Amount in seconds for this equipment to be available again.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override void Initialization()
        {
        }

        //IMPORTANT
        //AAAAAAAAA
        //CURRENTLY DOESN'T WORK IN HIDDEN REALMS
        //WHILE IS UP TO CHOICE IF IT DOES OR NOT WHAT I MEAN BY THIS IS THAT IT THROWS A NULL EXCEPTION!!!!!!!!

        //We could probably do this via the game's own SceneExitController but some stages such as the Ambry doesn't have one so we'll create one.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (Stage.instance.sceneDef.isFinalStage || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "moon")
            {
#if DEBUG
                Chat.AddMessage("Turbo Edition: " + EquipmentName + " this is a final stage, won't skip!");
#endif
                return false;
            }
            if (!(SceneCatalog.mostRecentSceneDef.sceneType == SceneType.Stage))
            {
#if DEBUG
                Chat.AddMessage("Turbo Edition: " + EquipmentName + " current scene is not a stage, won't skip!");
#endif
                return false;
            }
            int sceneExitCount = InstanceTracker.GetInstancesList<SceneExitController>().Count;
#if DEBUG
            TurboEdition._logger.LogWarning("sceneExitCount: " + sceneExitCount);
#endif
            if (TeleporterInteraction.instance || !SceneExitController.isRunning)
            {
                //Makes sure that theres only the game's own controller, if theres none at least it wont create more than three
                //Increased to two since Bazaar has two
                if (sceneExitCount <= 2)
                {
                    if (TeleporterInteraction.instance.chargeFraction >= 0.99 || Reflection.GetFieldValue<bool>(TeleporterInteraction.instance, "monstersCleared"))
                    {
                        SceneExitController sceneExitController = explicitSceneExitController;
                        if (!sceneExitController)
                        {
                            sceneExitController = InstanceTracker.FirstOrNull<SceneExitController>();
#if DEBUG
                            TurboEdition._logger.LogWarning("TE instanceTracker.FirstOrNull: " + sceneExitController);
#endif
                        }
                        SkipStage(sceneExitController);
                        return true;
                    }
                }
            }
#if DEBUG
            Chat.AddMessage("Turbo Edition: " + EquipmentName + " couldn't skip stage, see log for details.");
            TurboEdition._logger.LogWarning("TE chargeFraction: " + TeleporterInteraction.instance.chargeFraction);
            TurboEdition._logger.LogWarning("TE monstersCleared: " + Reflection.GetFieldValue<bool>(TeleporterInteraction.instance, "monstersCleared"));
            TurboEdition._logger.LogWarning("TE isRunning: " + SceneExitController.isRunning);
#endif
            return false;
        }

        private void SkipStage(SceneExitController self)
        {
            /*I actually dont know what this does
            //self = explicitSceneExitController.GetComponent<SceneExitController>();
            #if DEBUG
            Chat.AddMessage("Turbo Edition: " + EquipmentName + " getComponent " + self);
            #endif*/
            //CANNOT DO THIS!!!! Throws an exception on ExitStage.State newState!
            //self = new SceneExitController();

            InstanceTracker.Add(self);
#if DEBUG
            Chat.AddMessage("Turbo Edition: " + EquipmentName + " forced stage skip.");
#endif
            //The game does this so lets do too
            if (NetworkServer.active)
            {
                //SceneExitController.ExitState == SceneExitController.ExitState.Idle;
                self.SetState(SceneExitController.ExitState.Idle);
                self.Begin();
            }
            //If we are grabbing an already existing instance from the game thanks to InstanceTracker.FirstOrNull we are deleting that one
            //should be fine since we are leaving the scene though.
            InstanceTracker.Remove(self);
        }
    }
}
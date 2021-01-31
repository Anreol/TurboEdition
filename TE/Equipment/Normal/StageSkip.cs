using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using static TurboEdition.Utils.ItemHelpers;

namespace TurboEdition.Equipment
{
    [RequireComponent(typeof(SceneExitController))]
    public class StageSkip : EquipmentBase
    {
        public override string EquipmentName => "Emergency Button";

        public override string EquipmentLangTokenName => "STAGE_SKIP";

        public override string EquipmentPickupDesc => "Advances to next stage if teleporter is fully charged or boss is defeated.";

        public override string EquipmentFullDescription => "";

        public override string EquipmentLore => "";

        public override string EquipmentModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        public override string EquipmentIconPath => "@TurboEdition:Assets/Textures/Icons/Items/Equipment.png";
        public override float Cooldown => equipmentRecharge;

        public float equipmentRecharge;
        public SceneExitController sceneExitController { get; private set; }
        public bool extraSceneAdded;

        protected override void CreateConfig(ConfigFile config)
        {
            equipmentRecharge = config.Bind<float>("Equipment : " + EquipmentName, "Recharge time", 60f, "Amount in seconds for this equipment to be available again").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override void Initialization()
        {

        }

        //We could probably do this via the game's own SceneExitController but some stages such as the Ambry doesn't have one so we'll create one.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!extraSceneAdded)
            {
                int sceneExitCount = InstanceTracker.GetInstancesList<SceneExitController>().Count;
#if DEBUG
                TurboEdition._logger.LogWarning("sceneExitCount: " + sceneExitCount);
#endif
                if (TeleporterInteraction.instance || !SceneExitController.isRunning)
                {
                    //Makes sure that theres only the game's own controller, if theres none at least it wont create more than two
                    if (sceneExitCount <= 1)
                    {
                        if (TeleporterInteraction.instance.chargeFraction >= 0.99 || Reflection.GetFieldValue<bool>(TeleporterInteraction.instance, "monstersCleared"))
                        {
                            if (!extraSceneAdded)
                            {
                                SceneExitController sceneExitController = new SceneExitController();
                                SkipStage(sceneExitController);
                                extraSceneAdded = true;
                                return true;
                            }
                        }
                    }
                }
#if DEBUG
                Chat.AddMessage("Turbo Edition: " + EquipmentName + " couldn't skip stage, see log for details.");
                TurboEdition._logger.LogWarning("chargeFraction: " + TeleporterInteraction.instance.chargeFraction);
                TurboEdition._logger.LogWarning("monstersCleared: " + Reflection.GetFieldValue<bool>(TeleporterInteraction.instance, "monstersCleared"));
                TurboEdition._logger.LogWarning("isRunning: " + SceneExitController.isRunning);
#endif
            }
            return false;
        }

        void SkipStage(SceneExitController sceneExitController)
        {
            //sceneExitController = .GetComponent<SceneExitController>();
            InstanceTracker.Add(sceneExitController);
#if DEBUG
            Chat.AddMessage("Turbo Edition: " + EquipmentName + " forced stage skip.");
#endif
            //The game does this so lets do too
            if (NetworkServer.active)
            {
                //SceneExitController.ExitState == SceneExitController.ExitState.Idle;
                //sceneExitController.SetState(SceneExitController.ExitState(idle));
                sceneExitController.Begin();
            }
            // TODO instance of this sceneExitController has to be removed after the item gets used
            //Not sure if this will work since we havent gotten this far yet
            //InstanceTracker.Remove(sceneExitController);
        }

    }
}
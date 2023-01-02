﻿using RoR2;
using RoR2.Audio;
using RoR2.EntityLogic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Equipments
{
    public class LeaveStage : Equipment
    {
        public override EquipmentDef equipmentDef { get; set; } = Assets.mainAssetBundle.LoadAsset<EquipmentDef>("LeaveStage");
        private static NetworkSoundEventDef errorNetworkSound = Assets.mainAssetBundle.LoadAsset<NetworkSoundEventDef>("nseLeaveStageError");

        private static bool canLeave = true;

        public override void Initialize()
        {
            Stage.onStageStartGlobal += GlobalStageStart;
            SceneExitController.onBeginExit += ListenToSceneExitController;
        }

        private void GlobalStageStart(Stage obj)
        {
            canLeave = true;
            if (SceneCatalog.mostRecentSceneDef == SceneCatalog.GetSceneDefFromSceneName("moon"))
            {
                TELog.LogW("Going to fix the Arena Walls...");
                //GameObject.Find("SceneInfo/BrotherMissionController").GetComponent<EntityStateMachine>().;
                GameObject phase4Controller = PhaseCounter.instance.gameObject.GetComponent<ChildLocator>().FindChild("Phase4").gameObject;
                DelayedEvent delayedEvent = phase4Controller.AddComponent<DelayedEvent>();
                delayedEvent.action = new UnityEngine.Events.UnityEvent();
                delayedEvent.action.AddListener(() => { PhaseCounter.instance.gameObject.GetComponent<ChildLocator>().FindChild("ArenaWalls").gameObject.SetActive(false); });
                phase4Controller.GetComponent<CombatSquad>().onDefeatedServerLogicEvent.AddListener(() => { delayedEvent.CallDelayed(5); }); //Phases advance when the combat squad is dead. After Phase 4 it goes to Boss Death, which instantly goes to Encounter Finished, so this should do it.
            }
        }

        private void ListenToSceneExitController(SceneExitController obj)
        {
            canLeave = false;
        }

        public override bool FireAction(EquipmentSlot slot)
        {
            if (!slot.characterBody.isPlayerControlled) //Is not a player, avoid grief.
                return false;
            if (canLeave && UseThingie(slot))
            {
                if ((slot.characterBody != null) ? slot.characterBody.inventory : null)
                {
                    CharacterMasterNotificationQueue.SendTransformNotification(slot.characterBody.master, slot.characterBody.inventory.currentEquipmentIndex, EquipmentIndex.None, CharacterMasterNotificationQueue.TransformationType.Default);
                    slot.characterBody.inventory.SetEquipmentIndex(EquipmentIndex.None);
                }
                return true;
            }
            //PointSoundManager.EmitSoundServer(errorNetworkSound.index, slot.characterBody.transform.position);
            slot.subcooldownTimer = 3f;
            EntitySoundManager.EmitSoundServer(errorNetworkSound.index, slot.gameObject);
            return false;
        }

        public bool UseThingie(EquipmentSlot slot)
        {
            //Is anniversary moon. Do not know how to get it in a better way.
            if (SceneCatalog.mostRecentSceneDef == SceneCatalog.GetSceneDefFromSceneName("moon2") && MoonBatteryMissionController.instance.numChargedBatteries >= MoonBatteryMissionController.instance.numRequiredBatteries)
            {
                GameObject sceneExitGO;
                CreateSceneExitGameObject(out sceneExitGO);
                SceneExitController sceneExitController = sceneExitGO.GetComponent<SceneExitController>();
                sceneExitController.useRunNextStageScene = false;
                sceneExitController.destinationScene = SceneCatalog.GetSceneDefFromSceneName("moon");
                NetworkServer.Spawn(sceneExitGO);
                sceneExitController.Begin();
                return true;
            }
            if (SceneCatalog.mostRecentSceneDef == SceneCatalog.GetSceneDefFromSceneName("voidraid") && VoidRaidGauntletController.instance.gauntletIndex >= VoidRaidGauntletController.instance.phaseEncounters.Length)
            {
                GameObject sceneExitGO;
                CreateSceneExitGameObject(out sceneExitGO);
                NetworkServer.Spawn(sceneExitGO);
                sceneExitGO.GetComponent<SceneExitController>()?.Begin();
                return true;
            }
            //Not a stage, but an intermission. Done after othe stage checks as void or old moon might be intermissions
            if (SceneCatalog.mostRecentSceneDef.sceneType != SceneType.Stage || SceneCatalog.mostRecentSceneDef.isFinalStage)
            {
                //PointSoundManager.EmitSoundServer(errorNetworkSound.index, slot.characterBody.transform.position);
                return false;
            }
            if (TeleporterInteraction.instance.chargeFraction >= 0.99f || TeleporterInteraction.instance.monstersCleared || (TeleporterInteraction.instance.currentState is TeleporterInteraction.ChargedState && TeleporterInteraction.instance.monstersCleared))
            {
                GameObject sceneExitGO;
                CreateSceneExitGameObject(out sceneExitGO);
                NetworkServer.Spawn(sceneExitGO);
                sceneExitGO.GetComponent<SceneExitController>()?.Begin();
                return true;
            }
            //PointSoundManager.EmitSoundServer(errorNetworkSound.index, slot.characterBody.transform.position);
            return false;
        }

        public void CreateSceneExitGameObject(out GameObject sceneExitGO)
        {
            sceneExitGO = UnityEngine.Object.Instantiate(Assets.mainAssetBundle.LoadAsset<GameObject>("DummySceneExitController"));
            SceneExitController sceneExitController = sceneExitGO.GetComponent<SceneExitController>();
            sceneExitController.SetState(SceneExitController.ExitState.Idle); //Idle by default
        }
    }
}
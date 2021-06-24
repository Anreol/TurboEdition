using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TurboEdition.Equipments;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition
{
    internal static class InitPickups
    {
        public static Dictionary<EquipmentDef, Equipment> equipments = new Dictionary<EquipmentDef, Equipment>();
        public static Dictionary<EquipmentDef, Equipment> eliteEquipments = new Dictionary<EquipmentDef, Equipment>();

        public static void Initialize()
        {
            InitializeEquipments();

            On.RoR2.CharacterBody.OnEquipmentGained += CheckForTurboEqpGain;
            On.RoR2.CharacterBody.OnEquipmentLost += CheckForTurboEqpLoss;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += FireTurboEqp;
            CharacterBody.onBodyStartGlobal += AddItemManager;
            On.RoR2.CharacterBody.RecalculateStats += OnRecalculateStats;

            OneTimerItems();
        }

        private static void InitializeEquipments()
        {
            var equips = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Equipment)));
            foreach (var eqpType in equips)
            {
                var eqp = (Equipment)Activator.CreateInstance(eqpType);
                var def = eqp.equipmentDef;
                /*if (!eqp.isElite)
                {
                    eqp.Initialize();
                    equipments.Add(eqp.equipmentDef, eqp);
                }
                else*/
                {
                    eqp.Initialize();
                    eliteEquipments.Add(eqp.equipmentDef, eqp);
                }
            }
        }

        private static void AddItemManager(CharacterBody body)
        {
            if (!body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless) && body.master.inventory)
            {
                var itemManager = body.gameObject.AddComponent<TurboItemManager>();
                itemManager.CheckForTEItems(); //Initial check, should be useless considering the manager subscribes this method on awake to inventorychange
                itemManager.CheckForBuffs();
            }
        }

        private static void OneTimerItems()
        {
            //Hook because alternative would be using instance tracker and run that on a fixed update
            On.RoR2.HoldoutZoneController.Awake += HoldoutZoneController_Awake;
            MasterSummon.onServerMasterSummonGlobal += Items.ItemDeployerController.Activate;
        }

        private static void HoldoutZoneController_Awake(On.RoR2.HoldoutZoneController.orig_Awake orig, HoldoutZoneController self)
        {
            if (self.applyFocusConvergence)
            {
                self.gameObject.AddComponent<TeleporterRadiusController>();
            }
        }

        private static void CheckForTurboEqpGain(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);
            self.GetComponent<TurboItemManager>()?.CheckEqp(equipmentDef, true);
        }

        private static void CheckForTurboEqpLoss(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);
            self.GetComponent<TurboItemManager>()?.CheckEqp(equipmentDef, false);
        }

        private static bool FireTurboEqp(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Boolean RoR2.EquipmentSlot::PerformEquipmentAction(RoR2.EquipmentDef)' called on client");
                return false;
            }
            Equipment equipment;
            if (equipments.TryGetValue(equipmentDef, out equipment))
            {
                var body = self.characterBody;
                return equipment.FireAction(self);
            }
            return orig(self, equipmentDef);
        }

        private static void OnRecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            var manager = self.GetComponent<TurboItemManager>();
            manager?.RunStatRecalculationsStart();
            orig(self);
            manager?.RunStatRecalculationsEnd();
        }
    }
}
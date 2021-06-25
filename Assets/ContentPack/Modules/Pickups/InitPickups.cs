using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TurboEdition.Equipments;
using UnityEngine;
using UnityEngine.Networking;

using Item = TurboEdition.Items.Item;
using Equipment = TurboEdition.Equipments.Equipment;

namespace TurboEdition
{
    internal static class InitPickups
    {
        public static Dictionary<ItemDef, Item> itemList = new Dictionary<ItemDef, Item>();
        public static Dictionary<EquipmentDef, Equipment> equipmentList = new Dictionary<EquipmentDef, Equipment>();

        public static void Initialize()
        {
            InitializeEquipments();
            InitializeItems();

            On.RoR2.CharacterBody.OnEquipmentGained += CheckForTurboEqpGain;
            On.RoR2.CharacterBody.OnEquipmentLost += CheckForTurboEqpLoss;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += FireTurboEqp;
            CharacterBody.onBodyStartGlobal += AddItemManager;
            On.RoR2.CharacterBody.RecalculateStats += OnRecalculateStats;
        }

        private static void InitializeEquipments()
        {
            //GUIUtility.systemCopyBuffer = string.Join("\n", Assembly.GetExecutingAssembly().GetTypes().Select(t => $"{t.Name} .IsAbstract={t.IsAbstract} .IsSubclassOf({typeof(Equipment).Name})={t.IsSubclassOf(typeof(Equipment))}"));
            var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Equipment)));
            foreach (var item in EquipmentTypes)
            {
                Equipment eqp = (Equipment)System.Activator.CreateInstance(item);
                if (!eqp.equipmentDef)
                {
                    Debug.LogError("Equipment " + eqp + " is missing equipment Def. Check Unity Project. Skipping.");
                    continue;
                }
                eqp.Initialize();
                equipmentList.Add(eqp.equipmentDef, eqp);
            }
        }
        private static void InitializeItems()
        {
            var items = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Item)));
            foreach (var itemType in items)
            {
                Item item = (Item)System.Activator.CreateInstance(itemType);
                if (!item.itemDef)
                {
                    Debug.LogError("Item " + item + " is missing item Def. Check Unity Project. Skipping.");
                    continue;
                }
                item.Initialize();
                itemList.Add(item.itemDef, item);
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
            if (equipmentList.TryGetValue(equipmentDef, out equipment))
            {
                //var body = self.characterBody;
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
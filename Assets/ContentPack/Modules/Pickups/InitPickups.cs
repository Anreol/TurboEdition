using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Equipment = TurboEdition.Equipments.Equipment;
using Item = TurboEdition.Items.Item;

namespace TurboEdition
{
    public class InitPickups
    {
        public static Dictionary<ItemDef, Item> itemBehaviourList = new Dictionary<ItemDef, Item>();
        public static Dictionary<EquipmentDef, Equipment> equipmentList = new Dictionary<EquipmentDef, Equipment>();

        [SystemInitializer(typeof(PickupCatalog))]
        public static void HookInit()
        {
            InitializeEquipments();
            InitializeItems();

            TELog.LogI("Subscribing to delegates related to Items and Equipments.", true);

            CharacterBody.onBodyStartGlobal += AddItemManager;
            On.RoR2.CharacterBody.RecalculateStats += OnRecalculateStats;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += FireTurboEqp; //Reminder, SERVER ONLY!
        }

        public static IEnumerable<Item> InitializeItems()
        {
            TELog.LogD($"Getting Legacy Items found inside {Assembly.GetExecutingAssembly()}...", true);
            Assembly.GetExecutingAssembly().GetTypes()
                           .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Item)))
                           .Where(type => !type.GetCustomAttributes(true)
                                    .Select(obj => obj.GetType())
                                    .Contains(typeof(DisabledContent)))
                           .Select(itemType => (Item)Activator.CreateInstance(itemType)).ToList().ForEach(item => TrackItem(item));
            return null;
        }

        public static void TrackItem(Item item, Dictionary<ItemDef, Item> itemDictionary = null)
        {
            item.Initialize();
            itemBehaviourList.Add(item.itemDef, item);
            if (itemDictionary != null)
                itemDictionary.Add(item.itemDef, item);
        }

        public static IEnumerable<Equipment> InitializeEquipments()
        {
            TELog.LogD($"Getting the Equipments found inside {Assembly.GetExecutingAssembly()}...");
            Assembly.GetExecutingAssembly().GetTypes()
                           .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Equipment)))
                           .Where(type => !type.GetCustomAttributes(true)
                                    .Select(obj => obj.GetType())
                                    .Contains(typeof(DisabledContent)))
                           .Select(eqpType => (Equipment)Activator.CreateInstance(eqpType)).ToList().ForEach(eqp => TrackEquipment(eqp));
            return null;
        }

        public static void TrackEquipment(Equipment equip, Dictionary<EquipmentDef, Equipment> equipDictionary = null)
        {
            equip.Initialize();
            equipmentList.Add(equip.equipmentDef, equip);
            if (equipDictionary != null)
                equipDictionary.Add(equip.equipmentDef, equip);
        }

        private static void AddItemManager(CharacterBody body)
        {
            //Sure masterless its fine...? I mean we are adding it to the body... bodies can have inventories, too.
            if (!body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless) && body.master.inventory)
            {
                var itemManager = body.gameObject.AddComponent<TurboItemManager>();
                itemManager.CheckForTEItems(); //Just move it to the component's Awake? Lol
            }
        }

        private static void OnRecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            var manager = self.GetComponent<TurboItemManager>();
            var buffManager = self.GetComponent<TurboBuffManager>();
            manager?.RunStatRecalculationsStart();
            buffManager?.RunStatRecalculationsStart(self);
            orig(self);
            manager?.RunStatRecalculationsEnd();
            buffManager?.RunStatRecalculationsEnd();
        }

        private static bool FireTurboEqp(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            Equipment equipment;
            if (equipmentList.TryGetValue(equipmentDef, out equipment))
            {
                //var body = self.characterBody;
                return equipment.FireAction(self);
            }
            return orig(self, equipmentDef);
        }
    }
}
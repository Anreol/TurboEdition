using BepInEx;
using BepInEx.Logging;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TurboEdition.Equipment;
using UnityEngine;

namespace TurboEdition.Modules
{
    class TEEquipments
    {
        public static TEEquipments instance;
        public List<EquipmentDef> EquipmentDefs = new List<EquipmentDef>();
        public List<EquipmentBase> EquipmentList = new List<EquipmentBase>();

        public TEEquipments()
        {
            instance = this;
        }

        public void InitEquips()
        {
            var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));
            foreach (var equipmentType in EquipmentTypes)
            {
                EquipmentBase equipment = (EquipmentBase)System.Activator.CreateInstance(equipmentType);
                if (ValidateEquipment(equipment, EquipmentList))
                {
                    equipment.Init(TurboEdition.instance.Config);
                }
            }
        }

        /// <summary>
        /// A helper to easily set up and initialize an equipment from your equipment classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="equipment">A new instance of an EquipmentBase class. E.g. "new ExampleEquipment()"</param>
        /// <param name="equipmentList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateEquipment(EquipmentBase equipment, List<EquipmentBase> equipmentList)
        {
            if (TurboEdition.instance.Config.Bind<bool>("Equipment: " + equipment.EquipmentName, "Enable Equipment?", true, "Should this equipment appear in runs?").Value)
            {
                equipmentList.Add(equipment);
                return true;
            }
            return false;
        }
    }
}

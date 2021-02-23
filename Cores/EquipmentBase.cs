using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace TurboEdition.Equipment
{
    //Why is this in a folder named CORE? I dont know! I think the name is ok
    // The directly below is entirely from TILER2 API (by ThinkInvis) specifically the Item module. Utilized to keep instance checking functionality as I migrate off TILER2.
    // TILER2 API can be found at the following places:
    // https://github.com/ThinkInvis/RoR2-TILER2
    // https://thunderstore.io/package/ThinkInvis/TILER2/
    public abstract class EquipmentBase<T>:EquipmentBase where T : EquipmentBase<T> 
    {
        public static T instance {get;private set;}
        public EquipmentBase() 
        {
            if(instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBoilerplate/Equipment was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class EquipmentBase
    {
        public abstract string EquipmentName { get; }
        public abstract string EquipmentLangTokenName { get; }
        public abstract string EquipmentPickupDesc { get; }
        public abstract string EquipmentFullDescription { get; }
        public abstract string EquipmentLore { get; }

        public abstract string EquipmentModelPath { get; }
        public abstract string EquipmentIconPath { get; }

        public virtual bool AppearsInSinglePlayer { get; } = true;

        public virtual bool AppearsInMultiPlayer { get; } = true;

        public virtual bool CanDrop { get; } = true;

        public virtual float Cooldown { get; } = 60f;

        public virtual bool EnigmaCompatible { get; } = true;

        public virtual bool IsBoss { get; } = false;

        public virtual bool IsLunar { get; } = false;

        public EquipmentIndex Index;

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        protected abstract void Initialization();

        /// <summary>
        /// Take care to call base.Init()!
        /// </summary>
        internal virtual void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            Initialization();
            Hooks();
        }

        protected virtual void CreateConfig(ConfigFile config) { }


        /// <summary>
        /// Take care to call base.CreateLang()!
        /// </summary>
        protected virtual void CreateLang()
        {
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
            LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
        }

        protected void CreateEquipment()
        {
            EquipmentDef equipmentDef = new EquipmentDef()
            {
                name = "EQUIPMENT_" + EquipmentLangTokenName,
                nameToken = "EQUIPMENT_" + EquipmentLangTokenName + "_NAME",
                pickupToken = "EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP",
                descriptionToken = "EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION",
                loreToken = "EQUIPMENT_" + EquipmentLangTokenName + "_LORE",
                pickupModelPath = EquipmentModelPath,
                pickupIconPath = EquipmentIconPath,
                appearsInSinglePlayer = AppearsInSinglePlayer,
                appearsInMultiPlayer = AppearsInMultiPlayer,
                canDrop = CanDrop,
                cooldown = Cooldown,
                enigmaCompatible = EnigmaCompatible,
                isBoss = IsBoss,
                isLunar = IsLunar
            };
            var itemDisplayRules = CreateItemDisplayRules();
            Index = ItemAPI.Add(new CustomEquipment(equipmentDef, itemDisplayRules));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
        }

        private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentIndex equipmentIndex)
        {
            if (equipmentIndex == Index)
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentIndex);
            }
        }

        protected abstract bool ActivateEquipment(EquipmentSlot slot);

        public virtual void Hooks() { }
    }
}
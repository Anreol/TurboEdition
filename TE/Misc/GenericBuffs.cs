using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static TurboEdition.Utils.ItemHelpers;


//This file's purpose is to add generic buffs not linked to any item in specific, and has or could have a wider use.
namespace TurboEdition
{
    public class GenericBuffs
    {

        private void Hooks()
        {
            On.RoR2.CharacterBody.RecalculateStats += CheckBodyHasBuff;
        }

        private void CreateBuffs()
        {
            DefShaken();
        }

        public static BuffIndex shakenBuff;
        private void DefShaken()
        {
            var shakenBuffDef = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.white,
                canStack = true,
                isDebuff = true,
                name = "Shaken",
                iconPath = "@TurboEdition:Assets/Textures/Icons/Buffs/TODO"
            });
            shakenBuff = R2API.BuffAPI.Add(shakenBuffDef);
        }

        //For shaken add a hook on damaged and check if incoming damage is crit, then add at the end of all that a roll for crit based on debuff
        //or try to hook it up to the crit rolling, but that would require a damage report? cloning the damage and applying it with increased crit chance?
        //crit damage is calculated inside HealthComponent in line 569
        //crit rolling is calculated inside CharacterBody in line 2645
        //rolling function is at Util in line 399

        private void CheckBodyHasBuff(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(shakenBuff))
            {

            }
        }
    }
}



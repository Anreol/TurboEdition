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
            DefFortified();
            DefTaunt();
            DefHeated();
            DefShock();
            DefBuzzed();
            DefMorph();
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

        public static BuffIndex fortifiedBuff;
        private void DefFortified()
        {
            var fortifiedBuffDef = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.white,
                canStack = true,
                isDebuff = true,
                name = "Fortified",
                iconPath = "@TurboEdition:Assets/Textures/Icons/Buffs/TODO"
            });
            fortifiedBuff = R2API.BuffAPI.Add(fortifiedBuffDef);
        }

        //Apply flat % damage reduction and inmunity to knockback

        public static BuffIndex tauntBuff;
        private void DefTaunt()
        {
            var tauntBuffDef = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.white,
                canStack = true,
                isDebuff = true,
                name = "Shaken",
                iconPath = "@TurboEdition:Assets/Textures/Icons/Buffs/TODO"
            });
            tauntBuff = R2API.BuffAPI.Add(tauntBuffDef);
        }

        //Make enemies target you before anything else

        public static BuffIndex heatedBuff;
        private void DefHeated()
        {
            var heatedBuffDef = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.white,
                canStack = true,
                isDebuff = true,
                name = "Shaken",
                iconPath = "@TurboEdition:Assets/Textures/Icons/Buffs/TODO"
            });
            heatedBuff = R2API.BuffAPI.Add(heatedBuffDef);
        }

        //Inmunity to one fire stack per heated stack

        public static BuffIndex shockBuff;
        private void DefShock()
        {
            var shockBuffDef = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.white,
                canStack = true,
                isDebuff = true,
                name = "Shaken",
                iconPath = "@TurboEdition:Assets/Textures/Icons/Buffs/TODO"
            });
            shockBuff = R2API.BuffAPI.Add(shockBuffDef);
        }

        //Read the google doc i forgot what this did lol

        public static BuffIndex buzzedBuff;
        private void DefBuzzed()
        {
            var buzzedBuffDef = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.white,
                canStack = true,
                isDebuff = true,
                name = "Shaken",
                iconPath = "@TurboEdition:Assets/Textures/Icons/Buffs/TODO"
            });
            buzzedBuff = R2API.BuffAPI.Add(buzzedBuffDef);
        }

        //inhability to sprint or something

        public static BuffIndex morphBuff;
        private void DefMorph()
        {
            var morphBuffDef = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.white,
                canStack = true,
                isDebuff = true,
                name = "Shaken",
                iconPath = "@TurboEdition:Assets/Textures/Icons/Buffs/TODO"
            });
            morphBuff = R2API.BuffAPI.Add(morphBuffDef);
        }

        //Oh shit a Hisii is coming

        public static BuffIndex disableLunarBuff;
        private void DefDisableLunar()
        {
            var disableLunarBuffDef = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.white,
                canStack = true,
                isDebuff = true,
                name = "Shaken",
                iconPath = "@TurboEdition:Assets/Textures/Icons/Buffs/TODO"
            });
            disableLunarBuff = R2API.BuffAPI.Add(disableLunarBuffDef);
        }

        //shitting on gesture frens since NOW

        public static BuffIndex oiledBuff;
        private void DefOiled()
        {
            var oiledBuffDef = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.white,
                canStack = true,
                isDebuff = true,
                name = "Shaken",
                iconPath = "@TurboEdition:Assets/Textures/Icons/Buffs/TODO"
            });
            oiledBuff = R2API.BuffAPI.Add(oiledBuffDef);
        }

        //what if we make blazing elites actually treatening after the player has 10231823987123 items?

        private void CheckBodyHasBuff(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(shakenBuff))
            {

            }
        }
    }
}



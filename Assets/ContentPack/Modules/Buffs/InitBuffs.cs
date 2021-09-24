using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

using Buff = TurboEdition.Buffs.Buff;

namespace TurboEdition
{
    internal static class InitBuffs
    {
        public static Dictionary<BuffDef, Buff> buffList = new Dictionary<BuffDef, Buff>();

        public static void Init()
        {
            InitializeBuffs();

            CharacterBody.onBodyStartGlobal += AddBuffManager;
            On.RoR2.CharacterBody.OnClientBuffsChanged += CheckForBuffs;
        }

        public static void InitializeBuffs()
        {
            var buffs = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Buff)));
            foreach (var buffType in buffs)
            {
                Buff buff = (Buff)Activator.CreateInstance(buffType);
                if (!buff.buffDef)
                {
                    Debug.LogError("Buff " + buff + " is missing buff Def. Check Unity Project. Skipping.");
                    continue;
                }
                buff.Initialize();
                buffList.Add(buff.buffDef, buff);
            }
        }

        private static void AddBuffManager(CharacterBody body)
        {
            if (body)
            {
                var buffManager = body.gameObject.AddComponent<TurboBuffManager>();
                buffManager.CheckForBuffs();
            }
        }

        //Recalcstat hooks get done in InitPickups. Yes. But that shouldnt be an issue
        private static void CheckForBuffs(On.RoR2.CharacterBody.orig_OnClientBuffsChanged orig, CharacterBody self)
        {
            orig(self);
            self.GetComponent<TurboBuffManager>()?.CheckForBuffs();
        }
    }
}
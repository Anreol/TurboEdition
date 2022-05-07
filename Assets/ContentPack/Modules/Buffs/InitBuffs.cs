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
            On.RoR2.CharacterBody.OnClientBuffsChanged += OnClientBuffsChanged;
        }

        public static void InitializeBuffs()
        {
            var buffs = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Buff)));
            foreach (var buffType in buffs)
            {
                Buff buff = (Buff)Activator.CreateInstance(buffType);
                if (!buff.buffDef)
                {
                    Debug.LogError("Buff " + buff + " is missing the buffDef or it couldn't be found. Check the Unity project. Skipping.");
                    continue;
                }
                buff.Initialize();
                buffList.Add(buff.buffDef, buff);
            }
        }

        private static void AddBuffManager(CharacterBody body)
        {
            if (body && Util.HasEffectiveAuthority(body.networkIdentity))
            {
                var buffManager = body.gameObject.AddComponent<TurboBuffManager>();
                buffManager.CheckForBuffs();
            }
        }

        private static void OnClientBuffsChanged(On.RoR2.CharacterBody.orig_OnClientBuffsChanged orig, CharacterBody self)
        {
            orig(self);
            if (Util.HasEffectiveAuthority(self.networkIdentity)) //This only should be running in the client... but still
            {
                self.GetComponent<TurboBuffManager>()?.CheckForBuffs();
            }
        }
    }
}
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class Hitlag : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("Hitlag");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<HitlagBehavior>(stack);
        }

        public override void Initialize()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.rejected)
            {
                orig(self, damageInfo);
                return;
            }
            if (damageInfo.damageType == DamageType.BypassArmor || damageInfo.damageType == DamageType.VoidDeath || damageInfo.damageType == DamageType.FallDamage || damageInfo.damageType == DamageType.BypassOneShotProtection)
            {
                orig(self, damageInfo);
                return;
            }
            if (damageInfo.dotIndex != DotController.DotIndex.None) //If damage applies any kind of DoT
            {
                orig(self, damageInfo);
                return;
            }
            HitlagBehavior itemshit = self.body.GetComponent<HitlagBehavior>();
            if (itemshit)
            {
                itemshit.StoreDamage(orig, self, damageInfo);
                return;
            }
            orig(self, damageInfo);
            return;
        }

        internal class HitlagBehavior : CharacterBody.ItemBehavior//, IOnIncomingDamageServerReceiver
        {
            public List<HitlagInstance> instanceLists = new List<HitlagInstance>();
            public List<DamageInfo> damageInfos = new List<DamageInfo>();

            /*public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.rejected)
                {
                    return;
                }
                if (damageInfo.dotIndex != DotController.DotIndex.None || damageInfo.damageType == DamageType.VoidDeath || damageInfo.damageType == DamageType.FallDamage || damageInfo.damageType == DamageType.BypassArmor || damageInfo.damageType == DamageType.BypassOneShotProtection)
                {
                    return;
                }
                //StartCoroutine(DelayShit(1 + ((stack - 1) * 0.5f)));
                //damageInfos.Add(damageInfo);
                //damageInfo.
            }

            /*private IEnumerator DelayShit(float time)
            {
                yield return new WaitForSeconds(time);
            }*/

            private void FixedUpdate()
            {
                if (!NetworkServer.active)
                {
                    return;
                }
                if (base.body.healthComponent)
                {
                    var instanceBuffer = instanceLists;
                    foreach (var item in instanceBuffer)
                    {
                        if (item.FixedTimeStamp.timeSince >= 1 + ((stack - 1) / 2))
                        {
                            item.CmpOrig(item.CmpSelf, item.CmpDI);
                            instanceLists.Remove(item);
                        }
                    }
                    /*foreach (var hitlag in instanceLists)
                    {
                        if (hitlag.FixedTimeStamp.timeSince >= stack + ((stack - 1) * 0.5))
                        {
                            hitlag.CmpOrig(hitlag.CmpSelf, hitlag.CmpDI);
                            instanceLists.Remove(hitlag);
                        }
                    }*/
                }
            }

            private void OnDestroy()
            {
                //On.RoR2.HealthComponent.TakeDamage -= StoreDamage; I dont know the sideeffects of this so dont do it for now.
                if (base.body.healthComponent)
                {
                    var instanceBuffer = instanceLists;
                    foreach (var item in instanceBuffer)
                    {
                        item.CmpOrig(item.CmpSelf, item.CmpDI);
                        instanceLists.Remove(item);
                    }
                    /*foreach (var hitlag in instanceLists)
                    {
                        hitlag.CmpOrig(hitlag.CmpSelf, hitlag.CmpDI);
                        instanceLists.Remove(hitlag);
                    }*/
                }
            }

            //Special method that can be called whenever, is the same as OnDestroy, but doesn't destroy the object (duh)
            public void Cleanse()
            {
                if (base.body.healthComponent)
                {
                    int instanceCount = instanceLists.Count;
                    for (int i = 0; i < instanceCount; i++)
                    {
                        instanceLists[i].CmpOrig(instanceLists[i].CmpSelf, instanceLists[i].CmpDI);
                        instanceLists.RemoveAt(i);
                    }
                    /*foreach (var hitlag in instanceLists)
                    {
                        hitlag.CmpOrig(hitlag.CmpSelf, hitlag.CmpDI);
                        instanceLists.Remove(hitlag);
                    }*/
                }
            }

            public void StoreDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
            {
                if (body.healthComponent)
                {
                    HitlagInstance hitlagInstance = new HitlagInstance
                    {
                        CmpOrig = orig,
                        CmpSelf = self,
                        CmpDI = damageInfo,
                        FixedTimeStamp = RoR2.Run.FixedTimeStamp.now
                    };
                    instanceLists.Add(hitlagInstance);
                    return;
                }
                orig(self, damageInfo);
            }
        }

        public class HitlagInstance
        {
            private On.RoR2.HealthComponent.orig_TakeDamage cmpOrig; //Orig
            private HealthComponent cmpSelf; //self
            private DamageInfo cmpDI; //DamageInfo
            private Run.FixedTimeStamp fixedTimeStamp;
            public On.RoR2.HealthComponent.orig_TakeDamage CmpOrig { get => cmpOrig; set => cmpOrig = value; }
            public HealthComponent CmpSelf { get => cmpSelf; set => cmpSelf = value; }
            public DamageInfo CmpDI { get => cmpDI; set => cmpDI = value; }
            public Run.FixedTimeStamp FixedTimeStamp { get => fixedTimeStamp; set => fixedTimeStamp = value; }
        }
    }
}
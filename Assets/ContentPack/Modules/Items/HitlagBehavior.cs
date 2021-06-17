using RoR2;
using System.Collections;
using UnityEngine;

namespace TurboEdition
{
    internal class HitlagBehavior : CharacterBody.ItemBehavior, IOnIncomingDamageServerReceiver
    {
        //poop public List<HitlagInstance> instanceLists = new List<HitlagInstance>();
        //unused for now public List<DamageInfo> damageInfos = new List<DamageInfo>();

        public void OnIncomingDamageServer(DamageInfo damageInfo)
        {
            if (damageInfo.rejected)
            {
                return;
            }
            if (damageInfo.dotIndex != DotController.DotIndex.None || damageInfo.damageType == DamageType.VoidDeath || damageInfo.damageType == DamageType.FallDamage || damageInfo.damageType == DamageType.BypassArmor || damageInfo.damageType == DamageType.BypassOneShotProtection)
            {
                return;
            }
            StartCoroutine(DelayShit(1 + ((stack - 1) * 0.5f)));
            //damageInfos.Add(damageInfo);
            //damageInfo.
        }

        private IEnumerator DelayShit(float time)
        {
            yield return new WaitForSeconds(time);
        }

        private void OnEnable()
        {
            //poopy On.RoR2.HealthComponent.TakeDamage += StoreDamage;
            body.onInventoryChanged += ItemCheck;
        }

        private void ItemCheck()
        {
            if (body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("Hitlag")) <= 0)
            {
                Destroy(this);
            }
        }

        /* very poopy
        private void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            if (base.body.healthComponent)
            {
                foreach (var hitlag in instanceLists)
                {
                    if (hitlag.FixedTimeStamp.timeSince >= stack + ((stack - 1) * 0.5))
                    {
                        hitlag.CmpOrig(hitlag.CmpSelf, hitlag.CmpDI);
                        instanceLists.Remove(hitlag);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            On.RoR2.HealthComponent.TakeDamage -= StoreDamage;
            if (base.body.healthComponent)
            {
                foreach (var hitlag in instanceLists)
                {
                    hitlag.CmpOrig(hitlag.CmpSelf, hitlag.CmpDI);
                    instanceLists.Remove(hitlag);
                }
            }
        }

        //Special method that can be called whenever, is the same as OnDestroy, but doesn't destroy the object (duh)
        public void Cleanse()
        {
            if (base.body.healthComponent)
            {
                foreach (var hitlag in instanceLists)
                {
                    hitlag.CmpOrig(hitlag.CmpSelf, hitlag.CmpDI);
                    instanceLists.Remove(hitlag);
                }
            }
        }

        public void StoreDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (body.healthComponent)
            {
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
                HitlagInstance hitlagInstance = new HitlagInstance
                {
                    CmpOrig = orig,
                    CmpSelf = self,
                    CmpDI = damageInfo,
                    FixedTimeStamp = RoR2.Run.FixedTimeStamp.now
                };
                instanceLists.Add(hitlagInstance);
            }
            orig(self, damageInfo);
        }*/
    }

    /*
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
    } */
}
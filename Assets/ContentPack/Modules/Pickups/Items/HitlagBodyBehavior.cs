using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using System.Linq;

namespace TurboEdition.Items
{
    public class HitlagBodyBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
    {
        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.Hitlag;
        }

        public List<DelayedDamageInfo> damageInfos = new List<DelayedDamageInfo>();

        public void OnIncomingDamageServer(DamageInfo damageInfo)
        {
            if (damageInfo.rejected)
                return;
            if (damageInfo.dotIndex != DotController.DotIndex.None || damageInfo.damageType == DamageType.VoidDeath || damageInfo.damageType == DamageType.Freeze2s || damageInfo.damageType == DamageType.Nullify || damageInfo.damageType == DamageType.Silent || damageInfo.damageType == DamageType.FallDamage || damageInfo.damageType == DamageType.BypassArmor || damageInfo.damageType == DamageType.BypassOneShotProtection || damageInfo.procChainMask.HasProc(ProcType.AACannon))
                return;
            damageInfo.rejected = true;
            DelayedDamageInfo delayedDamageInfo = new DelayedDamageInfo
            {
                ogDamageInfoDamage = damageInfo.damage,
                ReducedDamageInfo = new DamageInfo
                {
                    attacker = damageInfo.attacker,
                    crit = damageInfo.crit,
                    damage = damageInfo.damage,
                    damageColorIndex = damageInfo.damageColorIndex,
                    damageType = damageInfo.damageType,
                    dotIndex = damageInfo.dotIndex,
                    force = damageInfo.force,
                    inflictor = damageInfo.inflictor,
                    position = damageInfo.position,
                    procChainMask = damageInfo.procChainMask,
                    procCoefficient = damageInfo.procCoefficient,
                    rejected = false,
                },
                FixedTimeStamp = Run.FixedTimeStamp.now
            };
            delayedDamageInfo.ReducedDamageInfo.procChainMask.AddProc(ProcType.AACannon); //l o l
            delayedDamageInfo.ReducedDamageInfo.damageType |= DamageType.BypassBlock;
            damageInfos.Add(delayedDamageInfo);
        }

        private void FixedUpdate()
        {
            List<DelayedDamageInfo> buffer = new List<DelayedDamageInfo>();
            for (int i = 0; i < damageInfos.Count; i++)
            {
                if (damageInfos[i].FixedTimeStamp.timeSince >= (float)stack)
                {
                    body.healthComponent.TakeDamage(damageInfos[i].ReducedDamageInfo);
                    buffer.Add(damageInfos[i]);
                }
            }
            damageInfos = damageInfos.Except(buffer).ToList();
        }

        private void OnDestroy()
        {
            if (base.body.healthComponent)
            {
                Cleanse();
            }
        }

        public void Heal(HealthComponent hc, float amount, ProcChainMask procChainMask)
        {
            amount /= 4;
            foreach (DelayedDamageInfo hitlag in damageInfos)
            {
                if (hitlag.ReducedDamageInfo.damage > hitlag.ogDamageInfoDamage / 2)
                {
                    hitlag.ReducedDamageInfo.damage -= amount;
                    amount = 0f;
                    if (hitlag.ReducedDamageInfo.damage < hitlag.ogDamageInfoDamage / 2)
                    {
                        amount += (hitlag.ogDamageInfoDamage / 2) - hitlag.ReducedDamageInfo.damage; //Refund
                        hitlag.ReducedDamageInfo.damage = hitlag.ogDamageInfoDamage / 2;
                    }
                    if (amount <= 0)
                        break;
                }
            }
        }

        //Special method that can be called whenever
        public void Cleanse()
        {
            if (base.body.healthComponent)
            {
                foreach (DelayedDamageInfo hitlag in damageInfos)
                {
                    body.healthComponent.TakeDamage(hitlag.ReducedDamageInfo);
                }
                damageInfos.Clear();
            }
        }

        [SystemInitializer(typeof(PickupCatalog))]
        public static void Initialize()
        {
            HealthComponent.onCharacterHealServer += onCharacterHealServer; //Hitlag
        }

        private static void onCharacterHealServer(HealthComponent arg1, float arg2, ProcChainMask procChainMask)
        {
            arg1.body.GetComponent<HitlagBodyBehavior>()?.Heal(arg1, arg2, procChainMask);
        }

    }

    public struct DelayedDamageInfo
    {
        public float ogDamageInfoDamage;
        public DamageInfo ReducedDamageInfo;
        public Run.FixedTimeStamp FixedTimeStamp;
    }
}
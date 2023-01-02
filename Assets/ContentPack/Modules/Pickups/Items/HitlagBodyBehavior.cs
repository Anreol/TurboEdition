using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TurboEdition.Items
{
    internal class HitlagBodyBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
    {
        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.Hitlag;
        }

        internal Queue<DelayedDamageInfo> damageInfos = new Queue<DelayedDamageInfo>();

        public void OnIncomingDamageServer(DamageInfo damageInfo)
        {
            if (damageInfo.rejected || damageInfo.attacker == body || damageInfo.inflictor == body)
                return;
            if (damageInfo.dotIndex != DotController.DotIndex.None || (damageInfo.damageType & (DamageType.VoidDeath | DamageType.Freeze2s | DamageType.Nullify | DamageType.Silent | DamageType.FallDamage | DamageType.BypassArmor | DamageType.BypassBlock | DamageType.BypassOneShotProtection)) != 0)
                return;
            if (damageInfo.damage == 0f)
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
            delayedDamageInfo.ReducedDamageInfo.damageType |= DamageType.BypassBlock;
            damageInfos.Enqueue(delayedDamageInfo);
        }

        private void FixedUpdate()
        {
            while (damageInfos.Count > 0 && damageInfos.Peek().FixedTimeStamp.timeSince >= (float)stack)
            {
                TryToTakeDamage(damageInfos.Dequeue().ReducedDamageInfo);
            }
        }

        private void OnDestroy()
        {

                Cleanse();
            
        }

        internal void Heal(HealthComponent hc, float amount, ProcChainMask procChainMask)
        {
            amount /= 4;
            if (damageInfos != null && damageInfos.Count > 0)
            {
                DelayedDamageInfo hitlag = damageInfos.FirstOrDefault(x => x.ReducedDamageInfo.damage > x.ogDamageInfoDamage / 2);
                while (amount > 0)
                {
                    if (amount > hitlag.ogDamageInfoDamage / 2)
                    {
                        amount -= hitlag.ogDamageInfoDamage / 2;
                        hitlag.ReducedDamageInfo.damage = hitlag.ogDamageInfoDamage / 2;
                    }
                    else
                    {
                        hitlag.ReducedDamageInfo.damage -= amount;
                        amount = 0;
                    }
                    hitlag = damageInfos.FirstOrDefault(x => x.ReducedDamageInfo.damage > x.ogDamageInfoDamage / 2);
                }
            }
        }

        //Special method that can be called whenever
        internal void Cleanse()
        {
            if (base.body.healthComponent)
            {
                while (damageInfos.Count > 0)
                {
                    TryToTakeDamage(damageInfos.Dequeue().ReducedDamageInfo);
                }
            }
        }

        /// <summary>
        /// For some reason, Hidden Invincibility, a buff given by many status effects (ie Merc skills) doesn't reject damage if the damage type has Bypass Block.
        /// In fact there's a mod that fixes this... but whatever.
        /// </summary>
        /// <param name="di">Damage Info</param>
        internal void TryToTakeDamage(DamageInfo di)
        {
            if (!body.HasBuff(RoR2Content.Buffs.HiddenInvincibility))
            {
                body.healthComponent.TakeDamage(di);
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

    internal struct DelayedDamageInfo
    {
        public float ogDamageInfoDamage;
        public DamageInfo ReducedDamageInfo;
        public Run.FixedTimeStamp FixedTimeStamp;
    }
}
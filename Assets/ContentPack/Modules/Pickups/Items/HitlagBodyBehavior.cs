using RoR2;
using RoR2.Items;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TurboEdition.ScriptableObjects;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    internal class HitlagBodyBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
    {
        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.Hitlag;
        }


        [SystemInitializer(typeof(PickupCatalog))]
        public static void Initialize()
        {
            HealthComponent.onCharacterHealServer += onCharacterHealServer; //Hitlag
        }

        /// <summary>
        /// Server-only queue where damages are stored.
        /// </summary>
        internal Queue<DelayedDamageInfo> serverDamageInfos = new Queue<DelayedDamageInfo>();

        /// <summary>
        /// Both server and client.
        /// </summary>
        private void OnEnable()
        {
            UI.HealthbarStyleHelper.barDataInstances.Add(new HitlagBarData(body.healthComponent));
        }

        /// <summary>
        /// Server
        /// </summary>
        /// <param name="damageInfo"></param>
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
                    canRejectForce = damageInfo.canRejectForce,
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
            serverDamageInfos.Enqueue(delayedDamageInfo);
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                while (serverDamageInfos.Count > 0 && serverDamageInfos.Peek().FixedTimeStamp.timeSince >= (float)stack)
                {
                    TryToTakeDamage(serverDamageInfos.Dequeue().ReducedDamageInfo);
                }
            }
        }

        private void OnDestroy()
        {
            CleanseServer();
        }

        internal void Heal(HealthComponent hc, float amount, ProcChainMask procChainMask)
        {
            amount /= 4;
            if (serverDamageInfos != null && serverDamageInfos.Count > 0)
            {
                DelayedDamageInfo hitlag = serverDamageInfos.FirstOrDefault(x => x.ReducedDamageInfo.damage > x.ogDamageInfoDamage / 2);
                while (amount > 0 && hitlag != default)
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
                    hitlag = serverDamageInfos.FirstOrDefault(x => x.ReducedDamageInfo.damage > x.ogDamageInfoDamage / 2);
                }
            }
        }

        /// <summary>
        /// Method that takes all damages in <see cref="serverDamageInfos"/> and calls <see cref="TryToTakeDamage(DamageInfo)"/> on them. Only for the server.
        /// </summary>
        internal void CleanseServer()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            if (base.body.healthComponent)
            {
                while (serverDamageInfos.Count > 0)
                {
                    TryToTakeDamage(serverDamageInfos.Dequeue().ReducedDamageInfo);
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
                GlobalEventManager.instance.OnHitEnemy(di, body.gameObject);
                GlobalEventManager.instance.OnHitAll(di, body.gameObject);
            }
        }


        private static void onCharacterHealServer(HealthComponent arg1, float arg2, ProcChainMask procChainMask)
        {
            arg1.body.GetComponent<HitlagBodyBehavior>()?.Heal(arg1, arg2, procChainMask);
        }

        internal struct DelayedDamageInfo
        {
            public float ogDamageInfoDamage;
            public DamageInfo ReducedDamageInfo;
            public Run.FixedTimeStamp FixedTimeStamp;

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is DelayedDamageInfo delayedDamage)
                {
                    return ogDamageInfoDamage == delayedDamage.ogDamageInfoDamage && ReducedDamageInfo == delayedDamage.ReducedDamageInfo && delayedDamage.FixedTimeStamp == FixedTimeStamp;
                }
                return false;
            }

            public static bool operator ==(DelayedDamageInfo c1, DelayedDamageInfo c2)
            {
                return c1.Equals(c2);
            }

            public static bool operator !=(DelayedDamageInfo c1, DelayedDamageInfo c2)
            {
                return !c1.Equals(c2);
            }
        }

        public class HitlagBarData : UI.HealthbarStyleHelper.HealthBarData
        {
            private HitlagBodyBehavior hbb;

            public HitlagBarData(HealthComponent healthComponent)
            {
                watcher = healthComponent;
            }

            public override HealthBarStyle.BarStyle GetStyle()
            {
                return Assets.mainAssetBundle.LoadAsset<SerializableBarStyle>("sbsBarStyles").style;
            }

            public override void CheckInventory(ref HealthBar healthBar)
            {
                base.CheckInventory(ref healthBar);
                hbb = healthBar.source.body.GetComponent<HitlagBodyBehavior>();
                if (!hbb)
                {
                    UI.HealthbarStyleHelper.barDataInstances.Remove(this);
                }
            }

            public override void UpdateBarInfo(ref HealthBar.BarInfo info, HealthComponent.HealthBarValues healthBarValues, ref HealthBar healthBarInstance)
            {
                float damageAccumulated = 0;
                if (hbb && hbb.serverDamageInfos != null)
                {
                    foreach (var item in hbb.serverDamageInfos)
                    {
                        damageAccumulated += item.ReducedDamageInfo.damage;
                    }
                }
                info.enabled = damageAccumulated > 0;
                info.normalizedXMin = Mathf.Clamp01(Util.Remap(damageAccumulated, 0, healthBarValues.healthDisplayValue, healthBarInstance.source.combinedHealthFraction, 0));
                info.normalizedXMax = healthBarInstance.source.combinedHealthFraction;
                base.UpdateBarInfo(ref info, healthBarValues, ref healthBarInstance);
            }
        }
    }
}
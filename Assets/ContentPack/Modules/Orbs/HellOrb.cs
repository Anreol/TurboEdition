using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using TurboEdition.Components;
using UnityEngine;

namespace TurboEdition.Orbs
{
    public class HellOrb : Orb
    {
        public float damageValue;
        public GameObject attacker;
        public GameObject inflictor;
        public bool isCrit;
        public ProcChainMask procChainMask;
        public float procCoefficient;
        public DamageColorIndex damageColorIndex;
        public DamageType damageType;
        public List<HealthComponent> bouncedObjects;
        public TeamIndex teamIndex;
        public float damageReductionPerBounce = 0.5f;
        public float speed = 100f;

        public override void Begin()
        {
            base.duration = 0.1f;
            EffectData effectData = new EffectData
            {
                origin = this.origin,
                genericFloat = base.duration
            };
            base.duration = base.distanceToTarget / this.speed;
            effectData.SetHurtBoxReference(this.target);
            //EffectManager.SpawnEffect(Assets.mainAssetBundle.LoadAsset<GameObject>("HellLightningOrbEffect"), effectData, true);
            EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/orbeffects/InfusionOrbEffect"), effectData, true);
        }

        public override void OnArrival()
        {
            if (this.target)
            {
                HealthComponent healthComponent = this.target.healthComponent;
                if (healthComponent)
                {
                    DamageInfo damageInfo = new DamageInfo();
                    damageInfo.damage = this.damageValue;
                    damageInfo.attacker = this.attacker;
                    damageInfo.inflictor = this.inflictor;
                    damageInfo.force = Vector3.zero;
                    damageInfo.crit = this.isCrit;
                    damageInfo.procChainMask = this.procChainMask;
                    damageInfo.procCoefficient = this.procCoefficient;
                    damageInfo.position = this.target.transform.position;
                    damageInfo.damageColorIndex = this.damageColorIndex;
                    damageInfo.damageType = this.damageType;
                    healthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                }
                GameObject linkGameObject = target.healthComponent.body.GetComponentInChildren<HellchainController>()?.gameObject;
                if (linkGameObject)
                {
                    var list = target.healthComponent.body.GetComponentInChildren<HellchainController>().listOLinks;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (this.bouncedObjects == null) //If list doesn't exist
                            return;
                        this.bouncedObjects.Add(this.target.healthComponent); //Add character recieving damage
                        CharacterBody nextTarget = list[i];
                        if (nextTarget)
                        {
                            if (!this.bouncedObjects.Contains(nextTarget.healthComponent) && this.teamIndex == nextTarget.teamComponent.teamIndex)
                            {
                                this.bouncedObjects.Add(nextTarget.healthComponent); //Add target

                                HellOrb hellOrb = new HellOrb();
                                hellOrb.origin = this.target.transform.position;
                                hellOrb.target = nextTarget.mainHurtBox;
                                hellOrb.attacker = this.attacker;
                                hellOrb.inflictor = this.inflictor;
                                hellOrb.teamIndex = this.teamIndex;
                                hellOrb.damageValue = this.damageValue * this.damageReductionPerBounce;
                                hellOrb.isCrit = this.isCrit;
                                hellOrb.bouncedObjects = this.bouncedObjects;
                                hellOrb.procChainMask = this.procChainMask;
                                hellOrb.procCoefficient = this.procCoefficient;
                                hellOrb.damageColorIndex = this.damageColorIndex;
                                hellOrb.damageReductionPerBounce = this.damageReductionPerBounce;
                                hellOrb.speed = this.speed;
                                hellOrb.damageType = this.damageType;
                                OrbManager.instance.AddOrb(hellOrb);
                            }
                        }
                    }
                    return;
                }
            }
        }
    }
}
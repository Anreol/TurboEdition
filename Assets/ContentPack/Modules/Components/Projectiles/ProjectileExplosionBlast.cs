using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components.Projectiles
{
    //I hate hopoo so much
    [RequireComponent(typeof(ProjectileController))]
    internal class ProjectileExplosionBlast : ProjectileExplosion
    {
        [Header("Explosion Blast Configuration")]
        public bool doExtraBlastAttack = true;
        public bool doDamage = false;
        public bool scaleBaseForceOffDamage = false;
        public BlastAttack.FalloffModel falloffModelExtra = BlastAttack.FalloffModel.Linear;
        public float blastRadiusExtra;
        public AttackerFiltering blastAttackerFilteringExtra;
        public Vector3 bonusBlastForceExtra;
        public bool canRejectForceExtra = true;
        public GameObject blastAttackEffect;
        public DamageType damageTypeOverride;

        public void SetDoExtraBlastAttack(bool set)
        {
            doExtraBlastAttack = set;
        }
        public void SetDoExtraBlastAttackDamage(bool set)
        {
            doDamage = set;
        }
        public void OnDestroy()
        {
            if (doExtraBlastAttack)
            {
                if (this.blastAttackEffect)
                {
                    EffectManager.SpawnEffect(this.blastAttackEffect, new EffectData
                    {
                        origin = base.transform.position,
                        scale = this.blastRadius
                    }, true);
                }
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.position = base.transform.position;
                blastAttack.radius = blastRadiusExtra;
                blastAttack.attacker = (this.projectileController.owner ? this.projectileController.owner.gameObject : null);
                blastAttack.inflictor = base.gameObject;
                blastAttack.teamIndex = this.projectileController.teamFilter.teamIndex;
                blastAttack.procChainMask = this.projectileController.procChainMask;
                blastAttack.procCoefficient = this.projectileController.procCoefficient * this.blastProcCoefficient;
                blastAttack.bonusForce = bonusBlastForceExtra;
                blastAttack.falloffModel = falloffModelExtra;
                blastAttack.attackerFiltering = blastAttackerFilteringExtra;
                blastAttack.canRejectForce = canRejectForceExtra;
                if (projectileDamage)
                {
                    blastAttack.baseForce = scaleBaseForceOffDamage ? this.projectileDamage.force * this.blastDamageCoefficient : this.projectileDamage.force;
                    blastAttack.baseDamage = doDamage ? this.projectileDamage.damage * this.blastDamageCoefficient : 0;
                    blastAttack.crit = doDamage ? this.projectileDamage.crit : false;
                    blastAttack.damageColorIndex = this.projectileDamage.damageColorIndex;
                    blastAttack.damageType = this.projectileDamage.damageType;
                    if (damageTypeOverride != DamageType.Generic)
                    {
                        blastAttack.damageType = damageTypeOverride;
                    }
                }
                BlastAttack.Result result = blastAttack.Fire();
            }
        }
    }
}
using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class FireMGLBase : GenericProjectileBaseState
    {
        [SerializeField]
        public float baseAnimDuration = 1f;

        [SerializeField]
        public float baseMinDuration = 0f;

        [Tooltip("Set to -1 to disable. Overrides the speed of the projectile.")]
        [SerializeField]
        public float speedOverride = -1;

        [Tooltip("Set to -1 to disable. Affects lifes of Fuse controllers and Projectile Impact Explosions.")]
        [SerializeField]
        public float fuseOverride = -1;

        private bool buttonReleased;
        private float minimumDuration;

        public override void OnEnter()
        {
            base.OnEnter(); //Call Base for it to do its things and then we override its values down here.
            minimumDuration = baseMinDuration / this.attackSpeedStat;
            if (base.characterBody)
                base.characterBody.SetAimTimer(1.5f); //I have no idea what this does
            base.PlayAnimation("Gesture, Additive", "FireMainWeapon", "FireMainWeapon.playbackRate", baseAnimDuration / this.attackSpeedStat);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!buttonReleased && !(base.inputBank && base.inputBank.skill1.down))
            {
                buttonReleased = true;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (base.fixedAge <= minimumDuration || !buttonReleased)
            {
                return InterruptPriority.PrioritySkill;
            }
            return InterruptPriority.Any;
        }

        public override void OnExit()
        {
            if (!buttonReleased && base.characterBody && base.skillLocator && base.skillLocator.primary.stock > 0)
            {
                base.characterBody.SetSpreadBloom(0f, false);
            }
            base.OnExit();
        }

        public override void FireProjectile()
        {
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                aimRay = this.ModifyProjectileAimRay(aimRay);
                aimRay.direction = Util.ApplySpread(aimRay.direction, this.minSpread, this.maxSpread, 1f, 1f, 0f, this.projectilePitchBonus);
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = projectilePrefab,
                    position = aimRay.origin,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    owner = gameObject,
                    damage = this.damageStat * this.damageCoefficient,
                    force = force,
                    crit = Util.CheckRoll(this.critStat, base.characterBody.master),
                    damageColorIndex = DamageColorIndex.Default,
                    target = null,
                    speedOverride = speedOverride,
                    fuseOverride = fuseOverride,
                    damageTypeOverride = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                //ProjectileManager.instance.FireProjectile(this.projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageStat * this.damageCoefficient, this.force, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
            }
        }
    }
}
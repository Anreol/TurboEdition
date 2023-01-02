using EntityStates;
using RoR2;
using UnityEngine;

namespace TurboEdition.EntityStates.CrabChest.Weapon
{
    internal class DoubleMinigunFire : DoubleMinigunState
    {
        public static GameObject muzzleVfxPrefab;
        public static float baseFireInterval;
        public static int baseBulletCount;
        public static float baseDamagePerSecondCoefficient;
        public static float baseForcePerSecond;
        public static float baseProcCoefficientPerSecond;
        public static float bulletMinSpread;
        public static float bulletMaxSpread;

        public static GameObject bulletTracerEffectPrefab;
        public static GameObject bulletHitEffectPrefab;

        public static bool bulletHitEffectNormal;
        public static float bulletMaxDistance;

        public static string fireSound;
        public static string startSound;
        public static string endSound;

        private float fireTimer;
        private Transform leftMuzzleVFXInstanceTransform;
        private Transform rightMuzzleVFXInstanceTransform;
        private float baseFireRate;
        private float baseBulletsPerSecond;
        private Run.FixedTimeStamp critEndTime;
        private Run.FixedTimeStamp lastCritCheck;

        public override void OnEnter()
        {
            base.OnEnter();
            if ((this.leftMuzzleTransform && rightMuzzleTransform) && DoubleMinigunFire.muzzleVfxPrefab)
            {
                this.leftMuzzleVFXInstanceTransform = UnityEngine.Object.Instantiate<GameObject>(DoubleMinigunFire.muzzleVfxPrefab, this.leftMuzzleTransform).transform;
                this.rightMuzzleVFXInstanceTransform = UnityEngine.Object.Instantiate<GameObject>(DoubleMinigunFire.muzzleVfxPrefab, this.rightMuzzleTransform).transform;
            }
            this.baseFireRate = 1f / DoubleMinigunFire.baseFireInterval;
            this.baseBulletsPerSecond = (float)DoubleMinigunFire.baseBulletCount * this.baseFireRate;
            this.critEndTime = Run.FixedTimeStamp.negativeInfinity;
            this.lastCritCheck = Run.FixedTimeStamp.negativeInfinity;
            Util.PlaySound(DoubleMinigunFire.startSound, base.gameObject);
            base.PlayCrossfade("Gesture, Additive", "FireMinigun", 0.2f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.fireTimer -= Time.fixedDeltaTime;
            if (this.fireTimer <= 0f)
            {
                float num = DoubleMinigunFire.baseFireInterval / this.attackSpeedStat;
                this.fireTimer += num;
                this.OnFireShared();
            }
            if (base.isAuthority && !base.skillButtonState.down)
            {
                this.outer.SetNextState(new DoubleMinigunSpindown());
                return;
            }
        }

        private void UpdateCrits()
        {
            if (this.lastCritCheck.timeSince >= 1f)
            {
                this.lastCritCheck = Run.FixedTimeStamp.now;
                if (base.RollCrit())
                {
                    this.critEndTime = Run.FixedTimeStamp.now + 2f;
                }
            }
        }

        public override void OnExit()
        {
            Util.PlaySound(DoubleMinigunFire.endSound, base.gameObject);
            if (this.leftMuzzleVFXInstanceTransform)
            {
                EntityState.Destroy(this.leftMuzzleVFXInstanceTransform.gameObject);
                this.leftMuzzleVFXInstanceTransform = null;
            }
            if (this.rightMuzzleVFXInstanceTransform)
            {
                EntityState.Destroy(this.rightMuzzleVFXInstanceTransform.gameObject);
                this.rightMuzzleVFXInstanceTransform = null;
            }
            base.PlayCrossfade("Gesture, Additive", "BufferEmpty", 0.2f);
            base.OnExit();
        }

        private void OnFireShared()
        {
            Util.PlayAttackSpeedSound(DoubleMinigunFire.fireSound, base.gameObject, attackSpeedStat);
            if (base.isAuthority)
            {
                this.OnFireAuthority();
            }
        }

        private void OnFireAuthority()
        {
            this.UpdateCrits();
            bool isCrit = !this.critEndTime.hasPassed;
            float damage = DoubleMinigunFire.baseDamagePerSecondCoefficient / this.baseBulletsPerSecond * this.damageStat;
            float force = DoubleMinigunFire.baseForcePerSecond / this.baseBulletsPerSecond;
            float procCoefficient = DoubleMinigunFire.baseProcCoefficientPerSecond / this.baseBulletsPerSecond;
            Ray aimRay = base.GetAimRay();
            new BulletAttack
            {
                bulletCount = (uint)DoubleMinigunFire.baseBulletCount,
                aimVector = aimRay.direction,
                origin = aimRay.origin,
                damage = damage,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Generic,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                maxDistance = DoubleMinigunFire.bulletMaxDistance,
                force = force,
                hitMask = LayerIndex.CommonMasks.bullet,
                minSpread = DoubleMinigunFire.bulletMinSpread,
                maxSpread = DoubleMinigunFire.bulletMaxSpread,
                isCrit = isCrit,
                owner = base.gameObject,
                muzzleName = DoubleMinigunFire.leftMuzzleName,
                smartCollision = false,
                procChainMask = default(ProcChainMask),
                procCoefficient = procCoefficient,
                radius = 0f,
                sniper = false,
                stopperMask = LayerIndex.CommonMasks.bullet,
                weapon = null, //Is set automatically to be owner if null
                tracerEffectPrefab = DoubleMinigunFire.bulletTracerEffectPrefab,
                spreadPitchScale = 1f,
                spreadYawScale = 1f,
                queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                hitEffectPrefab = DoubleMinigunFire.bulletHitEffectPrefab,
                HitEffectNormal = DoubleMinigunFire.bulletHitEffectNormal
            }.Fire();

            new BulletAttack
            {
                bulletCount = (uint)DoubleMinigunFire.baseBulletCount,
                aimVector = aimRay.direction,
                origin = aimRay.origin,
                damage = damage,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Generic,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                maxDistance = DoubleMinigunFire.bulletMaxDistance,
                force = force,
                hitMask = LayerIndex.CommonMasks.bullet,
                minSpread = DoubleMinigunFire.bulletMinSpread,
                maxSpread = DoubleMinigunFire.bulletMaxSpread,
                isCrit = isCrit,
                owner = base.gameObject,
                muzzleName = DoubleMinigunFire.rightMuzzleName,
                smartCollision = false,
                procChainMask = default(ProcChainMask),
                procCoefficient = procCoefficient,
                radius = 0f,
                sniper = false,
                stopperMask = LayerIndex.CommonMasks.bullet,
                weapon = null, //Is set automatically to be owner if null
                tracerEffectPrefab = DoubleMinigunFire.bulletTracerEffectPrefab,
                spreadPitchScale = 1f,
                spreadYawScale = 1f,
                queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                hitEffectPrefab = DoubleMinigunFire.bulletHitEffectPrefab,
                HitEffectNormal = DoubleMinigunFire.bulletHitEffectNormal
            }.Fire();
        }
    }
}
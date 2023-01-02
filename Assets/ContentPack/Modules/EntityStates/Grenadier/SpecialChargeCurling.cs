using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    internal class SpecialChargeCurling : AimThrowableBaseChargable
    {
        public override void OnEnter()
        {
            base.OnEnter();
            ProjectileCharacterController pcc = projectilePrefab.GetComponent<ProjectileCharacterController>();
            if (pcc)
            {
                //Fix up the base's missing speed since this prefab does not have a ProjectileSimple component
                targetProjectileBaseSpeed = pcc.velocity;
                float charge = CalcCharge();
                projectileBaseSpeed = targetProjectileBaseSpeed * charge;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            activatorSkillSlot.skillDef.canceledFromSprinting = !firedAtLeastOnce;
        }

        public override void ModifyProjectile(ref FireProjectileInfo fireProjectileInfo)
        {
            ProjectileCharacterController pcc = fireProjectileInfo.projectilePrefab.GetComponent<ProjectileCharacterController>();
            if (pcc)
            {
                pcc.velocity = fireProjectileInfo.speedOverride;
            }
            Vector3 forward = Vector3.ProjectOnPlane(base.inputBank.aimDirection, Vector3.up);
            fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(forward);

            base.ModifyProjectile(ref fireProjectileInfo);
        }
    }
}
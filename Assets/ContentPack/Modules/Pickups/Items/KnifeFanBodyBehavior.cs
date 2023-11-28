using RoR2;
using RoR2.Items;
using RoR2.Projectile;
using UnityEngine;

namespace TurboEdition.Items
{
    public class KnifeFanBodyBehavior : BaseItemBodyBehavior
    {
        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.KnifeFan;
        }

        private static GameObject projectilePrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("PRJ_KnifeFan"); //Resources.Load<GameObject>("prefabs/projectiles/EngiGrenadeProjectile"); for testing!

        private void Start()
        {
            base.body.onSkillActivatedServer += onSkillActivatedServer;
        }

        private void onSkillActivatedServer(GenericSkill obj)
        {
            if (projectilePrefab == null) return;
            if (body.GetComponent<SkillLocator>().utility == obj)
            {
                float y = Quaternion.LookRotation(body.inputBank.GetAimRay().direction).eulerAngles.y;
                float distance = 1f; //How away it will spawn from the body

                Vector3 aimDirection = body.inputBank.aimDirection;
                Vector3 crossVector = aimDirection == Vector3.up ? Vector3.down : Vector3.up;
                Vector3 up = Vector3.Cross(Vector3.Cross(aimDirection, crossVector), aimDirection);
                foreach (float angleSlice in new DegreeSlices(stack + 1, 0f))
                {
                    var angleAxisRot = Quaternion.AngleAxis(angleSlice, up) * aimDirection;
                    Vector3 origin = body.corePosition + Util.QuaternionSafeLookRotation(angleAxisRot) * (Vector3.forward * distance);
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        projectilePrefab = projectilePrefab,
                        position = origin,
                        rotation = Util.QuaternionSafeLookRotation(angleAxisRot),
                        owner = body.gameObject,
                        damage = body.damage * 2.85f,
                        force = 5,
                        crit = body.RollCrit(),
                        damageColorIndex = DamageColorIndex.Item
                    };
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }
        }

        private void OnDestroy()
        {
            base.body.onSkillActivatedServer -= onSkillActivatedServer;
        }
    }
}
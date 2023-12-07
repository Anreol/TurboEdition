using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace TurboEdition.Equipments
{
    internal class Hellchain : Equipment
    {
        public override EquipmentDef equipmentDef { get; set; } = Assets.mainAssetBundle.LoadAsset<EquipmentDef>("HellChain");
        public static GameObject projectilePrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("PRJ_HellChain_Mine");

        public override bool FireAction(EquipmentSlot slot)
        {
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = projectilePrefab,
                position = slot.characterBody.aimOrigin,
                rotation = Quaternion.LookRotation(slot.characterBody.inputBank.GetAimRay().direction),
                owner = slot.characterBody.gameObject,
                //damage = slot.characterBody.damage,
                //force = 0,
                //crit = body.RollCrit(),
                damageColorIndex = DamageColorIndex.DeathMark
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            slot.subcooldownTimer = 5f;
            return true;
        }
    }
}
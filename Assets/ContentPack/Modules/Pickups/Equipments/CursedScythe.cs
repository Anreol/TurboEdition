using HG;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Equipments
{
    public class CursedScythe : Equipment
    {
        public override EquipmentDef equipmentDef { get; set; } = Assets.mainAssetBundle.LoadAsset<EquipmentDef>("CursedScythe");
        private static GameObject projectilePrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("BigScytheProjectile");
        public override bool FireAction(EquipmentSlot slot)
        {
            if (!NetworkServer.active) return false;
            if (projectilePrefab == null) return false;
            return SpawnProjectile(slot.characterBody);
        }

        public bool SpawnProjectile(CharacterBody body)
        {
            int enemyCount = 1;
            for (TeamIndex teamCounter = TeamIndex.Neutral; teamCounter < TeamIndex.Count; teamCounter++)
            {
                if (TeamManager.IsTeamEnemy(body.teamComponent.teamIndex, teamCounter))
                {
                    if (teamCounter != TeamIndex.Neutral)
                    {
                        enemyCount += TeamComponent.GetTeamMembers(teamCounter).Count;
                    }
                }
            }
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = projectilePrefab,
                position = body.aimOrigin,
                rotation = Quaternion.LookRotation(body.inputBank.GetAimRay().direction),
                owner = body.gameObject,
                damage = body.damage * enemyCount,
                force = 15,
                crit = body.RollCrit(),
                damageColorIndex = DamageColorIndex.DeathMark
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            return true;
        }
	}
}

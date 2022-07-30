using RoR2;
using RoR2.Projectile;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Equipments
{
    public class CursedScythe : Equipment
    {
        public override EquipmentDef equipmentDef { get; set; } = Assets.mainAssetBundle.LoadAsset<EquipmentDef>("CursedScythe");
        private static GameObject projectilePrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("BigScytheOverlapAttack");

        public override bool FireAction(EquipmentSlot slot)
        {
            if (!NetworkServer.active || projectilePrefab == null) return false;
            return SpawnProjectile(slot.characterBody);
        }

        public bool OldSpawnProjectile(CharacterBody body)
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
                force = 0,
                crit = body.RollCrit(),
                damageColorIndex = DamageColorIndex.DeathMark
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            return true;
        }

        public bool SpawnProjectile(CharacterBody body)
        {
            Vector3 impactPosition = Vector3.zero;
            CharacterBody[] allCharacters = new CharacterBody[CharacterBody.readOnlyInstancesList.Count];
            CharacterBody.readOnlyInstancesList.ToArray<CharacterBody>().CopyTo(allCharacters, 0);
            Util.ShuffleArray<CharacterBody>(allCharacters);
            for (int i = 0; i < allCharacters.Length; i++)
            {
                if (allCharacters[i].teamComponent && body.teamComponent)
                {
                    if (TeamManager.IsTeamEnemy(allCharacters[i].teamComponent.teamIndex, body.teamComponent.teamIndex ) && allCharacters[i].coreTransform)
                    {
                        EntityStateMachine entityStateMachine = EntityStateMachine.FindByCustomName(allCharacters[i].gameObject, "Body");
                        if (entityStateMachine != null && entityStateMachine.state.GetType() == entityStateMachine.initialStateType.stateType)
                            continue; //Do not spawn on enemies that are still spawning, thats rude!
                        impactPosition = allCharacters[i].coreTransform.position;
                        break;
                    }
                }
            }
            if (impactPosition == Vector3.zero)
                return false;
            Vector3 origin = impactPosition + Vector3.up * 6f;
            Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
            onUnitSphere.y = -1f;
            RaycastHit raycastHit;
            if (Physics.Raycast(origin, onUnitSphere, out raycastHit, 12f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                impactPosition = raycastHit.point;
            }
            //Else snap to the ground no matter what
           /* else if (Physics.Raycast(impactPosition, Vector3.down, out raycastHit, float.PositiveInfinity, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                impactPosition = raycastHit.point;
            }*/

            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = projectilePrefab,
                position = impactPosition,
                rotation = new Quaternion(0,0,0,0),
                owner = body.gameObject,
                damage = body.damage * 100,
                force = 0,
                crit = body.RollCrit(),
                damageColorIndex = DamageColorIndex.Item
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            return true;
        }
    }
}
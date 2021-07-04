using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class KnifeFan : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("KnifeFan");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<KnifeFanBehavior>(stack);
        }

        internal class KnifeFanBehavior : CharacterBody.ItemBehavior
        {
            GameObject projectilePrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("KnifeFanProjectile");
            private void OnEnable()
            {
                Debug.LogWarning("woke");
                body.onSkillActivatedServer += Body_onSkillActivatedServer;
            }

            private void Body_onSkillActivatedServer(GenericSkill obj)
            {
                if (!NetworkServer.active) return;
                Debug.LogWarning("trol");
                if (body.GetComponent<SkillLocator>().utility == obj)
                {
                    Debug.LogWarning("trolo");
                    float y = Quaternion.LookRotation(body.GetComponent<TurboItemManager>().GetAimRay().direction).eulerAngles.y;
                    float distance = 3f; //How away it will spawn from the body
                    foreach (float num2 in new DegreeSlices(2 + (stack - 1), 0.5f))
                    {
                        Debug.LogWarning("trololo");
                        Quaternion rotation = Quaternion.Euler(0f, y + num2, 0f);
                        Quaternion rotation2 = Quaternion.Euler(0f, y + num2 + 180f, 0f);
                        Vector3 position = transform.position + rotation * (Vector3.forward * distance);
                        FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                        {
                            projectilePrefab = projectilePrefab,
                            position = position,
                            rotation = rotation2,
                            owner = body.gameObject,
                            damage = body.damage,
                            force = 200,
                            crit = body.RollCrit()
                        };
                        ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    }
                    Debug.LogWarning("trolololo");
                }
            }
        }
    }
}
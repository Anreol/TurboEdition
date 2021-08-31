/*using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class StandBonus : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("KnifeFan");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<KnifeFanBehaviour>(stack);
        }

        internal class KnifeFanBehaviour : CharacterBody.ItemBehavior
        {
            private static GameObject projectilePrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("KnifeFanProjectile");
            private void Start() //On Start since we need to subscribe to the body, ANYTHING THAT HAS TO DO WITH BODIES, CANNOT BE ON AWAKE() OR ONENABLE()
            {
                if (!body)
                {
                    Debug.LogWarning("Body not available or does not exist.");
                    return;
                }
                base.body.onSkillActivatedServer += Body_onSkillActivatedServer;
            }

            private void Body_onSkillActivatedServer(GenericSkill obj)
            {
                if (!NetworkServer.active) return;
                if (body.GetComponent<SkillLocator>().utility == obj)
                {
                    Debug.LogWarning("Skill was utility!");
                    float y = Quaternion.LookRotation(body.GetComponent<TurboItemManager>().GetAimRay().direction).eulerAngles.y;
                    float distance = 3f; //How away it will spawn from the body
                    foreach (float num2 in new DegreeSlices(2 + (stack - 1), 0.5f))
                    {
                        Debug.LogWarning("Spawning a knife.");
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
                    Debug.LogWarning("Done.");
                }
            }
        }
    }
}*/
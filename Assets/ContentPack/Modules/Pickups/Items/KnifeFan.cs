using RoR2;
using RoR2.Projectile;
using UnityEngine;

//TODO: Check if utility skill has been sucessfully used so that floating whore cannot spam this by cancelling ice wall
//make it so knifes use your aim vector or whatever

namespace TurboEdition.Items
{
    public class KnifeFan : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("KnifeFan");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<KnifeFanBehaviour>(stack);
        }

        internal class KnifeFanBehaviour : CharacterBody.ItemBehavior
        {
            private static GameObject projectilePrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("KnifeFanProjectile"); //Resources.Load<GameObject>("prefabs/projectiles/EngiGrenadeProjectile"); for testing!

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
                //if (!NetworkServer.active) return;
                /*if (!Util.HasEffectiveAuthority(body.networkIdentity))
                {
                    TELog.LogW("Function 'System.Void TurboEdition.Items.KnifeFan::Body_onSkillActivatedServer() called without authority.'");
                    return;
                }*/ //This isnt needed because its only run in the server...
                if (projectilePrefab == null) return;
                if (body.GetComponent<SkillLocator>().utility == obj)
                {
                    TELog.LogW("Skill was utility!");
                    float y = Quaternion.LookRotation(body.inputBank.GetAimRay().direction).eulerAngles.y;
                    float distance = 1f; //How away it will spawn from the body
                    foreach (float num2 in new DegreeSlices(2 + (stack - 1), 5f))
                    {
                        TELog.LogW("Spawning a knife.");
                        Quaternion rotationFromBody = Quaternion.Euler(0f, y + num2, 0f);
                        Quaternion rotation2 = Quaternion.Euler(0f, y + num2 + 180f, 0f); //Default is (0f, y + num2 + 180f, 0f)
                        Vector3 origin = transform.position + rotationFromBody * (Vector3.forward * distance);
                        FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                        {
                            projectilePrefab = projectilePrefab,
                            position = origin,
                            rotation = rotation2,
                            owner = body.gameObject,
                            damage = body.damage * 2.5f,
                            force = 15,
                            crit = body.RollCrit(),
                            damageColorIndex = DamageColorIndex.Bleed
                        };
                        ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    }
                    TELog.LogW("Done.");
                }
            }

            private void OnDestroy()
            {
                base.body.onSkillActivatedServer -= Body_onSkillActivatedServer;
            }
        }
    }
}
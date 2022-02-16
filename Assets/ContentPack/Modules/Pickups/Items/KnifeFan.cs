using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

//TODO: Check if utility skill has been sucessfully used so that floating whore cannot spam this by cancelling ice wall
//make it so knifes use your aim vector or whatever

namespace TurboEdition.Items
{
    public class KnifeFan : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("KnifeFan");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            if (NetworkServer.active)
                body.AddItemBehavior<KnifeFanBehaviourServer>(stack);
        }

        internal class KnifeFanBehaviourServer : CharacterBody.ItemBehavior
        {
            private static GameObject projectilePrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("KnifeFanProjectile"); //Resources.Load<GameObject>("prefabs/projectiles/EngiGrenadeProjectile"); for testing!

            private void Start() //On Start since we need to subscribe to the body, ANYTHING THAT HAS TO DO WITH BODIES, CANNOT BE ON AWAKE() OR ONENABLE()
            {
                if (!body)
                {
                    TELog.LogW("Body not available or does not exist.");
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
                base.body.onSkillActivatedServer -= Body_onSkillActivatedServer;
            }
        }
    }
}
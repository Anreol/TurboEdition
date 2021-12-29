using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine;

namespace TurboEdition.Items
{
    internal class SuperStickies : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("SuperStickies");

        public override void Initialize()
        {
            IL.RoR2.GlobalEventManager.OnHitEnemy += ILHook;
        }

        public static void ILHook(ILContext il) //Thanks bubbet for code
        {
            var c = new ILCursor(il);
            c.GotoNext(
                x => x.OpCode == OpCodes.Ldsfld && (x.Operand as FieldReference)?.Name == "StickyBomb",
                x => x.MatchCallvirt<Inventory>("GetItemCount")
            );
            // Done in two because theres a bunch of shit inbetween

            /*
            var cb = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(out cb),
                x => x.MatchCall(out _),
                x => x.MatchBrfalse(out _)
            );*/

            //c.Emit(OpCodes.Ldloc, cb);
            //c.EmitDelegate<Action<CharacterBody>>(body => StickyBombProced(body));

            c.GotoNext(MoveType.After,
            x => x.MatchCallvirt<ProjectileManager>("FireProjectile")
            );

            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<DamageInfo>>(damageInfo => StickyBombProced(damageInfo));
        }

        private static void StickyBombProced(DamageInfo damageInfo)
        {
            CharacterBody characterBody = damageInfo.attacker.GetComponent<CharacterBody>();
            if (characterBody)
            {
                CharacterMaster master = characterBody.master;
                if (master)
                {
                    if (master.inventory.GetItemCount(ItemCatalog.FindItemIndex("SuperStickies")) > 0 && Util.CheckRoll(15f * damageInfo.procCoefficient))
                    {
                        bool alive = characterBody.healthComponent.alive;
                        float num4 = 5f;
                        Vector3 position = damageInfo.position;
                        Vector3 forward = characterBody.corePosition - position;
                        float magnitude = forward.magnitude;
                        Quaternion rotation = (magnitude != 0f) ? Util.QuaternionSafeLookRotation(forward) : UnityEngine.Random.rotationUniform;
                        float damageCoefficient4 = 1.8f;
                        float damage = Util.OnHitProcDamage(damageInfo.damage, characterBody.damage, damageCoefficient4);
                        ProjectileManager.instance.FireProjectile(Resources.Load<GameObject>("Prefabs/Projectiles/StickyBomb"), position, rotation, damageInfo.attacker, damage, 100f, damageInfo.crit, DamageColorIndex.Item, null, alive ? (magnitude * num4) : -1f);
                    }
                }
            }

        }
    }
}
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.Items
{
    class SuperStickies : Item
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
                x => x.OpCode == OpCodes.Ldsfld && x.Operand.ToString() == "StickyBomb",
                x => x.OpCode == OpCodes.Callvirt //x.MatchCallvirt<Inventory>("GetItemCount")
            );
            // Done in two because theres a bunch of shit inbetween
           
            var cb = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(out cb),
                x => x.MatchCall(out _),
                x => x.MatchBrfalse(out _)
            );


            c.Emit(OpCodes.Ldloc, cb);
            c.EmitDelegate<Action<CharacterBody>>(body => StickyBombProced(body));
        }

        private static void StickyBombProced(CharacterBody body)
        {
            TELog.logger.LogWarning("Holy shit it procced???");
        }
    }
}

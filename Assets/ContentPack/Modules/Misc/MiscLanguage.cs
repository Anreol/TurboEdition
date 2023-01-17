using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Linq;

namespace TurboEdition.Utils
{
    internal class MiscLanguage
    {
        private static readonly string[] standardTurboDeathQuoteTokens = (from i in Enumerable.Range(0, 37)
                                                                          select "PLAYER_DEATH_QUOTE_TE_" + TextSerialization.ToStringInvariant(i)).ToArray<string>();

        public static void AddDeathMessages()
        {
            //HG.ArrayUtils.CloneTo(standardTurboDeathQuoteTokens, ref GlobalEventManager.standardDeathQuoteTokens); //Thanks bubbet for pointing this out lol
            //GlobalEventManager.standardDeathQuoteTokens = standardTurboDeathQuoteTokens.Union(GlobalEventManager.standardDeathQuoteTokens).ToArray();
            GlobalEventManager.standardDeathQuoteTokens = GlobalEventManager.standardDeathQuoteTokens.Concat(standardTurboDeathQuoteTokens).ToArray();
        }

        [SystemInitializer]
        public static void ExtraBodyModifiers()
        {
            IL.RoR2.Util.GetBestBodyName += (il) =>
            {
                MonoMod.Cil.ILCursor c = new MonoMod.Cil.ILCursor(il);
                c = c.GotoNext(x => x.MatchRet());
                c.Emit(OpCodes.Ldloc_0);//CharacterBody
                c.EmitDelegate<Func<string, CharacterBody, string>>((str, cb) =>
                {
                    if (cb && cb.inventory)
                    {
                        //if (cb.inventory.GetItemCount(Origin.OriginBonusItem) > 0)
                        //{
                        //    str += Language.GetString("RISKYARTIFACTS_ORIGIN_MODIFIER");
                        //}
                        //if (cb.inventory.GetItemCount(BrotherInvasion.BrotherInvasionBonusItem) > 0)
                        //{
                        //    str = Language.GetString("RISKYARTIFACTS_BROTHERINVASION_MODIFIER") + str;
                        //}
                    }
                    return str;
                });
            };
        }
    }
}
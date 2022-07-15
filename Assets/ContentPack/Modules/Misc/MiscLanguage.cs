using RoR2;
using System.Linq;

namespace TurboEdition.Misc
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
    }
}
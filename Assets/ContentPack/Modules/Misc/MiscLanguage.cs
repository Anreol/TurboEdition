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
            HG.ArrayUtils.CloneTo(standardTurboDeathQuoteTokens, ref GlobalEventManager.standardDeathQuoteTokens);
        }
    }
}
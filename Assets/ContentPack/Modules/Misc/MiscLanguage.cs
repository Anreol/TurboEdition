using RoR2;
using System.IO;
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
            GlobalEventManager.standardDeathQuoteTokens = standardTurboDeathQuoteTokens.Union(GlobalEventManager.standardDeathQuoteTokens).ToArray();
            //GlobalEventManager.standardDeathQuoteTokens.Concat(standardTurboDeathQuoteTokens);
        }

        //I'll never forgive the Hopoo
        /// <summary>
        /// Some fucked up shit to add your god damn tokens to the game. Its NOT grouped so make sure that every language is in its own folder.
        /// </summary>
        /// <param name="rootFolder"></param>
        public static void FixLanguageFolders(string rootFolder)
        {
            /*var allLanguageFolders = Directory.EnumerateDirectories(rootFolder);
            foreach (Language ltlfgt in Language.GetAllLanguages())
            {
                foreach (var threeam in allLanguageFolders)
                {
                    if (threeam.Contains(ltlfgt.name))
                    {
                        HG.ArrayUtils.ArrayAppend<string>(ref ltlfgt.folders, threeam);
                    }
                }
            }
                //Reload all folders, by this time, the language has already been initialized, thats why we are doing this.
            Language.currentLanguage.UnloadStrings();
            Language.currentLanguage.LoadStrings();
            Language.english.UnloadStrings();
            Language.english.LoadStrings();*/
        }
    }
}
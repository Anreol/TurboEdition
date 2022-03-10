using RoR2;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

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
            GlobalEventManager.standardDeathQuoteTokens.Concat(standardTurboDeathQuoteTokens);
        }

        //I'll never forgive the Hopoo
        /// <summary>
        /// Some fucked up shit to add your god damn tokens to the game
        /// </summary>
        /// <param name="rootFolder"></param>
        public static void FixLanguageFolders(string rootFolder)
        {
            var allLanguageFolders = Directory.EnumerateDirectories(rootFolder);
            var groupedByName = allLanguageFolders.GroupBy((string directory) => directory, StringComparer.OrdinalIgnoreCase);
            Debug.LogWarning(rootFolder);
            Debug.LogWarning(allLanguageFolders);
            foreach (string item in allLanguageFolders)
            {
                Debug.LogWarning(item);
            }
            Debug.LogWarning(groupedByName);

            foreach (Language ltlfgt in Language.GetAllLanguages())
            {
                foreach (var threeam in allLanguageFolders)
                {
                    if (threeam.EndsWith(ltlfgt.name))
                    {
                        HG.ArrayUtils.ArrayAppend<string>(ref ltlfgt.folders, threeam);
                    }
                }
            }
            /*//Folder root, name. (?)
            foreach (IGrouping<string, string> grouping in groupedByName)
            {
                Debug.LogWarning(grouping);
                string[] folders = grouping.ToArray();
                foreach (Language analingus in Language.GetAllLanguages())
                {
                    if (analingus.name == grouping.Key)
                    {
                        foreach (var shitfuckinghead in folders)
                        {
                            HG.ArrayUtils.ArrayAppend<string>(ref analingus.folders, shitfuckinghead);
                        }
                    }
                }
            }*/

            /*foreach (Language lang in Language.GetAllLanguages())
        {
            string[] vs = Directory.EnumerateDirectories(rootFolder).Where(god => god == lang.name).ToArray();
            foreach (string item in vs)
            {
                HG.ArrayUtils.ArrayAppend<string>(ref lang.folders, item);
            }
        }*/

            /*foreach (IGrouping<string, string> grouping in Directory.EnumerateDirectories(rootFolder).GroupBy((string languageRootFolder) => languageRootFolder, StringComparer.OrdinalIgnoreCase).ToArray<IGrouping<string, string>>())
            {
                string[] licious = grouping.ToArray();
                for (int i = 0; i < licious.Length; i++)
                {
                    HG.ArrayUtils.ArrayAppend<string>(ref Language.languagesByName[(grouping.Key)].folders, licious[i]);
                }
            }*/
        }
    }
}
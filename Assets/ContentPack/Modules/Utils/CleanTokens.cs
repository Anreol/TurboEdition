using RoR2;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace TurboEdition.Utils
{
    internal class CleanTokens
    {
        //you WILL make a dictionary
        //you WILL have slower lookups
        //you WONT enjoy looking up by index
        public static string[] cleanItemNames = new string[0];
        public static string[] cleanBodyNames = new string[0];
        public static ItemIndex[] itemIndices = new ItemIndex[0];
        public static BodyIndex[] bodyIndices = new BodyIndex[0];

        public static bool checkASCII = true;
        public static bool isCurrentLanguageASCII = true;

        [RoR2.SystemInitializer(new Type[] { typeof(RoR2.Language) })]
        public static void Init()
        {
            RoR2.Language.onCurrentLanguageChanged += Language_onCurrentLanguageChanged;
            Language_onCurrentLanguageChanged();
        }

        private static void Language_onCurrentLanguageChanged()
        {
            isCurrentLanguageASCII = true;
            if (checkASCII)
            {
                foreach (string name in nonAsciiLanguages)
                {
                    if (Language.currentLanguage.name == name)
                    {
                        isCurrentLanguageASCII = false;
                    }
                };
            }
            if (cleanItemNames.Length > 0)
            {
                cleanItemNames = new string[0];
                cleanBodyNames = new string[0];
                itemIndices = new ItemIndex[0];
                bodyIndices = new BodyIndex[0];
            }

            LoadCatalogs();
        }

        private static void LoadCatalogs()
        {
            foreach (ItemIndex item in RoR2.ItemCatalog.allItems)
            {
                ItemDef itemDef = ItemCatalog.GetItemDef(item);
                string itemString = (isCurrentLanguageASCII) ? Language.GetString(itemDef.nameToken) : (RoR2.Language.GetString(itemDef.nameToken, RoR2.Language.english.name));
                string toAdd = CleanString(itemString);
                if (toAdd.StartsWith("item_") || toAdd.Length <= 1 || toAdd == null)
                {
                    continue;
                }
                HG.ArrayUtils.ArrayAppend(ref cleanItemNames, toAdd);
                HG.ArrayUtils.ArrayAppend(ref itemIndices, item);
            }

            foreach (CharacterBody item in RoR2.BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                string bodyString = (isCurrentLanguageASCII) ? (RoR2.Language.GetString(item.baseNameToken)) : (RoR2.Language.GetString(item.baseNameToken, RoR2.Language.english.name));
                string toAdd = CleanString(bodyString);
                if (toAdd == "???" || toAdd.Contains("body_name") || toAdd.Length <= 1 || toAdd == null)
                {
                    continue;
                }
                HG.ArrayUtils.ArrayAppend(ref cleanBodyNames, toAdd);
                HG.ArrayUtils.ArrayAppend(ref bodyIndices, item.bodyIndex);
            }
            /*foreach (string item in cleanItemNames)
            {
                TELog.LogW(item);
            }
            foreach (string item in cleanBodyNames)
            {
                TELog.LogW(item);
            }*/
        }

        //Removes Diacritics, Control characters, and whitespaces
        private static string CleanString(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark && unicodeCategory != UnicodeCategory.Control && unicodeCategory != UnicodeCategory.SpaceSeparator && unicodeCategory != UnicodeCategory.OtherPunctuation && unicodeCategory != UnicodeCategory.DashPunctuation)
                {
                    stringBuilder.Append(c);
                }
            }
            //if (stringBuilder.ToString().Length == 0)
            //    return null;
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
        }

        //https://partner.steamgames.com/doc/store/localization
        public static string[] nonAsciiLanguages = new string[] {
            "ar",
            "bg",
            "zh-CN",
            "zh-TW",
            "cs",
            "el",
            "ja",
            "ko",
            "ru",
            "th",
            "tr",
            "uk",
            "vn"
        };
    }
}
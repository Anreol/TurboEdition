using RoR2;
using System;
using UnityEngine;

namespace TurboEdition.Misc
{
    internal class RulebookExtras
    {
        protected static readonly RuleDef wormEliteHonorRule = RuleCatalog.FindRuleDef("Misc.WormArtifactEliteHonor");
        public static bool runWormEliteHonor
        {
            get
            {
                if (Run.instance != null)
                {
                    Debug.LogWarning(wormEliteHonorRule);
                    Debug.LogWarning(RuleCatalog.FindRuleDef("Misc.WormArtifactEliteHonor"));
                    Debug.LogWarning(wormEliteHonorRule.globalName);
                    foreach (var item in wormEliteHonorRule.choices)
                    {
                        Debug.LogWarning(item);
                        Debug.LogWarning(item.globalName);
                    }
                    Debug.LogWarning(Run.instance.ruleBook.ruleValues[wormEliteHonorRule.globalIndex]);
                    return (bool)Run.instance.ruleBook.GetRuleChoice(wormEliteHonorRule).extraData;
                }
                return false;
            }
        }

        [SystemInitializer(new Type[]
        {
            typeof(RuleCatalog),
        })]
        private static void Init()
        {
            RuleDef wormEliteHonorRule = new RuleDef("Misc.WormArtifactEliteHonor", "RULE_MISC_WORMARTIFACT_ELITE_HONOR");
            
            RuleChoiceDef wormEliteHonorChoiceOn = wormEliteHonorRule.AddChoice("On", true, false);
            wormEliteHonorChoiceOn.tooltipNameToken = "RULE_WORMARTIFACTELITEHONOR_CHOICE_ON_NAME";
            wormEliteHonorChoiceOn.tooltipBodyToken = "RULE_WORMARTIFACTELITEHONOR_CHOICE_ON_DESC";
            wormEliteHonorChoiceOn.tooltipNameColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Artifact);
            //wormEliteHonorChoiceOn.tooltipBodyColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.BossItemDark); No rule does this
            wormEliteHonorChoiceOn.onlyShowInGameBrowserIfNonDefault = true;
            wormEliteHonorChoiceOn.unlockable = RoR2Content.Artifacts.EliteOnly.unlockableDef;
            wormEliteHonorChoiceOn.excludeByDefault = false;
            //wormEliteHonorChoiceOn.sprite =  

            RuleChoiceDef wormEliteHonorChoiceOff = wormEliteHonorRule.AddChoice("Off", false, false);
            wormEliteHonorChoiceOff.tooltipNameToken = "RULE_WORMARTIFACTELITEHONOR_CHOICE_OFF_NAME";
            wormEliteHonorChoiceOff.tooltipBodyToken = "RULE_WORMARTIFACTELITEHONOR_CHOICE_OFF_DESC";
            wormEliteHonorChoiceOff.tooltipNameColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable);
            //wormEliteHonorChoiceOff.tooltipBodyColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.BossItemDark); No rule does this
            wormEliteHonorChoiceOff.onlyShowInGameBrowserIfNonDefault = true;
            wormEliteHonorChoiceOff.excludeByDefault = false;

            wormEliteHonorRule.MakeNewestChoiceDefault();
            RuleCatalog_PatchedAddRule(wormEliteHonorRule, 4);

            Debug.LogWarning(wormEliteHonorRule);

        }

        //Categories as of pre-survivors of the void
        //0 - Difficulty, 1 - Artifacts, 2 - Items, 3 - Equipment, 4 - Misc
        private static void RuleCatalog_PatchedAddRule(RuleDef ruleDef, int RuleCategoryDefIndex) //Meant to be used post Init
        {
            ruleDef.category = RuleCatalog.GetCategoryDef(RuleCategoryDefIndex);
            ruleDef.globalIndex = RuleCatalog.allRuleDefs.Count;
            RuleCatalog.allCategoryDefs[RuleCategoryDefIndex].children.Add(ruleDef);
            RuleCatalog.allRuleDefs.Add(ruleDef);

            TELog.LogW(ruleDef.category.displayToken);

            if (RuleCatalog.highestLocalChoiceCount < ruleDef.choices.Count)
                RuleCatalog.highestLocalChoiceCount = ruleDef.choices.Count;

            RuleCatalog.ruleDefsByGlobalName[ruleDef.globalName] = ruleDef;
            for (int i = 0; i < ruleDef.choices.Count; i++)
            {
                RuleChoiceDef ruleChoiceDef = ruleDef.choices[i];
                ruleChoiceDef.localIndex = i;
                ruleChoiceDef.globalIndex = RuleCatalog.allChoicesDefs.Count;
                RuleCatalog.allChoicesDefs.Add(ruleChoiceDef);

                RuleCatalog.ruleChoiceDefsByGlobalName[ruleChoiceDef.globalName] = ruleChoiceDef;

                if (ruleChoiceDef.unlockable)
                    HG.ArrayUtils.ArrayAppend(ref RuleCatalog._allChoiceDefsWithUnlocks, ruleChoiceDef);
            }
        }
    }
}
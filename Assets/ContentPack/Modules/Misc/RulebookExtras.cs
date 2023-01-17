﻿using RoR2;
using System;
using UnityEngine;

namespace TurboEdition.Utils
{
    internal class RulebookExtras
    {
        public static bool runWormEliteHonor
        {
            get
            {
                if (Run.instance != null)
                {
                    return (bool)Run.instance.ruleBook.GetRuleChoice(RuleCatalog.FindRuleDef("Misc.WormArtifactEliteHonor")).extraData;
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
            wormEliteHonorChoiceOn.requiredUnlockable = RoR2Content.Artifacts.EliteOnly.unlockableDef;
            wormEliteHonorChoiceOn.excludeByDefault = false;
            wormEliteHonorChoiceOn.sprite = Assets.mainAssetBundle.LoadAsset<Sprite>("texHonorWormRuleOn");

            RuleChoiceDef wormEliteHonorChoiceOff = wormEliteHonorRule.AddChoice("Off", false, false);
            wormEliteHonorChoiceOff.tooltipNameToken = "RULE_WORMARTIFACTELITEHONOR_CHOICE_OFF_NAME";
            wormEliteHonorChoiceOff.tooltipBodyToken = "RULE_WORMARTIFACTELITEHONOR_CHOICE_OFF_DESC";
            wormEliteHonorChoiceOff.tooltipNameColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable);
            //wormEliteHonorChoiceOff.tooltipBodyColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.BossItemDark); No rule does this
            wormEliteHonorChoiceOff.onlyShowInGameBrowserIfNonDefault = true;
            wormEliteHonorChoiceOff.excludeByDefault = false;
            wormEliteHonorChoiceOff.sprite = Assets.mainAssetBundle.LoadAsset<Sprite>("texHonorWormRuleOff");

            wormEliteHonorRule.MakeNewestChoiceDefault();
            RuleCatalog_PatchedAddRule(wormEliteHonorRule, 5);
        }

        //Categories as of survivors of the void
        //0 - Difficulty, 1 - Expansions, 2 - Artifacts, 3 - Items, 4 - Equipment, 5 - Misc
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

                if (ruleChoiceDef.requiredUnlockable)
                    HG.ArrayUtils.ArrayAppend(ref RuleCatalog._allChoiceDefsWithUnlocks, ruleChoiceDef);
            }
        }
    }
}
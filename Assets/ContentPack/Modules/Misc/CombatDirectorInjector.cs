using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.Misc
{
    class CombatDirectorInjector
    {
        public static DccsPool.ConditionalPoolEntry frozenWallExtraEntry;
        public static DccsPool.ConditionalPoolEntry wispGraveyardExtraEntry;
        public static DccsPool.ConditionalPoolEntry dampCaveExtraEntry;

        [SystemInitializer(new Type[]
        {
            typeof(SceneCatalog),
            typeof(BodyCatalog),
            typeof(EliteCatalog), //The combat director has this
        })]
        public static void Init()
        {

            frozenWallExtraEntry = new DccsPool.ConditionalPoolEntry();
            frozenWallExtraEntry.requiredExpansions = new ExpansionDef[] { TEContent.Expansions.TurboExpansion };
            frozenWallExtraEntry.weight = 1;
            frozenWallExtraEntry.dccs = Assets.mainAssetBundle.LoadAsset<DirectorCardCategorySelection>("dccsFrozenWallMonstersTE");

            wispGraveyardExtraEntry = new DccsPool.ConditionalPoolEntry();
            wispGraveyardExtraEntry.requiredExpansions = new ExpansionDef[] { TEContent.Expansions.TurboExpansion };
            wispGraveyardExtraEntry.weight = 1;
            wispGraveyardExtraEntry.dccs = Assets.mainAssetBundle.LoadAsset<DirectorCardCategorySelection>("dccsWispGraveyardMonstersTE");

            dampCaveExtraEntry = new DccsPool.ConditionalPoolEntry();
            dampCaveExtraEntry.requiredExpansions = new ExpansionDef[] { TEContent.Expansions.TurboExpansion };
            dampCaveExtraEntry.weight = 1;
            dampCaveExtraEntry.dccs = Assets.mainAssetBundle.LoadAsset<DirectorCardCategorySelection>("dccsDampCaveMonstersTE");

            Stage.onServerStageBegin += onServerStageBegin; //Server has auth over stage
        }

        private static void onServerStageBegin(Stage obj)
        {
            if (obj.sceneDef == SceneCatalog.GetSceneDefFromSceneName("frozenwall"))
            {
                //Pool categories usually have: Standard, Family event, and (maybe?) VoidInvasion
                HG.ArrayUtils.ArrayAppend<DccsPool.ConditionalPoolEntry>(ref ClassicStageInfo.instance.monsterDccsPool.poolCategories[0].includedIfConditionsMet, frozenWallExtraEntry);
            }
            if (obj.sceneDef == SceneCatalog.GetSceneDefFromSceneName("wispgraveyard"))
            {
                //Pool categories usually have: Standard, Family event, and (maybe?) VoidInvasion
                HG.ArrayUtils.ArrayAppend<DccsPool.ConditionalPoolEntry>(ref ClassicStageInfo.instance.monsterDccsPool.poolCategories[0].includedIfConditionsMet, wispGraveyardExtraEntry);
            }
        }
    }
}

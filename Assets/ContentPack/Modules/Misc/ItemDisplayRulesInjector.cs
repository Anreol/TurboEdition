using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.Misc
{
    class ItemDisplayRulesInjector
    {
        public static void DoInjection()
        {
            AppendIDRSToBody(RoR2Content.Survivors.Commando.bodyPrefab.GetComponent<CharacterBody>());
        }
        public static void AppendIDRSToBody(CharacterBody characterBody)
        {
            ItemDisplayRuleSet idrs = characterBody.GetComponentInChildren<CharacterModel>().itemDisplayRuleSet;
            if (idrs)
            {
                ItemDisplayRuleSet loadedIDRS = Assets.mainAssetBundle.LoadAsset<ItemDisplayRuleSet>(idrs.name);
                if (loadedIDRS)
                {
                    idrs.keyAssetRuleGroups = loadedIDRS.keyAssetRuleGroups.Union(idrs.keyAssetRuleGroups).ToArray();
                }
            }
        }
    }
}

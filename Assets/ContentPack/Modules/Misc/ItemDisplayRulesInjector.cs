using RoR2;
using System;
using System.Linq;

namespace TurboEdition.Misc
{
    internal class ItemDisplayRulesInjector
    {
        [SystemInitializer(new Type[]
        {
            typeof(ItemCatalog),
            typeof(EquipmentCatalog),
            typeof(BodyCatalog)
        })]
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
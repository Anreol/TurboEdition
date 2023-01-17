using RoR2;
using System;
using System.Linq;

namespace TurboEdition.Utils
{
    internal class ItemDisplayRulesInjector
    {
        internal static readonly string[] bodyNames = { "VoidSurvivorBody", "EngiTurretBody", "EngiWalkerTurretBody" };

        [SystemInitializer(new Type[]
        {
            typeof(ItemCatalog),
            typeof(EquipmentCatalog),
            typeof(BodyCatalog)
        })]
        public static void DoInjection()
        {
            AppendIDRSToBody(RoR2Content.Survivors.Bandit2.bodyPrefab.GetComponent<CharacterBody>());
            AppendIDRSToBody(RoR2Content.Survivors.Captain.bodyPrefab.GetComponent<CharacterBody>());
            AppendIDRSToBody(RoR2Content.Survivors.Commando.bodyPrefab.GetComponent<CharacterBody>());
            AppendIDRSToBody(RoR2Content.Survivors.Croco.bodyPrefab.GetComponent<CharacterBody>());
            AppendIDRSToBody(RoR2Content.Survivors.Engi.bodyPrefab.GetComponent<CharacterBody>());
            AppendIDRSToBody(RoR2Content.Survivors.Huntress.bodyPrefab.GetComponent<CharacterBody>());
            AppendIDRSToBody(RoR2Content.Survivors.Loader.bodyPrefab.GetComponent<CharacterBody>());
            AppendIDRSToBody(RoR2Content.Survivors.Mage.bodyPrefab.GetComponent<CharacterBody>());
            AppendIDRSToBody(RoR2Content.Survivors.Merc.bodyPrefab.GetComponent<CharacterBody>());
            AppendIDRSToBody(RoR2Content.Survivors.Toolbot.bodyPrefab.GetComponent<CharacterBody>());
            AppendIDRSToBody(RoR2Content.Survivors.Treebot.bodyPrefab.GetComponent<CharacterBody>());
            AppendIDRSToBody(DLC1Content.Survivors.Railgunner.bodyPrefab.GetComponent<CharacterBody>());
            foreach (string body in bodyNames)
            {
                AppendIDRSToBody(BodyCatalog.FindBodyPrefab(body).GetComponent<CharacterBody>());
            }
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
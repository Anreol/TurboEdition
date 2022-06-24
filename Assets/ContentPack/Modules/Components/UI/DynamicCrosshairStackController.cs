using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.Components.UI
{
    [RequireComponent(typeof(CrosshairController))]
    public class DynamicCrosshairStackController : MonoBehaviour
    {
        [Tooltip("Should it perform an additional operation to remove any possible null reference objects. Not recommended as it should work just fine by default.")]
        public bool cleanupNulls;

        public DynamicSkillStackDisplay[] dynamicSkillStackDisplays;

        /// <summary>
        /// Skill stocks that the CrosshairController had when awoken.
        /// </summary>
        private CrosshairController.SkillStockSpriteDisplay[] baseSkillStocks = new CrosshairController.SkillStockSpriteDisplay[0];
        /// <summary>
        /// Skill stocks from the previous rebuild.
        /// </summary>
        private GameObject[] previousSkillStockObjects = new GameObject[0];
        private CrosshairController crossController;
        private SkillLocator bodySkillLocator; //Cache it!
        private int cacheMaxSkillLocatorStacks;

        private void Awake()
        {
            crossController = GetComponent<CrosshairController>();
            HG.ArrayUtils.CloneTo(crossController.skillStockSpriteDisplays, ref baseSkillStocks);
        }

        private void Start()
        {
            bodySkillLocator = crossController.hudElement.targetCharacterBody.GetComponent<SkillLocator>();
            RebuildDynamicDisplays();
        }

        private void Update()
        {
            if (crossController.hudElement.targetBodyObject && bodySkillLocator)
            {
                int newMaxSkillLocatorStacks = 0;
                for (int i = 0; i < dynamicSkillStackDisplays.Length; i++)
                {
                    GenericSkill genericSkill = bodySkillLocator.GetSkill(dynamicSkillStackDisplays[i].skillSlot);
                    if (genericSkill)
                    {
                        newMaxSkillLocatorStacks += genericSkill.maxStock;
                    }
                }
                if (newMaxSkillLocatorStacks > cacheMaxSkillLocatorStacks) //Do not rebuild unless theres new more, since the game already takes care of disabling the added dynamic ones
                {
                    cacheMaxSkillLocatorStacks = newMaxSkillLocatorStacks;
                    RebuildDynamicDisplays();
                }
            }
        }

        private void RebuildDynamicDisplays()
        {
            //NOT needed because crosshairs update in LATE update, while we do at UPDATE
            //crossController.skillStockSpriteDisplays = baseSkillStocks; //Reset to base before we remove the added ones.
            if (previousSkillStockObjects != null && previousSkillStockObjects.Length > 0)
            {
                foreach (var item in previousSkillStockObjects)
                {
                    UnityEngine.Object.Destroy(item); //Cleanse
                }
                previousSkillStockObjects = new GameObject[0];
            }
            List<CrosshairController.SkillStockSpriteDisplay> dynamicSkillStocks = new List<CrosshairController.SkillStockSpriteDisplay>(baseSkillStocks);
            foreach (DynamicSkillStackDisplay dssd in dynamicSkillStackDisplays)
            {
                GenericSkill genericSkill = bodySkillLocator.GetSkill(dssd.skillSlot);
                if (genericSkill)
                {
                    for (int i = 0; i < genericSkill.maxStock; i++)
                    {
                        GameObject go = UnityEngine.Object.Instantiate(dssd.prefab, dssd.container.transform);
                        HG.ArrayUtils.ArrayAppend(ref previousSkillStockObjects, go);
                        dynamicSkillStocks.Add(new CrosshairController.SkillStockSpriteDisplay
                        {
                            maximumStockCountToBeValid = 99, //Previously genericSkill.maxStock
                            minimumStockCountToBeValid = i + 1,
                            requiredSkillDef = dssd.requiredSkillDef ?? null,
                            skillSlot = dssd.skillSlot,
                            target = go
                        });
                    }
                }
            }
            crossController.skillStockSpriteDisplays = dynamicSkillStocks.ToArray(); //Final update.
        }

        [Serializable]
        public struct DynamicSkillStackDisplay
        {
            [Header("Object References")]
            [Tooltip("Object to instantiate.")]
            public GameObject prefab;

            [Tooltip("Container in which to instantiate the prefab.")]
            public GameObject container;

            [Header("Skill Parameters")]
            public SkillSlot skillSlot;

            public RoR2.Skills.SkillDef requiredSkillDef;
        }
    }
}
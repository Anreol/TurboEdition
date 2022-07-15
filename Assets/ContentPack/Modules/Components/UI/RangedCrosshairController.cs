using RoR2;
using RoR2.UI;
using System;
using UnityEngine;

namespace TurboEdition.Components.UI
{
    [RequireComponent(typeof(HudElement))]
    [RequireComponent(typeof(RectTransform))]
    public class RangedCrosshairController : MonoBehaviour
    {
        private HudElement hudElement;
        private SkillLocator bodySkillLocator; //Cache it!

        public SkillRangedSpriteDisplay[] skillRangeSpriteDisplays;

        private void Awake()
        {
            hudElement = base.GetComponent<HudElement>();
        }

        private void Start()
        {
            if (!hudElement.targetCharacterBody)
            {
                Destroy(this);
            }
            bodySkillLocator = hudElement.targetCharacterBody.GetComponent<SkillLocator>();
            SetSkillRangeDisplay();
        }

        private void LateUpdate()
        {
            SetSkillRangeDisplay();
        }

        private void SetSkillRangeDisplay()
        {
            if (this.hudElement.targetCharacterBody && bodySkillLocator)
            {
                foreach (SkillRangedSpriteDisplay skillRange in skillRangeSpriteDisplays)
                {
                    bool setToActive = false;
                    Color colorToSet = skillRange.defaultSpriteColor;
                    GenericSkill genericSkill = bodySkillLocator.GetSkill(skillRange.skillSlot);
                    if (skillRange.skillSlot == SkillSlot.None || (genericSkill && ((genericSkill.skillDef == skillRange.requiredSkillDef && genericSkill.CanExecute()) || skillRange.requiredSkillDef == null)))
                    {
                        RaycastHit raycastHit;
                        if (hudElement.targetCharacterBody.inputBank.GetAimRaycast(skillRange.maximumMetersToBeValid, out raycastHit))
                        {
                            if (raycastHit.distance >= skillRange.minimumMetersToBeValid)
                            {
                                setToActive = true;
                                colorToSet = skillRange.validSpriteColor;
                            }
                        }

                        //This is INSIDE THE IF to make sure the same element that is used in multiple cases has been found in this eval, else other evaluations down the line will set it to default.
                        if (skillRange.targetObjectToEnableOrDisable)
                        {
                            skillRange.targetObjectToEnableOrDisable.SetActive(setToActive);
                        }
                        if (skillRange.targetSpriteToRecolor)
                        {
                            skillRange.targetSpriteToRecolor.color = colorToSet;
                        }
                        if (skillRange.secondaryTargetSpriteToRecolor)
                        {
                            skillRange.secondaryTargetSpriteToRecolor.color = colorToSet;
                        }
                    }
                }
            }
        }

        [Serializable]
        public struct SkillRangedSpriteDisplay
        {
            [Header("Object References")]
            [Tooltip("Object to disable or enable when valid or not.")]
            public GameObject targetObjectToEnableOrDisable;

            [Tooltip("Sprite to recolor when valid.")]
            public UnityEngine.UI.Image targetSpriteToRecolor;

            public UnityEngine.UI.Image secondaryTargetSpriteToRecolor;

            [Header("Skill Parameters")]
            public SkillSlot skillSlot;

            public RoR2.Skills.SkillDef requiredSkillDef;

            [Header("Distance Parameters")]
            public int minimumMetersToBeValid;

            public int maximumMetersToBeValid;

            [Header("Image Color Parameters")]
            [Tooltip("Color of the Target's sprite by default.")]
            public Color defaultSpriteColor;

            [Tooltip("Color of the Target's sprite when valid.")]
            public Color validSpriteColor;
        }
    }
}
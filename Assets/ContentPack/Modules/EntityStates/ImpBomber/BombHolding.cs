using EntityStates;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace TurboEdition.EntityStates.ImpBomber.Weapon
{
    internal class BombHolding : BaseState
    {
        [SerializeField]
        public SkillDef replacementSkillDef;

        [HideInInspector]
        public GameObject boneBombInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            GenericSkill genericSkill = base.skillLocator ? base.skillLocator.secondary : null;
            if (genericSkill && isAuthority)
            {
                genericSkill.SetSkillOverride(this.outer, replacementSkillDef, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public override void OnExit()
        {
            GenericSkill genericSkill = base.skillLocator ? base.skillLocator.secondary : null;
            if (genericSkill && isAuthority)
            {
                genericSkill.UnsetSkillOverride(this.outer, replacementSkillDef, GenericSkill.SkillOverridePriority.Contextual);
            }
            if (boneBombInstance)
            {
                UnityEngine.Object.Destroy(boneBombInstance);
            }
            base.GetModelAnimator().SetBool("BombHolding.active", false);
            base.OnExit();
        }
    }
}
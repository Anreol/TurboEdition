using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;

namespace TurboEdition.Skills
{
    [CreateAssetMenu(menuName = "TurboEdition/SkillDef/GrenadierSpecialSkillDef")]
    internal class GrenadierSpecialSkillDef : SkillDef
    {
        public int maxExtraStocksFromReloading;

        [Tooltip("Description Token used whenever the user has its extra stocks full.")]
        public string holdingFullStocksDescriptionToken;

        public override SkillDef.BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new GrenadierSpecialSkillDef.InstanceData(skillSlot, maxExtraStocksFromReloading);
        }

        public override string GetCurrentDescriptionToken(GenericSkill skillSlot)
        {
            GrenadierSpecialSkillDef.InstanceData instanceData = (GrenadierSpecialSkillDef.InstanceData)skillSlot.skillInstanceData;
            if (instanceData.skillStocksExtra == instanceData.maxSkillStockExtra)
            {
                return holdingFullStocksDescriptionToken;
            }
            return base.GetCurrentDescriptionToken(skillSlot);
        }

        public override void OnUnassigned(GenericSkill skillSlot)
        {
            ((GrenadierSpecialSkillDef.InstanceData)skillSlot.skillInstanceData).Dispose();
            base.OnUnassigned(skillSlot);
        }

        public override void OnFixedUpdate(GenericSkill skillSlot)
        {
            base.OnFixedUpdate(skillSlot);
            GrenadierSpecialSkillDef.InstanceData instanceData = (GrenadierSpecialSkillDef.InstanceData)skillSlot.skillInstanceData;
            instanceData.FixedUpdate();
        }

        public override void OnExecute([NotNull] GenericSkill skillSlot)
        {
            skillSlot.stateMachine.SetInterruptState(this.InstantiateNextState(skillSlot), this.interruptPriority);
            if (this.cancelSprintingOnActivation)
            {
                skillSlot.characterBody.isSprinting = false;
            }
            skillSlot.stock -= this.stockToConsume;
            if (this.resetCooldownTimerOnUse)
            {
                skillSlot.rechargeStopwatch = 0f;
            }
            //Removed on skill executed from base
        }

        public override int GetMaxStock([NotNull] GenericSkill skillSlot)
        {
            GrenadierSpecialSkillDef.InstanceData instanceData = (GrenadierSpecialSkillDef.InstanceData)skillSlot.skillInstanceData;
            if (instanceData != null)
            {
                return this.baseMaxStock + instanceData.skillStocksExtra;
            }
            return this.baseMaxStock;
        }

        public class InstanceData : SkillDef.BaseSkillInstanceData, IDisposable
        {
            public GenericSkill primarySkillSlot; //Assigned by instance data itself
            public GenericSkill runningSkillSlot; //Assigned on ctor
            public bool hasFullyUnloadedPrimary; //Set whenever primary skill has stock equal zero
            public int maxSkillStockExtra; //Asigned on ctor
            public int skillStocksExtra; //Updated on stock deducted
            public int skillStocks; //Automatically updated
            public InstanceData(GenericSkill currentSkill, int maxStockFromReloading)
            {
                primarySkillSlot = currentSkill.characterBody.skillLocator.primary;
                runningSkillSlot = currentSkill;
                hasFullyUnloadedPrimary = false;
                maxSkillStockExtra = maxStockFromReloading;
                skillStocksExtra = 0;
                skillStocks = 0;
            }
            public void Dispose()
            {
                primarySkillSlot = null;
                runningSkillSlot = null;
                hasFullyUnloadedPrimary = false;
                maxSkillStockExtra = 0;
                skillStocksExtra = 0;
                skillStocks = 0;
            }

            public void FixedUpdate()
            {
                if (runningSkillSlot)
                {
                    if (runningSkillSlot.stock < skillStocks && runningSkillSlot.stock > 0) //Means it lost a stock since last updated
                    {
                        if (skillStocksExtra > 0)
                        {
                            skillStocksExtra--;
                            runningSkillSlot.RecalculateValues(); //Refresh to remove lost stock
                        }
                        if (runningSkillSlot.characterBody)
                            runningSkillSlot.characterBody.OnSkillActivated(runningSkillSlot);
                    }
                    skillStocks = runningSkillSlot.stock; //Update
                }
                if (primarySkillSlot.stock == 0)
                    hasFullyUnloadedPrimary = true;
                if (primarySkillSlot.stock == primarySkillSlot.maxStock && hasFullyUnloadedPrimary && primarySkillSlot.CanExecute())
                {
                    hasFullyUnloadedPrimary = false;
                    if (skillStocksExtra < maxSkillStockExtra)
                    {
                        skillStocksExtra++;
                        runningSkillSlot.RecalculateValues(); //Refresh to add new stock
                        if (runningSkillSlot.CanApplyAmmoPack()) //Refund new stock
                        {
                            runningSkillSlot.ApplyAmmoPack();
                            Util.PlaySound("Play_GrenadierSpecialGainExtraCharge", runningSkillSlot.gameObject);
                        }
                    }
                }
            }
        }
    }
}
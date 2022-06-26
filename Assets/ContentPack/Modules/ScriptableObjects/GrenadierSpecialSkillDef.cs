using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;

namespace TurboEdition.ScriptableObjects
{
    [CreateAssetMenu(menuName = "TurboEdition/SkillDef/GrenadierSpecialSkillDef")]
    internal class GrenadierSpecialSkillDef : SkillDef
    {
        [Header("Grenadier Parameters")]
        public int maxExtraStocksFromReloading;

        [Tooltip("Description Token used whenever the user has its extra stocks full.")]
        public string holdingFullStocksDescriptionToken;

        public override SkillDef.BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new GrenadierSpecialSkillDef.InstanceData(maxExtraStocksFromReloading);
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
            instanceData.FixedUpdate(skillSlot);
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
            /// <summary>
            /// Set whenever primary skill has stock equal zero
            /// </summary>
            public bool hasFullyUnloadedPrimary;

            /// <summary>
            /// Assigned on ctor, special data from this class type that defines how many extra stocks from reloading should be given.
            /// </summary>
            public int maxSkillStockExtra;

            /// <summary>
            /// Updated on stock deducted
            /// </summary>
            public int skillStocksExtra;

            /// <summary>
            /// Automatically updated, keeps the stocks of the running skill from the previous update cycle saved.
            /// </summary>
            public int skillStocks;

            public InstanceData(int maxStockFromReloading)
            {
                maxSkillStockExtra = maxStockFromReloading;
                skillStocksExtra = 0;
                skillStocks = 0;
            }

            public void Dispose()
            {
                hasFullyUnloadedPrimary = false;
                maxSkillStockExtra = 0;
                skillStocksExtra = 0;
                skillStocks = 0;
            }

            public void FixedUpdate(GenericSkill runningGS)
            {
                if (runningGS.stock < skillStocks /*&& runningGS.stock > 0*/) //Compare with values of last loop, if passes, it means it lost a stock or more since last updated
                {
                    if (skillStocks > runningGS.skillDef.baseMaxStock) //Lost additional stocks...?
                    {
                        skillStocksExtra -= Mathf.Max(skillStocks - runningGS.skillDef.baseMaxStock, 0); //Remove amount of additional stocks we have lost in this update
                        runningGS.RecalculateValues(); //Refresh to remove lost stock
                    }
       
                    if (runningGS.characterBody)
                        runningGS.characterBody.OnSkillActivated(runningGS);
                }
                skillStocks = runningGS.stock; //Update with values of this last loop.

                if (runningGS.characterBody.skillLocator.primary.stock == 0)
                    hasFullyUnloadedPrimary = true;

                if (runningGS.characterBody.skillLocator.primary.stock == runningGS.characterBody.skillLocator.primary.maxStock && hasFullyUnloadedPrimary /*&& runningGS.characterBody.skillLocator.primary.CanExecute()*/) //Don't care if we can execute or not. For now.
                {
                    hasFullyUnloadedPrimary = false;
                    if (skillStocksExtra < maxSkillStockExtra)
                    {
                        skillStocksExtra++;
                        runningGS.RecalculateValues(); //Refresh to add new stock
                        if (runningGS.CanApplyAmmoPack()) //Refund new stock
                        {
                            runningGS.ApplyAmmoPack();
                            Util.PlaySound("Play_Grenadier_Special_GainExtraCharge", runningGS.gameObject);
                        }
                    }
                }
            }
        }
    }
}
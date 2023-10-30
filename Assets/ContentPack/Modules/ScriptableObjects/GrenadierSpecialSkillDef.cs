﻿using JetBrains.Annotations;
using RoR2;
using RoR2.HudOverlay;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TurboEdition.ScriptableObjects
{
    [CreateAssetMenu(menuName = "TurboEdition/SkillDef/GrenadierSpecialSkillDef")]
    internal class GrenadierSpecialSkillDef : SkillDef
    {
        [Header("Grenadier Parameters")]
        public int maxExtraStocksFromReloading;

        [Tooltip("Description Token used whenever the user has its extra stocks full.")]
        public string holdingFullStocksDescriptionToken;

        [Tooltip("Wwise sound event string to play when a stock is gained.")]
        public string stockGainedSoundEffectString;

        public GameObject hudSkillOverlayPrefab;

        public override SkillDef.BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new GrenadierSpecialSkillDef.InstanceData(maxExtraStocksFromReloading);
        }

        public override string GetCurrentDescriptionToken(GenericSkill skillSlot)
        {
            GrenadierSpecialSkillDef.InstanceData instanceData = (GrenadierSpecialSkillDef.InstanceData)skillSlot.skillInstanceData;
            if (instanceData != null && instanceData.skillStocksExtra == instanceData.maxSkillStockExtra)
            {
                return holdingFullStocksDescriptionToken;
            }
            return base.GetCurrentDescriptionToken(skillSlot);
        }

        public override void OnUnassigned(GenericSkill skillSlot)
        {
            if (((GrenadierSpecialSkillDef.InstanceData)skillSlot.skillInstanceData) != null)
            {
                ((GrenadierSpecialSkillDef.InstanceData)skillSlot.skillInstanceData).Dispose();
            }
            base.OnUnassigned(skillSlot);
        }

        public override void OnFixedUpdate(GenericSkill skillSlot)
        {
            base.OnFixedUpdate(skillSlot);
            GrenadierSpecialSkillDef.InstanceData instanceData = (GrenadierSpecialSkillDef.InstanceData)skillSlot.skillInstanceData;
            if (instanceData != null)
            {
                if (instanceData.overlayController == null)
                {
                    //Get hud and shit
                    //TODO: Check if not checking if this is dirty or not breaks something.
                    HUD hud = HUD.instancesList.FirstOrDefault(x => x.targetBodyObject == skillSlot.characterBody.gameObject);
                    if (hud)
                    {
                        string entry = "";
                        if (skillSlot == skillSlot.characterBody.skillLocator.primary)
                            entry = "Skill1Root";
                        if (skillSlot == skillSlot.characterBody.skillLocator.secondary)
                            entry = "Skill2Root";
                        if (skillSlot == skillSlot.characterBody.skillLocator.utility)
                            entry = "Skill3Root";
                        if (skillSlot == skillSlot.characterBody.skillLocator.special)
                            entry = "Skill4Root";
                        if (entry.Length > 0)
                        {
                            OverlayCreationParams overlayCreationParams = new OverlayCreationParams
                            {
                                prefab = hudSkillOverlayPrefab,
                                childLocatorEntry = entry
                            };
                            instanceData.AssignOverlay(HudOverlayManager.AddOverlay(skillSlot.gameObject, overlayCreationParams));
                        }
                    }
                }
                instanceData.FixedUpdate(skillSlot, stockGainedSoundEffectString);
            }
        }

        public override void OnExecute([NotNull] GenericSkill skillSlot)
        {
            skillSlot.stateMachine.SetInterruptState(this.InstantiateNextState(skillSlot), this.interruptPriority);
            if (this.cancelSprintingOnActivation)
            {
                skillSlot.characterBody.isSprinting = false;
            }
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
            /// Updated on stock deducted, amount of extra stocks that the player currently has
            /// </summary>
            public int skillStocksExtra;

            /// <summary>
            /// Automatically updated, keeps the stocks of the running skill from the previous update cycle saved.
            /// </summary>
            public int previousSkillStocks;

            internal GameObject notification;
            internal GameObject flashPanel;
            internal GameObject highlightPanel;
            internal OverlayController overlayController;
            internal ImageFillController fillBarController;
            internal HGTextMeshProUGUI currentExtraStockText;

            public InstanceData(int maxStockFromReloading)
            {
                maxSkillStockExtra = maxStockFromReloading;
                skillStocksExtra = 0;
                previousSkillStocks = 0;
            }

            public void AssignOverlay(OverlayController newOverlay)
            {
                if (overlayController != null)
                {
                    overlayController.onInstanceAdded -= onOverlayAdded;
                    overlayController.onInstanceRemove -= onOverlayRemoved;
                    HudOverlayManager.RemoveOverlay(overlayController);
                }
                overlayController = newOverlay;
                overlayController.onInstanceAdded += onOverlayAdded;
                overlayController.onInstanceRemove += onOverlayRemoved;
            }

            private void onOverlayRemoved(OverlayController controller, GameObject instance)
            {
                notification = null;
                flashPanel = null;
                highlightPanel = null;
                fillBarController = null;
                currentExtraStockText = null;
            }

            private void onOverlayAdded(OverlayController controller, GameObject instance)
            {
                ChildLocator childLocator = instance.GetComponent<ChildLocator>();
                if (childLocator)
                {
                    notification = childLocator.FindChild("NotificationPanel").gameObject;
                    flashPanel = childLocator.FindChild("FlashPanel").gameObject;
                    highlightPanel = childLocator.FindChild("HighlightPanel").gameObject;
                    fillBarController = childLocator.FindChild("FillBar").GetComponent<ImageFillController>();
                    currentExtraStockText = childLocator.FindChild("CurrentExtraStockText").GetComponent<HGTextMeshProUGUI>();
                }
            }

            public void Dispose()
            {
                hasFullyUnloadedPrimary = false;
                maxSkillStockExtra = 0;
                skillStocksExtra = 0;
                previousSkillStocks = 0;
                if (overlayController != null)
                {
                    HudOverlayManager.RemoveOverlay(overlayController);
                    overlayController.onInstanceAdded -= onOverlayAdded;
                    overlayController.onInstanceRemove -= onOverlayRemoved;
                }
            }

            public void FixedUpdate(GenericSkill runningGS, string sfxToPlayOnExtraStockGained)
            {
                //if (runningGS.stock < previousSkillStocks /*&& runningGS.stock > 0*/) //Compare with values of last loop, if passes, it means it lost a stock or more since last updated
                //{
                //    if (previousSkillStocks > runningGS.skillDef.baseMaxStock) //Lost additional stocks...?
                //    {
                //        skillStocksExtra -= Mathf.Max(previousSkillStocks - runningGS.skillDef.baseMaxStock, 0); //Remove amount of additional stocks we have lost in this update
                //        runningGS.RecalculateValues(); //Refresh to remove lost stock
                //    }

                //    if (runningGS.characterBody)
                //        runningGS.characterBody.OnSkillActivated(runningGS);
                //}
                previousSkillStocks = runningGS.stock; //Update with values of this last loop.

                if (runningGS.characterBody.skillLocator.primary.stock == 0)
                    hasFullyUnloadedPrimary = true;

                //We check the primary stocks, we could technically change this to any skill but that'd require some rewriting
                if (runningGS.characterBody.skillLocator.primary.stock == runningGS.characterBody.skillLocator.primary.maxStock && hasFullyUnloadedPrimary /*&& runningGS.characterBody.skillLocator.primary.CanExecute()*/) //Don't care if we can execute or not. For now.
                {
                    hasFullyUnloadedPrimary = false;
                    if (skillStocksExtra < maxSkillStockExtra)
                    {
                        skillStocksExtra++;
                        runningGS.RecalculateValues(); //Refresh to add new stock
                        if (runningGS.CanApplyAmmoPack()) //Refund new stock
                        {
                            notification.SetActive(true);
                            highlightPanel.SetActive(true);
                            flashPanel.SetActive(true);
                            runningGS.ApplyAmmoPack();
                            Util.PlaySound(sfxToPlayOnExtraStockGained, runningGS.gameObject);
                        }
                    }
                }
                if (overlayController != null)
                {
                    UpdateUI();
                }
            }

            private void UpdateUI()
            {
                if (fillBarController)
                {
                    fillBarController.SetTValue(skillStocksExtra / (float)maxSkillStockExtra);
                    foreach (Image image in fillBarController.images)
                    {
                        image.color = skillStocksExtra == maxSkillStockExtra ? new Color(0.9987254f, 1, 0.4575472f) : Color.white;
                    }
                }
                if (currentExtraStockText)
                {
                    currentExtraStockText.SetText(Language.GetStringFormatted("GRENADIER_UI_SKILLDEF_FORMAT", skillStocksExtra));
                }
            }
        }
    }
}
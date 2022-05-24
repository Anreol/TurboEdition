using RoR2;
using RoR2.UI;
using System.Collections.ObjectModel;
using TurboEdition.Components;
using UnityEngine;
using UnityEngine.UI;

namespace TurboEdition.UI
{
    [RequireComponent(typeof(RectTransform))]
    internal class MoneyBankPanel : MonoBehaviour
    {
        internal MoneyBankInteractionController pickerController;
        internal uint displayMoneyAmount;
        internal uint displayTargetMoneyAmount;
        internal int[] contributeCosts;
        internal int[] withdrawCosts;
        internal bool dirtyUI;

        public string contributeToken;
        public string withdrawToken;

        public RectTransform buttonContributeContainer;
        public RectTransform buttonWithdrawContainer;
        public GameObject buttonContributeTemplate;
        public GameObject buttonWithdrawTemplate;
        public LanguageTextMeshController currentMoneyText;
        public LanguageTextMeshController targetMoneyText;
        private UIElementAllocator<MPButton> buttonContributeAllocator;
        private UIElementAllocator<MPButton> buttonWithdrawAllocator;

        private void Awake()
        {
            this.buttonContributeAllocator = new UIElementAllocator<MPButton>(this.buttonContributeContainer, this.buttonContributeTemplate, true, false);
            this.buttonContributeAllocator.onCreateElement = new UIElementAllocator<MPButton>.ElementOperationDelegate(this.OnCreatePositiveButton);
            this.buttonWithdrawAllocator = new UIElementAllocator<MPButton>(this.buttonWithdrawContainer, this.buttonWithdrawTemplate, true, false);
            this.buttonWithdrawAllocator.onCreateElement = new UIElementAllocator<MPButton>.ElementOperationDelegate(this.OnCreateNegativeButton);
        }
        private void Update()
        {
            if (dirtyUI)
            {
                currentMoneyText.formatArgs = new object[] { displayMoneyAmount };
                targetMoneyText.formatArgs = new object[] { displayTargetMoneyAmount };
            }
        }
        private void FixedUpdate()
        {
            if (buttonContributeAllocator != null)
            {
                for (int i = 0; i < buttonContributeAllocator.elements?.Count; i++)
                {
                    //Need this kind of update else it flickers
                    if (!buttonContributeAllocator.elements[i].interactable && contributeCosts[i] <= pickerController.networkUIPromptController.currentParticipantMaster.money && (pickerController.cachedSyncedMoneyAmount + contributeCosts[i]) <= TurboEdition.Items.MoneyBankManager.targetMoneyAmountToStore) 
                    {
                        buttonContributeAllocator.elements[i].interactable = true;
                    }
                    else if (buttonContributeAllocator.elements[i].interactable && contributeCosts[i] > pickerController.networkUIPromptController.currentParticipantMaster.money || (pickerController.cachedSyncedMoneyAmount + contributeCosts[i]) > TurboEdition.Items.MoneyBankManager.targetMoneyAmountToStore)
                    {
                        buttonContributeAllocator.elements[i].interactable = false;
                    }   
                }
            }
            if (buttonWithdrawAllocator != null)
            {
                for (int i = 0; i < buttonWithdrawAllocator.elements?.Count; i++)
                {
                    if (!buttonWithdrawAllocator.elements[i].interactable && withdrawCosts[i] <= pickerController.cachedSyncedMoneyAmount)
                    {
                        buttonWithdrawAllocator.elements[i].interactable = true;
                    }
                    else if (buttonWithdrawAllocator.elements[i].interactable && withdrawCosts[i] > pickerController.cachedSyncedMoneyAmount)
                    {
                        buttonWithdrawAllocator.elements[i].interactable = false;
                    }
                }
            }
        }
        private void OnCreatePositiveButton(int index, MPButton button)
        {
            LanguageTextMeshController lang = button.gameObject.GetComponent<LanguageTextMeshController>();
            if (lang)
            {
                lang.formatArgs = new object[] { contributeCosts[index] };
            }
            button.onClick.AddListener(delegate ()
            {
                this.pickerController.SubmitChoice(contributeCosts[index]);
            });
        }

        private void OnCreateNegativeButton(int index, MPButton button)
        {
            LanguageTextMeshController lang = button.gameObject.GetComponent<LanguageTextMeshController>();
            if (lang)
            {
                lang.formatArgs = new object[] { withdrawCosts[index] };
            }
            button.onClick.AddListener(delegate ()
            {
                this.pickerController.SubmitChoice(-withdrawCosts[index]);
            });
        }

        /// <summary>
        /// Setups the panel with buttons.
        /// </summary>
        /// <param name="positiveCount">Amount of buttons to have that adds to the bank.</param>
        /// <param name="negativeCount">Amount of buttons to have that removes from the bank.</param>
        public void SetButtons(int positiveCount, int negativeCount)
        {
            //Has to go first.
            contributeCosts = new int[positiveCount];
            withdrawCosts = new int[negativeCount];
            for (int i = 0; i < contributeCosts.Length; i++)
            {
                contributeCosts[i] = Run.instance.GetDifficultyScaledCost(Mathf.FloorToInt(Mathf.Pow(pickerController.baseContributeCost, i)));
            }
            for (int i = 0; i < withdrawCosts.Length; i++)
            {
                withdrawCosts[i] = Run.instance.GetDifficultyScaledCost(Mathf.FloorToInt(Mathf.Pow(pickerController.baseWithdrawCost, i)));
            }

            //Do things. Buttons now can access the previous arrays on creation.
            this.buttonContributeAllocator.AllocateElements(positiveCount);
            this.buttonWithdrawAllocator.AllocateElements(negativeCount);


            ///TODO: redo all this shit
            /*
            ReadOnlyCollection<MPButton> addElements = this.buttonContributeAllocator.elements;
            for (int j = 0; j < positiveCount; j++)
            {
                MPButton mpbutton = addElements[j];
                Navigation navigation = mpbutton.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                if (positiveCount > 1 && j > 0)
                {
                    MPButton selectOnTop = addElements[j - 1];
                    navigation.selectOnUp = selectOnTop;
                }
                if (j < positiveCount)
                {
                    MPButton selectOnBottom = addElements[j + 1];
                    navigation.selectOnDown = selectOnBottom;
                }
            }

            ReadOnlyCollection<MPButton> substractElements = this.buttonWithdrawAllocator.elements;
            for (int j = 0; j < negativeCount; j++)
            {
                MPButton mpbutton = substractElements[j];
                Navigation navigation = mpbutton.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                if (positiveCount > 0 && j == 0)
                {
                    MPButton lastPosButton = addElements[positiveCount];
                    Navigation lastNav = lastPosButton.navigation;
                    lastNav.selectOnDown = substractElements[j];

                    MPButton selectOnTop = addElements[positiveCount];
                    navigation.selectOnUp = selectOnTop;
                }
                if (negativeCount > 1 && j > 0)
                {
                    MPButton selectOnTop = substractElements[j - 1];
                    navigation.selectOnUp = selectOnTop;
                }
                if (j < negativeCount)
                {
                    MPButton selectOnBottom = substractElements[j + 1];
                    navigation.selectOnDown = selectOnBottom;
                }
            }*/
        }
    }
}
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurboEdition.Components;
using UnityEngine;
using UnityEngine.UI;

namespace TurboEdition.UI
{
	[RequireComponent(typeof(RectTransform))]
	class MoneyBankPanel : MonoBehaviour
	{
		internal MoneyBankInteractionController pickerController;
		internal uint moneyAmount;
		internal uint maxMoneyAmount;

		public GridLayoutGroup gridlayoutGroup;
		public RectTransform buttonContainer;
		public GameObject buttonAddPrefab;
		public GameObject buttonSubstractPrefab;
		private UIElementAllocator<MPButton> buttonAddAllocator;
		private UIElementAllocator<MPButton> buttonSubstractAllocator;
		private void Awake()
		{
			this.buttonAddAllocator = new UIElementAllocator<MPButton>(this.buttonContainer, this.buttonAddPrefab, true, false);
			this.buttonAddAllocator.onCreateElement = new UIElementAllocator<MPButton>.ElementOperationDelegate(this.OnCreatePositiveButton);
			this.buttonSubstractAllocator = new UIElementAllocator<MPButton>(this.buttonContainer, this.buttonSubstractPrefab, true, false);
			this.buttonSubstractAllocator.onCreateElement = new UIElementAllocator<MPButton>.ElementOperationDelegate(this.OnCreateNegativeButton);

			this.gridlayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		}
		private void OnCreatePositiveButton(int index, MPButton button)
		{
			button.onClick.AddListener(delegate ()
			{
				this.pickerController.SubmitChoice(Run.instance.GetDifficultyScaledCost(Mathf.FloorToInt(Mathf.Pow(5, index))));
			});
		}

		private void OnCreateNegativeButton(int index, MPButton button)
		{
			button.onClick.AddListener(delegate ()
			{
				this.pickerController.SubmitChoice(-(Run.instance.GetDifficultyScaledCost(Mathf.FloorToInt(Mathf.Pow(5, index)))));
			});
		}

		/// <summary>
		/// Setups the panel with buttons.
		/// </summary>
		/// <param name="positiveCount">Amount of buttons to have that adds to the bank.</param>
		/// <param name="negativeCount">Amount of buttons to have that removes from the bank.</param>
		public void SetButtons(int positiveCount, int negativeCount)
		{
			this.buttonAddAllocator.AllocateElements(positiveCount);
			this.buttonSubstractAllocator.AllocateElements(negativeCount);

			ReadOnlyCollection<MPButton> addElements = this.buttonAddAllocator.elements;
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

			ReadOnlyCollection<MPButton> substractElements = this.buttonSubstractAllocator.elements;
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
			}
		}
	}
}

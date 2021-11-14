using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TurboEdition.Misc
{
    class QuestObjectiveProvider : MonoBehaviour
    {
		private void OnEnable()
		{
			if (!InstanceTracker.Any<QuestObjectiveProvider>())
			{
				ObjectivePanelController.collectObjectiveSources += QuestObjectiveProvider.collectObjectiveSourcesDelegate;
			}
			InstanceTracker.Add<QuestObjectiveProvider>(this);
		}

		private void OnDisable()
		{
			InstanceTracker.Remove<QuestObjectiveProvider>(this);
			if (!InstanceTracker.Any<QuestObjectiveProvider>())
			{
				ObjectivePanelController.collectObjectiveSources -= QuestObjectiveProvider.collectObjectiveSourcesDelegate;
			}
		}
		private static void CollectObjectiveSources(CharacterMaster viewer, List<ObjectivePanelController.ObjectiveSourceDescriptor> dest)
		{
			foreach (QuestObjectiveProvider source in InstanceTracker.GetInstancesList<QuestObjectiveProvider>())
			{
				dest.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
				{
					master = viewer,
					objectiveType = typeof(QuestObjectiveProvider.QuestObjectiveTracker),
					source = source
				});
			}
		}
		public string objectiveToken;
		
        public bool markCompletedOnRetired = true;
        private Inventory rewardInventory;
        private int rewardAmount;
        private int numTilExpiration;
        private int numCurrentCount;
        private object numRequiredCount;
        private static readonly Action<CharacterMaster, List<ObjectivePanelController.ObjectiveSourceDescriptor>> collectObjectiveSourcesDelegate = new Action<CharacterMaster, List<ObjectivePanelController.ObjectiveSourceDescriptor>>(QuestObjectiveProvider.CollectObjectiveSources);
		private class QuestObjectiveTracker : ObjectivePanelController.ObjectiveTracker
		{
            private UnityEngine.GameObject gameObjectPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("QuestObjectiveStrip");
            private bool changed = false;

            public override void UpdateStrip()
            {
                base.UpdateStrip();
                if (!changed)
                {
                    FixStrip();
                    AssignInventory();
                    SwitchRewardPanels(this.inventoryDisplay.inventoryWasValid);
                }

                if (this.rewardMoney && !this.inventoryDisplay.inventoryWasValid)
                {
                    rewardMoney.text = GetInt().ToString();
                }
                if (this.inventoryDisplay.inventoryWasValid)
                {

                }
                if (this.expireCountdown && IsDirty())
                {
                    expireCountdown.text = GetCountdown();
                    if (this.expirePanelBG && this.numTilExpiration <= 0)
                    {
                        expirePanelBG.color = new Color(1f, 0.259f, 0.278f, 0.133f);
                    }
                }
            }
            public void FixStrip()
            {
                changed = true;

                Transform transform = stripObject.transform.parent;
                UnityEngine.Object.Destroy(this.stripObject);
                GameObject game = UnityEngine.Object.Instantiate<GameObject>(gameObjectPrefab, transform);
                game.SetActive(true);

                this.stripObject = game;
                ChildLocator childLocator = this.stripObject.GetComponent<ChildLocator>();
                //this.singlePanel = childLocator.FindChild("LabelPanelSingle").gameObject;
                //this.doublePanel = childLocator.FindChild("LabelPanelDouble").gameObject;
                //this.expireLabel = childLocator.FindChild("ExpirationPanel").transform.Find("ExpiresIn").GetComponent<TextMeshProUGUI>();
                this.expireCountdown = childLocator.FindChild("ExpirationPanel").transform.Find("StageNumber").GetComponent<TextMeshProUGUI>();
                this.expirePanelBG = childLocator.FindChild("ExpirationPanel").transform.Find("BackgroundPanel").GetComponent<RawImage>();
                this.rewardMoney = childLocator.FindChild("MoneyRoot").transform.Find("ValueText").GetComponent<TextMeshProUGUI>();
                this.inventoryDisplay = childLocator.FindChild("InventoryProvider").GetComponent<ItemInventoryDisplay>();
                this.label = childLocator.FindChild("LabelPanelSingle").transform.Find("Label").GetComponent<TextMeshProUGUI>();
                this.checkbox = childLocator.FindChild("Checkbox").GetComponent<Image>();

                //this.inventoryDisplay.itemIconPrefab = HUD.instancesList[0].itemInventoryDisplay.itemIconPrefab; //jej
            }
            private void SwitchRewardPanels(bool toInventory)
            {
                ChildLocator childLocator = this.stripObject.GetComponent<ChildLocator>();
                childLocator.FindChild("MoneyRoot").transform.gameObject.SetActive(!toInventory);
                childLocator.FindChild("InventoryProvider").transform.gameObject.SetActive(toInventory);
            }
            public void AssignInventory()
            {
                if (((QuestObjectiveProvider)this.sourceDescriptor.source).rewardInventory != null)
                {
                    this.inventoryDisplay.SetSubscribedInventory(((QuestObjectiveProvider)this.sourceDescriptor.source).rewardInventory);
                }
            }
            public int GetInt()
            {
                if (this.IsMoneyDirty())
                {
                    this.cachedInt = ((QuestObjectiveProvider)this.sourceDescriptor.source).rewardAmount;
                }
                return cachedInt;
            }
            public string GetCountdown()
            {
                if (this.IsDirty())
                {
                    this.numTilExpiration = ((QuestObjectiveProvider)this.sourceDescriptor.source).numTilExpiration;
                }
                if (this.numTilExpiration <= 0)
                {
                    return Language.GetString("QUEST_PANELUI_EXPIRESNOW");
                }
                return Language.GetStringFormatted("QUEST_PANELUI_EXPIRESTAGE", this.numTilExpiration);
            }
            public override string GenerateString()
            {
                QuestObjectiveProvider questObjectiveProvider = (QuestObjectiveProvider)this.sourceDescriptor.source;
                this.previousToken = questObjectiveProvider.objectiveToken;
                this.numCurrentCount = questObjectiveProvider.numCurrentCount;
                return string.Format(Language.GetString(questObjectiveProvider.objectiveToken), this.questTarget, this.numCurrentCount, questObjectiveProvider.numRequiredCount);
            }

            public override bool IsDirty()
            {
                return ((QuestObjectiveProvider)this.sourceDescriptor.source).numCurrentCount != this.numCurrentCount || ((QuestObjectiveProvider)this.sourceDescriptor.source).numTilExpiration != this.numTilExpiration;
            }
            private bool IsMoneyDirty()
            {
                return ((QuestObjectiveProvider)this.sourceDescriptor.source).rewardAmount != this.cachedInt;
            }
            private int numCurrentCount = -1;
            private int numTilExpiration = -1;
            private string questTarget = null;

            //protected TextMeshProUGUI timerLabel;
            //protected GameObject doublePanel;
            //protected GameObject singlePanel;

            //protected TextMeshProUGUI expireLabel;
            protected RawImage expirePanelBG;
            protected TextMeshProUGUI expireCountdown;
            protected TextMeshProUGUI rewardMoney;
            protected ItemInventoryDisplay inventoryDisplay;

            protected int cachedInt;
            private string previousToken;

            public override bool shouldConsiderComplete
			{
				get
				{
					return this.retired && ((GenericObjectiveProvider)this.sourceDescriptor.source).markCompletedOnRetired;
				}
			}

			
		}
	}
}

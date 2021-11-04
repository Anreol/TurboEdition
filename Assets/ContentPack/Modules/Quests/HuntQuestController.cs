using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using TMPro;
using TurboEdition.Components;
using TurboEdition.Quests;
using UnityEngine;
using UnityEngine.UI;

namespace TurboEdition.Quests
{
    internal class HuntQuestController : QuestComponent
    {
        private Type trackerType = typeof(HuntQuestObjectiveTracker);

        public override void OnEnable()
        {
            base.OnEnable();
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            ObjectivePanelController.collectObjectiveSources += ReportObjective;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            ObjectivePanelController.collectObjectiveSources -= ReportObjective;
            GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
        }
        public override void GenerateObjective()
        {

        }
        private void ReportObjective(CharacterMaster characterMaster, List<ObjectivePanelController.ObjectiveSourceDescriptor> output)
        {
            if (QuestCatalog.GetQuestDef(base.questIndexSpawner).hidden)
                return;
            if (base.teamIndex != TeamIndex.None && characterMaster.teamIndex == base.teamIndex)
            {
                output.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    master = characterMaster,
                    objectiveType = trackerType,
                    source = this
                });
            }
            else if (characterMaster.netId == base.masterNetIdOrigin)
            {
                output.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    master = characterMaster,
                    objectiveType = trackerType,
                    source = this
                });
            }
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport obj)
        {
            throw new NotImplementedException();
        }
    }
}

public class HuntQuestObjectiveTracker : ObjectivePanelController.ObjectiveTracker
{
    private UnityEngine.GameObject gameObjectPrefab = TurboEdition.Assets.mainAssetBundle.LoadAsset<GameObject>("QuestObjectiveStrip");
    private bool changed = false;

    public override void UpdateStrip()
    {
        base.UpdateStrip();
        if (!changed)
            FixStrip();

        if (this.rewardMoney)
        {
            //this.rewardImage.sprite =
        }
        if (this.inventoryDisplay)
        {

        }
        if (this.expireCountdown)
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
    }

    public string GetCountdown()
    {
        if (this.IsDirty())
        {
            HuntQuestController huntQuestController = (HuntQuestController)this.sourceDescriptor.source;
            this.numTilExpiration = huntQuestController.numTilExpiration;
        }
        if (this.numTilExpiration <= 0)
        {
            return Language.GetString("QUEST_PANELUI_EXPIRESNOW");
        }
        return Language.GetStringFormatted("QUEST_PANELUI_EXPIRESTAGE", this.numTilExpiration);
    }
    public override string GenerateString()
    {
        HuntQuestController huntQuestController = (HuntQuestController)this.sourceDescriptor.source;
        this.numCurrentCount = huntQuestController.numCurrentCount;
        return string.Format(Language.GetString(huntQuestController.objectiveToken), this.questTarget, this.numCurrentCount, huntQuestController.numRequiredCount);
    }

    public override bool IsDirty()
    {
        return ((HuntQuestController)this.sourceDescriptor.source).numCurrentCount != this.numCurrentCount || ((HuntQuestController)this.sourceDescriptor.source).numTilExpiration != this.numTilExpiration;
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
}
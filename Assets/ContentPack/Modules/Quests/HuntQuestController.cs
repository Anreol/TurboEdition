using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using TurboEdition.Components;
using TurboEdition.Quests;
using TurboEdition;
using UnityEngine;
using UnityEngine.UI;

namespace TurboEdition.Quests
{
    internal class HuntQuestController : QuestComponent
    {
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

        private void ReportObjective(CharacterMaster characterMaster, List<ObjectivePanelController.ObjectiveSourceDescriptor> output)
        {
            if (base.teamIndex != TeamIndex.None && characterMaster.teamIndex == base.teamIndex)
            {
                output.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    master = characterMaster,
                    objectiveType = typeof(HuntQuestObjectiveTracker),
                    source = this
                });
            }
            else if (characterMaster.netId == base.masterNetIdOrigin)
            {
                output.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    master = characterMaster,
                    objectiveType = typeof(HuntQuestObjectiveTracker),
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

        if (this.rewardImage)
        {
            //this.rewardImage.sprite =
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
        this.rewardImage = this.stripObject.transform.Find("RewardSprite").GetComponent<Image>();
    }
    public override string GenerateString()
    {
        HuntQuestController huntQuestController = (HuntQuestController)this.sourceDescriptor.source;
        this.numCurrentCount = huntQuestController.numCurrentCount;
        return string.Format(Language.GetString(huntQuestController.objectiveToken), this.numCurrentCount, huntQuestController.numRequiredCount);
    }

    public override bool IsDirty()
    {
        return ((HuntQuestController)this.sourceDescriptor.source).numCurrentCount != this.numCurrentCount;
    }

    private int numCurrentCount = -1;

    protected Image rewardImage;
    //protected HGTextMeshProUGUI labelLose;
}
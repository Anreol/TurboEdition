using RoR2;
using RoR2.UI;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace TurboEdition.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(HudElement))]
    internal class StatBarSimpleController : MonoBehaviour
    {
        [Tooltip("Panel where Stat bars will get instantiated.")]
        public RectTransform viewport;

        [Tooltip("Prefab that all bars will use")]
        public GameObject barPrefab;

        [Tooltip("HudElement that will get assigned the HUD on enable.")]
        public HudElement hudElement;

        private IStatBarProvider[] statBarProviders = new IStatBarProvider[] { };
        private UIElementAllocator<RectTransform> elementAllocator;
        private CharacterBody _characterBody;

        private void onInventoryChanged()
        {
            Build();
            UpdateBars();
        }

        private void FixedUpdate()
        {
            if (!this.hudElement.hud && GetComponentInParent<HUD>().targetMaster) //Initial setup, let's constantly check til the hud has a target
            {
                this.hudElement.hud = GetComponentInParent<HUD>();
                SetSubscribedBody(this.hudElement.targetCharacterBody);
            }
            if (this.hudElement.hud)
            {
                if (this.hudElement.targetCharacterBody != GetComponentInParent<HUD>().targetMaster.GetBody())
                {
                    this.hudElement.targetCharacterBody = GetComponentInParent<HUD>().targetMaster.GetBody();
                    SetSubscribedBody(this.hudElement.targetCharacterBody);
                }
                bool active = hudElement.hud.localUserViewer != null && hudElement.hud.localUserViewer.inputPlayer != null && hudElement.hud.localUserViewer.inputPlayer.GetButton("info");
                this.viewport.gameObject.SetActive(active);
                UpdateBars();
            }
        }

        private void OnDisable()
        {
            if (hudElement.targetCharacterBody)
            {
                hudElement.targetCharacterBody.onInventoryChanged -= onInventoryChanged;
            }
        }
        private void SetSubscribedBody(CharacterBody newCharacter)
        {
            if (newCharacter == this._characterBody) return;
            if (this._characterBody)
                this._characterBody.onInventoryChanged -= onInventoryChanged;
            this._characterBody = newCharacter;
            this._characterBody.onInventoryChanged += onInventoryChanged;
            Build();
        }

        private void Build()
        {
            if (this.elementAllocator == null)
                this.elementAllocator = new UIElementAllocator<RectTransform>(viewport, barPrefab, true, false);
            if (!hudElement.targetCharacterBody)
                return;
            statBarProviders = hudElement.targetCharacterBody.gameObject.GetComponents<IStatBarProvider>();
            this.elementAllocator.AllocateElements(statBarProviders.Length);
            ReadOnlyCollection<RectTransform> elements = this.elementAllocator.elements;
            for (int i = 0; i < elements.Count; i++)
            {
                RectTransform fillRectTransform = elements.ElementAt(i).GetComponent<ChildLocator>().FindChild("fillRectTransform").GetComponent<RectTransform>();
                RawImage sprite = elements.ElementAt(i).GetComponent<ChildLocator>().FindChild("spriteIcon").GetComponent<RawImage>();
                TooltipProvider tooltipProvider = elements.ElementAt(i).gameObject.transform.GetComponentInChildren<TooltipProvider>();
                StatBarData statBarData = statBarProviders[i].GetStatBarData();

                if (tooltipProvider)
                    tooltipProvider.SetContent(statBarData.tooltipContent);
                if (fillRectTransform)
                    fillRectTransform.GetComponent<RawImage>().color = statBarProviders[i].GetStatBarData().fillBarColor;

                if (sprite)
                    sprite.texture = statBarProviders[i].GetStatBarData().sprite.texture;
            }
        }

        private void UpdateBars()
        {
            if (elementAllocator == null)
                return;
            ReadOnlyCollection<RectTransform> elements = this.elementAllocator.elements;
            for (int i = 0; i < elements.Count; i++)
            {
                RectTransform fillRectTransform = elements.ElementAt(i).GetComponent<ChildLocator>().FindChild("fillRectTransform").GetComponent<RectTransform>();
                TextMeshProUGUI text = elements.ElementAt(i).GetComponent<ChildLocator>().FindChild("textValue").GetComponent<TextMeshProUGUI>();
                TooltipProvider tooltipProvider = elements.ElementAt(i).gameObject.transform.GetComponentInChildren<TooltipProvider>();

                float maxValue = statBarProviders[i].GetStatBarData().maxData;
                float currentValue = statBarProviders[i].GetStatBarData().currentData;
                float representedValue = currentValue/maxValue;

                if (fillRectTransform)
                {
                    fillRectTransform.anchorMin = new Vector2(0f, 0f);
                    fillRectTransform.anchorMax = new Vector2(representedValue, 1f);
                    fillRectTransform.sizeDelta = new Vector2(1f, 1f);
                }
                if (text)
                    text.text = currentValue.ToString() + "/" + maxValue.ToString();

                if (tooltipProvider.overrideBodyText != statBarProviders[i].GetStatBarData().tooltipContent.overrideBodyText)
                    tooltipProvider.SetContent(statBarProviders[i].GetStatBarData().tooltipContent);

            }
        }
    }
}
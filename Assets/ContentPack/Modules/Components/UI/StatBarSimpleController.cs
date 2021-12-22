using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TurboEdition.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(HudElement))]
    class StatBarSimpleController : MonoBehaviour
    {
        [Tooltip("Panel where Stat bars will get instantiated.")]
        public RectTransform viewport;
        [Tooltip("Prefab that all bars will use")]
        public GameObject barPrefab;
        [Tooltip("HudElement that will get assigned the HUD on enable.")]
        public HudElement hudElement;

        private IStatBarProvider[] statBarProviders = new IStatBarProvider[] { };
        private UIElementAllocator<Canvas> elementAllocator;
        private void OnEnable()
        {
            this.hudElement.hud = base.GetComponentInParent<HUD>();

            hudElement.targetCharacterBody.onInventoryChanged += onInventoryChanged;

            Build();
            UpdateBars();
        }
        private void onInventoryChanged()
        {
            Build();
            UpdateBars();
        }

        private void FixedUpdate()
        {
            bool active = hudElement.hud.localUserViewer != null && hudElement.hud.localUserViewer.inputPlayer != null && hudElement.hud.localUserViewer.inputPlayer.GetButton("info");
            this.viewport.gameObject.SetActive(active);
            UpdateBars();
        }
        private void OnDisable()
        {
            hudElement.targetCharacterBody.onInventoryChanged -= onInventoryChanged;
        }
        private void Build()
        {
            this.elementAllocator = new UIElementAllocator<Canvas>(viewport, barPrefab, true, false);
            statBarProviders = hudElement.targetCharacterBody.gameObject.GetComponents<IStatBarProvider>();
            this.elementAllocator.AllocateElements(statBarProviders.Length);
            ReadOnlyCollection<Canvas> elements = this.elementAllocator.elements;
            for (int i = 0; i < elements.Count; i++)
            {
                RectTransform fillRectTransform = elements.ElementAt(i).GetComponent<ChildLocator>().FindChild("fillRectTransform").GetComponent<RectTransform>();
                RawImage sprite = elements.ElementAt(i).GetComponent<ChildLocator>().FindChild("spriteIcon").GetComponent<RawImage>();

                if (fillRectTransform)
                {
                    fillRectTransform.GetComponent<RawImage>().color = statBarProviders[i].GetColor();
                }
                if (sprite)
                    sprite.texture = statBarProviders[i].GetSprite().texture;
            }
        }

        private void UpdateBars()
        {
            if (elementAllocator == null)
                return;
            ReadOnlyCollection<Canvas> elements = this.elementAllocator.elements;
            for (int i = 0; i < elements.Count; i++)
            {
                RectTransform fillRectTransform = elements.ElementAt(i).GetComponent<ChildLocator>().FindChild("fillRectTransform").GetComponent<RectTransform>();
                TextMeshProUGUI text = elements.ElementAt(i).GetComponent<ChildLocator>().FindChild("textValue").GetComponent<TextMeshProUGUI>();

                float maxValue = statBarProviders[i].GetDataMax();
                float currentValue = statBarProviders[i].GetDataCurrent();
                float representedValue = Mathf.Lerp(0, maxValue, currentValue);

                if (fillRectTransform)
                {
                    fillRectTransform.anchorMin = new Vector2(0f, 0f);
                    fillRectTransform.anchorMax = new Vector2(representedValue, 1f);
                    fillRectTransform.sizeDelta = new Vector2(1f, 1f);
                }
                if (text)
                    text.text = currentValue.ToString() + "/" + maxValue.ToString();
            } 
        }
    }
}

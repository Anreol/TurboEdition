namespace TurboEdition.UI
{
    /*
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(HudElement))]
    internal class StatBarController : MonoBehaviour
    {
        public IStatBarProvider[] statBarProviders = new IStatBarProvider[] { };

        private RectTransform rectTransform;
        private HudElement hudElement;

        private Dictionary<HealthBarStyle, UIElementAllocator<Image>> styleAndUI = new Dictionary<HealthBarStyle, UIElementAllocator<Image>>();
        private HealthBar.BarInfoCollection[] barInfos;

        [Tooltip("The container rect for the actual bars.")]
        public RectTransform barContainer;

        private bool scaleHealthbarWidth;
        private CharacterBody.ItemBehavior _source;
        private float valueFractionVelocity;
        private object cachedFractionalValue;

        private void Awake()
        {
            this.rectTransform = (RectTransform)base.transform;
            this.hudElement = base.GetComponent<HudElement>();
        }

        private void Start()
        {
            this.UpdateHealthbar(0f);
        }

        public void Update()
        {
            this.UpdateHealthbar(Time.deltaTime);
        }

        public void OnDisable()
        {
        }

        private void GetBars()
        {
            statBarProviders = hudElement.targetCharacterBody.GetComponents<IStatBarProvider>();
            int length = barInfos.Length;
            HG.ArrayUtils.Clear(barInfos, ref length);
            foreach (IStatBarProvider item in statBarProviders)
            {
                HealthBarStyle style = item.GetStyle();
                if (!styleAndUI.ContainsKey(style))
                {
                    styleAndUI.Add(item.GetStyle(), new UIElementAllocator<Image>(this.barContainer, style.barPrefab, true, false));
                }
            }
            barInfos = new HealthBar.BarInfoCollection[styleAndUI.Count];
        }

        private void UpdateHealthbar(float deltaTime)
        {
            float num = 1f;
            if (this.source)
            {
                if (this.scaleHealthbarWidth && hudElement.targetCharacterBody)
                {
                    float x = Util.Remap(Mathf.Clamp((body.baseMaxHealth + body.baseMaxShield) * num, 0f, this.maxHealthbarHealth), this.minHealthbarHealth, this.maxHealthbarHealth, this.minHealthbarWidth, this.maxHealthbarWidth);
                    this.rectTransform.sizeDelta = new Vector2(x, this.rectTransform.sizeDelta.y);
                }
                if (this.currentHealthText)
                {
                    float num2 = Mathf.Ceil(this.source.combinedHealth);
                    if (num2 != this.displayStringCurrentHealth)
                    {
                        this.displayStringCurrentHealth = num2;
                        this.currentHealthText.text = num2.ToString();
                    }
                }
                if (this.fullHealthText)
                {
                    float num3 = Mathf.Ceil(fullHealth);
                    if (num3 != this.displayStringFullHealth)
                    {
                        this.displayStringFullHealth = num3;
                        this.fullHealthText.text = num3.ToString();
                    }
                }
                if (this.deadImage)
                {
                    this.deadImage.enabled = !this.source.alive;
                }
            }
            this.UpdateBarInfos();
            this.ApplyBars();
        }

        private void UpdateBarInfos()
        {
            if (!this.hudElement)
            {
                return;
            }
            foreach (IStatBarProvider item in statBarProviders)
            {
                HealthComponent.HealthBarValues healthBarValues = item.GetData();
                float fullCombinedHealth = healthBarValues.maxHealthDisplayValue;
                float num = 1f / fullCombinedHealth;
                this.cachedFractionalValue = Mathf.SmoothDamp(this.cachedFractionalValue, healthBarValues.healthFraction, ref this.healthFractionVelocity, 0.4f, float.PositiveInfinity, Time.deltaTime);
                ref HealthBar.BarInfo ptr = ref this.barInfoCollection.trailingBarInfo;
                ptr.normalizedXMin = 0f;
                ptr.normalizedXMax = this.cachedFractionalValue;
                ptr.enabled = !ptr.normalizedXMax.Equals(ptr.normalizedXMin);
                HealthBar.< UpdateBarInfos > g__ApplyStyle | 33_1(ref ptr, ref this.style.trailingBarStyle);
                ref HealthBar.BarInfo ptr2 = ref this.barInfoCollection.healthBarInfo;
                ptr2.enabled = (healthBarValues.healthFraction > 0f);
                HealthBar.< UpdateBarInfos > g__ApplyStyle | 33_1(ref ptr2, ref this.style.healthBarStyle);
                if (this.healthCritical && this.style.flashOnHealthCritical)
                {
                    ptr2.color = HealthBar.GetCriticallyHurtColor();
                }
                HealthBar.< UpdateBarInfos > g__AddBar | 33_0(ref ptr2, healthBarValues.healthFraction, ref CS$<> 8__locals1);
                this.barInfoCollection.shieldBarInfo.enabled = (healthBarValues.shieldFraction > 0f);
                HealthBar.< UpdateBarInfos > g__ApplyStyle | 33_1(ref this.barInfoCollection.shieldBarInfo, ref this.style.shieldBarStyle);
                HealthBar.< UpdateBarInfos > g__AddBar | 33_0(ref this.barInfoCollection.shieldBarInfo, healthBarValues.shieldFraction, ref CS$<> 8__locals1);
                this.barInfoCollection.curseBarInfo.enabled = (healthBarValues.curseFraction > 0f);
                HealthBar.< UpdateBarInfos > g__ApplyStyle | 33_1(ref this.barInfoCollection.curseBarInfo, ref this.style.curseBarStyle);
                this.barInfoCollection.curseBarInfo.normalizedXMin = 1f - healthBarValues.curseFraction;
                this.barInfoCollection.curseBarInfo.normalizedXMax = 1f;
                this.barInfoCollection.barrierBarInfo.enabled = (this.source.barrier > 0f);
                HealthBar.< UpdateBarInfos > g__ApplyStyle | 33_1(ref this.barInfoCollection.barrierBarInfo, ref this.style.barrierBarStyle);
                this.barInfoCollection.barrierBarInfo.normalizedXMin = 0f;
                this.barInfoCollection.barrierBarInfo.normalizedXMax = healthBarValues.barrierFraction;
                this.barInfoCollection.magneticBarInfo.enabled = (this.source.magnetiCharge > 0f);
                this.barInfoCollection.magneticBarInfo.normalizedXMin = 0f;
                this.barInfoCollection.magneticBarInfo.normalizedXMax = healthBarValues.magneticFraction;
                this.barInfoCollection.magneticBarInfo.color = new Color(75f, 0f, 130f);
                HealthBar.< UpdateBarInfos > g__ApplyStyle | 33_1(ref this.barInfoCollection.magneticBarInfo, ref this.style.magneticStyle);
                ref HealthBar.BarInfo ptr3 = ref this.barInfoCollection.flashBarInfo;
                ptr3.normalizedXMin = this.barInfoCollection.healthBarInfo.normalizedXMin;
                ptr3.normalizedXMax = this.barInfoCollection.healthBarInfo.normalizedXMax;
                float num2 = (ptr3.normalizedXMin + ptr3.normalizedXMax) * 0.5f;
                float num3 = ptr3.normalizedXMax - num2;
                float num4 = 1f - this.source.health / this.source.fullHealth;
                float num5 = 2f * num4;
                this.theta += Time.deltaTime * num5;
                if (this.theta > 1f)
                {
                    this.theta -= this.theta - this.theta % 1f;
                }
                float num6 = 1f - Mathf.Cos(this.theta * 3.1415927f * 0.5f);
                num3 += num6 * 20f * num4;
                ptr3.normalizedXMin = num2 - num3;
                ptr3.normalizedXMax = num2 + num3;
                HealthBar.< UpdateBarInfos > g__ApplyStyle | 33_1(ref ptr3, ref this.style.flashBarStyle);
                Color color = ptr3.color;
                color.a = (1f - num6) * num4 * 0.7f;
                ptr3.color = color;
                float num7 = healthBarValues.cullFraction;
                if (healthBarValues.isElite && this.viewerBody)
                {
                    num7 = Mathf.Max(num7, this.viewerBody.executeEliteHealthFraction);
                }
                this.barInfoCollection.cullBarInfo.enabled = (num7 > 0f);
                this.barInfoCollection.cullBarInfo.normalizedXMin = 0f;
                this.barInfoCollection.cullBarInfo.normalizedXMax = num7;
                HealthBar.< UpdateBarInfos > g__ApplyStyle | 33_1(ref this.barInfoCollection.cullBarInfo, ref this.style.cullBarStyle);
                float ospFraction = healthBarValues.ospFraction;
                this.barInfoCollection.ospBarInfo.enabled = (ospFraction > 0f);
                this.barInfoCollection.ospBarInfo.normalizedXMin = 0f;
                this.barInfoCollection.ospBarInfo.normalizedXMax = ospFraction;
                HealthBar.< UpdateBarInfos > g__ApplyStyle | 33_1(ref this.barInfoCollection.ospBarInfo, ref this.style.ospStyle);
            }
        }
    }*/
}
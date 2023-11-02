using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TurboEdition.Components.UI
{
    public class PickupPickerPanelReroller : MonoBehaviour
    {
        [Header("Button Instancing")]
        [Tooltip("Whenever should all this stuff below should be used. If true, it will not use mpButton")]
        public bool doInstancing = false;

        public GameObject mpButtonPrefab;
        public Transform container;
        public string buttonTextToken;
        public Sprite buttonSprite;

        [Header("Built-In Button")]
        [Tooltip("The MPButton to add a listener to, to enable and disable whenever reroll times are exceeded. Ignored if doInstancing is true.")]
        [SerializeField] private MPButton mpButton;

        internal PickupPickerRerollerController pickupPickerRerollerController;
        private LanguageTextMeshController languageTextMeshController;
        private MPButton mpButtonInstance;

        private void OnEnable()
        {
            if (doInstancing)
            {
                if (mpButtonPrefab == null || container == null)
                {
                    TELog.LogE("PickupPickerPanelReroller instance has no mpButtonPrefab or container, disabling self.", true);
                    enabled = false;
                }
                if (mpButtonPrefab && container)
                {
                    mpButtonInstance = UnityEngine.Object.Instantiate<GameObject>(mpButtonPrefab, container).GetComponent<MPButton>();
                    if (mpButtonInstance)
                    {
                        languageTextMeshController = mpButtonInstance.gameObject.GetComponent<LanguageTextMeshController>();
                        if (buttonTextToken.Length > 0 && languageTextMeshController)
                        {
                            languageTextMeshController.token = buttonTextToken;
                        }
                        ChildLocator childLocator = mpButtonInstance.gameObject.GetComponent<ChildLocator>();
                        if (childLocator)
                        {
                            Image component = childLocator.FindChild("Icon").GetComponent<Image>();
                            if (buttonSprite && component)
                            {
                                component.sprite = buttonSprite;
                            }
                        }
                        mpButtonInstance.onClick.AddListener(delegate ()
                        {
                            mpButtonInstance.interactable = pickupPickerRerollerController.timesRerolled < pickupPickerRerollerController.rerollMaxTimes;
                        });
                    }
                }
                return;
            }
            //We aren't instancing, so just use the one that should be part of the prefab.
            if (mpButton == null)
            {
                TELog.LogE("PickupPickerPanelReroller instance has no mpButton while doInstancing is false, disabling self.", true);
                enabled = false;
            }
            mpButton.onClick.AddListener(delegate ()
            {
                mpButton.interactable = pickupPickerRerollerController.timesRerolled < pickupPickerRerollerController.rerollMaxTimes;
            });
        }

        private void OnDisable()
        {
            if (doInstancing)
            {
                UnityEngine.GameObject.Destroy(mpButtonInstance?.gameObject);
            }
        }

        public void SubmitGenerateNewOptions()
        {
            if (pickupPickerRerollerController == null)
            {
                TELog.LogE("Tried to call SubmitGenerateNewOptions in PickupPickerPanelReroller while the pickupPickerRerollerController reference is null.", true);
                return;
            }
            if (pickupPickerRerollerController)
            {
                pickupPickerRerollerController.GenerateNewOptions();
            }
        }

        public void SubmitCurrentOptions()
        {
            if (pickupPickerRerollerController == null)
            {
                TELog.LogE("Tried to call SubmitCurrentOptions in PickupPickerPanelReroller while the pickupPickerRerollerController reference is null.", true);
                return;
            }
            if (pickupPickerRerollerController)
            {
                pickupPickerRerollerController.SetWithCurrentServerOptions();
            }
        }
    }
}
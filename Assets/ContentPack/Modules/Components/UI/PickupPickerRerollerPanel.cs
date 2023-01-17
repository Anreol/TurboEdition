using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TurboEdition.Components.UI
{
    public class PickupPickerRerollerPanel : MonoBehaviour
    {
        public GameObject mpButtonPrefab;
        public Transform container;
        public string buttonTextToken;
        public Sprite buttonSprite;
        internal PickupPickerRerollerController pickupPickerRerollerController;
        private LanguageTextMeshController languageTextMeshController;
        private MPButton mpButtonInstance;

        private void OnEnable()
        {
            if (mpButtonPrefab == null || container == null)
            {
                TELog.LogE("PickupPickerRerollerController instance has no mpButtonPrefab or container, disabling self.", true);
                this.enabled = false;
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
        }

        private void OnDisable()
        {
            UnityEngine.GameObject.Destroy(mpButtonInstance?.gameObject);
        }

        public void SubmitGenerateNewOptions()
        {
            if (pickupPickerRerollerController)
            {
                pickupPickerRerollerController.GenerateNewOptions();
            }
        }

        public void SubmitCurrentOptions()
        {
            if (pickupPickerRerollerController)
            {
                pickupPickerRerollerController.SetWithCurrentServerOptions();
            }
        }
    }
}
using RoR2.HudOverlay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    internal class StaffMagePassiveController : NetworkBehaviour
    {


        [SyncVar]
        public float energy;

        public GameObject overlayPrefab;
        string overlayChildLocatorEntry;
        private OverlayController overlayController;

        private void OnEnable()
        {
            OverlayCreationParams overlayCreationParams = new OverlayCreationParams
            {
                prefab = overlayPrefab,
                childLocatorEntry = overlayChildLocatorEntry
            };
            overlayController = HudOverlayManager.AddOverlay(gameObject, overlayCreationParams);
            overlayController.onInstanceAdded += OnOverlayInstanceAdded;
            overlayController.onInstanceRemove += OnOverlayInstanceRemoved;
        }

        private void OnDisable()
        {
            if (overlayController != null)
            {
                overlayController.onInstanceAdded -= OnOverlayInstanceAdded;
                overlayController.onInstanceRemove -= OnOverlayInstanceRemoved;
                HudOverlayManager.RemoveOverlay(overlayController);
            }
        }
        private void OnOverlayInstanceAdded(OverlayController controller, GameObject @object)
        {
            throw new NotImplementedException();
        }

        private void OnOverlayInstanceRemoved(OverlayController controller, GameObject @object)
        {
            throw new NotImplementedException();
        }

    }
}

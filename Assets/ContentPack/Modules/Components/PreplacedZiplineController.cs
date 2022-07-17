using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    internal class PreplacedZiplineController : ZiplineController
    {
        public Transform pointASpawnTransform;
        public Transform pointBSpawnTransform;

        private void Start()
        {
            if (NetworkServer.active)
            {
                if (pointASpawnTransform)
                {
                    base.SetPointAPosition(pointASpawnTransform.position);
                }
                if (pointBSpawnTransform)
                {
                    base.SetPointBPosition(pointBSpawnTransform.position);
                }
            }
        }
    }
}
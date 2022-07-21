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
            //if (NetworkServer.active) //Late clients got desynced... so commenting this out for now, hopefully no issues?
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
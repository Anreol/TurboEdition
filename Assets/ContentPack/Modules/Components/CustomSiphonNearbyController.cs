using RoR2;
using UnityEngine;

namespace TurboEdition.Components
{
    public class CustomSiphonNearbyController : MonoBehaviour
    {
        SphereSearch _sphereSearch;
        public void OnEnable()
        {
            _sphereSearch = new SphereSearch();
        }

        public void OnDisable()
        {
            _sphereSearch = null;
        }
    }
}
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TurboEdition.Components
{
    [ExecuteAlways]
    public class InjectRoR2Camera : MonoBehaviour
    {
        private void Awake()
        {
            var cam = Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Core/Main Camera.prefab").WaitForCompletion(), gameObject.transform);
        }
    }
}
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TurboEdition.Components
{
    [ExecuteAlways]
    public class InjectRoR2Camera : MonoBehaviour
    {
#if UNITY_EDITOR

        private void OnEnable()
        {
            CleanUpChildren();

            var cam = Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Core/Main Camera.prefab").WaitForCompletion(), gameObject.transform);
            cam.gameObject.transform.localPosition = Vector3.zero;
            cam.gameObject.transform.localRotation = Quaternion.identity;
        }

        private void OnDisable()
        {
            CleanUpChildren();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CleanUpChildren()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (Application.isPlaying)
                    Destroy(transform.GetChild(i).gameObject);
                else
                {
                    //This is to be used in the editor only!
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }

#endif
    }
}
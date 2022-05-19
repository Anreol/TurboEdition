using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TurboEdition.Components
{
    [ExecuteAlways]
    internal class AddressableFieldLoader : MonoBehaviour
    {
        public Component component;
        public string fieldName;
        public string assetAddress;

        private static readonly MethodInfo LoadAssetAsyncInfo = typeof(Addressables).GetMethod(nameof(Addressables.LoadAssetAsync), new[] { typeof(string) });

        private void LoadAsset(bool dontSave = false)
        {
            var typ = component.GetType();
            var field = typ.GetField(fieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
            PropertyInfo property = null;
            if (field == null)
            {
                property = typ.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
                if (property == null) return;
            }
            var meth = LoadAssetAsyncInfo.MakeGenericMethod(field?.FieldType ?? property.PropertyType);
            var awaiter = meth.Invoke(null, new object[] { assetAddress });
            var wait = awaiter.GetType().GetMethod("WaitForCompletion", BindingFlags.Instance | BindingFlags.Public);
            var asset = wait.Invoke(awaiter, null);
            var assetObject = (UnityEngine.Object)asset;
            if (assetObject != null)
            {
                if (dontSave)
                {
                    assetObject.hideFlags |= HideFlags.DontSave;
                }
                field?.SetValue(component, asset);
                property?.SetValue(component, asset);
            }
        }

        private IEnumerator WaitAndLoadAsset()
        {
            yield return new WaitUntil(() => Addressables.InternalIdTransformFunc != null);
            LoadAsset(true);
        }

        private void Start()
        {
            LoadAsset();
        }

        private void OnValidate()
        {
            if (gameObject.activeInHierarchy) StartCoroutine(WaitAndLoadAsset());
        }
    }
}
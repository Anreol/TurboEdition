using RoR2;
using UnityEngine;

namespace TurboEdition.Components
{
    public class EventFunctionsExtended : EventFunctions
    {
        /// <summary>
        /// Requires a model locator and a child locator for this to work
        /// </summary>
        /// <param name="effectObj"></param>
        public void CreateLocalEffectInChildIndex(GameObject effectObj, short childIndex)
        {
            EffectManager.SpawnEffect(effectObj, new EffectData
            {
                origin = transform.position,
                modelChildIndex = childIndex,
            }, false);
        }
        /// <summary>
        /// Requires a model locator and a child locator for this to work
        /// </summary>
        /// <param name="effectObj"></param>
        public void CreateNetworkedEffectInChildIndex(GameObject effectObj, short childIndex)
        {
            EffectManager.SpawnEffect(effectObj, new EffectData
            {
                origin = transform.position,
                modelChildIndex = childIndex
            }, true);
        }
        /// <summary>
        /// Sets root as self
        /// </summary>
        /// <param name="effectObj"></param>
        public void CreateLocalEffectInSelf(GameObject effectObj)
        {
            EffectManager.SpawnEffect(effectObj, new EffectData
            {
                origin = transform.position,
                rootObject = gameObject,
            }, false);
        }
        /// <summary>
        /// Sets root as self
        /// </summary>
        /// <param name="effectObj"></param>
        public void CreateNetworkedEffectInSelf(GameObject effectObj)
        {
            EffectManager.SpawnEffect(effectObj, new EffectData
            {
                origin = transform.position,
                rootObject = gameObject,
            }, true);;
        }
    }
}
using HG;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(TetherVfxOrigin))]
    internal class SimpleListTether : MonoBehaviour
    {
        //Must be feed with a sphereSearch that has not been .ClearCandidates()
        [HideInInspector]
        public List<UnityEngine.Object> objectList;

        [Tooltip("Tether VFX Origin prefab to use.")]
        public TetherVfxOrigin tetherVfxOrigin;

        protected void FixedUpdate()
        {
            if (this.tetherVfxOrigin && objectList.Count > 0)
            {
                List<Transform> list2 = CollectionPool<Transform, List<Transform>>.RentCollection();
                int i = 0;
                int count = objectList.Count;
                while (i < count)
                {
                    HurtBox hurtContainer = null;
                    HealthComponent healthContainer = null;
                    Transform transformToSet = null;

                    if (objectList[i].GetType() == typeof(HealthComponent))
                        healthContainer = (HealthComponent)objectList[i];
                    else
                        hurtContainer = (HurtBox)objectList[i];

                    if (hurtContainer)
                    {
                        healthContainer = hurtContainer.healthComponent;
                        if (healthContainer)
                        {
                            Transform coreTransform = healthContainer.body.coreTransform;
                            if (coreTransform)
                            {
                                transformToSet = coreTransform;
                            }
                        }
                        list2.Add(transformToSet);
                    }
                    else if (healthContainer)
                    {
                        {
                            Transform coreTransform = healthContainer.body.coreTransform;
                            if (coreTransform)
                            {
                                transformToSet = coreTransform;
                            }
                        }
                        list2.Add(transformToSet);
                    }
                    i++;
                }
                this.tetherVfxOrigin.SetTetheredTransforms(list2);
                CollectionPool<Transform, List<Transform>>.ReturnCollection(list2);
            }
        }

        private bool IsHealthComponent(UnityEngine.Object unObject)
        {
            return unObject.GetType() == typeof(HealthComponent);
        }
    }
}
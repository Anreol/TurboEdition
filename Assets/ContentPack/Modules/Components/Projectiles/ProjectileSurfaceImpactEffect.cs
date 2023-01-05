using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using UnityEngine;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileSurfaceImpactEffect : MonoBehaviour, IProjectileImpactBehavior
    {
        public GameObject prefabEffect;
        [Tooltip("Should the effect have a specific size instead of using the EffectData's default scale. -1 to disable")]
        public float scaleOverride = 1;
        public bool useSurfaceDefEffectToo;
        [Tooltip("Should both the surfaceDef's and effect prefab rotate to the impact's normal angles.")]
        public bool tryToNormalize;
        [Tooltip("Should the prefab effect use the color override instead of figuring out the color off the hit surfaceDef's color. Does not change the surfaceDef's effect color.")]
        public bool useColorOverride = false;
        public Color colorOverride = Color.white;

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            Vector3 normal = (impactInfo.estimatedImpactNormal == default) ? -base.gameObject.transform.forward : impactInfo.estimatedImpactNormal;
            Vector3 position = (impactInfo.estimatedPointOfImpact == default) ? base.gameObject.transform.position : impactInfo.estimatedPointOfImpact;
            SurfaceDef surfaceDef = SurfaceDefProvider.GetObjectSurfaceDef(impactInfo.collider, position);
            if (surfaceDef)
            {
                Color color = surfaceDef.approximateColor;
                if (surfaceDef.impactEffectPrefab && useSurfaceDefEffectToo)
                {
                    EffectData effectData = new EffectData()
                    {
                        color = color,
                        origin = position,
                        surfaceDefIndex = surfaceDef.surfaceDefIndex,
                        rotation = tryToNormalize ? Util.QuaternionSafeLookRotation(normal) : EffectData.defaultRotation,
                        scale = scaleOverride != -1 ? scaleOverride : EffectData.defaultScale,
                    };
                    EffectManager.SpawnEffect(surfaceDef.impactEffectPrefab, effectData, false); //Will be local, we dont need to transmit
                    string impactSoundString = surfaceDef.impactSoundString;
                    if (!string.IsNullOrEmpty(impactSoundString))
                    {
                        PointSoundManager.EmitSoundLocal((AkEventIdArg)impactSoundString, effectData.origin);
                    }
                }
                if (prefabEffect)
                {
                    EffectData effectData = new EffectData()
                    {
                        color = useColorOverride ? colorOverride : color,
                        origin = position,
                        surfaceDefIndex = surfaceDef.surfaceDefIndex,
                        rotation = tryToNormalize ? Util.QuaternionSafeLookRotation(normal) : EffectData.defaultRotation,
                        scale = scaleOverride != -1 ? scaleOverride : EffectData.defaultScale,
                    };
                    EffectManager.SpawnEffect(prefabEffect, effectData, false);
                }
                return;
            }
            if (prefabEffect)
            {
                EffectManager.SimpleImpactEffect(prefabEffect, position, tryToNormalize ? normal : EffectData.defaultRotation.eulerAngles, useColorOverride ? colorOverride : Color.white, false);
            }
        }
    }
}
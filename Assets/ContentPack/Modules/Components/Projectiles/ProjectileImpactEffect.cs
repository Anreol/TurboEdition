using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using UnityEngine;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileImpactEffect : MonoBehaviour, IProjectileImpactBehavior
    {
        public GameObject prefabEffect;
        public float scaleOverride = 1;
        public bool useSurfaceDefEffectToo;
        public bool tryToNormalize;

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            Vector3 normal = (impactInfo.estimatedImpactNormal == new Vector3(0, 0, 0)) ? -base.gameObject.transform.forward : impactInfo.estimatedImpactNormal;
            Vector3 position = (impactInfo.estimatedPointOfImpact == new Vector3(0, 0, 0)) ? base.gameObject.transform.position : impactInfo.estimatedPointOfImpact;
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
                        color = color,
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
                EffectManager.SimpleImpactEffect(prefabEffect, position, normal, false);
            }
        }
    }
}
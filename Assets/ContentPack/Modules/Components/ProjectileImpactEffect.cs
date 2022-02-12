using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileImpactEffect : MonoBehaviour, IProjectileImpactBehavior
    {
        public GameObject prefabEffect;
        public bool useSurfaceDefEffectToo;

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
                        rotation = Util.QuaternionSafeLookRotation(normal),
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
                        rotation = Util.QuaternionSafeLookRotation(normal)
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

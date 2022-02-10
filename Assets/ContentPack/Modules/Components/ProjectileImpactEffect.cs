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
    public class ProjectileImpactEffect : MonoBehaviour, IProjectileImpactBehavior
    {
        public GameObject prefabEffect;
        public bool useSurfaceDefEffectToo;

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            SurfaceDef surfaceDef = SurfaceDefProvider.GetObjectSurfaceDef(impactInfo.collider, impactInfo.estimatedImpactNormal);
            if (surfaceDef)
            {
                Color color = surfaceDef.approximateColor;
                if (surfaceDef.impactEffectPrefab && useSurfaceDefEffectToo)
                {
                    EffectData effectData = new EffectData()
                    {
                        origin = impactInfo.estimatedPointOfImpact
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
                        origin = impactInfo.estimatedImpactNormal,
                        surfaceDefIndex = surfaceDef.surfaceDefIndex,
                        rotation = Util.QuaternionSafeLookRotation(impactInfo.estimatedImpactNormal)
                    };
                    EffectManager.SpawnEffect(prefabEffect, effectData, false);
                }
                return;
            }
            if (prefabEffect)
            {
                EffectManager.SimpleImpactEffect(prefabEffect, impactInfo.estimatedPointOfImpact, impactInfo.estimatedImpactNormal, false);
            }
        }
    }
}

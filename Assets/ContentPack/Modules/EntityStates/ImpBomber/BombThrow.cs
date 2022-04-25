using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.EntityStates.ImpBomber.Weapon
{
    class BombThrow : GenericProjectileBaseState
    {
        [Tooltip("The stages in which to use a specific model. Parallel to stageProjectilePrefabs.")]
        [SerializeField]
        public string[] stageNames;
        [Tooltip ("The specific projectile prefabs to use per stage. Parallel to stageNames.")]
        [SerializeField]
        public GameObject[] stageProjectilePrefabs;

        public override void OnEnter()
        {
            base.OnEnter();
            for (int i = 0; i < stageNames.Length; i++)
            {
                if (SceneCatalog.mostRecentSceneDef == SceneCatalog.GetSceneDefFromSceneName(stageNames[i]) && stageProjectilePrefabs[i])
                {
                    base.projectilePrefab = stageProjectilePrefabs[i];
                    base.OnEnter();
                    return;
                }
            }
            base.OnEnter();
        }
        public override void PlayAnimation(float duration)
        {
            base.PlayAnimation(duration);
            base.PlayCrossfade("Gesture, Additive", "ThrowBomb", "BombThrow.playbackRate", duration, 0.1f);
        }

    }
}

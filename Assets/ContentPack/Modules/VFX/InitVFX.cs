using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;

using TemporaryVFX = TurboEdition.TempVFX.TemporaryVFX;

namespace TurboEdition
{
    class InitVFX
    {
        public static Dictionary<TemporaryVFX, GameObject> temporaryVfx = new Dictionary<TemporaryVFX, GameObject>();
        //public static TemporaryOverlay[] temporaryOverlays = new TemporaryOverlay[] { };
        public static void Initialize()
        {
            InitializeVfx();
            //InitializeOverlays();

            CharacterBody.onBodyStartGlobal += AddVFXManager;
            SceneCamera.onSceneCameraPreRender += SceneCamera_onSceneCameraPreRender;
        }

        private static void SceneCamera_onSceneCameraPreRender(SceneCamera sceneCamera)
        {
            if (sceneCamera.cameraRigController)
            {
                foreach (TurboVFXManager vFXManager in InstanceTracker.GetInstancesList<TurboVFXManager>())
                {
                    vFXManager.UpdateForCamera(sceneCamera.cameraRigController);
                }
            }
        }

        private static void InitializeVfx()
        {
            var VFXTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(TemporaryVFX)));
            foreach (var item in VFXTypes)
            {
                TemporaryVFX vfx = (TemporaryVFX)System.Activator.CreateInstance(item);
                if (!vfx.temporaryVisualEffect)
                {
                    Debug.LogError("TempVFX " + vfx + " is missing the visual effect. Check Unity Project. Skipping.");
                    continue;
                }
                vfx.Initialize();
                temporaryVfx.Add(vfx, vfx.temporaryVisualEffect.gameObject);
            }
        }

        /*private static void InitializeOverlays()
        {
            temporaryOverlays = Assets.mainAssetBundle.LoadAllAssets<TemporaryOverlay>();
        }*/

        private static void AddVFXManager(CharacterBody body)
        {
            if (body && body.modelLocator)
            {
                var vfxManager = body.gameObject.AddComponent<TurboVFXManager>();
            }
        }

    }
}

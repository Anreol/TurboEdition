using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using TemporaryVFX = TurboEdition.TempVFX.TemporaryVFX;

namespace TurboEdition
{
    internal class InitVFX
    {
        public static Dictionary<TemporaryVFX, GameObject> temporaryVfx = new Dictionary<TemporaryVFX, GameObject>();

        //public static TemporaryOverlay[] temporaryOverlays = new TemporaryOverlay[] { };
        public static void Init()
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
                if (!vfx.tempVfxRootGO)
                {
                    TELog.LogE("TempVFX " + vfx + " is missing the visual effect root GameObject. Check Unity Project. Skipping.", true);
                    continue;
                }
                vfx.Initialize();
                temporaryVfx.Add(vfx, vfx.tempVfxRootGO);
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
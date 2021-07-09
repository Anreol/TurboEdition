using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;

namespace TurboEdition
{
    class InitVFX
    {
        public static Dictionary<TemporaryVisualEffect, GameObject> temporaryVfx = new Dictionary<TemporaryVisualEffect, GameObject>();
        public static TemporaryOverlay[] temporaryOverlays = new TemporaryOverlay[] { };
        public static void Initialize()
        {
            InitializeVfx();
            InitializeOverlays();

            CharacterBody.onBodyStartGlobal += AddVFXManager;
        }

        private static void InitializeVfx()
        {
            TemporaryVisualEffect[] vfxBuffer = Assets.mainAssetBundle.LoadAllAssets<TemporaryVisualEffect>();
            foreach (var item in vfxBuffer)
            {
                temporaryVfx.Add(item, item.gameObject);
            }
        }

        private static void InitializeOverlays()
        {
            temporaryOverlays = Assets.mainAssetBundle.LoadAllAssets<TemporaryOverlay>();
        }
    }
}

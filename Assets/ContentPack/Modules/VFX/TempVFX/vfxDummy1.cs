using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.TempVFX
{
    class vfxDummy1 : TemporaryVFX
    {
        public override GameObject tempVfxRootGO { get; set; } = Assets.mainAssetBundle.LoadAsset<GameObject>("vfxTonicTest");
        public override bool IsEnabled(ref CharacterBody body)
        {
            if (body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("Hitlag")) > 0)
            {
                Debug.LogWarning("FUCK <- YOU ->.");
                return true;
            }
            return false;
        }
    }
}

using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.TempVFX
{
    class vfxDeathCards : TemporaryVFX
    {
        public override GameObject tempVfxRootGO { get; set; } = Assets.mainAssetBundle.LoadAsset<GameObject>("vfxHellLinkedBuffEffect");
        public override bool IsEnabled(ref CharacterBody body)
        {
            if (body.HasBuff(Assets.mainAssetBundle.LoadAsset<BuffDef>("BuffHellLinked")))
            {
                Debug.LogWarning("Started spewing cards... or should.");
                return true;
            }
            return false;
        }
    }
}

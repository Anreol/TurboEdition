using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.TempVFX
{
    class VfxVoidWarbannerBuffIndicator : TemporaryVFX
    {
        public override GameObject tempVfxRootGO { get; set; } = Assets.mainAssetBundle.LoadAsset<GameObject>("vfxVoidWarbannerBuffEffect");
        public override bool IsEnabled(ref CharacterBody body)
        {
            if (body.GetBuffCount(TEContent.Buffs.WardOnLevelVoid) > 0)
            {
                return true;
            }
            return false;
        }
    }
}

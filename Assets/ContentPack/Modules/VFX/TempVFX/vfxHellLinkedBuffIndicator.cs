using RoR2;
using UnityEngine;

namespace TurboEdition.TempVFX
{
    internal class VfxHellLinkedBuffIndicator : TemporaryVFX
    {
        public override GameObject tempVfxRootGO { get; set; } = Assets.mainAssetBundle.LoadAsset<GameObject>("FX_HellLinked_Buff_Effect");

        public override bool IsEnabled(ref CharacterBody body)
        {
            if (body.GetBuffCount(TEContent.Buffs.HellLinked) > 0)
            {
                return true;
            }
            return false;
        }
    }
}
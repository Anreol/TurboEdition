using RoR2;
using UnityEngine;

namespace TurboEdition.TempVFX
{
    internal class VfxVoidWarbannerBuffIndicator : TemporaryVFX
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
using RoR2;
using UnityEngine;

namespace TurboEdition.TempVFX
{
    internal class vfxDummy1 : TemporaryVFX
    {
        public override GameObject tempVfxRootGO { get; set; } = Assets.mainAssetBundle.LoadAsset<GameObject>("vfxTonicTest");

        public override bool IsEnabled(ref CharacterBody body)
        {
            /*if (body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("Hitlag")) > 0)
            {
                return true;
            }*/
            return false;
        }
    }
}
using RoR2;
using UnityEngine;

namespace TurboEdition.TempVFX
{
    internal class vfxDeathCards : TemporaryVFX
    {
        public override GameObject tempVfxRootGO { get; set; } = Assets.mainAssetBundle.LoadAsset<GameObject>("vfxDeathCards");

        public override bool IsEnabled(ref CharacterBody body)
        {
            if (body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("DropletDupe")) > 0 && body.GetComponent<Items.DropletDupe.DropletDupeBehavior>().suicideReady)
            {
                Debug.LogWarning("Started spewing cards... or should.");
                return true;
            }
            return false;
        }
    }
}
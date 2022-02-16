using RoR2;
using UnityEngine;

namespace TurboEdition.TempVFX
{
    internal class vfxDeathCards : TemporaryVFX
    {
        public override GameObject tempVfxRootGO { get; set; } = Assets.mainAssetBundle.LoadAsset<GameObject>("vfxDeathCards");

        public override bool IsEnabled(ref CharacterBody body)
        {
            if (!body.inventory)
                return false;
            if (body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("DropletDupe")) > 0)
            {
                Items.DropletDupe.DropletDupeBehaviorServer dropletDupeBehaviorServer = body.GetComponent<Items.DropletDupe.DropletDupeBehaviorServer>();
                if (dropletDupeBehaviorServer)
                {
                    if (dropletDupeBehaviorServer.suicideReady)
                    {
                        TELog.LogW("Started spewing cards... or should.");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
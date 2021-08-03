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

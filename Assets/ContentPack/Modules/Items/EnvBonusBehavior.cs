using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using RoR2;
using UnityEngine;

namespace TurboEdition
{
    //Stage.OnStageStartGlobal
    
    class EnvBonusBehavior : CharacterBody.ItemBehavior
    {
        private void OnEnable()
        {
            body.onInventoryChanged += ItemCheck;
        }

        private void ItemCheck()
        {
            if (body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("EnvBonus")) <= 0)
                Destroy(this);
        }
    }
}

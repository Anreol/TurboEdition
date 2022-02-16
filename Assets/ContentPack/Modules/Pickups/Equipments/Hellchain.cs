using HG;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.Equipments
{
    internal class Hellchain : Equipment
    {
        public override EquipmentDef equipmentDef { get; set; } = Assets.mainAssetBundle.LoadAsset<EquipmentDef>("HellChain");

        public override bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
    }
}
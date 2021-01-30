using RoR2;
using R2API;
using R2API.Utils;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Anreol.TurboEdition
{
    public class SynergyItemList
    {

        // Here's the dictionary that contains all the items necessary for each combo
        //Two types, any and all
        //Any makes the item spawn as long as theres ONE item requirement fulfilled
        //All makes the item spawn once ALL requirements have been fulfilled
        public Dictionary<ItemIndex, Tuple<ItemIndex, ItemIndex, ItemIndex>> synRequirementAll = new Dictionary<ItemIndex, Tuple<ItemIndex, ItemIndex, ItemIndex>>();
        public Dictionary<ItemIndex, Tuple<ItemIndex, ItemIndex, ItemIndex>> synRequirementAny = new Dictionary<ItemIndex, Tuple<ItemIndex, ItemIndex, ItemIndex>>();

        //Down here we should uh, add other type of requirements i.e based on player stats so its easier to keep track of like crit stat, instead of doing 10 Lensmaker Glasses.
        //Like here, lol.
    }
}

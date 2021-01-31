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
        //public Dictionary<ItemIndex, Tuple<ItemIndex, ItemIndex, ItemIndex>> synRequirementAll = new Dictionary<ItemIndex, Tuple<ItemIndex, ItemIndex, ItemIndex>>();
        //public Dictionary<ItemIndex, Tuple<ItemIndex, ItemIndex, ItemIndex>> synRequirementAny = new Dictionary<ItemIndex, Tuple<ItemIndex, ItemIndex, ItemIndex>>();

        //Down here we should uh, add other type of requirements i.e based on player stats so its easier to keep track of like crit stat, instead of doing 10 Lensmaker Glasses.
        //Like here, lol.


        //Okay forget about all the above
        //Syngergy items should have requirements
        //In case of items, it can be specific items, and/or by tags. ie isHealing
        //In case of conditions, a requirement can be made up of multiple items
        //i.e requirement is getting 100% crit. This can be obtained by checking user's crit stat, certain item amount or equipment.
        //Synergy items should have a certain amount of requirements
        //Synergy items have a way to meet those requirements
        //ANY or ALL, Any means that as long as one is fullfilled its fine
        //All means that all have to be fullfilled

    }
}

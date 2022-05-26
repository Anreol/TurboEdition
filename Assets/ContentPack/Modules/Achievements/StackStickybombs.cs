using RoR2;
using RoR2.Achievements;
using RoR2.Networking;
using RoR2.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Achievements
{
    [RegisterAchievement("StackStickybombs", "Items.SuperStickies", "RepeatedlyDuplicateItems", null)]
    class StackStickybombs : BaseStatMilestoneAchievement
    {
        public override StatDef statDef => PerItemStatDef.totalCollected.FindStatDef(RoR2Content.Items.StickyBomb.itemIndex);

        public override ulong statRequirement => 60UL;

    }
}

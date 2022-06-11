using RoR2;
using RoR2.Achievements;
using RoR2.Stats;

namespace TurboEdition.Achievements
{
    [RegisterAchievement("StackStickybombs", "Items.SuperStickies", "RepeatedlyDuplicateItems", null)]
    public class StackStickybombs : BaseStatMilestoneAchievement
    {
        public override StatDef statDef => PerItemStatDef.totalCollected.FindStatDef(RoR2Content.Items.StickyBomb.itemIndex);

        public override ulong statRequirement => 60UL;
    }
}
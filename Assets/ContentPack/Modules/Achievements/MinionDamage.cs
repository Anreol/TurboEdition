using RoR2;
using RoR2.Achievements;
using RoR2.Stats;

namespace TurboEdition.Achievements
{
    [RegisterAchievement("MinionDamage", "Items.ItemDeployer", "TotalDronesRepaired", null)]
    public class MinionDamage : BaseStatMilestoneAchievement
    {
        public override StatDef statDef => StatDef.totalMinionDamageDealt;

        public override ulong statRequirement => 20000UL;
    }
}
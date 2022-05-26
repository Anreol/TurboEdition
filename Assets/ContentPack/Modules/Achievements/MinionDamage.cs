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
    [RegisterAchievement("MinionDamage", "Items.ItemDeployer", "TotalDronesRepaired", null)]
    class MinionDamage : BaseStatMilestoneAchievement
    {
        public override StatDef statDef => StatDef.totalMinionDamageDealt;

        public override ulong statRequirement => 20000UL;

    }
}

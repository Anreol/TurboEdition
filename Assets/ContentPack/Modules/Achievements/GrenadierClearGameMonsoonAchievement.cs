﻿using RoR2;
using RoR2.Achievements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.Achievements
{
    [RegisterAchievement("GrenadierClearGameMonsoon", "Skins.Grenadier.Alt1", null, null)]
    class GrenadierClearGameMonsoonAchievement : BasePerSurvivorClearGameMonsoonAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("GrenadierBody");
        }
    }
}

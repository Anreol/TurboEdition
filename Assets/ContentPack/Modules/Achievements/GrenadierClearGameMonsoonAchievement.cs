using RoR2;
using RoR2.Achievements;

namespace TurboEdition.Achievements
{
    [RegisterAchievement("GrenadierClearGameMonsoon", "Skins.Grenadier.Alt1", null, null)]
    internal class GrenadierClearGameMonsoonAchievement : BasePerSurvivorClearGameMonsoonAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("GrenadierBody");
        }
    }
}
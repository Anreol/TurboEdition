using RoR2;

namespace TurboEdition
{
    public interface IStatBuffBehavior
    {
        void RecalculateStatsEnd();

        void RecalculateStatsStart(ref CharacterBody characterBody);
    }
}
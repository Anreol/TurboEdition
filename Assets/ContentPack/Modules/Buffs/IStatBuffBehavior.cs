using RoR2;
namespace TurboEdition
{
    public interface IStatBuffBehavior
    {
        void RecalculateStatsEnd(ref CharacterBody characterBody);

        void RecalculateStatsStart();
    }
}
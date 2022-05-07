using RoR2;

namespace TurboEdition.Buffs
{
    public class BuffElectrostatic : Buff
    {
        public override BuffDef buffDef { get; set; } = TEContent.Buffs.ElectroStatic;

        public override void Initialize()
        {
        }

        public override void BuffStep(ref CharacterBody body, int stack)
        {
        }

        public override void OnBuffFirstStackGained(ref CharacterBody body)
        {
        }

        public override void OnBuffLastStackLost(ref CharacterBody body)
        {
        }

        public override void RecalcStatsStart(ref CharacterBody body)
        {
        }

        public override void RecalcStatsEnd(ref CharacterBody body)
        {
        }
    }
}
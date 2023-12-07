using RoR2;

namespace TurboEdition.Buffs
{
    public class BuffHellLinked : Buff
    {
        public override BuffDef buffDef { get; set; } = TEContent.Buffs.HellLinked;

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

        /// <summary>
        /// Class that handles transmitting damage from one body to another.
        /// </summary>
        public class HellLinkedManager
        {

        }
    }
}
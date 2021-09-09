using RoR2;

namespace TurboEdition.Buffs
{
    public abstract class Buff
    {
        public abstract BuffDef buffDef { get; set; }

        public Buff()
        { }

        /// <summary>
        /// Initialization, gets called on BOOTUP
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Meant to add a ItemBehavior, gets called in CheckForBuffs, which has the update rate of OnClientBuffsChanged
        /// </summary>
        public virtual void AddBehavior(ref CharacterBody body, int stack)
        {
        }


        /// <summary>
        /// Single step of the buff. Gets called everytime the buff updates.
        /// </summary>
        public virtual void BuffStep(ref CharacterBody body, int stack)
        {
        }

        /// <summary>
        /// Called only when the buff gets gained.
        /// </summary>
        public virtual void OnBuffFirstStackGained(ref CharacterBody body)
        {
        }

        /// <summary>
        /// Called only when the buff gets lost.
        /// </summary>
        public virtual void OnBuffLastStackLost(ref CharacterBody body)
        {
        }

        /// <summary>
        /// Method in replacement of the Recalc Interface. Gets called at the start, and at the same rate as Recalculate Stats.
        /// </summary>
        public virtual void RecalcStatsStart(ref CharacterBody body)
        {
        }

        /// <summary>
        /// Method in replacement of the Recalc Interface. Gets called at the end, and at the same rate as Recalculate Stats.
        /// </summary>
        public virtual void RecalcStatsEnd(ref CharacterBody body)
        {
        }
    }
}
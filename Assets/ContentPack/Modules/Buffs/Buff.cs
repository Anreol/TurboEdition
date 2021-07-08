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
        /// Main body of the buff, 
        /// </summary>
        public virtual void UpdateBuff(ref CharacterBody body, int stack)
        {
        }

        public virtual void BuffStep(ref CharacterBody body, int stack)
        {
        }

        public virtual void OnBuffFirstStackGained(ref CharacterBody body)
        {
        }

        public virtual void OnBuffLastStackLost(ref CharacterBody body)
        {
        }

        public virtual void RecalcStatsStart(ref CharacterBody body)
        {
        }
        public virtual void RecalcStatsEnd(ref CharacterBody body)
        {
        }
    }
}
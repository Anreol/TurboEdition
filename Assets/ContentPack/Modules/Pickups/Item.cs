using RoR2;

namespace TurboEdition.Items
{
    /// <summary>
    /// Legacy Item base, nowadays used for simple things that shouldn't go inside a component
    /// </summary>
    public abstract class Item
    {
        public abstract ItemDef itemDef { get; set; }

        /// <summary>
        /// For the love of god PLEASE use this as minimally as possible for hooks, use itemBehaviors wherever possible
        /// </summary>
        public virtual void Initialize()
        {
        }

        public virtual void AddBehavior(ref CharacterBody body, int stack)
        {
        }

        /// <summary>
        /// Simple RecalcStats so it doesn't need to be a component
        /// </summary>
        public virtual void RecalcStatsStart(ref CharacterBody body, int stack)
        {
        }

        public virtual void RecalcStatsEnd(ref CharacterBody body, int stack)
        {
        }
    }
}
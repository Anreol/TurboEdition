using RoR2;

namespace TurboEdition.Equipments
{
    public abstract class Equipment
    {
        public abstract EquipmentDef equipmentDef { get; set; }

        /// <summary>
        /// For the love of god PLEASE use this as minimally as possible
        /// </summary>

        public virtual void Initialize()
        {
        }

        public virtual void AddBehavior(ref CharacterBody body, int stack)
        {
        }

        /// <summary>
        /// Only runs in the server.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public virtual bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
    }
}
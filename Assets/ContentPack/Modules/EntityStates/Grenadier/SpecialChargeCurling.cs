using TurboEdition.Components;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    internal class SpecialChargeCurling : SpecialChargeThrowBase
    {
        private bool shotOnce;
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            activatorSkillSlot.skillDef.canceledFromSprinting = !shotOnce;
        }

        public override void FireOnce(bool wasForced)
        {
            base.FireOnce(wasForced);
            shotOnce = true;
        }
    }
}
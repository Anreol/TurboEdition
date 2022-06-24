namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    internal class SpecialChargeCurling : AimThrowableBaseChargable
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            activatorSkillSlot.skillDef.canceledFromSprinting = !firedAtLeastOnce;
        }
    }
}
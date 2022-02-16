namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    internal class SpecialChargeCurling : SpecialChargeThrowBase
    {
        public override SpecialThrowBase GetNextState()
        {
            return new SpecialThrowCurling();
        }
    }
}
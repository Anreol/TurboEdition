namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class SpecialChargeSatchels : SpecialChargeThrowBase
    {
        public override SpecialThrowBase GetNextState()
        {
            return new SpecialThrowSatchels();
        }
    }
}
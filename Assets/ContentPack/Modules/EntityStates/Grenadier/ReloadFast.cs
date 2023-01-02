namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class ReloadFast : Reload
    {
        public override Reload GetNextState()
        {
            return new ReloadFast();
        }
    }
}
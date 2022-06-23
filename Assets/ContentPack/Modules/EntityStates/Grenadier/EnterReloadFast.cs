using EntityStates;
using RoR2;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class EnterReloadFast : EnterReload
    {
        public override Reload GetNextState()
        {
            return new ReloadFast();
        }
    }
}
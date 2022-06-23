using EntityStates;
using RoR2;
using TurboEdition.Components;
using UnityEngine;

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    class SpecialChargeCurling : SpecialChargeThrowBase
    {
        public override SpecialThrowBase GetNextState()
        {
            return new SpecialThrowCurling();
        }
    }
}

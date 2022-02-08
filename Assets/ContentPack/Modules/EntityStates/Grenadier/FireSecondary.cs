using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    class FireSecondary : GenericProjectileBaseState
    {
        public static float selfForce;
        public override void FireProjectile()
        {
            base.FireProjectile();
            if (base.characterMotor)
            {
                base.characterMotor.ApplyForce(GetAimRay().direction * -selfForce, false, false);
            }
        }
    }
}

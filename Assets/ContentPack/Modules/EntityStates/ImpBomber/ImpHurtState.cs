using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.EntityStates.ImpBomber
{
    class ImpHurtState : HurtState
    {
        private EntityStateMachine resolvedWeaponMachine;

        public override void OnEnter()
        {
            resolvedWeaponMachine = EntityStateMachine.FindByCustomName(characterBody.gameObject, "Weapon");
            if (resolvedWeaponMachine.state.GetType() == typeof(ImpBomber.Weapon.BombHolding))
            {
                resolvedWeaponMachine.SetNextState(new ImpBomber.Weapon.BombThrowForced());
            }
            base.OnEnter();
        }
    }
}

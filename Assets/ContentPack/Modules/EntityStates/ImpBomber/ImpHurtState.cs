using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.EntityStates.ImpBomber
{
    class ImpHurtState : HurtState
    {
        private EntityStateMachine resolvedWeaponMachine;

        public override void OnEnter()
        {
            base.OnEnter();
            resolvedWeaponMachine = EntityStateMachine.FindByCustomName(characterBody.gameObject, "Weapon");
            if (resolvedWeaponMachine.state.GetType() == typeof(ImpBomber.Weapon.BombHolding) || resolvedWeaponMachine.state.GetType() == typeof(ImpBomber.Weapon.BombGet))
            {
                resolvedWeaponMachine.SetNextState(new ImpBomber.Weapon.BombThrowForced());
            }
        }
    }
}

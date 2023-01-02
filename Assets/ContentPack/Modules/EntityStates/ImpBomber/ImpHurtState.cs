using EntityStates;
using RoR2;

namespace TurboEdition.EntityStates.ImpBomber
{
    internal class ImpHurtState : HurtState
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
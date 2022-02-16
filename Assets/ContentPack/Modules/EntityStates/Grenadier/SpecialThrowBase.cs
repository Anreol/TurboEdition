using EntityStates;
using RoR2.Projectile;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class SpecialThrowBase : BaseSkillState
    {
        public FireProjectileInfo fireProjectileInfo;
        public int projectileCount;

        public override void OnEnter()
        {
            base.OnEnter();
        }
    }
}
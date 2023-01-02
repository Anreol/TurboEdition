using UnityEngine;

namespace TurboEdition.EntityStates.ImpBomber.Weapon
{
    internal class BombThrowForced : BombThrow
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (skillLocator && skillLocator.primary)
            {
                base.skillLocator.primary.DeductStock(base.skillLocator.primary.stock);
            }
        }

        public override Ray ModifyProjectileAimRay(Ray aimRay)
        {
            aimRay.direction = Vector3.up;
            return base.ModifyProjectileAimRay(aimRay);
        }
    }
}
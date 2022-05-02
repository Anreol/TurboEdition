using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.EntityStates.ImpBomber.Weapon
{
    class BombThrow : GenericProjectileBaseState
    {

        public override void OnEnter()
        {
            base.GetModelAnimator().SetBool("BombHolding.active", false);
            base.OnEnter();
        }
        public override void PlayAnimation(float duration)
        {
            base.PlayAnimation(duration);
            base.PlayCrossfade("Gesture, Additive", "ThrowBomb", "BombThrow.playbackRate", duration, 0.1f);
        }

    }
}

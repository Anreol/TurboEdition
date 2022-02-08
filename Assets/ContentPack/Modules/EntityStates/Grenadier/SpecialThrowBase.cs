using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class SpecialThrowBase : GenericProjectileBaseState
    {
        public int projectileCount;
        public float charge;
        public override void OnEnter()
        {
            base.OnEnter();
        }

    }
}

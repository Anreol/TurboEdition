using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

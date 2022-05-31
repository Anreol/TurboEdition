using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.Components
{
    public class MarkReducedSelfDamage : MonoBehaviour
    {
        public int reduceDamageFraction = 2;
        public bool forceNoCrit = true;
        public bool clearDots = true;
        public DamageType damageTypeOverride = DamageType.NonLethal;
    }
}

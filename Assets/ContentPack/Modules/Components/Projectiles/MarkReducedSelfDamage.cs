using RoR2;
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
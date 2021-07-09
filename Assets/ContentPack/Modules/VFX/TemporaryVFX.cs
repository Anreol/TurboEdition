using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;

//I am so fucking sick of this system organization but classes are extremely powerful.
//1. VFX need a case where to activate
//2. Even if everything was in the manager, you cannot dump everything into the array that gets looped without knowing which one is it for the activation case
//3. lets store the effect and prefab in the same place
namespace TurboEdition.TempVFX
{
    public abstract class TemporaryVFX
    {
        public abstract TemporaryVisualEffect temporaryVisualEffect { get; set; }
        public virtual void Initialize()
        {
        }
        public virtual float GetEffectRadius(ref CharacterBody body)
        {
            return body.radius;
        }
        public virtual string GetChildOverride(ref CharacterBody body)
        {
            return "";
        }
        public virtual bool IsEnabled(ref CharacterBody body)
        {
            return false;
        }

    }
}

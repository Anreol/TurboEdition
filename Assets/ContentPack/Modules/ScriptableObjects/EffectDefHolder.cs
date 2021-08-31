using RoR2;
using UnityEngine;

namespace TurboEdition.ScriptableObjects
{
    //EffectDefs aren't scriptable Objects atm so this is a workaround
    [CreateAssetMenu(fileName = "EffectDef Holder", menuName = "TurboEdition/EffectDef Holder", order = 2)]
    public class EffectDefHolder : ScriptableObject
    {
        public GameObject[] effectPrefabs;

        public static EffectDef ToEffectDef(GameObject effect)
        {
            return new EffectDef(effect);
        }

    }
}
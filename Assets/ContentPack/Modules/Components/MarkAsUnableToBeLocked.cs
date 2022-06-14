using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace TurboEdition.Components
{
    public class MarkAsUnableToBeLockedHook
    {
        [SystemInitializer]
        public static void Initialize()
        {
            IL.RoR2.OutsideInteractableLocker.ChestLockCoroutine += ChestLockCoroutine;
        }

        private static void ChestLockCoroutine(MonoMod.Cil.ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchLdloc(10), x => x.MatchCallOrCallvirt<UnityEngine.Object>("op_Implicit")))
            {
                bool hasMarker = false;
                c.Emit(OpCodes.Ldloc, 10);
                c.EmitDelegate<Action<OutsideInteractableLocker.Candidate>>(candidate =>
                {
                    if (candidate.purchaseInteraction.gameObject.GetComponent<MarkAsUnableToBeLocked>() != null)
                    {
                        hasMarker = true;
                    }
                });
                c.EmitDelegate<Func<bool, bool>>((origBool) =>
                {
                    return origBool && hasMarker;
                });
            }
        }
    }
    public class MarkAsUnableToBeLocked : MonoBehaviour
    {

    }
}
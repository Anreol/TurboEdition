using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
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
            //IL.RoR2.OutsideInteractableLocker.ChestLockCoroutine += ChestLockCoroutine;

            Type nestedType = typeof(OutsideInteractableLocker).GetNestedType("<ChestLockCoroutine>d__20", System.Reflection.BindingFlags.NonPublic);
            var reflection = nestedType.GetMethod("MoveNext", (System.Reflection.BindingFlags)(-1));
            _ = new ILHook(reflection, (ILContext.Manipulator)((il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(MoveType.After,
                    x => x.MatchLdfld(typeof(OutsideInteractableLocker.Candidate), "purchaseInteraction"));
                c.EmitDelegate<Func<PurchaseInteraction, PurchaseInteraction>>((purchase) =>
                {
                    if (purchase.GetComponent<MarkAsUnableToBeLocked>())
                    {
                        return null;
                    }
                    return purchase;
                });
            }));
        }

        private static void ChestLockCoroutine(MonoMod.Cil.ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchLdloc(10), x => x.MatchCallOrCallvirt(typeof(UnityEngine.Object).GetMethod("op_Implicit", (System.Reflection.BindingFlags)(-1)))))
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
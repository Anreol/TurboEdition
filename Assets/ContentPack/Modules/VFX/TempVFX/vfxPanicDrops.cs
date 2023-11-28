using RoR2;
using TurboEdition.EntityStates.AI.Walker;
using UnityEngine;

namespace TurboEdition.TempVFX
{
    internal class vfxPanicDrops : TemporaryVFX
    {
        public override GameObject tempVfxRootGO { get; set; } = Assets.mainAssetBundle.LoadAsset<GameObject>("FX_Panic_Buff_Effect");

        public override string GetChildOverride(ref CharacterBody body)
        {
            return "Head";
        }

        public override bool IsEnabled(ref CharacterBody body)
        {
            if (body.master)
            {
                foreach (var item in body.master.aiComponents)
                {
                    if (item.stateMachine.state as ForceFlee != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
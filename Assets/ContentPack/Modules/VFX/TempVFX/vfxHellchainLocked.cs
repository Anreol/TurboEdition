using RoR2;
using TurboEdition.Components;
using UnityEngine;

namespace TurboEdition.TempVFX
{
    internal class vfxHellchainLocked : TemporaryVFX
    {
        public override GameObject tempVfxRootGO { get; set; } = Assets.mainAssetBundle.LoadAsset<GameObject>("vfxHellLinkedBuffEffect");

        public override bool IsEnabled(ref CharacterBody body)
        {
            if (body.HasBuff(Assets.mainAssetBundle.LoadAsset<BuffDef>("BuffHellLinked")) && body.GetComponent<HellchainController>())
            {
                Debug.LogWarning("Should be locked..");
                return true;
            }
            return false;
        }
    }
}
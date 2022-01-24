using RoR2;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class VoidWarbanner : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("VoidWarbanner");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            if (!NetworkServer.active) return;
            body.AddItemBehavior<VoidWarbannerServer>(stack);
        }

        internal class VoidWarbannerServer : CharacterBody.ItemBehavior
        {
            private float activationWindow = 30f;

            private void Start()
            {
                if (!NetworkServer.active) return;
                //if (body.hasEffectiveAuthority)
                {
                    if (Stage.instance.entryTime.timeSince <= activationWindow && body)
                        body.AddTimedBuff(Assets.mainAssetBundle.LoadAsset<BuffDef>("BuffEnvBonus"), 45 + ((stack - 1) * 30));
                }
                Destroy(this);
            }
        }
    }
}
using RoR2;

namespace TurboEdition.Items
{
    public class EnvBonus : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("EnvBonus");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<EnvBonusBehavior>(stack);
        }

        internal class EnvBonusBehavior : CharacterBody.ItemBehavior
        {
            private float activationWindow = 30f;

            private void Start()
            {
                //if (!NetworkServer.active) return;
                if (body.hasEffectiveAuthority)
                {
                    if (Stage.instance.entryTime.timeSince <= activationWindow && body)
                        body.AddTimedBuff(Assets.mainAssetBundle.LoadAsset<BuffDef>("BuffEnvBonus"), 15 + ((stack - 1) * 10));
                }
                Destroy(this);
            }
        }
    }
}
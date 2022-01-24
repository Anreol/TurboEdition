using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class MeleeArmor : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("MeleeArmor");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            if (NetworkServer.active)
                body.AddItemBehavior<MeleeArmorBehaviorServer>(stack);
        }

        internal class MeleeArmorBehaviorServer : CharacterBody.ItemBehavior, IOnTakeDamageServerReceiver
        {
            private float detectRadius = 21f;

            private void Start()
            {
                if (body.healthComponent)
                    HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onTakeDamageReceivers, this);
            }

            void IOnTakeDamageServerReceiver.OnTakeDamageServer(DamageReport damageReport)
            {
                if (!NetworkServer.active) return;
                if (damageReport.attackerBody)
                {
                    float distance = Vector3.Distance(damageReport.victimBody.transform.position, damageReport.attackerBody.transform.position);
                    if (distance <= detectRadius && stack + 1 > body.GetBuffCount(Assets.mainAssetBundle.LoadAsset<BuffDef>("BuffMeleeArmor")))
                    {
                        body.AddTimedBuff(Assets.mainAssetBundle.LoadAsset<BuffDef>("BuffMeleeArmor"), 10);
                    }
                }
            }

            private void OnDestroy()
            {
                //This SHOULDNT cause any errors because nothing should be fucking with the order of things in this list... I hope.
                if (body.healthComponent)
                {
                    int i = System.Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                    if (i > -1)
                        HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onIncomingDamageReceivers, body.healthComponent.onIncomingDamageReceivers.Length, i);
                }
            }
        }
    }
}
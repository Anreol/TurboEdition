using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition
{
    internal class MeleeArmorBehavior : CharacterBody.ItemBehavior, IOnTakeDamageServerReceiver
    {
        private float detectRadius = 21f;

        private void OnEnable()
        {
            body.onInventoryChanged += ItemCheck;
        }

        private void ItemCheck()
        {
            if (body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("Hitlag")) <= 0)
                Destroy(this);
        }

        void IOnTakeDamageServerReceiver.OnTakeDamageServer(DamageReport damageReport)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            if (damageReport.attackerBody)
            {
                float distance = Vector3.Distance(damageReport.victimBody.transform.position, damageReport.attackerBody.transform.position);
                if (distance <= detectRadius && stack > body.GetBuffCount(Assets.mainAssetBundle.LoadAsset<BuffDef>("MeleeArmor")))
                {
                    body.AddTimedBuff(Assets.mainAssetBundle.LoadAsset<BuffDef>("MeleeArmor").buffIndex, 25);
                }
            }
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            body.armor += (body.GetBuffCount(Assets.mainAssetBundle.LoadAsset<BuffDef>("MeleeArmor")) * 25);
        }
    }
}
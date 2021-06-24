using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    internal class MeleeArmorBehavior : CharacterBody.ItemBehavior, IOnTakeDamageServerReceiver, IStatItemBehavior
    {
        private float detectRadius = 21f;

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
                    body.AddTimedBuff(Assets.mainAssetBundle.LoadAsset<BuffDef>("MeleeArmor"), 15);
                }
            }
        }

        public void RecalculateStatsEnd()
        {
            body.armor += (body.GetBuffCount(Assets.mainAssetBundle.LoadAsset<BuffDef>("MeleeArmor")) * 25);
        }

        public void RecalculateStatsStart()
        { }
    }
}
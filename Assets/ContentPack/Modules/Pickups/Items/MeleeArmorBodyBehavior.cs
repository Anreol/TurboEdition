using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class MeleeArmorBodyBehavior : BaseItemBodyBehavior, IOnTakeDamageServerReceiver
    {
        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.MeleeArmor;
        }

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
                if (distance <= detectRadius && stack + 1 > body.GetBuffCount(TEContent.Buffs.MeleeArmor))
                {
                    body.AddTimedBuff(TEContent.Buffs.MeleeArmor, 10);
                }
            }
        }

        private void OnDestroy()
        {
            if (body.healthComponent)
            {
                int i = System.Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                if (i > -1)
                    HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onIncomingDamageReceivers, body.healthComponent.onIncomingDamageReceivers.Length, i);
            }
        }
    }
}
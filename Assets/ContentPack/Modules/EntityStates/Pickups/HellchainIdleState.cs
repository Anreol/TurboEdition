using HG;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using TurboEdition.Orbs;
using UnityEngine.Networking;

namespace TurboEdition.States.Pickups
{
    internal class HellchainIdleState : HellchainBaseState, IOnTakeDamageServerReceiver
    {
        protected override bool shouldVFXAppear
        {
            get
            {
                return true;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (base.isAuthority)
            {
                if (attachedBody.healthComponent)
                    HG.ArrayUtils.ArrayAppend(ref attachedBody.healthComponent.onTakeDamageReceivers, this);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            List<UnityEngine.Object> lol = CollectionPool<UnityEngine.Object, List<UnityEngine.Object>>.RentCollection();
            foreach (UnityEngine.Object item in base.hellchainController.listOLinks) //I dont know how else to do this... sorry....
            {
                lol.Add(item);
            }
            base.tetherList.objectList = lol;
            CollectionPool<UnityEngine.Object, List<UnityEngine.Object>>.ReturnCollection(lol);
        }

        public override void OnExit()
        {
            base.OnExit();
            //This SHOULDNT cause any errors because nothing should be fucking with the order of things in this list... I hope.
            if (attachedBody.healthComponent)
            {
                int i = System.Array.IndexOf(attachedBody.healthComponent.onIncomingDamageReceivers, this);
                if (i > -1)
                    HG.ArrayUtils.ArrayRemoveAtAndResize(ref attachedBody.healthComponent.onIncomingDamageReceivers, attachedBody.healthComponent.onIncomingDamageReceivers.Length, i);
            }
        }

        void IOnTakeDamageServerReceiver.OnTakeDamageServer(DamageReport damageReport)
        {
            if (!NetworkServer.active) return;
            if (damageReport.dotType != DotController.DotIndex.None && !damageReport.damageInfo.rejected) return;

            if (damageReport.damageInfo.procChainMask.HasProc(ProcType.BounceNearby) || damageReport.damageInfo.procChainMask.HasProc(ProcType.ChainLightning) || damageReport.damageInfo.procChainMask.HasProc(ProcType.Missile) || damageReport.damageInfo.procChainMask.HasProc(ProcType.Thorns))
            {
                return;
            }
            foreach (CharacterBody cb in base.hellchainController.listOLinks)
            {
                float damageCoefficient2 = 0.8f;
                float damageValue2 = Util.OnHitProcDamage(damageReport.damageInfo.damage, damageReport.attackerBody.damage, damageCoefficient2);

                HellOrb orbFromOuterSpace = new HellOrb();
                orbFromOuterSpace.origin = damageReport.damageInfo.position;
                orbFromOuterSpace.damageValue = damageValue2;
                orbFromOuterSpace.isCrit = damageReport.damageInfo.crit;
                orbFromOuterSpace.teamIndex = attachedBody.teamComponent.teamIndex;
                orbFromOuterSpace.attacker = damageReport.damageInfo.attacker;
                orbFromOuterSpace.bouncedObjects = new List<HealthComponent>
                {
                    damageReport.victim.GetComponent<HealthComponent>()
                };
                orbFromOuterSpace.procChainMask = damageReport.damageInfo.procChainMask;
                orbFromOuterSpace.procChainMask.AddProc(ProcType.ChainLightning); //because i hate uke or somghtnhgh
                orbFromOuterSpace.procChainMask.AddProc(ProcType.BounceNearby); //idk what uses this
                orbFromOuterSpace.procCoefficient = 0.2f;
                orbFromOuterSpace.damageColorIndex = DamageColorIndex.Item;
                orbFromOuterSpace.target = cb.mainHurtBox;
                OrbManager.instance.AddOrb(orbFromOuterSpace);
            }
        }
    }
}
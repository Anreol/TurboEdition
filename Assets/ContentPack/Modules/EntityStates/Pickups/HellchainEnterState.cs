using HG;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.States.Pickups
{
    internal class HellchainEnterState : HellchainBaseState
    {
        public Run.FixedTimeStamp readyTime { get; private set; }
        public static float baseDuration = 10f;

        public static float sphereSearchRadius = 25f;

        protected override bool shouldVFXAppear //Not actually needed but I do not care
        {
            get
            {
                return false;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (base.isAuthority)
            {
                this.readyTime = Run.FixedTimeStamp.now + HellchainEnterState.baseDuration;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            base.OnExit();
            if (!NetworkServer.active)
                return;
            float duration = base.GetRemainingDuration();
            if (duration > 8f)
            {
                base.sphereSearch = new SphereSearch()
                {
                    mask = LayerIndex.entityPrecise.mask,
                    origin = base.attachedBody.transform.position,
                    queryTriggerInteraction = QueryTriggerInteraction.Collide,
                    radius = sphereSearchRadius
                };
                TeamMask sameTeam = new TeamMask();
                sameTeam.AddTeam(attachedBody.teamComponent.teamIndex);
                List<HurtBox> hurtBoxes = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(sameTeam).FilterCandidatesByDistinctHurtBoxEntities().OrderCandidatesByDistance().GetHurtBoxes(hurtBoxes);
                for (int i = 0; i < hurtBoxes.Count; i++)
                {
                    if (hurtBoxes[i].healthComponent)
                    {
                        if (!hurtBoxes[i].healthComponent.body.HasBuff(base.buffDef))
                        {
                            hurtBoxes[i].healthComponent.body.AddTimedBuff(base.buffDef, duration / 2 - 1);
                        }
                        base.hellchainController.listOLinks.Add(hurtBoxes[i].healthComponent.body);
                    }
                }
                CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(hurtBoxes);
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.readyTime);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.readyTime = reader.ReadFixedTimeStamp();
        }
    }
}
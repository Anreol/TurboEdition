using HG;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Scripts
{
    internal class HellchainLinkNearbyController : NetworkBehaviour, IOnTakeDamageServerReceiver
    {
        private SphereSearch sphereSearch;

        //Transform transform;
        private NetworkedBodyAttachment networkedBodyAttachment;

        private List<CharacterBody> bounceListFromBounce;

        //Editor
        public TetherVfxOrigin tetherVfxOrigin;

        public GameObject activeVfx;
        public float searchRadius;

        [SyncVar]
        public int maxTargets = 4;

        private bool isTetheredToAtLeastOneObject;

        protected void Awake()
        {
            this.bounceListFromBounce = new List<CharacterBody>();
            //this.transform = base.transform;
            this.networkedBodyAttachment = base.GetComponent<NetworkedBodyAttachment>();
            this.sphereSearch = new SphereSearch();
        }

        protected void FixedUpdate()
        {
            if (!networkedBodyAttachment.attachedBody.HasBuff(Assets.mainAssetBundle.LoadAsset<BuffDef>("HellLinked")))
            {
                Destroy(this);
            }
        }

        public void OnTakeDamageServer(DamageReport damageReport)
        {
            if (damageReport.damageInfo.rejected || damageReport.isFallDamage || damageReport.dotType != DotController.DotIndex.None)
            {
                return; // Begone
            }
            if (bounceListFromBounce.Contains(damageReport.victimBody))
            {
                return; // Shouldn't be the case considering bounceList is built inside DamageWithinRadius
            }
            DamageWithinRadius(damageReport);
        }

        protected void DamageWithinRadius(DamageReport damageReport)
        {
            this.bounceListFromBounce.Add(damageReport.attackerBody); // We add the attacker sauce right away to blacklist, just in case.
            List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
            this.SearchForTargets(list, damageReport.attackerBody.teamComponent.teamIndex);
            List<Transform> list2 = CollectionPool<Transform, List<Transform>>.RentCollection();
            for (int i = 0; i < list.Count; i++)
            {
                HurtBox hurtBox = list[i];
                if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.alive)
                {
                    HealthComponent healthComponent = hurtBox.healthComponent;
                    Transform transform = healthComponent.body.coreTransform ?? hurtBox.transform;
                    list2.Add(transform);

                    if (NetworkServer.active)
                    {
                        HellRelay hellRelay = new HellRelay(damageReport);
                        hellRelay.bouncedBodies.Union(bounceListFromBounce); // Me shitting the game's fps by making list unions
                        hellRelay.linkDistance = Vector3.Distance(networkedBodyAttachment.transform.position, healthComponent.body.transform.position);
                        hellRelay.linkSeverity = Mathf.Clamp01(1f - hellRelay.linkDistance / sphereSearch.radius);
                        hellRelay.BounceToBody(hurtBox.healthComponent.body);
                    }
                }
                bounceListFromBounce.Clear(); // Sent damages, clear blacklist
                if (list2.Count >= this.maxTargets)
                {
                    break;
                }
            }
            this.isTetheredToAtLeastOneObject = ((float)list2.Count > 0f);
            if (this.tetherVfxOrigin)
            {
                this.tetherVfxOrigin.SetTetheredTransforms(list2);
            }
            if (this.activeVfx)
            {
                this.activeVfx.SetActive(this.isTetheredToAtLeastOneObject);
            }
            CollectionPool<Transform, List<Transform>>.ReturnCollection(list2);
            CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
        }

        protected void SearchForTargets(List<HurtBox> dest, TeamIndex teamToGetEnemiesOf)
        {
            if (this.searchRadius > 0f)
            {
                TeamMask mask = TeamMask.GetEnemyTeams(teamToGetEnemiesOf);
                //mask.AddTeam();
                this.sphereSearch.mask = LayerIndex.entityPrecise.mask;
                this.sphereSearch.origin = this.transform.position;
                this.sphereSearch.radius = this.searchRadius;
                this.sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
                this.sphereSearch.RefreshCandidates();
                this.sphereSearch.FilterCandidatesByHurtBoxTeam(mask);
                this.sphereSearch.OrderCandidatesByDistance();
                this.sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                this.sphereSearch.GetHurtBoxes(dest);
                this.sphereSearch.ClearCandidates();
            }
        }

        public float NetworksearchRadius
        {
            get
            {
                return this.searchRadius;
            }
            [param: In]
            set
            {
                base.SetSyncVar<float>(value, ref this.searchRadius, 1U);
            }
        }

        public int NetworkmaxTargets
        {
            get
            {
                return this.maxTargets;
            }
            [param: In]
            set
            {
                base.SetSyncVar<int>(value, ref this.maxTargets, 1U);
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.WritePackedUInt32((uint)this.maxTargets);
                writer.Write(this.searchRadius);
                return true;
            }
            bool flag = false;
            if ((base.syncVarDirtyBits & 1U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.WritePackedUInt32((uint)this.maxTargets);
            }
            if ((base.syncVarDirtyBits & 2U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.Write((uint)this.searchRadius);
            }
            if (!flag)
            {
                writer.WritePackedUInt32(base.syncVarDirtyBits);
            }
            return flag;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                this.maxTargets = (int)reader.ReadPackedUInt32();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                this.maxTargets = (int)reader.ReadPackedUInt32();
            }
        }

        public class HellRelay
        {
            public List<CharacterBody> bouncedBodies;
            public DamageReport damageBounce;
            public float linkDistance;
            public float linkSeverity;

            public HellRelay(DamageReport damage)
            {
                this.bouncedBodies = new List<CharacterBody>();
                this.damageBounce = damage;
            }

            public void BounceToBody(CharacterBody characterBody)
            {
                if (!bouncedBodies.Contains(characterBody))
                {
                    bouncedBodies.Add(characterBody);
                    float minDamage = (50 / damageBounce.damageInfo.damage) * 100;
                    //float maxDamage = (90 / damageBounce.damageInfo.damage) * 100;
                    damageBounce.damageInfo.damage = Mathf.Lerp(minDamage, damageBounce.damageInfo.damage, linkSeverity);
                    if (characterBody.healthComponent)
                    {
                        characterBody.GetComponent<HellchainLinkNearbyController>()?.bounceListFromBounce.Union(bouncedBodies);
                        characterBody.healthComponent.TakeDamage(damageBounce.damageInfo);
                    }
                }
            }
        }
    }
}
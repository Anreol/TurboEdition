using EntityStates;
using HG;
using RoR2;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.EntityStates.Projectiles
{
    /// <summary>
    /// State for <see cref="Equipments.Hellchain"/>.
    /// </summary>
    public class HellChainCasting : EntityState
    {
        [SerializeField] public float debuffDuration;
        [SerializeField] public float areaSize;
        [SerializeField] public GameObject castEffectPrefab;

        private TeamFilter teamFilter;
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                teamFilter = base.GetComponent<TeamFilter>();
                if (teamFilter)
                {
                    CastAndDestroy();
                }
            }
        }

        /// <summary>
        /// Server only method!
        /// </summary>
        private void CastAndDestroy()
        {
            if (castEffectPrefab != null)
            {
                EffectManager.SpawnEffect(castEffectPrefab, new EffectData()
                {
                    origin = transform.position,
                    rotation = transform.rotation,
                    scale = areaSize
                }, true);
            }
            SphereSearch sphereSearch = new SphereSearch()
            {
                queryTriggerInteraction = QueryTriggerInteraction.Collide,
                mask = LayerIndex.entityPrecise.mask,
                origin = transform.position,
                radius = areaSize
            };

            List<HurtBox> hurtBoxes = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
            sphereSearch.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(teamFilter.teamIndex)).GetHurtBoxes(hurtBoxes);
            for (int i = 0; i < hurtBoxes.Count; i++)
            {
                if (hurtBoxes[i].healthComponent && hurtBoxes[i].healthComponent.body)
                {
                    hurtBoxes[i].healthComponent.body.AddTimedBuff(TEContent.Buffs.HellLinked, debuffDuration);
                }
            }
            CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(hurtBoxes);
            EntityState.Destroy(gameObject);
        }
    }
}
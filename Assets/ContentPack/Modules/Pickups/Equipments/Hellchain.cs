using HG;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.Equipments
{
    internal class Hellchain : Equipment
    {
        public override EquipmentDef equipmentDef { get; set; } = Assets.mainAssetBundle.LoadAsset<EquipmentDef>("HellChain");

        public override bool FireAction(EquipmentSlot slot)
        {
            return DoSearch(slot);
        }

        public bool DoSearch(EquipmentSlot slut)
        {
            SphereSearch sphereSearch = new SphereSearch()
            {
                mask = LayerIndex.entityPrecise.mask,
                origin = slut.characterBody.transform.position,
                queryTriggerInteraction = QueryTriggerInteraction.Collide,
                radius = 12f
            };
            TeamMask enemyTeam = new TeamMask();
            enemyTeam = TeamMask.GetEnemyTeams(slut.characterBody.teamComponent.teamIndex);
            List<HurtBox> hurtBoxes = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
            sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeam).FilterCandidatesByDistinctHurtBoxEntities().OrderCandidatesByDistance().GetHurtBoxes(hurtBoxes);
            for (int i = 0; i < hurtBoxes.Count; i++)
            {
                if (hurtBoxes[i].healthComponent)
                {
                    hurtBoxes[i].healthComponent.body.AddTimedBuff(Assets.mainAssetBundle.LoadAsset<BuffDef>("BuffHellLinked"), 24f);
                }
            }
            CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(hurtBoxes);
            return true;
        }
    }
}
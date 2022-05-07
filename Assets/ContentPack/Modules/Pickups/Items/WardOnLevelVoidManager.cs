using HG;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class WardOnLevelVoidManager
    {
        private static float activationWindow = 30f;
        private static float radiusEffect = 8f;
        private static readonly SphereSearch sphereSearch = new SphereSearch();

        [SystemInitializer(typeof(PickupCatalog))]
        public static void Initialize()
        {
            RoR2.CharacterBody.onBodyStartGlobal += onBodyStartGlobal;
            GlobalEventManager.onCharacterLevelUp += onCharacterLevelUp;
        }

        private static void onCharacterLevelUp(CharacterBody obj)
        {
            if (!NetworkServer.active) return;
            if (!obj || !obj.inventory) return;
            int c = obj.inventory.GetItemCount(TEContent.Items.WardOnLevelVoid.itemIndex);
            if (c > 0)
            {
                obj.AddTimedBuff(TEContent.Buffs.WardOnLevelVoid, 45 + ((c - 1 * 30)));

                List<HurtBox> hurtBoxes = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                SearchForTargets(hurtBoxes, obj);
                for (int i = 0; i < hurtBoxes.Count; i++)
                {
                    hurtBoxes[i].healthComponent.body?.AddTimedBuff(TEContent.Buffs.WardOnLevelVoid, 30 + ((c - 1 * 30) / 2));
                }
                CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(hurtBoxes);
            }
        }

        private static void onBodyStartGlobal(CharacterBody obj)
        {
            if (!NetworkServer.active) return;
            if (!obj || !obj.inventory) return;
            int c = obj.inventory.GetItemCount(TEContent.Items.WardOnLevelVoid.itemIndex);
            if (Stage.instance.entryTime.timeSince <= activationWindow && c > 0)
            {
                //obj.AddTimedBuff(TEContent.Buffs.WardOnLevelVoid, 45 + ((c - 1 * 30)));
                List<HurtBox> hurtBoxes = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                SearchForTargets(hurtBoxes, obj);
                for (int i = 0; i < hurtBoxes.Count; i++)
                {
                    hurtBoxes[i].healthComponent.body?.AddTimedBuff(TEContent.Buffs.WardOnLevelVoid, 30 + ((c - 1 * 30) / 2));
                }
                CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(hurtBoxes);
            }
        }

        protected static void SearchForTargets(List<HurtBox> dest, CharacterBody orig)
        {
            TeamMask teamMask = new TeamMask();
            teamMask.AddTeam(orig.teamComponent.teamIndex);

            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.origin = orig.gameObject.transform.position;
            sphereSearch.radius = radiusEffect;
            sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByHurtBoxTeam(teamMask);
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            sphereSearch.GetHurtBoxes(dest);
            sphereSearch.ClearCandidates();
        }
    }
}
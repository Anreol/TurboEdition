using HG;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class BaneMask : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("BaneMask");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<BaneMaskBehavior>(stack);
        }

        internal class BaneMaskBehavior : CharacterBody.ItemBehavior
        {
            public static GameObject pulsePrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("BaneMaskPulse");

            //Prefab info:
            //Final radius: 10, Duration 1.0
            //Destroy On Timer: 3 Seconds
            //Has Shaker Emiter
            private bool alreadyIn = false;

            private SphereSearch sphereSearch;

            private void FixedUpdate()
            {
                if (!NetworkServer.active)
                {
                    return;
                }
                if (!body.outOfCombat && !alreadyIn)
                {
                    GeneratePulse();
                    alreadyIn = true;
                }
                else if (body.outOfCombat && alreadyIn)
                {
                    alreadyIn = false;
                }
            }

            private void GeneratePulse()
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(pulsePrefab, body.transform.position, body.transform.rotation);
                PulseController component = gameObject.GetComponent<PulseController>();
                sphereSearch = new RoR2.SphereSearch()
                {
                    queryTriggerInteraction = UnityEngine.QueryTriggerInteraction.Collide,
                    mask = LayerIndex.entityPrecise.mask,
                    origin = body.transform.position,
                };

                component.finalRadius += stack;
                component.duration += (stack * 0.1f); //Artificially increase duration because in higher radius its almost instant
                component.performSearch += Component_performSearch;
                component.onPulseHit += Component_onPulseHit;
                component.StartPulseServer();
                NetworkServer.Spawn(gameObject);
            }

            private void Component_performSearch(PulseController pulseController, Vector3 origin, float radius, System.Collections.Generic.List<PulseController.PulseSearchResult> dest)
            {
                sphereSearch.origin = origin;
                sphereSearch.radius = radius;
                if (sphereSearch.radius <= 0)
                {
                    return;
                }
                List<HurtBox> hurtBoxes = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                sphereSearch.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(body.teamComponent.teamIndex)).GetHurtBoxes(hurtBoxes);
                for (int i = 0; i < hurtBoxes.Count; i++)
                {
                    if (hurtBoxes[i].healthComponent)
                    {
                        RoR2.PulseController.PulseSearchResult pulseSearchResult = new PulseController.PulseSearchResult();
                        pulseSearchResult.hitObject = hurtBoxes[i].healthComponent;
                        pulseSearchResult.hitPos = hurtBoxes[i].healthComponent.body.transform.position;
                        dest.Add(pulseSearchResult);
                    }
                }
                CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(hurtBoxes);
            }

            private void Component_onPulseHit(PulseController pulseController, PulseController.PulseHit hitInfo)
            {
                if (hitInfo.hitObject)
                {
                    HealthComponent hc = (HealthComponent)hitInfo.hitObject;
                    if (TeamManager.IsTeamEnemy(hc.body.teamComponent.teamIndex, body.teamComponent.teamIndex))
                    {
                        /*if (BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex("BuffFear"))) //Lazy to check for SS2 installation, check if catalog has fear in
                        {
                            hc.body.AddTimedBuff(BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex("BuffFear")), (4 + stack) * hitInfo.hitSeverity);
                            return;
                        }*/
                        hc.body.AddTimedBuff(RoR2Content.Buffs.Cripple, (4 + stack) * hitInfo.hitSeverity);
                        return;
                    }
                }
            }
        }
    }
}
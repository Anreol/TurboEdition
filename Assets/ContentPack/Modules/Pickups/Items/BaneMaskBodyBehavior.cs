using HG;
using RoR2;
using RoR2.Audio;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class BaneMaskBodyBehavior : BaseItemBodyBehavior, IOnTakeDamageServerReceiver
    {
        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.BaneMask;
        }

        private static GameObject pulsePrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("BaneMaskPulse");
        private static NetworkSoundEventDef rechargeProcSound = Assets.mainAssetBundle.LoadAsset<NetworkSoundEventDef>("nseItem_Proc_BaneMask_Recharge");

        //Prefab info:
        //Final radius: 10, Duration 1.0
        //Destroy On Timer: 3 Seconds
        //Has Shaker Emiter
        private bool alreadyOut = true;

        private bool percentTriggered = false;

        private SphereSearch sphereSearch;

        private void Start()
        {
            if (body.healthComponent)
                HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onTakeDamageReceivers, this);
        }

        private void OnDestroy()
        {
            //This SHOULDNT cause any errors because nothing should be fucking with the order of things in this list... I hope.
            if (body.healthComponent)
            {
                int i = System.Array.IndexOf(body.healthComponent.onTakeDamageReceivers, this);
                if (i > -1)
                    HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onTakeDamageReceivers, body.healthComponent.onTakeDamageReceivers.Length, i);
            }
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active)
                return;
            if (!body.outOfDanger && alreadyOut)
            {
                GeneratePulse();
                alreadyOut = false;
            }
            else if (body.outOfDanger && !alreadyOut)
            {
                alreadyOut = true;
                percentTriggered = false;
                EntitySoundManager.EmitSoundServer(rechargeProcSound.akId, body.gameObject);
            }
        }

        private void GeneratePulse() //Runs in server
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(pulsePrefab, body.transform.position, body.transform.rotation);
            PulseController component = gameObject.GetComponent<PulseController>();
            sphereSearch = new RoR2.SphereSearch()
            {
                queryTriggerInteraction = UnityEngine.QueryTriggerInteraction.Collide,
                mask = LayerIndex.entityPrecise.mask,
                origin = body.transform.position,
            };

            component.finalRadius = (16f + ((stack - 1) * 5f));
            component.duration = Mathf.Min(stack, 3);
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
                    if (hc.body.isChampion || hc.body.isBoss)
                        return;
                    float num2 = UnityEngine.Random.Range(20f, 100f); //Guarantees enemies like lemurians and beetles always pass
                    if (num2 <= Mathf.Pow(1.50f, hc.body.bestFitRadius)) //Best fit radius: Golems are 7.5, beetles are 1.82
                        return;
                    hc.body.AddTimedBuff(TEContent.Buffs.Panic, (10) * hitInfo.hitSeverity);
                    return;
                }
            }
        }

        public void OnTakeDamageServer(DamageReport damageReport)
        {
            if (!damageReport.damageInfo.rejected)
                if (Util.CheckRoll(damageReport.damageDealt / body.healthComponent.fullCombinedHealth, body.master) && !percentTriggered)
                {
                    GeneratePulse();
                    percentTriggered = true;
                }
        }
    }
}
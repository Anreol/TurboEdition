using RoR2;
using System.Collections.Generic;
using TurboEdition.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    /// <summary>
    /// MonoBeahviour wrapper for 
    /// </summary>
    public class UnsafeZoneDamageController : MonoBehaviour
    {
        [Tooltip("An initial list of unsafe zones behaviors where bodies will be dealt damage in.")]
        [SerializeField] private BaseZoneBehavior[] initialUnsafeZones;

        [Header("Team stuff")]
        [Tooltip("Used to control which teams to damage. If it's null, it damages ALL teams")]
        [SerializeField] private TeamFilter teamFilter;

        [Tooltip("If true, it damages all OTHER teams than the one specified.  If false, it damages the specified team.")]
        [SerializeField] private bool invertTeamFilter;

        [Header("Damage things")]
        [SerializeField] private DamageType damageType;

        [SerializeField] private DamageColorIndex damageColorIndex;

        [Range(0f, 1f)]
        [Tooltip("The fraction of combined health to deduct per second.  Note that damage is actually applied per tick, not per second.")]
        [SerializeField] private float healthFractionPerTick;

        [Tooltip("The coefficient to increase the damage by, for every tick they take inside the zones.")]
        [SerializeField] private float healthFractionRampCoefficientPerSecond;

        private Dictionary<CharacterBody, int> characterBodyToStacks = new Dictionary<CharacterBody, int>();
        private List<IZone> unsafeZones = new List<IZone>();
        private float damageTimer;
        private float dictionaryValidationTimer;
        private float tickPeriodSeconds;

        UnsafeZoneDamageManager unsafeZoneDamageManager;

        private void Start()
        {
            unsafeZoneDamageManager = new UnsafeZoneDamageManager(initialUnsafeZones);
        }

        public void AddUnsafeZone(IZone zone)
        {
            unsafeZones.Add(zone);
        }

        public void RemoveUnsafeZone(IZone zone)
        {
            unsafeZones.Remove(zone);
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active && unsafeZoneDamageManager != null)
            {
                unsafeZoneDamageManager.ServerFixedUpdate(Time.fixedDeltaTime);
            }
        }

        private void EvaluateTeam(TeamIndex teamIndex)
        {
            foreach (TeamComponent teamComponent in TeamComponent.GetTeamMembers(teamIndex))
            {
                CharacterBody body = teamComponent.body;
                bool isInBounds = false;

                using (List<IZone>.Enumerator unsafeZonesEnumerator = unsafeZones.GetEnumerator())
                {
                    //Go through every unsafe zone
                    while (unsafeZonesEnumerator.MoveNext())
                    {
                        //If its in bounds
                        if (unsafeZonesEnumerator.Current.IsInBounds(teamComponent.transform.position))
                        {
                            //Set true and break as we don't need to search anymore
                            isInBounds = true;
                            break;
                        }
                    }
                }

                //If its being tracked
                if (characterBodyToStacks.ContainsKey(body))
                {
                    //If its in bounds
                    if (isInBounds)
                    {
                        //Add a stack
                        characterBodyToStacks[body]++;
                        continue;
                    }
                    //Else remove it
                    characterBodyToStacks.Remove(body);
                    continue;
                }
                //If it didn't pass above, that means the body is not being tracked. Add it to the dictionary and add a stack.
                characterBodyToStacks.Add(body, 1);
            }
        }

        public IEnumerable<CharacterBody> GetAffectedBodies()
        {
            int teamDefLength = TeamCatalog.teamDefs.Length;
            if (teamFilter)
            {
                if (invertTeamFilter)
                {
                    for (int currentTeam = 0; currentTeam < teamDefLength; currentTeam++)
                    {
                        if ((TeamIndex)currentTeam != teamFilter.teamIndex && (TeamIndex)currentTeam != TeamIndex.None && (TeamIndex)currentTeam != TeamIndex.Neutral)
                        {
                            IEnumerable<CharacterBody> affectedBodiesOnInvertedTeamFilter = GetAffectedBodiesOnTeam((TeamIndex)currentTeam);
                            foreach (CharacterBody characterBody in affectedBodiesOnInvertedTeamFilter)
                            {
                                yield return characterBody;
                            }
                        }
                    }
                }
                else
                {
                    IEnumerable<CharacterBody> affectedBodiesOnTeamFilter = GetAffectedBodiesOnTeam(teamFilter.teamIndex);
                    foreach (CharacterBody characterBody in affectedBodiesOnTeamFilter)
                    {
                        yield return characterBody;
                    }
                }
            }
            else
            {
                for (int currentTeam = 0; currentTeam < teamDefLength; currentTeam++)
                {
                    IEnumerable<CharacterBody> affectedBodiesOnAllTeams = GetAffectedBodiesOnTeam((TeamIndex)currentTeam);
                    foreach (CharacterBody characterBody in affectedBodiesOnAllTeams)
                    {
                        yield return characterBody;
                    }
                }
            }
            yield break;
        }

        public IEnumerable<CharacterBody> GetAffectedBodiesOnTeam(TeamIndex teamIndex)
        {
            foreach (TeamComponent teamComponent in TeamComponent.GetTeamMembers(teamIndex))
            {
                using (List<IZone>.Enumerator enumerator2 = unsafeZones.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        if (enumerator2.Current.IsInBounds(teamComponent.transform.position))
                        {
                            yield return teamComponent.body;
                            break;
                        }
                    }
                }
            }
            yield break;
        }
    }
}
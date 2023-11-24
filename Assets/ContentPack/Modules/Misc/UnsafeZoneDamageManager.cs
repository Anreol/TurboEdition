using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Utils
{
    /// <summary>
    /// Custom version of <see cref="FogDamageController"/> but not a MonoBehavior
    /// </summary>
    public class UnsafeZoneDamageManager
    {
        /// <summary>
        /// Used to control which teams to damage. If it's null, it damages ALL teams
        /// </summary>
        public TeamFilter teamFilter;

        /// <summary>
        /// If true, it damages all OTHER teams than the one specified. If false, it damages the specified team.
        /// </summary>
        public bool invertTeamFilter;

        public DamageType damageType;

        public DamageColorIndex damageColorIndex;

        /// <summary>
        /// The fraction of combined health to deduct per second. Note that damage is actually applied per tick, not per second.
        /// </summary>
        public float healthFractionPerTick;

        [Tooltip("The coefficient to increase the damage by, for every tick they take inside the zones.")]
        [SerializeField] private float healthFractionRampCoefficientPerTick;

        private Dictionary<CharacterBody, int> characterBodyToStacks = new Dictionary<CharacterBody, int>();
        private List<IZone> unsafeZones = new List<IZone>();
        private float damageTimer;
        private float dictionaryValidationTimer;
        private float tickPeriodSeconds;

        public UnsafeZoneDamageManager() {}
        public UnsafeZoneDamageManager(IZone[] initialUnsafeZones)
        {
            foreach (IZone zone in initialUnsafeZones)
            {
                AddUnsafeZone(zone);
            }
        }

        public void AddUnsafeZone(IZone zone)
        {
            unsafeZones.Add(zone);
        }

        public void RemoveUnsafeZone(IZone zone)
        {
            unsafeZones.Remove(zone);
        }

        public void ServerFixedUpdate(float fixedDeltaTime)
        {
            if (NetworkServer.active)
            {
                damageTimer += fixedDeltaTime;
                dictionaryValidationTimer += fixedDeltaTime;
                if (dictionaryValidationTimer > 60f)
                {
                    dictionaryValidationTimer = 0f;
                    CharacterBody[] array = new CharacterBody[characterBodyToStacks.Keys.Count];
                    characterBodyToStacks.Keys.CopyTo(array, 0);
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (!array[i])
                        {
                            characterBodyToStacks.Remove(array[i]);
                        }
                    }
                }

                //WARNING: JANK CODE BELOW
                //HOPOO CANNOT INTO TEAMDEFS
                while (damageTimer > tickPeriodSeconds)
                {
                    damageTimer -= tickPeriodSeconds;
                    int teamDefLength = TeamCatalog.teamDefs.Length;

                    //check the team filter
                    if (teamFilter)
                    {
                        if (invertTeamFilter)
                        {
                            for (int teamIndex = 0; teamIndex < teamDefLength; teamIndex++)
                            {
                                if ((TeamIndex)teamIndex != teamFilter.teamIndex && (TeamIndex)teamIndex != TeamIndex.None && (TeamIndex)teamIndex != TeamIndex.Neutral)
                                {
                                    EvaluateTeam((TeamIndex)teamIndex);
                                }
                            }
                        }
                        else
                        {
                            EvaluateTeam(teamFilter.teamIndex);
                        }
                    }
                    else
                    {
                        //Else Check all teams
                        for (int teamIndex2 = 0; teamIndex2 < teamDefLength; teamIndex2++)
                        {
                            EvaluateTeam((TeamIndex)teamIndex2);
                        }
                    }
                    foreach (KeyValuePair<CharacterBody, int> keyValuePair in characterBodyToStacks)
                    {
                        CharacterBody characterBody = keyValuePair.Key;
                        if (characterBody && characterBody.transform && characterBody.healthComponent)
                        {
                            int stacks = keyValuePair.Value - 1;
                            float num2 = healthFractionPerTick * (1f + (float)stacks * healthFractionRampCoefficientPerTick * tickPeriodSeconds) * tickPeriodSeconds * characterBody.healthComponent.fullCombinedHealth;
                            if (num2 > 0f)
                            {
                                characterBody.healthComponent.TakeDamage(new DamageInfo
                                {
                                    damage = num2,
                                    position = characterBody.corePosition,
                                    damageType = damageType,
                                    damageColorIndex = damageColorIndex
                                });
                            }
                        }
                    }
                }
            }
        }

        public void EvaluateTeam(TeamIndex teamIndex)
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
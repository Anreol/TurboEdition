using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Utils
{
    /// <summary>
    /// Custom version of <see cref="FogDamageController"/> but not a MonoBehavior.
    /// </summary>
    public class UnsafeZoneDamageManager
    {
        /// <summary>
        /// Used to control which teams TO DAMAGE.
        /// </summary>
        public TeamMask teamMask;

        public DamageType damageType;

        public DamageColorIndex damageColorIndex;

        /// <summary>
        /// he period in seconds in between each tick
        /// </summary>
        public float tickPeriodSeconds;

        /// <summary>
        /// Should <see cref="healthFractionEachSecond"/> and <see cref="healthFractionRampCoefficientPerSecond"/> be used.
        /// </summary>
        public bool dealFractionDamage = false;

        /// <summary>
        /// The fraction of <see cref="HealthComponent.fullCombinedHealth"/> to deduct per tick. Note that damage is actually dealt each tick.
        /// </summary>
        public float healthFractionEachSecond;

        /// <summary>
        /// The coefficient to increase the damage by, for every tick they take inside the zones.
        /// </summary>
        public float healthFractionRampCoefficientPerSecond;

        /// <summary>
        /// Flat equivalent of <see cref="healthFractionEachSecond"/>
        /// </summary>
        public float flatDamageEachSecond;

        /// <summary>
        /// Flat equivalent of <see cref="healthFractionRampCoefficientPerSecond"/>
        /// </summary>
        public float flatDamageRampPerSecond;

        /// <summary>
        /// Possible attacker, can be null. The <see cref="CharacterBody"/> would go here
        /// </summary>
        public GameObject attacker;

        public List<IZone> UnsafeZones { get => unsafeZones; }

        public event Action<DamageInfo, HealthComponent> OnDamageDealtAnywhere;

        private Dictionary<CharacterBody, int> characterBodyToStacks = new Dictionary<CharacterBody, int>();
        private List<IZone> unsafeZones = new List<IZone>();
        private float damageTimer;
        private float dictionaryValidationTimer;

        public UnsafeZoneDamageManager()
        { }

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

                //I have no idea why this exists
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
                    //check the team mask
                    for (int teamIndex = 0; teamIndex < teamDefLength; teamIndex++)
                    {
                        if (teamMask.HasTeam((TeamIndex)teamIndex))
                        {
                            EvaluateTeam((TeamIndex)teamIndex);
                        }
                    }

                    foreach (KeyValuePair<CharacterBody, int> keyValuePair in characterBodyToStacks)
                    {
                        CharacterBody characterBody = keyValuePair.Key;
                        if (characterBody && characterBody.transform && characterBody.healthComponent)
                        {
                            int stacks = keyValuePair.Value - 1;
                            float damageToDeal = CalcDamage(stacks, characterBody);
                            if (damageToDeal > 0f)
                            {
                                DamageInfo damageInfo = new DamageInfo
                                {
                                    damage = damageToDeal,
                                    position = characterBody.corePosition,
                                    damageType = damageType,
                                    damageColorIndex = damageColorIndex,
                                    
                                    attacker = attacker
                                };
                                OnDamageDealtAnywhere?.Invoke(damageInfo, characterBody.healthComponent);
                                characterBody.healthComponent.TakeDamage(damageInfo);
                            }
                        }
                    }
                }
            }
        }

        private float CalcDamage(int stacks, CharacterBody victim)
        {
            if (dealFractionDamage)
            {
                return healthFractionEachSecond * (1f + stacks * healthFractionRampCoefficientPerSecond * tickPeriodSeconds) * tickPeriodSeconds * victim.healthComponent.fullCombinedHealth;
            }
            return ((flatDamageEachSecond + (flatDamageRampPerSecond * stacks))) * tickPeriodSeconds;
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
                if (isInBounds)
                {
                    //If it didn't pass above, that means the body is not being tracked. Add it to the dictionary and add a stack.
                    characterBodyToStacks.Add(body, 1);
                }
            }
        }

        public IEnumerable<CharacterBody> GetAffectedBodies()
        {
            int teamDefLength = TeamCatalog.teamDefs.Length;

            for (int currentTeam = 0; currentTeam < teamDefLength; currentTeam++)
            {
                if (teamMask.HasTeam((TeamIndex)currentTeam))
                {
                    IEnumerable<CharacterBody> affectedBodies = GetAffectedBodiesOnTeam((TeamIndex)currentTeam);
                    foreach (CharacterBody characterBody in affectedBodies)
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
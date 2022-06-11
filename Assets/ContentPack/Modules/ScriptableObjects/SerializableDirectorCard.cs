using RoR2;
using UnityEngine;

namespace TurboEdition.ScriptableObjects
{
    [CreateAssetMenu(menuName = "TurboEdition/SpawnCards/SerializableDirectorCard")]
    internal class SerializableDirectorCard : ScriptableObject
    {
        public SpawnCard spawnCard;

        [Tooltip("Name of scenes to spawn in.")]
        public string[] sceneNamesToBeUsedIn;

        [Tooltip("The enemy category or interactable category to add itself to.")]
        public string categoryName;

        [Tooltip("For reference, Chance Shrines has a weight of 4.")]
        public int selectionWeight;

        [Tooltip("Only used by monster spawns, how far should it spawn from players.")]
        public DirectorCore.MonsterSpawnDistance spawnDistance;

        [Tooltip("Should it prevent spawning under terrain.")]
        public bool preventOverhead;

        [Tooltip("The minimum (inclusive) number of stages COMPLETED (not reached) before this card becomes available.")]
        public int minimumStageCompletions;

        public UnlockableDef requiredUnlockableDef;
        public UnlockableDef forbiddenUnlockableDef;

        public DirectorCard CreateDirectorCard()
        {
            return new DirectorCard()
            {
                spawnCard = spawnCard,
                selectionWeight = selectionWeight,
                spawnDistance = spawnDistance,
                preventOverhead = preventOverhead,
                minimumStageCompletions = minimumStageCompletions,
                requiredUnlockableDef = requiredUnlockableDef,
                forbiddenUnlockableDef = forbiddenUnlockableDef
            };
        }
    }
}
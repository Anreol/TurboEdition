using RoR2;
using UnityEngine;
using static RoR2.Artifacts.BombArtifactManager;
using static TurboEdition.Utils.ItemHelpers;

namespace TurboEdition.Artifacts
{
    //This wouldnt be possible (or wouldve taken me a lot of time) if Chen didn't show me their own spite artifact mods and how to get BombRequests
    //thank
    internal class Bomb2ArtifactManager : DelayerManager<BombRequest>
    {
        public override float releaseAt => 1f;
        public override int entriesToRelease => 30;
        public override float interval => 0.1f;

        //set the amount of items per interval to release to 1
        //set interval to something pretty short, maybe 0.5
        //the getters and setters dont seem to work here

        //Copied and pasted from the game's own queue method, since i cant just add my shit to BombArtifactManager
        //It still uses the same properties, though
        public override void ProcessQueue(BombRequest bombRequest)
        {
            Ray ray = new Ray(bombRequest.raycastOrigin + new Vector3(0f, maxBombStepUpDistance, 0f), Vector3.down);
            float maxDistance = maxBombStepUpDistance + maxBombFallDistance;
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                SpawnBomb(bombRequest, raycastHit.point.y);
            }
        }

        //See: OnServerCharacterDeath
        //We skip the whole team check and other garbo since we do that somewhere else and this just spawns da bombs
        /// <summary>
        /// Spawn spite bombs the same way the game does.
        /// </summary>
        /// <param name="body"></param>
        public void SpawnBombFromBody(CharacterBody body, int? bombOverride = null)
        {
            Vector3 corePosition = body.corePosition;
            int num = bombOverride ?? Mathf.Min(maxBombCount, Mathf.CeilToInt(body.bestFitRadius * extraBombPerRadius * cvSpiteBombCoefficient.value));
            for (int i = 0; i < num; i++)
            {
                Vector3 b = UnityEngine.Random.insideUnitSphere * (bombSpawnBaseRadius + body.bestFitRadius * bombSpawnRadiusCoefficient);
                BombRequest bombRequest = new BombRequest
                {
                    spawnPosition = corePosition,
                    raycastOrigin = corePosition + b,
                    bombBaseDamage = body.damage * bombDamageCoefficient,
                    attacker = body.gameObject,
                    teamIndex = body.teamComponent.teamIndex,
                    velocityY = UnityEngine.Random.Range(5f, 25f)
                };
                //We add it to our own custom queue (thats why of the DelayerManager)
                AddEntry(bombRequest);
            }
        }

        /// <summary>
        /// Spawn spite bombs the same way the game does, except the bomb's team is ALWAYS monster.
        /// </summary>
        /// <param name="body"></param>
        public void SpawnMonsterBombFromBody(CharacterBody body, int? bombOverride = null)
        {
            Vector3 corePosition = body.corePosition;
            int num = bombOverride ?? Mathf.Min(maxBombCount, Mathf.CeilToInt(body.bestFitRadius * extraBombPerRadius * cvSpiteBombCoefficient.value));
            for (int i = 0; i < num; i++)
            {
                Vector3 b = UnityEngine.Random.insideUnitSphere * (bombSpawnBaseRadius + body.bestFitRadius * bombSpawnRadiusCoefficient);
                BombRequest bombRequest = new BombRequest
                {
                    spawnPosition = corePosition,
                    raycastOrigin = corePosition + b,
                    bombBaseDamage = body.damage * bombDamageCoefficient,
                    attacker = body.gameObject,
                    teamIndex = TeamIndex.Monster,
                    velocityY = UnityEngine.Random.Range(5f, 25f)
                };
                //We add it to our own custom queue (thats why of the DelayerManager)
                AddEntry(bombRequest);
            }
        }
    }
}
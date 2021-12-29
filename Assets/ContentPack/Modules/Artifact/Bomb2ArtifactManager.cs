using RoR2;
using RoR2.Artifacts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Artifacts
{
    internal class Bomb2ArtifactManager
    {
        public static ArtifactDef artifact = Assets.mainAssetBundle.LoadAsset<ArtifactDef>("Spite2Artifact");
        private static int maxBombCount = BombArtifactManager.maxBombCount;
        private static float extraBombPerRadius = BombArtifactManager.extraBombPerRadius / 1.25f;
        private static float bombSpawnBaseRadius = BombArtifactManager.bombSpawnBaseRadius;
        private static float bombSpawnRadiusCoefficient = BombArtifactManager.bombSpawnRadiusCoefficient / 1.25f;
        private static float bombSpawnFromDamageChance = 25f;
        private static readonly Queue<BombArtifactManager.BombRequest> bombRequestQueue = new Queue<BombArtifactManager.BombRequest>();

        [SystemInitializer(new Type[]
        {
            typeof(ArtifactCatalog)
        })]
        private static void Init()
        {
            RunArtifactManager.onArtifactEnabledGlobal += RunArtifactManager_onArtifactEnabledGlobal;
            RunArtifactManager.onArtifactDisabledGlobal += RunArtifactManager_onArtifactDisabledGlobal;
        }
        private static void RunArtifactManager_onArtifactDisabledGlobal([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            if (artifactDef != artifact)
                return;

            Bomb2ArtifactManager.bombRequestQueue.Clear();
            RoR2Application.onFixedUpdate -= ProcessBombQueue;
            GlobalEventManager.onCharacterDeathGlobal -= OnServerCharacterDeath;
            GlobalEventManager.onServerDamageDealt -= OnServerCharacterDeath;
        }

        private static void RunArtifactManager_onArtifactEnabledGlobal([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            if (!NetworkServer.active) //uNet Weaver doesnt like [Server] Tags on something that isnt a network behavior
                return;
            if (artifactDef != artifact)
                return;

            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            GlobalEventManager.onCharacterDeathGlobal += OnServerCharacterDeath;
            RoR2Application.onFixedUpdate += ProcessBombQueue;
        }

        private static void OnServerDamageDealt(DamageReport obj)
        {
            if (!TeamManager.IsTeamEnemy(TeamIndex.Player, obj.victimTeamIndex) || obj.victimTeamIndex == TeamIndex.Player) //If it's an ally to player or player
                return;
            if (Util.CheckRoll((obj.damageDealt * 2) / (obj.combinedHealthBeforeDamage / 10), obj.damageInfo.procCoefficient)) //*fucks ur proc chance*
            {
                ProcessDamage(obj);
            }
        }

        private static void OnServerCharacterDeath(DamageReport obj)
        {
            if (TeamManager.IsTeamEnemy(TeamIndex.Player, obj.victimTeamIndex) && !RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.Bomb)) //Don't duplicate Spite effect unless Spite is enabled
                return;
            ProcessDamage(obj);
        }

        public static void ProcessDamage(DamageReport obj)
        {
            CharacterBody victimBody = obj.victimBody;
            Vector3 corePosition = victimBody.corePosition;
            int num = Mathf.Min(Bomb2ArtifactManager.maxBombCount, Mathf.CeilToInt(victimBody.bestFitRadius * Bomb2ArtifactManager.extraBombPerRadius * BombArtifactManager.cvSpiteBombCoefficient.value));
            for (int i = 0; i < num; i++)
            {
                Vector3 b = UnityEngine.Random.insideUnitSphere * (Bomb2ArtifactManager.bombSpawnBaseRadius + victimBody.bestFitRadius * Bomb2ArtifactManager.bombSpawnRadiusCoefficient);
                BombArtifactManager.BombRequest item = new BombArtifactManager.BombRequest
                {
                    spawnPosition = corePosition,
                    raycastOrigin = corePosition + b,
                    bombBaseDamage = victimBody.damage * BombArtifactManager.bombDamageCoefficient,
                    attacker = victimBody.gameObject,
                    teamIndex = obj.victimTeamIndex,
                    velocityY = UnityEngine.Random.Range(5f, 25f)
                };
                Bomb2ArtifactManager.bombRequestQueue.Enqueue(item);
            }
        }
        private static void ProcessBombQueue()
        {
            if (Bomb2ArtifactManager.bombRequestQueue.Count > 0)
            {
                BombArtifactManager.BombRequest bombRequest = Bomb2ArtifactManager.bombRequestQueue.Dequeue();
                Ray ray = new Ray(bombRequest.raycastOrigin + new Vector3(0f, BombArtifactManager.maxBombStepUpDistance, 0f), Vector3.down);
                float maxDistance = BombArtifactManager.maxBombStepUpDistance + BombArtifactManager.maxBombFallDistance;
                RaycastHit raycastHit;
                if (Physics.Raycast(ray, out raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    BombArtifactManager.SpawnBomb(bombRequest, raycastHit.point.y);
                }
            }
        }
    }
}
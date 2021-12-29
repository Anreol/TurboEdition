using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace TurboEdition.Artifacts
{
    class WormsArtifactManager
    {
        public static ArtifactDef artifact = Assets.mainAssetBundle.LoadAsset<ArtifactDef>("WormsArtifact");
        public static GameObject directorObject = Assets.mainAssetBundle.LoadAsset<GameObject>("WormDirector");
        private static DirectorCardCategorySelection dccsWormArtifact = Assets.mainAssetBundle.LoadAsset<DirectorCardCategorySelection>("dccsWormArtifact");
        private static GameObject directorInstance = null;

        [SystemInitializer(new Type[]
        {
            typeof(ArtifactCatalog)
        })]
        private static void Init()
        {
            RunArtifactManager.onArtifactEnabledGlobal += RunArtifactManager_onArtifactEnabledGlobal;
            RunArtifactManager.onArtifactDisabledGlobal += RunArtifactManager_onArtifactDisabledGlobal;
            SetupCards(ref dccsWormArtifact);
        }
        private static void RunArtifactManager_onArtifactDisabledGlobal([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            if (artifactDef != artifact)
                return;

            Stage.onServerStageComplete -= onServerStageCompleted;
            Stage.onServerStageBegin -= onServerStageBegin;
        }
        private static void RunArtifactManager_onArtifactEnabledGlobal([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            if (!NetworkServer.active) //uNet Weaver doesnt like [Server] Tags on something that isnt a network behavior
                return;
            if (artifactDef != artifact)
                return;
            Stage.onServerStageBegin += onServerStageBegin;
            Stage.onServerStageComplete += onServerStageCompleted;
        }

        private static void onServerStageCompleted(Stage obj)
        {
            if (directorInstance)
            {
                UnityEngine.Object.Destroy(directorInstance);
                NetworkServer.Destroy(directorInstance);
            }
        }

        private static void onServerStageBegin(Stage obj)
        {
            if (SceneCatalog.mostRecentSceneDef.sceneType != SceneType.Stage)
                return;
            if (!directorInstance)
            {
                directorInstance = UnityEngine.Object.Instantiate(directorObject);
                directorInstance.GetComponent<CombatDirector>().monsterCards = dccsWormArtifact;
                if (directorInstance)
                {
                    NetworkServer.Spawn(directorInstance);
                    directorInstance.GetComponent<CombatDirector>().onSpawnedServer.AddListener(new UnityAction<GameObject>(ModifySpawnedMasters));
                }
            }
        }

        private static void ModifySpawnedMasters(GameObject gameObject)
        {
            if (!NetworkServer.active) return;
            CharacterBody cb = gameObject.GetComponent<CharacterMaster>().GetBody();
            if (cb)
            {
                DeathRewards  deathRewards = cb.GetComponent<DeathRewards>();
                if (deathRewards)
                {
                    deathRewards.spawnValue = (int)Mathf.Max(1f, (float)deathRewards.spawnValue / 4f);
                    deathRewards.expReward = (uint)Mathf.Ceil(deathRewards.expReward / 4f);
                    deathRewards.goldReward = (uint)Mathf.Ceil(deathRewards.goldReward / 4f);
                }
            }
        }
        private static void SetupCards(ref DirectorCardCategorySelection dccsWormArtifact)
        {
            List<CharacterSpawnCard> spawnCards = new List<CharacterSpawnCard>();
            spawnCards.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawncards/cscMagmaWorm"));
            spawnCards.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawncards/cscElectricWorm"));
            foreach (CharacterSpawnCard item in spawnCards)
            {
                item.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<CharacterBody>().baseMaxHealth /= 3;
                //DeathRewards death = item.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<DeathRewards>();
                WormBodyPositionsDriver wormBodyPositionsDriver = item.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<WormBodyPositionsDriver>();
                /*if (death != null)
                {
                    death.spawnValue = (int)Mathf.Max(1f, (float)death.spawnValue / 3f);
                    death.expReward = (uint)Mathf.Ceil(death.expReward / 3f);
                    death.goldReward = (uint)Mathf.Ceil(death.goldReward / 3f);
                }*/
                if (wormBodyPositionsDriver != null)
                {
                    wormBodyPositionsDriver.allowShoving = true;
                    wormBodyPositionsDriver.wormForceCoefficientAboveGround += 0.5f;
                    wormBodyPositionsDriver.maxBreachSpeed *= 2f;
                    wormBodyPositionsDriver.ySpringConstant *= 2.5f;
                }
            }
            List<CharacterSpawnCard> eliteCards = spawnCards;
            foreach (CharacterSpawnCard item in eliteCards)
            {
                item.name += "Elite";
                item.directorCreditCost /= 2;
                item.eliteRules = SpawnCard.EliteRules.ArtifactOnly;
                item.noElites = false;
            }
            spawnCards.AddRange(eliteCards);

            for (int i = 0; i < dccsWormArtifact.categories[0].cards.Length; i++)
            {
                dccsWormArtifact.categories[0].cards[i].spawnCard = spawnCards[i];
            }
        }
    }
}

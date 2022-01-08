using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace TurboEdition.Artifacts
{
    internal class WormsArtifactManager
    {
        public static ArtifactDef artifact = Assets.mainAssetBundle.LoadAsset<ArtifactDef>("WormsArtifact");
        public static GameObject directorObject = Assets.mainAssetBundle.LoadAsset<GameObject>("WormDirector");

        private static DirectorCardCategorySelection dccsWormArtifact = Assets.mainAssetBundle.LoadAsset<DirectorCardCategorySelection>("dccsWormArtifact");
        private static DirectorCardCategorySelection dccsWormNonEliteArtifact = Assets.mainAssetBundle.LoadAsset<DirectorCardCategorySelection>("dccsWormNonEliteArtifact");

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
            SetupNonEliteCards(ref dccsWormNonEliteArtifact);
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
                directorInstance.GetComponent<CombatDirector>().enabled = false;
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
                if (Misc.RulebookExtras.runWormEliteHonor) //Return true if its off...?
                {
                    directorInstance.GetComponent<CombatDirector>().monsterCards = dccsWormNonEliteArtifact;
                }
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
                DeathRewards deathRewards = cb.GetComponent<DeathRewards>();
                if (deathRewards)
                {
                    deathRewards.spawnValue = (int)Mathf.Max(1f, (float)deathRewards.spawnValue / 8f);
                    deathRewards.expReward = (uint)Mathf.Ceil(deathRewards.expReward / 8f);
                    deathRewards.goldReward = (uint)Mathf.Ceil(deathRewards.goldReward / 8f);
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
                WormBodyPositionsDriver wormBodyPositionsDriver = item.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<WormBodyPositionsDriver>();
                if (wormBodyPositionsDriver != null)
                {
                    wormBodyPositionsDriver.allowShoving = true;
                    wormBodyPositionsDriver.wormForceCoefficientAboveGround += 0.5f;
                    wormBodyPositionsDriver.maxBreachSpeed *= 2f;
                    wormBodyPositionsDriver.ySpringConstant *= 2.5f;
                }
                WormBodyPositions2[] wormBodyPositions2s = item.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponents<WormBodyPositions2>();
                foreach (WormBodyPositions2 component in wormBodyPositions2s)
                {
                    component.meatballCount /= 2;
                    component.impactCooldownDuration += 1f;
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

        private static void SetupNonEliteCards(ref DirectorCardCategorySelection dccsWormArtifact)
        {
            List<CharacterSpawnCard> spawnCards = new List<CharacterSpawnCard>();
            spawnCards.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawncards/cscMagmaWorm"));
            spawnCards.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawncards/cscElectricWorm"));
            foreach (CharacterSpawnCard item in spawnCards)
            {
                item.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<CharacterBody>().baseMaxHealth /= 3;
                WormBodyPositionsDriver wormBodyPositionsDriver = item.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<WormBodyPositionsDriver>();
                if (wormBodyPositionsDriver != null)
                {
                    wormBodyPositionsDriver.allowShoving = true;
                    wormBodyPositionsDriver.wormForceCoefficientAboveGround += 0.5f;
                    wormBodyPositionsDriver.maxBreachSpeed *= 2f;
                    wormBodyPositionsDriver.ySpringConstant *= 2.5f;
                }
                WormBodyPositions2[] wormBodyPositions2s = item.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponents<WormBodyPositions2>();
                foreach (WormBodyPositions2 component in wormBodyPositions2s)
                {
                    component.meatballCount /= 2;
                    component.impactCooldownDuration += 1f;
                }
            }

            for (int i = 0; i < dccsWormArtifact.categories[0].cards.Length; i++)
            {
                dccsWormArtifact.categories[0].cards[i].spawnCard = spawnCards[i];
            }
        }

        [ConCommand(commandName = "te_worminfo", flags = ConVarFlags.None, helpText = "Dump information about the worms artifact.")]
        private static void CCTEWormInfo(ConCommandArgs args)
        {
            if (!Run.instance)
            {
                Debug.Log("Cannot be used outside of a run.");
                return;
            }
            List<string> list = new List<string>();
            string item = string.Format("  {{ArtifactActive={0} DirectorActive={1} }}", RunArtifactManager.instance.IsArtifactEnabled(artifact), directorInstance.activeSelf);
            list.Add(item);

            string item2 = string.Format("  {{wormEliteHonorRule={0} wormEliteHonorRule.displayToken={1} wormEliteHonorRule.GetRuleChoice().extraData={2}}}", RuleCatalog.FindRuleDef("Misc.WormArtifactEliteHonor"), RuleCatalog.FindRuleDef("Misc.WormArtifactEliteHonor").displayToken, Misc.RulebookExtras.runWormEliteHonor);
            list.Add(item2);

            Debug.Log(string.Join("\n", list));
        }
    }
}
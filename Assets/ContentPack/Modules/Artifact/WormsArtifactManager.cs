using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        /// <summary>
        /// The current instance of the director object. Only gets instantiated in <see cref="onServerStageBegin(Stage)"/>, meaning its server only.
        /// </summary>
        private static GameObject directorInstance = null;

        [SystemInitializer(new Type[]
        {
            typeof(ArtifactCatalog),
            typeof(ItemCatalog),
        })]
        private static void Init()
        {
            RunArtifactManager.onArtifactEnabledGlobal += RunArtifactManager_onArtifactEnabledGlobal;
            RunArtifactManager.onArtifactDisabledGlobal += RunArtifactManager_onArtifactDisabledGlobal;

            List<CharacterSpawnCard> spawnCards = SetupBaseCards();
            for (int i = 0; i < dccsWormNonEliteArtifact.categories[0].cards.Length; i++)
            {
                dccsWormNonEliteArtifact.categories[0].cards[i].spawnCard = spawnCards[i];
            }
            List<CharacterSpawnCard> eliteCards = SetupEliteCards(spawnCards);
            eliteCards.AddRange(spawnCards);
            for (int i = dccsWormArtifact.categories[0].cards.Length - 1; i >= 0; i--)
            {
                dccsWormArtifact.categories[0].cards[i].spawnCard = eliteCards[i];
            }
        }

        private static void RunArtifactManager_onArtifactDisabledGlobal([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            if (artifactDef != artifact)
                return;

            Stage.onServerStageComplete -= onServerStageCompleted;
            Stage.onServerStageBegin -= onServerStageBegin;

            if (directorInstance)
            {
                directorInstance.GetComponent<CombatDirector>().enabled = false;
                UnityEngine.Object.Destroy(directorInstance);
                NetworkServer.Destroy(directorInstance);
            }
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
                //directorInstance.GetComponent<CombatDirector>().monsterCards = dccsWormArtifact;
                if (directorInstance)
                {
                    switch (Utils.RulebookExtras.runWormEliteHonor) //nintendo swix
                    {
                        case true:
                            directorInstance.GetComponent<CombatDirector>().monsterCards = dccsWormArtifact;
                            break;

                        case false:
                            directorInstance.GetComponent<CombatDirector>().monsterCards = dccsWormNonEliteArtifact;
                            break;
                    }
                    directorInstance.GetComponent<CombatDirector>().onSpawnedServer.AddListener(new UnityAction<GameObject>(ModifySpawnedMasters));
                    NetworkServer.Spawn(directorInstance);
                }
            }
        }

        private static void ModifySpawnedMasters(GameObject gameObject)
        {
            if (!NetworkServer.active) return;
            CharacterBody cb = gameObject.GetComponent<CharacterMaster>().GetBody();
            if (cb)
            {
                if (cb.isElite && (!Utils.RulebookExtras.runWormEliteHonor && CombatDirector.IsEliteOnlyArtifactActive())) //They shouldnt be elite... this is a failsafe to ensure they aren't fucking elite
                {
                    UnEliteSpawn(cb, true);
                }
                DeathRewards deathRewards = cb.GetComponent<DeathRewards>();
                if (deathRewards)
                {
                    deathRewards.spawnValue = (int)Mathf.Max(1f, (float)deathRewards.spawnValue / 8f);
                    deathRewards.expReward = (uint)Mathf.CeilToInt(deathRewards.expReward / 8f);
                    deathRewards.goldReward = (uint)Mathf.CeilToInt(deathRewards.goldReward / 8f);
                }

                cb.baseMaxHealth /= 4f;
                cb.levelMaxHealth *= 0.5f;
                cb.baseArmor = 0;
                cb.levelArmor = 0;
                cb.baseDamage *= 0.5f;

                //No death events.
                cb.healthComponent.globalDeathEventChanceCoefficient = 0f;

                //Wacky things that I don't even know what they do
                WormBodyPositionsDriver wormBodyPositionsDriver = cb.GetComponent<WormBodyPositionsDriver>();
                if (wormBodyPositionsDriver != null)
                {
                    //Shoving is... something weird. It's not player shoving, it lets worms fly up and down, akin to ror1. This behavior seems to be used in the beta, but got removed.
                    wormBodyPositionsDriver.allowShoving = true;
                    wormBodyPositionsDriver.yShoveForce = wormBodyPositionsDriver.yShoveForce == 0 ? wormBodyPositionsDriver.yShoveForce = 50f : wormBodyPositionsDriver.yShoveForce *= 2f;
                    wormBodyPositionsDriver.wormForceCoefficientAboveGround *= 2f;
                    wormBodyPositionsDriver.maxTurnSpeed *= 0.5f; //Yes, halve.
                    wormBodyPositionsDriver.maxBreachSpeed *= 2f;
                    wormBodyPositionsDriver.ySpringConstant = wormBodyPositionsDriver.ySpringConstant == 0 ? wormBodyPositionsDriver.ySpringConstant = 300f : wormBodyPositionsDriver.ySpringConstant *= 3f; //Just fuck me up
                }
                WormBodyPositions2[] wormBodyPositions2s = cb.GetComponents<WormBodyPositions2>();
                foreach (WormBodyPositions2 component in wormBodyPositions2s)
                {
                    component.speedMultiplier *= 2f; //Default seems to be 2. Just crank it up.
                    component.meatballCount /= 2;
                    component.impactCooldownDuration += 1f;
                }
            }
        }

        private static List<CharacterSpawnCard> SetupBaseCards()
        {
            List<CharacterSpawnCard> spawnCards = new List<CharacterSpawnCard>
            {
                UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/MagmaWorm/cscMagmaWorm.asset").WaitForCompletion()),
                UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/ElectricWorm/cscElectricWorm.asset").WaitForCompletion())
            };

            foreach (CharacterSpawnCard item in spawnCards)
            {
                item.name += "Artifact";
            }
            return spawnCards;
        }

        private static List<CharacterSpawnCard> SetupEliteCards(List<CharacterSpawnCard> baseCards)
        {
            List<CharacterSpawnCard> eliteCards = new List<CharacterSpawnCard>();
            for (int i = 0; i < baseCards.Count; i++)
            {
                eliteCards.Add(UnityEngine.Object.Instantiate(baseCards[i]));
            }
            foreach (CharacterSpawnCard item in eliteCards)
            {
                item.name += "Elite";
                item.directorCreditCost /= 2; //Too lazy to lookup actual elite cards cost multiplication
                item.eliteRules = SpawnCard.EliteRules.ArtifactOnly;
                item.noElites = false;
            }
            return eliteCards;
        }

        /// <summary>
        /// Some fucky wucky stuff happens with combat directors and their available elite tiers.
        /// I cannot think of any better way than this right now.
        /// Affects the Worm Artifact director, not any other director.
        /// </summary>
        /// <param name="characterBody"></param>
        private static void UnEliteSpawn(CharacterBody characterBody, bool resetDirectorEliteness)
        {
            //In this entire process we assume the body has a inventory, hopefully this wont ever break.

            bool doneCheckingEliteEquipment = false;
            CombatDirector.EliteTierDef eliteTierDef = null;
            while (characterBody.inventory.currentEquipmentIndex != EquipmentIndex.None || !doneCheckingEliteEquipment)
            {
                foreach (CombatDirector.EliteTierDef item in CombatDirector.eliteTiers)
                {
                    if (item.eliteTypes.Length > 0) //Do we EVEN need to check this??
                    {
                        for (int i = 0; i < item.eliteTypes.Length; i++) //do NOT use a foreach here, do NOT.
                        {
                            if (item.eliteTypes[i] && item.eliteTypes[i].eliteEquipmentDef)
                            {
                                if (characterBody.inventory.GetEquipment((uint)characterBody.inventory.activeEquipmentSlot).equipmentDef == item.eliteTypes[i].eliteEquipmentDef) //Remember to check if its null!
                                {
                                    eliteTierDef = item;
                                    characterBody.inventory.SetEquipmentIndex(EquipmentIndex.None);
                                    doneCheckingEliteEquipment = true; //Just in case, idk, want to break free earlier.
                                }
                            }
                        }
                    }
                }
                doneCheckingEliteEquipment = true;
            }
            if (eliteTierDef == null) //Enemy was not a combat director elite, we do not want to do anything else as it might have items or something else given or modified by a third-party
                return;
            float boostHP = eliteTierDef.eliteTypes[0].healthBoostCoefficient;
            if (characterBody.isChampion) //or master is boss?
            {
                boostHP *= Mathf.Pow((float)Run.instance.livingPlayerCount, 1f);
            }
            characterBody.inventory.RemoveItem(RoR2Content.Items.BoostHp.itemIndex, Mathf.RoundToInt((boostHP - 1f) * 10f));
            characterBody.inventory.RemoveItem(RoR2Content.Items.BoostDamage.itemIndex, Mathf.RoundToInt((eliteTierDef.eliteTypes[0].damageBoostCoefficient - 1f) * 10f));

            if (!directorInstance)
                return;
            CombatDirector combatDirector = directorInstance.GetComponent<CombatDirector>();
            if (!combatDirector)
                return; //as we do not have the neccesary stuff for the calculations.

            DeathRewards dr = characterBody.GetComponent<DeathRewards>();
            if (dr)
            {
                //Hopefully the current monster cost is still on the wave as this spawn. Should be as this is almost instantaneous right after spawning, while changing monster cards do have delay.
                int monsterCostThatMayOrMayNotBeElite = (int)(combatDirector.currentMonsterCard.cost * eliteTierDef.costMultiplier);
                int futureExpReward = (int)(dr.expReward - (monsterCostThatMayOrMayNotBeElite * combatDirector.expRewardCoefficient * Run.instance.compensatedDifficultyCoefficient));
                int futureGoldReward = (int)(dr.goldReward - (monsterCostThatMayOrMayNotBeElite * combatDirector.expRewardCoefficient * 2f * Run.instance.compensatedDifficultyCoefficient));

                dr.spawnValue -= (int)Mathf.Max(1f, monsterCostThatMayOrMayNotBeElite * combatDirector.expRewardCoefficient);
                dr.expReward = futureExpReward < 0 ? dr.expReward : (uint)futureExpReward;
                dr.goldReward = futureGoldReward < 0 ? dr.goldReward : (uint)futureGoldReward;
            }

            //Refund spent credits
            combatDirector.monsterCredit += (combatDirector.currentMonsterCard.cost * eliteTierDef.costMultiplier) - combatDirector.currentMonsterCard.cost;

            //Should clean up the current monster wave and make sure that the remaining spawns wont be elite.
            if (resetDirectorEliteness)
            {
                combatDirector.currentActiveEliteTier = CombatDirector.eliteTiers[0];
                combatDirector.currentActiveEliteDef = CombatDirector.eliteTiers[0].eliteTypes[0]; //or alternatively known as NULL, as that tier is empty.
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
            string item = string.Format("{{ArtifactActive={0} DirectorActive={1}}}", RunArtifactManager.instance.IsArtifactEnabled(artifact), directorInstance.activeSelf);
            list.Add(item);

            string item2 = string.Format("{{wormEliteHonorRule={0} wormEliteHonorRule.displayToken={1} wormEliteHonorRule.GetRuleChoice().extraData={2}}}\n", RuleCatalog.FindRuleDef("Misc.WormArtifactEliteHonor"), RuleCatalog.FindRuleDef("Misc.WormArtifactEliteHonor").displayToken, Utils.RulebookExtras.runWormEliteHonor);
            list.Add(item2);
            for (int i = 0; i < dccsWormArtifact.categories[0].cards.Length; i++)
            {
                string item3 = string.Format("{{dccsWormArtifact={0} index={1} spawnCard={2} noElites={3}}}", dccsWormArtifact, i, dccsWormArtifact.categories[0].cards[i].spawnCard, ((CharacterSpawnCard)dccsWormArtifact.categories[0].cards[i].spawnCard).noElites);
                list.Add(item3);
            }
            for (int x = 0; x < dccsWormNonEliteArtifact.categories[0].cards.Length; x++)
            {
                string item4 = string.Format("{{dccsWormNonEliteArtifact={0} index={1} spawnCard={2} noElites={3}}}", dccsWormNonEliteArtifact, x, dccsWormNonEliteArtifact.categories[0].cards[x].spawnCard, ((CharacterSpawnCard)dccsWormNonEliteArtifact.categories[0].cards[x].spawnCard).noElites);
                list.Add(item4);
            }
            Debug.Log(string.Join("\n", list));
        }
    }
}
using RoR2;
using RoR2.Stats;
using System;
using TurboEdition.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(CombatDirector))]
    [RequireComponent(typeof(CombatSquad))]
    [RequireComponent(typeof(PurchaseInteraction))]
    internal class ShrineTeleporterOverchargerBehavior : NetworkBehaviour
    {
        public static event Action<ShrineTeleporterOverchargerBehavior> onCombatSquadDefeatedGlobal;

        private PurchaseInteraction purchaseInteraction;
        private CombatDirector shrineCombatDirector;
        private DirectorCard chosenDirectorCardToSpawn;

        /// <summary>
        /// Whenever it's waiting for it to be reactivated. Handled by the server only.
        /// </summary>
        private bool waitingForRefresh = false;

        private float refreshTimer;
        private int purchaseCount;

        private string[] bodyNamesSpawned = new string[0];

        /// <summary>
        /// Amount of accumulated rewards from sequential activations without finishing the bosses. Handled by the server only.
        /// </summary>
        private int accumulatedRewards;

        [Tooltip("Only used for HUD Health bar and title.")]
        public BossGroup bossGroup;

        [Header("Rewards")]
        [Tooltip("The drop table to use for the rewards")]
        [SerializeField]
        public PickupDropTable dropTable;

        [Tooltip("Use this tier to get a pickup index for the reward.  The droplet's visuals will correspond to this.  Set to Assigned at Runtime so it calculates off the most valuable option")]
        [SerializeField]
        protected ItemTier rewardDisplayTier;

        [Tooltip("The number of options to display when the player interacts with the reward pickup.")]
        [SerializeField]
        private int rewardOptionCount;

        [SerializeField]
        [Tooltip("The prefab to use for the reward pickup.")]
        protected GameObject rewardPickupPrefab;

        [Tooltip("Where to spawn the reward droplets relative to the spawn target (spawnTarget, or the teleporter).")]
        [SerializeField]
        private Vector3 rewardOffset;

        [Tooltip("Where to spawn the reward droplets.")]
        [SerializeField]
        private Transform spawnTarget;

        [Tooltip("Should the spawn target be overriden with the teleporter.")]
        public bool setSpawnTargetAsTeleporter = false;

        [Header("Purchase options")]
        [SerializeField]
        private float refreshInterval = 2f;

        [SerializeField]
        private int maxPurchaseCount;

        [Tooltip("Cost multiplier per purchase, hard caps at 99.")]
        [SerializeField]
        private float costMultiplierPerPurchase;

        public float baseMonsterCredit;

        [Tooltip("Difficulty coefficient gets multiplied by this * current purchase amount.")]
        [SerializeField]
        private float monsterCreditCoefficientPerPurchase;

        private Xoroshiro128Plus rng;

        private float calculatedMonsterCredit
        {
            get
            {
                return (float)this.baseMonsterCredit * Stage.instance.entryDifficultyCoefficient * (1f + (float)this.purchaseCount * (this.monsterCreditCoefficientPerPurchase - 1f));
            }
        }

        private void Awake()
        {
            //Fix cost type to the custom one.
            this.purchaseInteraction = base.GetComponent<PurchaseInteraction>();
            purchaseInteraction.costType = (CostTypeIndex)Utils.CostAndStatExtras.teleporterCostIndex;
            rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
            if (NetworkServer.active)
            {
                this.shrineCombatDirector = base.GetComponent<CombatDirector>();
                this.shrineCombatDirector.combatSquad.onDefeatedServer += this.OnDefeatedServer;
                shrineCombatDirector.onSpawnedServer.AddListener(new UnityAction<GameObject>(ModifySpawnedMasters));
            }

            //Make sure no item will spawn from the boss group
            if (bossGroup)
            {
                bossGroup.dropPosition = null;
                bossGroup.rng = null;
            }
        }

        private void Start()
        {
            if (!TeleporterInteraction.instance)
            {
                TELog.LogI("Could not find Teleporter for the Teleporter Overcharger Shrine, setting as non purchasable and returning.", true);
                this.purchaseInteraction.SetAvailable(false);
                return;
            }
            if (setSpawnTargetAsTeleporter)
            {
                this.shrineCombatDirector.currentSpawnTarget = TeleporterInteraction.instance.gameObject;
                spawnTarget = TeleporterInteraction.instance.transform;
            }
            if (dropTable == null)
            {
                dropTable = Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier1Item.asset").WaitForCompletion();
            }
        }

        [Server]
        private void ModifySpawnedMasters(GameObject gameObject)
        {
            CharacterBody cb = gameObject.GetComponent<CharacterMaster>().GetBody();
            if (cb)
            {
                cb.inventory.GiveItem(RoR2Content.Items.AdaptiveArmor);
                cb.inventory.GiveItem(RoR2Content.Items.LevelBonus, purchaseCount + 1);
            }
        }

        private void OnDefeatedServer()
        {
            onCombatSquadDefeatedGlobal?.Invoke(this);
        }

        public void FixedUpdate()
        {
            //Purchase interaction initial activation & disable handled by TeleporterEventRelay
            if (this.waitingForRefresh /*&& TeleporterInteraction.instance && (TeleporterInteraction.instance.isCharging || TeleporterInteraction.instance.isCharged)*/)
            {
                this.refreshTimer -= Time.fixedDeltaTime;
                if (this.refreshTimer <= 0f && this.purchaseCount < this.maxPurchaseCount)
                {
                    this.purchaseInteraction.SetAvailable(true);
                    this.purchaseInteraction.Networkcost = Mathf.Min((int)(this.purchaseInteraction.cost * this.costMultiplierPerPurchase), 99);
                    this.waitingForRefresh = false;
                }
            }
        }

        /// <summary>
        /// Called through the purchase interaction event in the editor.
        /// </summary>
        /// <param name="interactor"></param>
        [Server]
        public void AddShrineStack(Interactor interactor)
        {
            //Activate
            if (TeleporterInteraction.instance)
            {
                OverchargeActivation(shrineCombatDirector, chosenDirectorCardToSpawn, interactor);
            }

            EffectManager.SpawnEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab").WaitForCompletion(), new EffectData
            {
                origin = base.transform.position,
                rotation = Quaternion.identity,
                scale = 1f,
                color = new Color(0.7372549f, 0.90588236f, 0.94509804f)
            }, true);

            //Set unavailable and as waiting for refresh
            purchaseInteraction.SetAvailable(false);
            this.waitingForRefresh = true;
            this.refreshTimer = refreshInterval;

            //Increase purchases
            this.purchaseCount++;
        }

        /// <summary>
        /// Used at initial spawn and after every purchase.
        /// </summary>
        [Server]
        public void RollDirectorCard()
        {
            this.chosenDirectorCardToSpawn = this.shrineCombatDirector.SelectMonsterCardForCombatShrine(this.calculatedMonsterCredit);
            if (this.chosenDirectorCardToSpawn == null)
            {
                Debug.Log("Could not find appropriate spawn card for Teleporter Overcharger Shrine");
                this.purchaseInteraction.SetAvailable(false);
            }
        }

        public void OverchargeActivation(CombatDirector director, DirectorCard chosenDirectorCard, Interactor interactor)
        {
            //Do director stuff, first, reset.
            director.hasStartedWave = false;
            director.enabled = true;

            //Then add credits. This has the side-effect of overloading other directors in case of excess credits.
            director.monsterCredit += calculatedMonsterCredit;
            director.OverrideCurrentMonsterCard(chosenDirectorCard);
            director.monsterSpawnTimer = 0f;

            //Show chat message.
            CharacterBody component = interactor.GetComponent<CharacterBody>();
            Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
            {
                subjectAsCharacterBody = component,
                baseToken = "SHRINE_TELEPORTEROVERCHARGER_USE_MESSAGE",
                paramTokens = new string[]{
                    purchaseInteraction.Networkcost.ToString()
                }
            });

            //Do squad stuff
            if (NetworkServer.active && purchaseCount > 0)
            {
                //If not defeated yet, add one stack to the rewards, else reset so it drops at least one more drop when defeated
                if (!shrineCombatDirector.combatSquad.defeatedServer)
                {
                    accumulatedRewards++;
                }
                else
                {
                    shrineCombatDirector.combatSquad.defeatedServer = false;
                    shrineCombatDirector.combatSquad.memberHistory = new System.Collections.Generic.List<NetworkInstanceId>();
                }
            }

            //Change memories
            if (bossGroup)
            {
                bossGroup.bestObservedName = string.Join(" & ", bodyNamesSpawned);
                bossGroup.bestObservedSubtitle = Language.GetString("TELEPORTEROVERCHARGERSQUAD_SUBTITLE");
            }

            //and push stats
            StatSheet statSheet = PlayerStatsComponent.FindBodyStatSheet(component);
            if (statSheet != null)
            {
                statSheet.PushStatValue(TurboEdition.Utils.CostAndStatExtras.totalTeleporterOverchargerUsed, 1UL);
                statSheet.PushStatValue(TurboEdition.Utils.CostAndStatExtras.highestTeleporterOverchargerUsed, statSheet.GetStatValueULong(TurboEdition.Utils.CostAndStatExtras.totalTeleporterOverchargerUsed));
            }
        }

        [Server]
        public void DropRewards()
        {
            int participatingPlayerCount = Run.instance.participatingPlayerCount;
            if (participatingPlayerCount > 0 && spawnTarget && dropTable)
            {
                int rewardCount = participatingPlayerCount * (accumulatedRewards + 1);
                float angle = 360f / (float)rewardCount;
                Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                Vector3 position = spawnTarget.transform.position + rewardOffset;
                int i = 0;
                while (i < rewardCount)
                {
                    PickupPickerController.Option[] pickerOptions = PickupPickerController.GenerateOptionsFromDropTable(this.rewardOptionCount, dropTable, this.rng);
                    PickupDropletController.CreatePickupDroplet(new GenericPickupController.CreatePickupInfo
                    {
                        pickupIndex = rewardDisplayTier == ItemTier.AssignedAtRuntime ? TurboUtils.FindMostValuablePickupInOptions(pickerOptions).pickupIndex : PickupCatalog.FindPickupIndex(this.rewardDisplayTier),
                        pickerOptions = pickerOptions,
                        rotation = Quaternion.identity,
                        prefabOverride = this.rewardPickupPrefab
                    }, position, vector);
                    i++;
                    vector = rotation * vector;
                }
            }

            //Reset rewards as they have been dropped
            accumulatedRewards = 0;
        }

        private void OnValidate()
        {
            if (!base.GetComponent<CombatDirector>().combatSquad)
            {
                Debug.LogError("ShrineTeleporterOverchargerBehavior's sibling CombatDirector must use a CombatSquad.", base.gameObject);
            }
        }
    }
}
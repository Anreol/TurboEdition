using RoR2;
using RoR2.Stats;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(CombatDirector))]
    [RequireComponent(typeof(CombatSquad))]
    [RequireComponent(typeof(BossGroup))]
    [RequireComponent(typeof(PurchaseInteraction))]
    internal class ShrineTeleporterOverchargerBehavior : NetworkBehaviour
    {
        public static event Action<ShrineTeleporterOverchargerBehavior> onDefeatedServerGlobal;

        private PurchaseInteraction purchaseInteraction;
        private CombatDirector shrineCombatDirector;
        private BossGroup bossGroup;
        private DirectorCard chosenDirectorCardToSpawn;
        private bool waitingForRefresh = false;
        private float refreshTimer;
        private int purchaseCount;

        public float refreshInterval = 2f;
        public int maxPurchaseCount;
        public float costMultiplierPerPurchase;
        public float baseMonsterCredit;
        public float monsterCreditCoefficientPerPurchase;
        public Transform symbolTransform;

        private float calculatedMonsterCredit
        {
            get
            {
                return (float)this.baseMonsterCredit * Stage.instance.entryDifficultyCoefficient * (1f + (float)this.purchaseCount * (this.monsterCreditCoefficientPerPurchase - 1f));
            }
        }

        private void Awake()
        {
            if (NetworkServer.active)
            {
                this.purchaseInteraction = base.GetComponent<PurchaseInteraction>();
                this.shrineCombatDirector = base.GetComponent<CombatDirector>();
                this.bossGroup = base.GetComponent<BossGroup>();
                this.shrineCombatDirector.combatSquad.onDefeatedServer += this.OnDefeatedServer;
                shrineCombatDirector.onSpawnedServer.AddListener(new UnityAction<GameObject>(ModifySpawnedMasters));
            }
            purchaseInteraction.costType = (CostTypeIndex)Misc.CostAndStatExtras.teleporterCostIndex;
        }

        [Server]
        private void ModifySpawnedMasters(GameObject gameObject)
        {
            CharacterBody cb = gameObject.GetComponent<CharacterMaster>().GetBody();
            if (cb)
            {
                cb.inventory.GiveItem(RoR2Content.Items.AdaptiveArmor);
                cb.inventory.GiveItem(RoR2Content.Items.LevelBonus, purchaseCount);
            }
        }

        private void Start()
        {
            if (!TeleporterInteraction.instance)
            {
                Debug.Log("Could not find Teleporter for the Teleporter Overcharger Shrine");
                this.purchaseInteraction.SetAvailable(false);
                return;
            }
            RollDirectorCard();
            this.shrineCombatDirector.currentSpawnTarget = TeleporterInteraction.instance.gameObject;
            bossGroup.dropPosition = TeleporterInteraction.instance.transform;
            bossGroup.dropTable = Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier1Item.asset").WaitForCompletion();
        }

        private void OnDefeatedServer()
        {
            onDefeatedServerGlobal?.Invoke(this);
        }

        public void FixedUpdate()
        {
            //Teleporter checks handled by TeleporterEventRelay
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

        [Server]
        public void AddShrineStack(Interactor interactor)
        {
            this.waitingForRefresh = true;
            if (TeleporterInteraction.instance)
            {
                OverchargeActivation(shrineCombatDirector, calculatedMonsterCredit, chosenDirectorCardToSpawn);
                RollDirectorCard(); //Roll a different card for next activation
            }
            purchaseInteraction.SetAvailable(false);
            CharacterBody component = interactor.GetComponent<CharacterBody>();
            Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
            {
                subjectAsCharacterBody = component,
                baseToken = "SHRINE_TELEPORTEROVERCHARGER_USE_MESSAGE"
            });
            StatSheet statSheet = PlayerStatsComponent.FindBodyStatSheet(component);
            if (statSheet != null)
            {
                statSheet.PushStatValue(TurboEdition.Misc.CostAndStatExtras.totalTeleporterOverchargerUsed, 1UL);
                statSheet.PushStatValue(TurboEdition.Misc.CostAndStatExtras.highestTeleporterOverchargerUsed, statSheet.GetStatValueULong(TurboEdition.Misc.CostAndStatExtras.totalTeleporterOverchargerUsed));
            }
            /*EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
			{
				origin = base.transform.position,
				rotation = Quaternion.identity,
				scale = 1f,
				color = new Color(0.7372549f, 0.90588236f, 0.94509804f)
			}, true);*/
            this.purchaseCount++;
            this.refreshTimer = refreshInterval;
            if (this.purchaseCount >= this.maxPurchaseCount)
            {
                this.symbolTransform.gameObject.SetActive(false);
            }
        }

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

        public void OverchargeActivation(CombatDirector director, float monsterCredit, DirectorCard chosenDirectorCard)
        {
            director.enabled = true;
            director.monsterCredit += monsterCredit;
            director.OverrideCurrentMonsterCard(chosenDirectorCard);
            director.monsterSpawnTimer = 0f;
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
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    internal class BloodMoney : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("BloodEconomy");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            if (NetworkServer.active)
                body.AddItemBehavior<BloodMoneyBehaviorServer>(stack);
        }

        internal class BloodMoneyBehaviorServer : CharacterBody.ItemBehavior//, IOnTakeDamageServerReceiver
        {
            //private int currentMl = 0;
            //private const int bagMlSize = 250;
            //private bool shouldScale = true;

            //Cleans up everytime the body dies ie. death and stage change
            private PurchaseInteraction[] purchaseList = new PurchaseInteraction[0];

            private int[] purchaseTimes = new int[0];

            private InteractionDriver interactionDriver;

            private float interactableCooldown;
            private bool inputReceived;

            private void Start()
            {
                interactionDriver = body.GetComponent<InteractionDriver>();
                if (interactionDriver)
                {
                    interactableCooldown = interactionDriver.interactableCooldown;
                }
            }

            private void FixedUpdate()
            {
                /*
                if (currentMl >= bagMlSize)
                {
                    int moneyReward = 25 + ((stack - 1) * 15);
                    moneyReward = shouldScale ? Run.instance.GetDifficultyScaledCost(moneyReward) : moneyReward;
                    //body.master.GiveMoney((uint)moneyReward);
                    TeamManager.instance.GiveTeamMoney(body.teamComponent.teamIndex, (uint)moneyReward);
                    currentMl = -bagMlSize;
                }*/

                if (interactionDriver)
                {
                    this.interactableCooldown -= Time.fixedDeltaTime;
                    this.inputReceived = (body.inputBank.interact.justPressed || (body.inputBank.interact.down && this.interactableCooldown <= 0f));
                    if (body.inputBank.interact.justReleased)
                    {
                        this.inputReceived = false;
                        this.interactableCooldown = 0f;
                    }
                }
                if (body.healthComponent.combinedHealthFraction < 0.35)
                    return;
                //Debug.Log("interaction driver " + interactionDriver + " input received " + inputReceived);
                if (inputReceived)
                {
                    if (!interactionDriver)
                        return;
                    GameObject gameObject = interactionDriver.FindBestInteractableObject();
                    if (!gameObject)
                        return;
                    IInteractable interactable = gameObject.GetComponent<IInteractable>();
                    Debug.Log("lol");
                    if (interactable != null && ((MonoBehaviour)interactable).isActiveAndEnabled && interactable.GetInteractability(interactionDriver.interactor) == Interactability.ConditionsNotMet)
                    {
                        Debug.Log("lol1");
                        PurchaseInteraction purchaseInteraction = gameObject.GetComponent<PurchaseInteraction>();
                        if (purchaseInteraction && purchaseInteraction.costType == CostTypeIndex.Money)
                        {
                            if (purchaseInteraction.cost <= 5 || purchaseInteraction.cost < body.master.money)
                                return;
                            Debug.Log("lol2");
                            int index;
                            Debug.Log("lol3");
                            for (index = 0; index < purchaseList.Length; index++)
                            {
                                Debug.Log("Searching list!");
                                if (purchaseList[index] == purchaseInteraction)
                                {
                                    Debug.Log("Found in list!");
                                    if (purchaseTimes[index] < stack)
                                    {
                                        Debug.Log("Already decreased enough!");
                                        DecreasePriceOfPurchaseInteractor(purchaseInteraction);
                                        purchaseTimes[index]++;
                                    }
                                    return;
                                }
                            }
                            Debug.Log("Wasn't in list, appending");
                            HG.ArrayUtils.ArrayAppend(ref purchaseList, purchaseInteraction);
                            HG.ArrayUtils.ArrayAppend(ref purchaseTimes, 1);
                            DecreasePriceOfPurchaseInteractor(purchaseInteraction);
                        }
                    }
                }
            }

            public void DecreasePriceOfPurchaseInteractor(PurchaseInteraction purchaseInteraction)
            {
                float damage = body.healthComponent.fullCombinedHealth * 0.35f;
                body.healthComponent.TakeDamage(new DamageInfo
                {
                    damage = damage,
                    attacker = purchaseInteraction.gameObject,
                    position = purchaseInteraction.gameObject.transform.position,
                    damageType = (DamageType.NonLethal | DamageType.BypassArmor)
                });
                if (!body.healthComponent.isHealthLow)
                {
                    DotController.InflictDot(body.gameObject, body.gameObject, DotController.DotIndex.Bleed, 3f, 0.0005f);
                }

                purchaseInteraction.cost = Mathf.Max(5, purchaseInteraction.cost -= Run.instance.GetDifficultyScaledCost(2));
            }

            /*
            public void OnTakeDamageServer(DamageReport damageReport)
            {
                switch (damageReport.dotType)
                {
                    case DotController.DotIndex.Bleed:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt * 1.5f);
                        break;

                    case DotController.DotIndex.Burn:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt);
                        break;

                    case DotController.DotIndex.Helfire:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt / 4);
                        break;

                    case DotController.DotIndex.PercentBurn:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt);
                        break;

                    case DotController.DotIndex.Poison:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt);
                        break;

                    case DotController.DotIndex.Blight:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt);
                        break;

                    case DotController.DotIndex.SuperBleed:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt * 1.5f);
                        break;

                    default:
                        currentMl += Mathf.RoundToInt(damageReport.damageDealt);
                        break;
                }
            }*/
        }
    }
}
using RoR2;
using RoR2.Items;
using System;
using UnityEngine;

namespace TurboEdition.Items
{
    [Obsolete]
    public class BloodEconomyBodyBehavior : BaseItemBodyBehavior
    {
        //[BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = false, useOnClient = true)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.BloodEconomy;
        }

        //Cleans up everytime the body dies ie. death and stage change
        private PurchaseInteraction[] localPurchaseList = new PurchaseInteraction[0];

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
                GameObject gameObject = interactionDriver.FindBestInteractableObject();
                if (!gameObject)
                    return;
                IInteractable interactable = gameObject.GetComponent<IInteractable>();
                if (interactable != null && ((MonoBehaviour)interactable).isActiveAndEnabled && interactable.GetInteractability(interactionDriver.interactor) == Interactability.ConditionsNotMet)
                {
                    PurchaseInteraction purchaseInteraction = gameObject.GetComponent<PurchaseInteraction>();
                    if (purchaseInteraction && purchaseInteraction.costType == CostTypeIndex.Money)
                    {
                        if (purchaseInteraction.cost <= 5 || purchaseInteraction.cost < body.master.money)
                            return;
                        int index;
                        for (index = 0; index < localPurchaseList.Length; index++)
                        {
                            if (localPurchaseList[index] == purchaseInteraction)
                            {
                                if (purchaseTimes[index] < stack)
                                {
                                    DecreasePriceOfPurchaseInteractor(purchaseInteraction);
                                    purchaseTimes[index]++;
                                }
                                return;
                            }
                        }
                        HG.ArrayUtils.ArrayAppend(ref localPurchaseList, purchaseInteraction);
                        HG.ArrayUtils.ArrayAppend(ref purchaseTimes, 1);
                        DecreasePriceOfPurchaseInteractor(purchaseInteraction);
                    }
                }
            }
        }

        public void DecreasePriceOfPurchaseInteractor(PurchaseInteraction purchaseInteraction)
        {
            float damage = 20;
            body.healthComponent.TakeDamage(new DamageInfo
            {
                damage = damage,
                attacker = purchaseInteraction.gameObject,
                position = purchaseInteraction.gameObject.transform.position,
                damageType = (DamageType.NonLethal | DamageType.BypassArmor)
            });
            if (!body.healthComponent.isHealthLow)
            {
                DotController.InflictDot(body.gameObject, body.gameObject, DotController.DotIndex.Bleed, 3f, 0.000001f, 1);
            }

            purchaseInteraction.cost = Mathf.Max(5, purchaseInteraction.cost -= Run.instance.GetDifficultyScaledCost(2));
        }
    }
}
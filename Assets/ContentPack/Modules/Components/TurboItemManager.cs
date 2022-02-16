using RoR2;
using System.Collections;
using UnityEngine;

namespace TurboEdition
{
    [RequireComponent(typeof(CharacterBody))]
    public class TurboItemManager : MonoBehaviour
    {
        public IStatItemBehavior[] statItemBehaviors = new IStatItemBehavior[] { };
        private CharacterBody body;

        private void Awake()
        {
            body = gameObject.GetComponent<CharacterBody>();
            body.onInventoryChanged += CheckForTEItems;
        }

        public void CheckForTEItems()
        {
            //It seems counter-intuitive to add an item behavior for something even if it has none of them, but the game actually destroys the behavior if there isn't one which is what we want and it doesn't add a component if it doesn't have any of the item
            foreach (var item in InitPickups.itemList)
                item.Value.AddBehavior(ref body, body.inventory.GetItemCount(item.Key.itemIndex));
            foreach (var equipment in InitPickups.equipmentList)
                equipment.Value.AddBehavior(ref body, System.Convert.ToInt32(body.equipmentSlot?.equipmentIndex == equipment.Value.equipmentDef.equipmentIndex));

            StartCoroutine(GetInterfaces());
        }

        public void CheckEqp(EquipmentDef equipmentDef, bool gainOrLoss)
        {
            Equipments.Equipment equipment;
            if (InitPickups.equipmentList.TryGetValue(equipmentDef, out equipment))
            {
                equipment.AddBehavior(ref body, System.Convert.ToInt32(gainOrLoss));
                StartCoroutine(GetInterfaces());
            }
        }

        //This will eventually break if someone adds a interface that doesnt want to be added right away
        private IEnumerator GetInterfaces()
        {
            yield return new WaitForEndOfFrame();
            statItemBehaviors = GetComponents<IStatItemBehavior>();
            body.healthComponent.onIncomingDamageReceivers = GetComponents<IOnIncomingDamageServerReceiver>();
            body.healthComponent.onTakeDamageReceivers = GetComponents<IOnTakeDamageServerReceiver>();
        }

        public void RunStatRecalculationsStart()
        {
            foreach (var statBehavior in statItemBehaviors)
                statBehavior.RecalculateStatsStart();
        }

        public void RunStatRecalculationsEnd()
        {
            foreach (var statBehavior in statItemBehaviors)
                statBehavior.RecalculateStatsEnd();
        }
    }
}
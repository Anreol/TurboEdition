using RoR2;
using UnityEngine;

namespace TurboEdition
{
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
            //TODO: Find some way to automate this
            body.AddItemBehavior<Items.BaneMaskBehavior>(body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("BaneMask")));
            body.AddItemBehavior<Items.DropletDupeBehavior>(body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("DropletDupe")));
            body.AddItemBehavior<Items.EnvBonusBehavior>(body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("EnvBonus")));
            body.AddItemBehavior<Items.HitlagBehavior>(body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("Hitlag")));
            body.AddItemBehavior<Items.MeleeArmorBehavior>(body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("MeleeArmor")));
            body.AddItemBehavior<Items.PackMagnetBehavior>(body.inventory.GetItemCount(Assets.mainAssetBundle.LoadAsset<ItemDef>("PackMagnet")));

            statItemBehaviors = GetComponents<IStatItemBehavior>();
        }

        public void CheckForBuffs()
        {
            statItemBehaviors = GetComponents<IStatItemBehavior>();
        }


        public void CheckEqp(EquipmentDef equipmentDef, bool gainOrLoss)
        {
            Equipments.Equipment equipment;
            if (InitPickups.equipments.TryGetValue(equipmentDef, out equipment))
            {
                equipment.AddBehavior(ref body, System.Convert.ToInt32(gainOrLoss));
                statItemBehaviors = GetComponents<IStatItemBehavior>();
            }
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
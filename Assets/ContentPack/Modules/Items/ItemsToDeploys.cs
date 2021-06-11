using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

//TODO
//Add a delayer or whatever to slow down the item giving process
//Please
namespace TurboEdition.Items
{
    public class ItemsToDeploys : ItemBase<ItemsToDeploys>
    {
        public override string ItemName => "Glued Tape Roll";
        public override string ItemLangTokenName => "ITEMSTODEPLOYS";
        public override string ItemPickupDesc => "Give one of your items to a drone.";
        public override string ItemFullDescription => $"Give <style=cIsUtility>{itemsToGive} of your items</style> to a drone up to <style=cIsUtility>{itemAddStack} times </style>. <style=cStack>(+{itemsToGive} item, +{itemAddStack} times, per stack).</style>";
        public override string ItemLore => "Its like Arms Race but instead of making it go shoot we allow it to do the same as the player does.";

        public override ItemTier Tier => ItemTier.Tier1;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.CannotCopy };
        public override bool AIBlacklisted => false;
        public override bool BrotherBlacklisted => false;

        public override GameObject ItemModel => TurboEdition.MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Default.prefab");
        public override Sprite ItemIcon => TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Items/Tier1.png");

        //whatever the hell is this
        private List<RoR2.Orbs.ItemTransferOrb> inFlightOrbs;

        public static Xoroshiro128Plus random = new Xoroshiro128Plus((ulong)System.DateTime.Now.Ticks);

        //Item properties
        public int itemsToGive;

        public int itemAddStack;
        public int itemGivingCap;
        public int itemStackingCap;
        public bool squidAllowed;
        public bool useOrbs;
        public bool cappedByInventory;

        protected override void CreateConfig(ConfigFile config)
        {
            itemsToGive = config.Bind<int>("Item: " + ItemName, "Available items per stack", 1, "Number of unique items each stack will give to a drone.").Value;
            itemAddStack = config.Bind<int>("Item: " + ItemName, "Added items per stack", 3, "Maximum amount of stacks of the selected item the drone will get.").Value;
            itemGivingCap = config.Bind<int>("Item: " + ItemName, "Max number of items given to a drone", -1, "Maximum number of items a drone can get from you, set to -1 for no limit.").Value;
            itemStackingCap = config.Bind<int>("Item: " + ItemName, "Max stack of items given to a drone", -1, "Maximum number of stacls a drone can get from you, set to -1 for no limit.").Value;
            //squidAllowed = config.Bind<bool>("Item: " + ItemName, "Combat Squids allowed", true, "Whenever to give combat squids items or not.").Value;
            useOrbs = config.Bind<bool>("Item: " + ItemName, "Orb item transfer", true, "Whenever to create item orbs when transfering items.").Value;
            cappedByInventory = config.Bind<bool>("Item: " + ItemName, "Limited by inventory count", true, "Whenever drones should NOT get more unique items than you currently have.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override void Initialization()
        {
        }

        public override void Hooks()
        {
            MasterSummon.onServerMasterSummonGlobal += TryGrantItem;
            //On.RoR2.CharacterMaster.OnInventoryChanged += TryGrantItem;
        }

        private void TryGrantItem(MasterSummon.MasterSummonReport summonReport)
        {
            var master = summonReport.leaderMasterInstance;
            var servant = summonReport.summonMasterInstance;
            var servantBody = servant.GetBody();

            var InventoryCount = GetCount(master);
            if (InventoryCount > 0)
            {
                if (servantBody && (servantBody.bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None || servantBody.name == "SquidTurretBody")
                {
                    //if (!squidAllowed) return;
                    inFlightOrbs = new List<RoR2.Orbs.ItemTransferOrb>();
                    int itemStack = InventoryCount * itemAddStack;
                    int itemGiven = InventoryCount * itemsToGive;
#if DEBUG
                    TurboEdition._logger.LogWarning(ItemName + " Calculated numbers, itemStack: " + itemStack + " itemGiven: " + itemGiven);
#endif
                    if (itemStackingCap != -1) { itemStack = Mathf.Min(itemStack, itemStackingCap); }
                    if (itemGivingCap != -1) { itemGiven = Mathf.Min(itemGiven, itemGivingCap); }

#if DEBUG
                    TurboEdition._logger.LogWarning(ItemName + " Re-calculated numbers, itemStack: " + itemStack + " itemGiven: " + itemGiven);
#endif
                    var itemList = master.inventory.itemAcquisitionOrder;
                    int randomIndex;
                    bool foundBlacklisted;
                    if (cappedByInventory) { itemGiven = Mathf.Min(itemGiven, itemList.Count); }; //Let's not give more items than what the player has
                    for (int i = 0; i < itemGiven; i++)
                    {
                        //Get a delayer here PLEASE, game shits itself with high item count or when summoning multiple at the same time i.e reinforcement!
                        //Would let the player know which items the drones are getting too!
                        do
                        {
                            foundBlacklisted = false;
                            randomIndex = random.RangeInt(0, itemList.Count);
                            if (ItemCatalog.GetItemDef(itemList[randomIndex]).ContainsTag(ItemTag.CannotCopy))
                            {
                                foundBlacklisted = true;
                            }

                            foreach (ItemDef item in itemBlacklist)
                            {
                                if (ItemCatalog.GetItemDef(itemList[randomIndex]) == item)
                                {
#if DEBUG
                                    Chat.AddMessage("Turbo Edition: " + ItemName + " Found a blacklisted item (" + item + ") when giving items to a summon, rerolling.");
#endif
                                    foundBlacklisted = true;
                                }
                            }
                        } while (foundBlacklisted);

                        itemStack = Mathf.Min(itemStack, master.inventory.GetItemCount(itemList[randomIndex]));
#if DEBUG
                        TurboEdition._logger.LogWarning(ItemName + " Getting either max item count or max item stack, itemStack: " + itemStack);
#endif
                        if (useOrbs)
                        {
                            RoR2.Orbs.ItemTransferOrb item = RoR2.Orbs.ItemTransferOrb.DispatchItemTransferOrb(master.GetBody().transform.position, servant.inventory, itemList[randomIndex], itemStack, delegate (RoR2.Orbs.ItemTransferOrb orb)
                            {
#if DEBUG
                                TurboEdition._logger.LogWarning(ItemName + " Gave " + itemList[randomIndex] + " via orbs to " + servant + " " + itemStack + " times. Item " + i + " out of " + itemGiven);
#endif
                                servant.inventory.GiveItem(orb.itemIndex, orb.stack);
                                this.inFlightOrbs.Remove(orb);
                            }, servant.networkIdentity);
                            this.inFlightOrbs.Add(item);
                        }
                        else
                        {
#if DEBUG
                            TurboEdition._logger.LogWarning(ItemName + " Gave " + itemList[randomIndex] + " directly to " + servant + " " + itemStack + " times. Item " + i + " out of " + itemGiven);
#endif
                            servant.inventory.GiveItem(itemList[randomIndex], itemStack);
                        }
                    }
                }
            }
        }

        public static ItemDef[] itemBlacklist = new ItemDef[]
        {
            RoR2Content.Items.SprintWisp,
            RoR2Content.Items.TitanGoldDuringTP,
            RoR2Content.Items.TreasureCache,
            RoR2Content.Items.Feather,
            RoR2Content.Items.Firework,
            RoR2Content.Items.SprintArmor,
            RoR2Content.Items.JumpBoost,
            RoR2Content.Items.GoldOnHit,
            RoR2Content.Items.WardOnLevel,
            RoR2Content.Items.BeetleGland,
            RoR2Content.Items.ArtifactKey,
            RoR2Content.Items.DrizzlePlayerHelper,
            RoR2Content.Items.RoboBallBuddy,
            RoR2Content.Items.RandomDamageZone,
            RoR2Content.Items.MonstersOnShrineUse,
            RoR2Content.Items.LunarBadLuck,
            RoR2Content.Items.CrippleWardOnLevel,
            RoR2Content.Items.TPHealingNova,
            RoR2Content.Items.FocusConvergence,
            RoR2Content.Items.ScrapWhite,
            RoR2Content.Items.ScrapGreen,
            RoR2Content.Items.ScrapRed,
            RoR2Content.Items.ScrapYellow,
            RoR2Content.Items.AdaptiveArmor,
            RoR2Content.Items.ArtifactKey,
            RoR2Content.Items.DrizzlePlayerHelper,
            RoR2Content.Items.WarCryOnCombat,
            RoR2Content.Items.ExtraLife,
            RoR2Content.Items.ExtraLifeConsumed
        };

        /*
        public class WaitForSecondsExample : MonoBehaviour
        {
            void Start()
            {
                //Start the coroutine we define below named ExampleCoroutine.
                StartCoroutine(ExampleCoroutine(5));
            }

            public IEnumerator ExampleCoroutine(int input)
            {
                yield return new WaitForSeconds(input);
            }
        }*/
    }
}
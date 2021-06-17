using BepInEx.Configuration;
using RoR2;
using UnityEngine;

//TODO Get a countdown or anything that indicates that you are about to be obliterated
//That includes getting a particle effect or explosion or ANYTHING that isn't the void reaver explosion
//Check how the battery cell does it
//Idea: Get card deck to show up somewhere, on item duping, pull a card off, on death, start dropping every card til its empty, then a random timer til death, because not knowing when you will stop existing is fun.
//Chest duping MUST be the same item or at /the very least/ come from the same item pool. Sacrifice Item Pool != Default Item pool.
namespace TurboEdition.Items
{
    public class DupeItemDrops : ItemBase<DupeItemDrops>
    {
        public override string ItemName => "Trading Deck";
        public override string ItemLangTokenName => "DUPEITEMDROPS";
        public override string ItemPickupDesc => $"Have a <style=cIsUtility>{dupeChanceInitial}% chance</style> of <style=cIsUtility>duplicating items</style> <style=cDeath>but</style> have a <style=cDeath>{deathChanceInitial}%</style> chance of <style=cDeath>instantly dying</style> when hurt. <style=cIsUtility>Death chance affected by luck</style>.";
        public override string ItemFullDescription => $"<style=cIsUtility>{dupeChanceInitial}% chance</style> to <style=cIsUtility>duplicate items</style> when opening a chest. <style=cStack>(+{dupeChanceStack}% per stack).</style> <style=cDeath>But</style> have a <style=cDeath>{deathChanceInitial}%</style> chance of <style=cDeath>instantly dying</style> when hurt. <style=cStack>(+{deathChanceStack}% per stack)";

        public override string ItemLore => "I play the Trading Deck Item, this item allows me to get one more item!";
        public override ItemTier Tier => ItemTier.Lunar;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlacklisted => true; //I mean is there really a reason to NOT be black listed? They cannot get items and having a chance of instakilling enemies isnt fun...
        public override bool BrotherBlacklisted => true;

        public override GameObject ItemModel => TurboEdition.MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Default.prefab");
        public override Sprite ItemIcon => TurboEdition.MainAssets.LoadAsset<Sprite>("@TurboEdition:Assets/Textures/Icons/Items/TLunar.png");

        private static readonly Xoroshiro128Plus treasureRng = new Xoroshiro128Plus(0UL);
        private static PickupDropTable dropTable = Resources.Load<PickupDropTable>("DropTables/dtSacrificeArtifact");
        private PickupIndex chestPickup = PickupIndex.none;

        //Item properties
        public float deathChanceInitial;

        public float deathChanceStack;
        public float dupeChanceInitial;
        public float dupeChanceStack;
        public bool useLuck;
        public bool onlySacrifice;
        public bool easyModo;

        protected override void CreateConfig(ConfigFile config)
        {
            deathChanceInitial = config.Bind<float>("Item: " + ItemName, "Initial death chance", 3f, "Death % chance when getting the item for the first time.").Value;
            deathChanceStack = config.Bind<float>("Item: " + ItemName, "Stack death chance", 3f, "Added death % chance when stacking the item.").Value;
            dupeChanceInitial = config.Bind<float>("Item: " + ItemName, "Initial duplication chance", 8f, "Duplication % chance when getting the item for the first time.").Value;
            dupeChanceStack = config.Bind<float>("Item: " + ItemName, "Stack duplication chance", 2.5f, "Added duplication % chance when stacking the item.").Value;
            //this is like doing just a if check but im kinda lazy to do it. useLuck = config.Bind<bool>("Item: " + ItemName, "Use luck", true, "Should master's luck affect the chances of instant death.").Value;
            onlySacrifice = config.Bind<bool>("Item: " + ItemName, "Sacrifice rolls with sacrifice on", true, "Whenever or not enemy drops from killing enemies should take place outside Sacrifice being activated. Turning it false would turn it into a clover-like effect.").Value;
            easyModo = config.Bind<bool>("Item: " + ItemName, "Easy mode", false, "Should the item ignore damage sources like DoT and fall damage before rolling to see if the player should get obliterated.").Value;
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
            GlobalEventManager.onServerDamageDealt += CheckDeathChance;
            GlobalEventManager.onCharacterDeathGlobal += DuplicateDropSacrifice;
            On.RoR2.ChestBehavior.ItemDrop += DuplicateDropChest;
            //Roulette chests have their own controller, so the item wont affect those, that seems to be the only thing that doesnt use Chest Behaviour. Even Scavanger bags use it.
            //This shit doesnt work and I dont know how to do it On.RoR2.Artifacts.SacrificeArtifactManager.OnServerCharacterDeath += DuplicateDropSacrifice;
        }

        private void CheckDeathChance(DamageReport damageReport)
        {
            var InventoryCount = GetCount(damageReport.victimBody);
            if (InventoryCount > 0)
            {
                var DeathChance = deathChanceInitial + ((InventoryCount - 1) * deathChanceStack);
                var victim = damageReport.victim;
                var attacker = damageReport.attacker;
                if (easyModo && (damageReport.isFallDamage || damageReport.damageInfo.dotIndex != DotController.DotIndex.None))
                {
#if DEBUG
                    TurboEdition._logger.LogWarning(ItemName + " Easy Modo is on, and the damage type was fall damage or DoT, damage rejected");
#endif
                    return;
                }
#if DEBUG
                Chat.AddMessage("Turbo Edition: " + ItemName + " is being held by someone who got hurt, check log for details.");
                TurboEdition._logger.LogWarning(ItemName + " is being held by " + victim.body + ", rolling death with a chance of " + DeathChance + " and a inverted luck chance of " + -victim.body.master.luck);
#endif
                if (Util.CheckRoll(DeathChance, -victim.body.master.luck) && victim.body.healthComponent)
                {
#if DEBUG
                    Chat.AddMessage("Turbo Edition: " + ItemName + " failed the death roll.\nChange the planet, this is my final message. Goodbye.");
#endif
                    victim.body.healthComponent.Suicide(attacker, attacker, DamageType.VoidDeath);
                }
            }
        }

        private void DuplicateDropChest(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            orig(self);
            if (GetCountFromPlayers(ItemDef, true) > 0)
            {
                //chestPickup = Reflection.GetFieldValue<PickupIndex>(self, "dropPickup");
                chestPickup = dropTable.GenerateDrop(treasureRng);
                var HighestChance = GetCountHighestFromPlayers(ItemDef, true); //Doesn't work well across multiple players, but I don't really want to make clover but lunar and it fucking kills you dont know?
                var DupeChance = dupeChanceInitial + ((HighestChance - 1) * dupeChanceStack); //Also I do not know how to get the cb that interacted and opened the chest
#if DEBUG
                Chat.AddMessage("Turbo Edition: " + ItemName + " duplication taking place, check logs.");
                TurboEdition._logger.LogWarning(ItemName + " duplication chance of: " + DupeChance + ", duplicating pickup index: " + chestPickup);
#endif
                if (Util.CheckRoll(DupeChance)) //hopefully this rolls only the chance WITHOUT luck
                {
                    if (chestPickup == PickupIndex.none)
                    {
#if DEBUG
                        Chat.AddMessage("Turbo Edition: " + ItemName + " Chest didn't have a pickup, how did this happen?");
#endif
                        return;
                    }
#if DEBUG
                    Chat.AddMessage("Turbo Edition: " + ItemName + " Duplicated a chest drop.");
#endif
                    PickupDropletController.CreatePickupDroplet(chestPickup, self.dropTransform.position + Vector3.up * 1.5f, Vector3.up * self.dropUpVelocityStrength + self.dropTransform.forward * self.dropForwardVelocityStrength);
                    //Reflection.SetFieldValue(self, "dropPickup", PickupIndex.none); //I actually dont know what or why is this here?
                }
            }
        }

        private void DuplicateDropSacrifice(DamageReport damageReport)
        {
            //ArtifactIndex sacrificeIndex = ArtifactCatalog.FindArtifactIndex("Sacrifice");
            if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.sacrificeArtifactDef) || !onlySacrifice)
            {
                var InventoryCount = GetCount(damageReport.attackerMaster);
                if (InventoryCount > 0)
                {
                    var DupeChance = dupeChanceInitial + ((InventoryCount - 1) * dupeChanceStack);
#if DEBUG
                    Chat.AddMessage("Turbo Edition: " + ItemName + " in hands of " + damageReport.attackerMaster + ", rolling with a chance of " + DupeChance);
#endif
                    if (Util.CheckRoll(DupeChance))
                    {
                        if (!damageReport.victimMaster)
                        {
                            return;
                        }
                        if (damageReport.attackerTeamIndex == damageReport.victimTeamIndex && damageReport.victimMaster.minionOwnership.ownerMaster)
                        {
                            return;
                        }
                        float expAdjustedDropChancePercent = Util.GetExpAdjustedDropChancePercent(5f, damageReport.victim.gameObject);
                        TurboEdition._logger.LogWarning(("Drop chance from {0}: {1}", new object[]
                        {
                        damageReport.victimBody,
                        expAdjustedDropChancePercent
                        }));
                        if (Util.CheckRoll(expAdjustedDropChancePercent, 0f, null))
                        {
                            PickupIndex pickupIndex = dropTable.GenerateDrop(treasureRng);
                            if (pickupIndex != PickupIndex.none)
                            {
#if DEBUG
                                Chat.AddMessage("Turbo Edition: " + ItemName + " created a new extra sacrifice drop.");
#endif
                                PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
                            }
                        }
                        return;
                    }
                }
            }
            else
            {
#if DEBUG
                TurboEdition._logger.LogWarning(ItemName + " isn't running with sacrifice on!");
#endif
            }
        }
    }
}
/*using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using HG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static TurboEdition.Utils.ItemHelpers;

//TODO:
//I'm sick of custom buffs so I'd rather it give the stat increase directly than via the buff (I wish I could find a way to do this, for now lets do buffs)
//Get visuals to replace the UI thing
//This also makes the buff unremovable :^)
//For SS2:
//Hook up to the director or some shit and detect the EventDirector
//Then detect the storm event card
//Must be OnSelected = WarnStorm!! Not OnActivated!!
//If the Storm falls short, i.e arrived at a stage and it was already raining, disable buff (OnDeactivated = StopStorm)
//Figure out a way how to detect is already happening, see above, plus any players that might respawn midstorm (probably just using isActive)
namespace TurboEdition.Items
{
    public class EnvBonus : ItemBase<EnvBonus>
    {
        public override string ItemName => "UVB-78 Radio";
        public override string ItemLangTokenName => "ENVBONUS";
        public override string ItemPickupDesc => $"<style=cIsUtility>Gain a buff</style> after entering a stage. Effects vary on the enviroment.";
        public override string ItemFullDescription => $"Gain a buff for <style=cIsUtility>{durationInitial} seconds</style>. <style=cStack>(+{durationStack} seconds per stack)</style> upon entering a stage. Effects vary on the enviroment.";
        public override string ItemLore => "Asuka is best girl?";

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlacklisted => false;
        public override bool BrotherBlacklisted => false;

        public override GameObject ItemModel => TurboEdition.MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Default.prefab");
        public override Sprite ItemIcon => TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Items/Tier2.png");

        public static BuffIndex envBuff;
        private Starstorm2.Cores.EventDirector eventDirector;
        private Starstorm2.Cores.EventCard eventCard;

        //Item properties
        public int durationInitial;
        public int durationStack;
        public bool hiddenRealmsBonus;

        public bool ssStormBonus;
        public float ssStormBoost;

        internal override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateBuff();
            CreateItem();
            //Initialization();
            Hooks();
        }

        protected override void CreateConfig(ConfigFile config)
        {
            durationInitial = config.Bind<int>("Item: " + ItemName, "Initial duration", 30, "Duration in seconds for the item to be active on first pickup.").Value;
            durationStack = config.Bind<int>("Item: " + ItemName, "Stack duration", 15, "Duration in seconds for the item to be active when stacking the item.").Value;
            hiddenRealmsBonus = config.Bind<bool>("Item: " + ItemName, "Hidden Realms bonus", true, "Should the item give a general bonus within Hidden Realms.").Value;

            ssStormBonus = config.Bind<bool>("Item: " + ItemName, "Storm Bonus", true, "Should the item give a bonus if there's a storm happening. Needs Starstorm 2 to be installed.").Value;
            ssStormBoost = config.Bind<float>("Item: " + ItemName, "Storm bonus boost", 2f, "Amount to multiply for the current effects while a storm is happening.").Value;
        }

        private void CreateBuff()
        {
            var envBuffDef = new R2API.CustomBuff(
            new RoR2.BuffDef
            {
                buffColor = Color.white,
                canStack = true, //should never get past 2 stacks, however...
                isDebuff = false,
                name = "Environment Radio",
                iconPath = "@TurboEdition:Assets/Textures/Icons/Buffs/envradio.png"
            });
            envBuff = R2API.BuffAPI.Add(envBuffDef);
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
            On.RoR2.CharacterMaster.Respawn += CharacterMaster_Respawn;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            //I wonder, whats better, applying armor via reflection, or doing it on self and do the orig call at the end?

            if (self.HasBuff(envBuff))
            {
                var InventoryCount = GetCount(self);
                float armorBonus = 0;
                float hpregenBonus = 0;

                if (scenesDay.Contains(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
                {
                }

                if (scenesNight.Contains(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
                {
                    armorBonus += (4 * Run.instance.stageClearCount + 1);
                }

                if (TurboEdition.starstormInstalled && ssStormBonus)
                {
                    //Not checking if event director is alive because for a card to be selected the director has to exist
                    //I wanted this to be "On Selected" and not just when its Active but so be it
                    //Also this is getting all event cards? ie if theres a different type of event it will also trigger in that event
                    if (eventCard.isActive)
                    {
                        armorBonus *= ssStormBoost;
                        hpregenBonus *= ssStormBoost;
                    }
                }
            }
            self.regen;

            orig(self);
        }

        private CharacterBody CharacterMaster_Respawn(On.RoR2.CharacterMaster.orig_Respawn orig, CharacterMaster self, Vector3 footPosition, Quaternion rotation, bool tryToGroundSafely)
        {
            var InventoryCount = GetCount(self.GetBody());
            if (InventoryCount > 0)
            {
                if (!(SceneCatalog.mostRecentSceneDef.sceneType == SceneType.Stage) && !hiddenRealmsBonus)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning(ItemName + " Scene is not a stage (Intermission) and hiddenRealmsBonus is FALSE.");
#endif
                    return orig(self, footPosition, rotation, tryToGroundSafely);
                }

                int buffDuration = (durationInitial + ((InventoryCount - 1) * durationStack));
                Run.FixedTimeStamp buffDurationToStamp = Run.FixedTimeStamp.zero + buffDuration;
                float buffTime = buffDurationToStamp - Stage.instance.entryTime; //You cant believe how fucking stupid I find this out to be
                if (buffTime > 0)
                {
                    self.GetBody().AddTimedBuff(envBuff, buffTime);
                }
            }
            return orig(self, footPosition, rotation, tryToGroundSafely);
        }

        //iirc Hidden Realms aren't considered "stages" but "intermissions", adding them anyways
        public static String[] scenesHRs = new String[]
        {
            "arena",
            "artifactworld",
            "bazaar",
            "goldshores",
            "limbo",
            "mysteryspace"
        };
        public static String[] scenesDay = new String[]
        {
            "blackbeach",
            "foggyswamp",
            "golemplains",
            "golemplains2",
            "goolake",
            "rootjungle",
            "shipgraveyard",
            "skymeadow",
            "wispgraveyard"
        };
        public static String[] scenesNight = new String[]
        {
            "frozenwall",
            "goldshores"
        };

        //Attacks may apply slow
        public static String[] effectIce = new String[]
        {
            "icewall"
        };
        //Attacks may apply weak
        public static String[] effectPoisonWeak = new String[]
        {
            "foggyswamp",
            "rootjungle"
        };
        //Attacks may apply goo
        public static String[] effectGoo = new String[]
        {
            "goolake"
        };
        //Attacks may ignite
        public static String[] effectFire = new String[]
        {
            "wispgraveyard"
        };
        //Attacks may shock
        public static String[] effectElectro = new String[]
        {
            "shipgraveyard"
        };
    }
}*/
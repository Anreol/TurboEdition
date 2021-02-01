using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static TurboEdition.Utils.ItemHelpers;

namespace TurboEdition.Items
{
    public class AddTeleporterRadius : ItemBase
    {
        public override string ItemName => "Expanse Expander";

        public override string ItemLangTokenName => "ADD_TELEPORTER_RADIUS";

        public override string ItemPickupDesc => "Increase the teleporter zone.";

        public override string ItemFullDescription => $"Increase teleporter radius by <style=cIsUtility>{addFirstRadius} meters</style>. <style=cStack>(+{addStackRadius} meters per stack).</style>";

        public override string ItemLore => "Yet another mod that adds an item that increases Teleporter Radius, how creative are we today huh?";
        public override ItemTier Tier => ItemTier.Tier1;

        private Run.FixedTimeStamp enabledAtTime;
        private TeleporterInteraction currentTele;


        public override string ItemModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        public override string ItemIconPath => "@TurboEdition:Assets/Textures/Icons/Items/Tier1.png";


        //Item properties
        public float addFirstRadius;
        public float addStackRadius;
        public float startupDelay;
        public float rampupTime;
        public float itemStackingCap;

        public override void CreateConfig(ConfigFile config)
        {
            addFirstRadius = config.Bind<float>("Item: " + ItemName, "Added radius per first item", 8f, "Extend the Teleporter Radius by this on first item pickup.").Value;
            addStackRadius = config.Bind<float>("Item: " + ItemName, "Added radius per stack", 1f, "Extend the Teleporter Radius by this on item stacking.").Value;
            startupDelay = config.Bind<float>("Item: " + ItemName, "Delay in seconds", 6f, "Delay in seconds for the item to be active, Focus Convergence has 3 seconds.").Value;
            rampupTime = config.Bind<float>("Item: " + ItemName, "Delay in seconds", 8f, "Delay in seconds for the teleporter radius be fully expanded").Value;
            itemStackingCap = config.Bind<int>("Item: " + ItemName, "Max items per team", -1, "Maximum amount of items a whole team can have. Keep at -1 for no limit.").Value;
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
            TeleporterInteraction.onTeleporterBeginChargingGlobal += ExpandTeleporterRadius;
            On.RoR2.Run.FixedUpdate += UpdateRadiusSize;
        }

        private void ExpandTeleporterRadius(TeleporterInteraction teleporterobject)
        {
            currentTele = teleporterobject;
            this.enabledAtTime = Run.FixedTimeStamp.now;
            //Using the team that activated the teleporter, if not, use TeamIndex.Player to get the player team.
            int TeamCount = Util.GetItemCountForTeam(teleporterobject.holdoutZoneController.chargingTeam, this.Index, true, false);
            int LivingPlayers = Run.instance.livingPlayerCount;
            if (TeamCount == 0) return;
            teleporterobject.holdoutZoneController.baseRadius += (((TeamCount - LivingPlayers) * addStackRadius) + (LivingPlayers * addFirstRadius));
        }

        private void UpdateRadiusSize(On.RoR2.Run.orig_FixedUpdate orig, Run self)
        {
            orig(self);
            //TODO get item count
            this.currentFocusConvergenceCount = Util.GetItemCountForTeam(ExpandTeleporterRadius.chargingTeam, this.Index, true, false);
            if (this.enabledAtTime.timeSince < this.startupDelay) return;
            //TODO get item count
            if(itemStackingCap != -1)
            {
                this.itemcount = Mathf.Min(itemcount, itemStackingCap);
            }

            //I have absolutely no idea what this does, but I assume it sets the HoldoutZoneController's current value (radius? color?) to the float from the smooth transition thing
            //Totally unrelated to the actual size and function but ¯\_(ツ)_/¯
            float intToFloat = ((float)this.itemcount > 0f) ? 1f : 0f;
            float smoothTransition = Mathf.MoveTowards(Reflection.GetFieldValue<float>(currentTele.holdoutZoneController, "currentValue"), intToFloat, rampupTime * Time.fixedDeltaTime);
            Reflection.SetFieldValue<float>(currentTele.holdoutZoneController, "currentValue", smoothTransition);
        }
    }
}
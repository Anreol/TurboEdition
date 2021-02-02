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
    public class AddTeleporterRadius : ItemBase<AddTeleporterRadius>
    {
        public override string ItemName => "Expanse Expander";

        public override string ItemLangTokenName => "ADDTELEPORTERRADIUS";

        public override string ItemPickupDesc => "Increase the teleporter zone.";

        public override string ItemFullDescription => $"Increase teleporter radius by <style=cIsUtility>{addFirstRadius} meters</style>. <style=cStack>(+{addStackRadius} meters per stack).</style>";

        public override string ItemLore => "Yet another mod that adds an item that increases Teleporter Radius, how creative are we today huh?";
        public override ItemTier Tier => ItemTier.Tier1;

        private Run.FixedTimeStamp enabledAtTime;
        private TeleporterInteraction currentTele;
        private float lastRadiusCalculated;

        //For Item Counts
        private int TeamCount;
        private int UniqueItemsInTeam;


        public override string ItemModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        public override string ItemIconPath => "@TurboEdition:Assets/Textures/Icons/Items/Tier1.png";


        //Item properties
        public float addFirstRadius;
        public float addStackRadius;
        public float startupDelay;
        public float rampupTime;
        public int itemStackingCap;

        public override void CreateConfig(ConfigFile config)
        {
            addFirstRadius = config.Bind<float>("Item: " + ItemName, "Added radius per first item", 8f, "Extend the Teleporter Radius by this on first item pickup.").Value;
            addStackRadius = config.Bind<float>("Item: " + ItemName, "Added radius per stack", 1f, "Extend the Teleporter Radius by this on item stacking.").Value;
            startupDelay = config.Bind<float>("Item: " + ItemName, "Delay in seconds", 6f, "Delay in seconds for the item to be active, Focus Convergence has 3 seconds.").Value;
            rampupTime = config.Bind<float>("Item: " + ItemName, "Delay in seconds", 8f, "Delay in seconds for the teleporter radius be fully expanded").Value;
            itemStackingCap = config.Bind<int>("Item: " + ItemName, "Max teleporter radius", -1, "Maximum radius that the teleporter can expand, probably in meters. Keep at -1 for no limit.").Value;
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
            TeleporterInteraction.onTeleporterBeginChargingGlobal += Begin;
            On.RoR2.HoldoutZoneController.FixedUpdate += UpdateRadiusSize;
        }

        private void Begin(TeleporterInteraction teleporterobject)
        {
#if DEBUG
            Chat.AddMessage("Turbo Edition: " + ItemName + " now working. Check log for details.");
            enabledAtTime = Run.FixedTimeStamp.now;
            currentTele = teleporterobject;
            TurboEdition._logger.LogWarning("TE: enabledAtTime " + enabledAtTime);
            TurboEdition._logger.LogWarning("TE: currentTele " + currentTele);
            TurboEdition._logger.LogWarning("TE: teleporterobject " + teleporterobject);
            TurboEdition._logger.LogWarning("TE: " + ItemName + " about to do ExpandTeleporterRadius, CheckForItemChange " + CheckForItemChange() + " lastRadiusCalculated " + lastRadiusCalculated);
            Chat.AddMessage("TE: if you don't see a CHECK! now, panic!");
#endif
            ExpandTeleporterRadius(teleporterobject);
        }

        private bool CheckForItemChange()
        {
            //Lets get the number of unique items that alive players have
            UniqueItemsInTeam = GetUniqueItemCountForTeam(currentTele.holdoutZoneController.chargingTeam, Index, true, false);
            TeamCount = Util.GetItemCountForTeam(currentTele.holdoutZoneController.chargingTeam, Index, true, false);
            float newCalculatedRadius = CalculateRadiusIncrease(UniqueItemsInTeam, TeamCount);
            if (newCalculatedRadius == lastRadiusCalculated)
            {
                return false;
            }
            lastRadiusCalculated = newCalculatedRadius;
#if DEBUG
            Chat.AddMessage("Turbo Edition: " + ItemName + " item counts and radius recalculated. Check log for details.");
            TurboEdition._logger.LogWarning("TE: UniqueItemsInTeam " + UniqueItemsInTeam);
            TurboEdition._logger.LogWarning("TE: TeamCount " + TeamCount);
            TurboEdition._logger.LogWarning("TE: lastRadiusCalculated " + lastRadiusCalculated);
#endif
            return true;
        }

        private float CalculateRadiusIncrease(float firstStack, float normalStacks)
        {
            return (firstStack * addFirstRadius) + ((normalStacks - firstStack) * addStackRadius);
        }
        private void ExpandTeleporterRadius(TeleporterInteraction teleporterobject)
        {
#if DEBUG
            Chat.AddMessage("Turbo Edition: " + ItemName + " CHECK!");
#endif
            //currentTele = teleporterobject;
            //Using the team that activated the teleporter, if not, use TeamIndex.Player to get the player team.
            if (TeamCount == 0) return;
            teleporterobject.holdoutZoneController.baseRadius += lastRadiusCalculated;
#if DEBUG
            Chat.AddMessage("Turbo Edition: " + ItemName + " expanding teleporter radius, check logs.");
            TurboEdition._logger.LogWarning("TE: " + ItemName + " lastRadiusCalculated " + lastRadiusCalculated);
            TurboEdition._logger.LogWarning("TE: " + ItemName + " baseRadius " + teleporterobject.holdoutZoneController.baseRadius);
#endif
        }

        private void UpdateRadiusSize(On.RoR2.HoldoutZoneController.orig_FixedUpdate orig, Run self)
        {
            orig(self);
            //Check if there was any changes, since this is now constantly on Fixed update, it will run forever, so lets save some stuff
            if (!CheckForItemChange())
            {
                //Chat.AddMessage("Turbo Edition: " + ItemName + " there was no item change, won't update teleporter radius.");
                return;
            }
            Chat.AddMessage("Turbo Edition: " + ItemName + " item count changed, updating teleporter radius.");
            //if (!currentTele) { return; }
            float currentItemCount = this.lastRadiusCalculated;
            if (enabledAtTime.timeSince < startupDelay) return;
            if(itemStackingCap != -1)
            {  
                currentItemCount = Mathf.Min(currentItemCount, itemStackingCap);
#if DEBUG
                Chat.AddMessage("Turbo Edition: " + ItemName + " max stack is on, check log for details");
                TurboEdition._logger.LogWarning("TE: " + ItemName + " currentItemCount" + currentItemCount);
                TurboEdition._logger.LogWarning("TE: " + ItemName + " itemStackingCap" + itemStackingCap);
#endif
            }

            ExpandTeleporterRadius(currentTele);
            //I have absolutely no idea what this does, but I assume it sets the HoldoutZoneController's current value (radius? color?) to the float from the smooth transition thing
            //Totally unrelated to the actual size and function but ¯\_(ツ)_/¯
            float intToFloat = ((float)currentItemCount > 0f) ? 1f : 0f;
            float smoothTransition = Mathf.MoveTowards(Reflection.GetFieldValue<float>(currentTele.holdoutZoneController, "currentValue"), intToFloat, rampupTime * Time.fixedDeltaTime);
            Reflection.SetFieldValue<float>(currentTele.holdoutZoneController, "currentValue", smoothTransition);
#if DEBUG
            TurboEdition._logger.LogWarning("TE: " + ItemName + "currentTele.holdoutZoneController currentValue " + Reflection.GetFieldValue<float>(currentTele.holdoutZoneController, "currentValue"));
#endif
        }
    }
}
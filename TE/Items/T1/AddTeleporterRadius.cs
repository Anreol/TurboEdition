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
        private float lastRadiusCalculated;

        private float currentValue;
        private float lastIncreaseBy;
        private float newCalculatedRadius;

        private static readonly AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        private static readonly Color newMaterialColor = new Color(0f, 3.9411764f, 5f, 1f);


        //For Item Counts
        private int TeamCount;
        private int UniqueItemsInTeam;


        public override string ItemModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        public override string ItemIconPath => "@TurboEdition:Assets/Textures/Icons/Items/Tier1.png";


        //Item properties
        public float addFirstRadius;
        public float addStackRadius;
        public float startupDelay; //shouldnt this be a Run.FixedTimeStamp to compare to enabledAtTime??
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
            TeleporterInteraction.onTeleporterBeginChargingGlobal += GetMinorThings;
            //On.RoR2.TeleporterInteraction.FixedUpdate += UpdateRadiusSize;
            On.RoR2.HoldoutZoneController.FixedUpdate += UpdateRadiusSize;
        }

        private void GetMinorThings(TeleporterInteraction teleporterInteraction)
        {
            enabledAtTime = Run.FixedTimeStamp.now;
        }

        private void OnEnable(HoldoutZoneController self)
        {
            self.calcColor += ChangeColor;
            self.calcRadius += ChangeRadius;
        }
        private void OnDisable(HoldoutZoneController self)
        {
            self.calcColor -= ChangeColor;
            self.calcRadius -= ChangeRadius;
        }
        private void ChangeColor(ref Color color)
        {
            color = Color.Lerp(color, newMaterialColor, colorCurve.Evaluate(currentValue));
        }

        private void ChangeRadius(ref float radius)
        {
            if (newCalculatedRadius > 0)
            {
                radius = newCalculatedRadius;
            }
        }

        private bool CheckForItemChange(float thingyToCheck)
        {
            //Lets get the number of unique items that alive players have
            if (thingyToCheck == lastRadiusCalculated)
            {
                return false;
            }
            lastRadiusCalculated = thingyToCheck;
            #if DEBUG
            Chat.AddMessage("Turbo Edition: " + ItemName + " item counts and radius recalculated. Check log for details.");
            TurboEdition._logger.LogWarning("TE: lastRadiusCalculated " + lastRadiusCalculated);
            #endif
            return true;
        }

        private float CalculateRadiusIncrease(float firstStack, float normalStacks)
        {
            if (normalStacks > 0)
            {
                float calc = (firstStack * addFirstRadius) + ((normalStacks - firstStack) * addStackRadius);
                #if DEBUG
                TurboEdition._logger.LogWarning("TE: calculated the radius we have to increase by: " + calc);
                #endif
                return calc;
            }
            return 0;
        }


        private void UpdateRadiusSize(On.RoR2.HoldoutZoneController.orig_FixedUpdate orig, HoldoutZoneController self)
        {
            orig(self);
            TeleporterInteraction CurrentTele = self.GetComponent<TeleporterInteraction>();
            if (!CurrentTele) { return; }

            UniqueItemsInTeam = GetUniqueCountFromPlayers(Index, true);
            TeamCount = GetCountFromPlayers(Index, true);
            #if DEBUG
            TurboEdition._logger.LogWarning("TE: Item Index for " + ItemName + ": " + Index);
            TurboEdition._logger.LogWarning("TE: UniqueItemsInTeam " + UniqueItemsInTeam);
            TurboEdition._logger.LogWarning("TE: TeamCount " + TeamCount);
            #endif
            newCalculatedRadius = CalculateRadiusIncrease(UniqueItemsInTeam, TeamCount);
            //Check if there was any changes, since this is now constantly on Fixed update, it will run forever, so lets save some stuff
            if (CheckForItemChange(newCalculatedRadius))
            {
                //Chat.AddMessage("Turbo Edition: " + ItemName + " there was no item change, won't update teleporter radius.");
                return;
            }
            //Chat.AddMessage("Turbo Edition: " + ItemName + " item count changed, updating teleporter radius.");
            //if (enabledAtTime.timeSince < startupDelay) return;
            if (itemStackingCap != -1)
            {
                newCalculatedRadius = Mathf.Min(newCalculatedRadius, itemStackingCap);
                #if DEBUG
                Chat.AddMessage("Turbo Edition: " + ItemName + " max stack is on, check log for details");
                TurboEdition._logger.LogWarning("TE: " + ItemName + " currentItemCount" + newCalculatedRadius);
                TurboEdition._logger.LogWarning("TE: " + ItemName + " itemStackingCap" + itemStackingCap);
                #endif
            }

            //I have absolutely no idea what this does, but I assume it sets the HoldoutZoneController's current value (radius? color?) to the float from the smooth transition thing
            //Totally unrelated to the actual size and function but ¯\_(ツ)_/¯
            float intToFloat = (newCalculatedRadius > 0f) ? 1f : 0f;
            float smoothTransition = Mathf.MoveTowards(currentValue, intToFloat, rampupTime * Time.fixedDeltaTime);
            if(currentValue < 0f && smoothTransition > 0f)
            {
                //we play a cool sound here indicating the zone is getting bigger
            }
            currentValue = smoothTransition;
            #if DEBUG
            TurboEdition._logger.LogWarning("TE: " + ItemName + " currentValue " + currentValue);
            #endif
            ExpandTeleporterRadius(CurrentTele, newCalculatedRadius);
        }

        private void ExpandTeleporterRadius(TeleporterInteraction obj, float increaseBy)
        {
#if DEBUG
            Chat.AddMessage("Turbo Edition: " + ItemName + " now working. Check log for details.");
            //TurboEdition._logger.LogWarning("TE: enabledAtTime " + enabledAtTime);
            TurboEdition._logger.LogWarning("TE: obj " + obj);
            TurboEdition._logger.LogWarning("TE: increaseBy " + increaseBy);
#endif
            //Using the team that activated the teleporter, if not, use TeamIndex.Player to get the player team.
            if (Util.GetItemCountForTeam(obj.holdoutZoneController.chargingTeam, Index, true, false) == 0) return;
            //OnEnable(obj.holdoutZoneController);
            obj.holdoutZoneController.baseRadius += increaseBy;
            lastIncreaseBy = increaseBy;
#if DEBUG
            Chat.AddMessage("Turbo Edition: " + ItemName + " expanding teleporter radius, check logs.");
            TurboEdition._logger.LogWarning("TE: " + ItemName + " increaseBy " + increaseBy);
            TurboEdition._logger.LogWarning("TE: " + ItemName + " lastIncreaseBy " + lastIncreaseBy);
            TurboEdition._logger.LogWarning("TE: " + ItemName + " baseRadius " + obj.holdoutZoneController.baseRadius);
#endif
        }

    }
}
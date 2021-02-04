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
        public override bool AIBlacklisted => true;

        private static Run.FixedTimeStamp enabledAtTime;
        private static float lastRadiusCalculated;
        private static TeleporterInteraction CurrentTele;

        private static float currentValue;
        private static float newCalculatedRadius;

        //For colors
        private static float mSum;
        private static float rVal = 0.7f, gVal = 0.7f, bVal = 0.2f;
        float rMove = -0.1f, gMove = 0.1f, bMove = -0.1f;

        private static readonly AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        private static Color newMaterialColor; // = new Color(rVal, gVal, bVal, 1f);

        //For Item Counts
        private static int TeamCount;
        private static int UniqueItemsInTeam;


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
            addFirstRadius = config.Bind<float>("Item: " + ItemName, "Added radius per first item", 6f, "Extend the Teleporter Radius by this on first item pickup.").Value;
            addStackRadius = config.Bind<float>("Item: " + ItemName, "Added radius per stack", 2.5f, "Extend the Teleporter Radius by this on item stacking.").Value;
            startupDelay = config.Bind<float>("Item: " + ItemName, "Delay in seconds", 6f, "Delay in seconds for the item to be active, Focus Convergence has 3 seconds.").Value;
            rampupTime = config.Bind<float>("Item: " + ItemName, "Delay in seconds", 16f, "Delay in seconds for the teleporter radius be fully expanded").Value;
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
            On.RoR2.HoldoutZoneController.FixedUpdate += UpdateRadiusSize;
        }

        private void GetMinorThings(TeleporterInteraction teleporterInteraction)
        {
            enabledAtTime = Run.FixedTimeStamp.now;
            if (newMaterialColor != Color.clear)
            {
                newMaterialColor = new Color(rVal, gVal, bVal, 1f);
            }
        }

        private void UpdateRadiusSize(On.RoR2.HoldoutZoneController.orig_FixedUpdate orig, HoldoutZoneController self)
        {
            orig(self);
            CurrentTele = self.GetComponent<TeleporterInteraction>();
            if (!CurrentTele) { return; }
            if (Util.GetItemCountForTeam(self.chargingTeam, ItemCatalog.GetItemDef(cIndex).itemIndex, true, false) == 0) return;

            UniqueItemsInTeam = GetUniqueCountFromPlayers(ItemCatalog.GetItemDef(cIndex).itemIndex, true);
            TeamCount = GetCountFromPlayers(ItemCatalog.GetItemDef(cIndex).itemIndex, true);
            newCalculatedRadius = CalculateRadiusIncrease(UniqueItemsInTeam, TeamCount);

#if DEBUG
            TurboEdition._logger.LogWarning("TE: Item Index for " + ItemName + ": " + cIndex);
            TurboEdition._logger.LogWarning("TE: UniqueItemsInTeam " + UniqueItemsInTeam);
            TurboEdition._logger.LogWarning("TE: TeamCount " + TeamCount);
#endif

            //Check if there was any changes, since this is now constantly on Fixed update, it will run forever, so lets save some stuff
            if (!CheckForItemChange(newCalculatedRadius)) { return; }
            //Chat.AddMessage("Turbo Edition: " + ItemName + " item count changed, updating teleporter radius.");
            if (enabledAtTime.timeSince < startupDelay)
            {
                #if DEBUG
                TurboEdition._logger.LogWarning("TE: " + ItemName + " teleporter still not ready!");
                #endif
                return;
            }
                
            if (itemStackingCap != -1)
            {
                newCalculatedRadius = Mathf.Min(newCalculatedRadius, itemStackingCap);
#if DEBUG
                Chat.AddMessage("Turbo Edition: " + ItemName + " max stack is on, check log for details");
                TurboEdition._logger.LogWarning("TE: " + ItemName + " currentItemCount" + newCalculatedRadius);
                TurboEdition._logger.LogWarning("TE: " + ItemName + " itemStackingCap" + itemStackingCap);
#endif
            }


            //ART ATTACK
            //Makes current calculated radius a float between 1 and 0, then keeps updating the old smoothTransition
            float intToFloat = (newCalculatedRadius > 0f) ? 1f : 0f;
            float smoothTransition = Mathf.MoveTowards(currentValue, intToFloat, rampupTime * Time.fixedDeltaTime);
            if (currentValue < 0f && smoothTransition > 0f)
            {
                //we play a cool sound here indicating the zone is getting bigger
            }
            currentValue = smoothTransition;
            #if DEBUG
            TurboEdition._logger.LogWarning("TE: " + ItemName + " currentValue " + currentValue);
            #endif
            OnEnable(self);
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
            //This is supposed to work as stages, each x2, x3, and so of the base teleporter radius it will change colors
            if (CurrentTele.holdoutZoneController.currentRadius > mSum + CurrentTele.holdoutZoneController.baseRadius)
            {
                mSum += CurrentTele.holdoutZoneController.baseRadius;
                newMaterialColor = CalculateTeleporterColor(-0.1f); //-1 by default, remember, think of a color slider, you want it to go down, reverse to go up!!
            }
            else if (CurrentTele.holdoutZoneController.currentRadius < mSum + CurrentTele.holdoutZoneController.baseRadius)
            {
                mSum -= CurrentTele.holdoutZoneController.baseRadius;
                newMaterialColor = CalculateTeleporterColor(0.1f);
            }
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

        private void CalculateTeleporterColor(ref Color newMaterialColor, float colorCycle)
        {
            //What if we made the teleporter change colors?
            rVal += rMove;
            gVal += gMove;
            bVal += bMove;

            if (rVal >= 0.85f || rVal <= 0.25f)
            {
                rMove *= colorCycle;
            }
            if (gVal >= 0.85f || gVal <= 0.25f)
            {
                gMove *= colorCycle;
            }
            if (bVal >= 0.85f || bVal <= 0.25f)
            {
                bMove *= colorCycle;
            }

            newMaterialColor.r = rVal;
            newMaterialColor.g = gVal;
            newMaterialColor.b = bVal;
            #if DEBUG
            TurboEdition._logger.LogWarning("TE: newMaterialColor " + newMaterialColor);
            #endif
            //return newMaterialColor;
            //HOLY SHIT
        }
    }
}
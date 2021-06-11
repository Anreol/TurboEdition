/*using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

//TODO Teleporter radius recalculation happens with any item pickup, change it so its only recalculated when player gets or loses this item
//APPARENTLY INFUSION ACTIVATES ONINVENTORYCHANGE SO EVERY KILL WITH INFUSION FORCES AN UPDATE, GOTTA FIX THIS SOON
//AAAAAAAAAAAAAAAA
//Oh also this item doesnt have character display rules, rather, it will show up over the teleporter, because I think the teleporter gaining items is cool
namespace TurboEdition.Items
{
    public class AddTeleporterRadius : ItemBase<AddTeleporterRadius>
    {
        public override string ItemName => "Chromatic Lens";
        public override string ItemLangTokenName => "ADDTELEPORTERRADIUS";
        public override string ItemPickupDesc => "Increase the teleporter zone.";
        public override string ItemFullDescription => $"Increase teleporter radius by <style=cIsUtility>{addFirstRadius} meters</style>. <style=cStack>(+{addStackRadius} meters per stack).</style>";
        public override string ItemLore => "Yet another mod that adds an item that increases Teleporter Radius, how creative are we today huh?";

        public override ItemTier Tier => ItemTier.Tier1;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlacklisted => true;
        public override bool BrotherBlacklisted => true;

        private static TeleporterInteraction CurrentTele;

        private static float currentValue;
        private static float newCalculatedRadius;

        //For colors
        private static int nTimes;

        private static float rVal = 4f, gVal = 4f, bVal = 2f;
        private float rMove = -1f, gMove = 1f, bMove = -1f;

        private static readonly AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        private static Color newColor;

        public override GameObject ItemModel => TurboEdition.MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Default.prefab");
        public override Sprite ItemIcon => TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Items/Tier1.png");

        //Item properties
        public float addFirstRadius;

        public float addStackRadius;
        public float startupDelay; //shouldnt this be a Run.FixedTimeStamp to compare to enabledAtTime??
        public float rampupTime;
        public int itemStackingCap;

        protected override void CreateConfig(ConfigFile config)
        {
            addFirstRadius = config.Bind<float>("Item: " + ItemName, "Added radius per first item", 8f, "Extend the Teleporter Radius by this on first item pickup.").Value;
            addStackRadius = config.Bind<float>("Item: " + ItemName, "Added radius per stack", 3f, "Extend the Teleporter Radius by this on item stacking.").Value;
            startupDelay = config.Bind<float>("Item: " + ItemName, "Delay in seconds", 6f, "Delay in seconds for the item to be active, Focused Convergence has 3f.").Value;
            rampupTime = config.Bind<float>("Item: " + ItemName, "Speed", 3.5f, "Speed for the teleporter color to be changed to the custom one. Focused Convergence has 5f.").Value;
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
            On.RoR2.HoldoutZoneController.FixedUpdate += UpdateThingies;
            On.RoR2.CharacterMaster.OnInventoryChanged += CheckForItem;
        }

        private void GetMinorThings(TeleporterInteraction teleporterInteraction)
        {
            CurrentTele = teleporterInteraction;
            if (newColor != Color.clear)
            {
                newColor = new Color(rVal, gVal, bVal, 1f);
            }
            new WaitForSeconds(startupDelay);
            OnEnable(teleporterInteraction.holdoutZoneController);
        }

        private void CheckForItem(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
        {
            orig(self);
            var InventoryCount = GetCount(self);
            if (InventoryCount > 0)
            {
                newCalculatedRadius = CalculateRadiusIncrease(GetUniqueCountFromPlayers(ItemDef, true), GetCountFromPlayers(ItemDef, true));
                if (itemStackingCap != -1)
                {
                    newCalculatedRadius = ReturnRadiusIfCapped(itemStackingCap);
                }
#if DEBUG
                Chat.AddMessage("Turbo Edition: " + ItemName + " item counts, radius, recalculated.");
#endif
            }
        }

        private float ReturnRadiusIfCapped(int cap)
        {
#if DEBUG
            Chat.AddMessage("Turbo Edition: " + ItemName + " max stack is on, check log for details");
            TurboEdition._logger.LogWarning("TE: " + ItemName + " currentItemCount" + newCalculatedRadius);
            TurboEdition._logger.LogWarning("TE: " + ItemName + " itemStackingCap" + itemStackingCap);
#endif
            return Mathf.Min(newCalculatedRadius, itemStackingCap);
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

        private void UpdateThingies(On.RoR2.HoldoutZoneController.orig_FixedUpdate orig, HoldoutZoneController self)
        {
            orig(self);
            if ((GetCountFromPlayers(ItemDef, true) <= 0)) { return; };
            //ART ATTACK
            //Makes current calculated radius a float between 1 and 0, then keeps updating the old smoothTransition
            float intToFloat = (newCalculatedRadius > 0f) ? 1f : 0f;
            float smoothTransition = Mathf.MoveTowards(currentValue, intToFloat, rampupTime * Time.fixedDeltaTime);
            if (currentValue < 0f && smoothTransition > 0f)
            {
                //we play a cool sound here indicating the zone is getting bigger
            }
            //Current Value now goes to the color thing, if this isnt on a fixed update, it wont draw the teleporter radius!
            currentValue = smoothTransition;
#if DEBUG
            TurboEdition._logger.LogWarning("currentValue: " + currentValue);
            TurboEdition._logger.LogWarning("smoothTransition: " + smoothTransition);
#endif
        }

        private void ChangeColor(ref Color color)
        {
            //This is supposed to work as stages, each x2, x3, and so of the base teleporter radius (by a fourth) it will change colors
            if ((CurrentTele.holdoutZoneController.currentRadius > (nTimes * (CurrentTele.holdoutZoneController.baseRadius / 4))))
            {
                nTimes++;
                CalculateTeleporterColor(-1.2f); //-1 by default, remember, think of a color slider, you want it to go down, reverse to go up!!

#if DEBUG
                TurboEdition._logger.LogWarning("Hit a multiplier of baseRadius, newColor is: " + newColor + " and color: " + color);
#endif
            }
            else if ((CurrentTele.holdoutZoneController.currentRadius < (nTimes - 1 * (CurrentTele.holdoutZoneController.baseRadius / 4))))
            {
                nTimes--;
                CalculateTeleporterColor(1.2f);
#if DEBUG
                TurboEdition._logger.LogWarning("Dropped from a multipler of baseRadius, newColor is: " + newColor + " and color: " + color);
#endif
            }
            color = Color.Lerp(color, newColor, colorCurve.Evaluate(currentValue));
#if DEBUG
            TurboEdition._logger.LogWarning("Color didnt change, current color: " + color);
#endif
        }

        private void ChangeRadius(ref float radius)
        {
            //if (newCalculatedRadius > 0)
            {
                radius += newCalculatedRadius;
#if DEBUG
                TurboEdition._logger.LogWarning("Radius is now: " + radius);
#endif
            }
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

        private void CalculateTeleporterColor(float colorCycle)
        {

            rVal += rMove;
            gVal += gMove;
            bVal += bMove;

            if (rVal >= 5.99f || rVal <= 1f)
            {
                rMove *= colorCycle;
            }
            if (gVal >= 5.99f || gVal <= 1f)
            {
                gMove *= colorCycle;
            }
            if (bVal >= 5.99f || bVal <= 1f)
            {
                bMove *= colorCycle;
            }

            newColor.r = rVal;
            newColor.g = gVal;
            newColor.b = bVal;
            //the alpha is declared at the top when the teleporter activates, if newColor doesnt have a color yet.
            //lets do it either way because its giving me errors
            newColor.a = 1f;

#if DEBUG
            TurboEdition._logger.LogWarning("TE: newColor " + newColor);
#endif

            //return newColor;
            //HOLY SHIT
        }
    }
}*/
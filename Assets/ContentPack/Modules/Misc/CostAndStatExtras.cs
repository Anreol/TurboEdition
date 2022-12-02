using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Items;
using RoR2.Stats;
using RoR2.UI.LogBook;
using System;
using System.Collections.Generic;

namespace TurboEdition.Misc
{
    internal class CostAndStatExtras
    {
        public static readonly StatDef totalTeleporterOverchargerUsed = StatDef.Register("totalTeleporterOverchargerUsed", StatRecordType.Sum, StatDataType.ULong, 250.0, null);
        public static readonly StatDef highestTeleporterOverchargerUsed = StatDef.Register("highestTeleporterOverchargerUsed", StatRecordType.Max, StatDataType.ULong, 0.0, null);
        public static CostTypeDef teleporterCostType;
        public static int teleporterCostIndex;

        public static void Init()
        {
            CostTypeCatalog.modHelper.getAdditionalEntries += getAdditionalEntries;
            IL.RoR2.UI.LogBook.PageBuilder.StatsPanel += appendGeneralStats;
            Run.onClientGameOverGlobal += appendStatsToReportScreen;
        }

        private static void appendStatsToReportScreen(Run arg1, RunReport arg2)
        {
            RoR2.UI.GameEndReportPanelController gameEndReportPanelController = GameOverController.instance.gameEndReportPanelPrefab.GetComponent<RoR2.UI.GameEndReportPanelController>();
            HG.ArrayUtils.ArrayAppend<string>(ref gameEndReportPanelController.statsToDisplay, totalTeleporterOverchargerUsed.name);
            Run.onClientGameOverGlobal -= appendStatsToReportScreen; //Just do it once and desubscribe, else it keeps appending every game over.
        }

        private static void appendGeneralStats(ILContext il)
        {
            var c = new ILCursor(il);

            UserProfile userProfile = null;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<PageBuilder>>(build => userProfile = (UserProfile)build.entry.extraData);

            if (c.TryGotoNext(MoveType.After, x => x.MatchStelemAny<ValueTuple<string, string, UnityEngine.Texture>>(), x => x.MatchLdloca(out _), x => x.MatchCallOrCallvirt(typeof(RoR2.UI.LogBook.PageBuilder).GetMethod("<StatsPanel>g__SetStats|26_13", (System.Reflection.BindingFlags)(-1)))))
            {
                if (c.TryGotoPrev(x => x.MatchLdloca(out _)))
                {
                    c.EmitDelegate<Func<ValueTuple<string, string, UnityEngine.Texture>[], ValueTuple<string, string, UnityEngine.Texture>[]>>(array =>
                    {
                        List<ValueTuple<string, string, UnityEngine.Texture>> newArray = new List<ValueTuple<string, string, UnityEngine.Texture>>(array);
                        
                        //totalTeleporterOverchargerUsed stat
                        newArray.Insert(17, (Language.GetString(totalTeleporterOverchargerUsed.displayToken), userProfile.statSheet.GetStatDisplayValue(totalTeleporterOverchargerUsed), null));
                        
                        //Return
                        return newArray.ToArray();
                    });
                }
            }
        }

        private static void getAdditionalEntries(List<CostTypeDef> obj)
        {
            teleporterCostType = new CostTypeDef();
            teleporterCostType.costStringFormatToken = "COST_TELEPORTERCHARGE_FORMAT";
            teleporterCostType.isAffordable = delegate (CostTypeDef costTypeDef, CostTypeDef.IsAffordableContext context)
            {
                if (TeleporterInteraction.instance)
                {
                    NetworkUser networkUser = Util.LookUpBodyNetworkUser(context.activator.gameObject);
                    if (TeleporterInteraction.instance.holdoutZoneController && TeleporterInteraction.instance.holdoutZoneController.chargingTeam == networkUser.master.teamIndex)
                    {
                        return (TeleporterInteraction.instance.chargePercent) >= context.cost;
                    }
                }
                return false;
            };
            teleporterCostType.payCost = delegate (CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
            {
                NetworkUser networkUser = Util.LookUpBodyNetworkUser(context.activator.gameObject);
                if (TeleporterInteraction.instance && TeleporterInteraction.instance.holdoutZoneController.chargingTeam == networkUser.master.teamIndex)
                {
                    TeleporterInteraction.instance.holdoutZoneController.charge -= context.cost / 100f; //Divide by 100, as the charge is normalized from 0 to 1!
                }
                MultiShopCardUtils.OnNonMoneyPurchase(context);
            };
            teleporterCostType.colorIndex = ColorCatalog.ColorIndex.Teleporter;
            teleporterCostType.saturateWorldStyledCostString = true;
            teleporterCostType.darkenWorldStyledCostString = false;
            teleporterCostIndex = CostTypeCatalog.costTypeDefs.Length + obj.Count; //Get base length plus the length of the current list
            obj.Add(teleporterCostType);
        }
    }
}
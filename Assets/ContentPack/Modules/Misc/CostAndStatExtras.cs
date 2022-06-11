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
        public static readonly StatDef totalTeleporterOverchargerUsed = StatDef.Register("totalTeleporterOverchargerUsed", StatRecordType.Sum, StatDataType.ULong, 100.0, null);
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
        }

        private static void appendGeneralStats(ILContext il)
        {
            var c = new ILCursor(il);

            UserProfile userProfile = null;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<PageBuilder>>(build => userProfile = (UserProfile)build.entry.extraData);

            /*if (c.TryGotoNext(MoveType.After, x => x.MatchCastclass<UserProfile>(), x => x.MatchStloc(out _)))
            {
                c.Emit(OpCodes.Ldloc_1);
                c.EmitDelegate<Action<UserProfile>>(userProf => { UnityEngine.Debug.Log(userProf); userProfile = userProf;});

            }*/
            /*
            //Go to the first addition to the array
            c.GotoNext(x => x.OpCode == OpCodes.Ldsfld && x.MatchLdsfld(typeof(StatDef), nameof(RoR2.Stats.StatDef.totalGamesPlayed)));

            //Go back and find instructions with a local int and right after it a new array
            int baseArrayLength = 0;
            if (c.TryGotoPrev(x => x.MatchLdcI4(out baseArrayLength), x => x.MatchNewarr<ValueTuple<string, string, UnityEngine.Texture>>()))
            {
                //Get the int that sets the length of the array, add X to it. Next.Operand because the cursor, when moving, by default moves BEFORE the match it has found.
                c.Next.Operand = (sbyte)(baseArrayLength + 1);

                //find instructions with this same order, to position ourselves and to append our entry before it.
                if (c.TryGotoNext(x => x.MatchLdcI4(out _), x => x.MatchLdloc(out _), x => x.MatchLdsfld(typeof(StatDef), nameof(RoR2.Stats.StatDef.totalCrocoInfectionsInflicted))))
                {
                    //Add our new entry to array
                    /*c.Emit(OpCodes.Ldloc_0);
                    c.EmitDelegate<Action<ValueTuple<string, string, UnityEngine.Texture>[]>>(array =>
                    {
                        ValueTuple<string, string, UnityEngine.Texture> result;
                        result.Item1 = Language.GetString(totalTeleporterOverchargerUsed.displayToken);
                        result.Item2 =
                        HG.ArrayUtils.ArrayAppend<ValueTuple<string, string, UnityEngine.Texture>>(ref array, result);
                    });*/

            //After our new entry has been added, find every next entries index. This might cause issues if appending is done at the very last entry.
            /*do
            {
                //Then add one to their index so they are added to the array X elements later, compensanting for our new element.
                int index = -1;
                c.GotoNext(x => x.MatchLdcI4(out index));
                if (index <= 255)
                {
                    c.Next.Operand = (sbyte)(index + 1);
                }
                else //if its larger than a byte, change opcode type to support longer numbers
                {
                    c.Next.OpCode = OpCodes.Ldc_I4;
                    c.Next.Operand = (index + 1);
                }
            }
            while (c.TryGotoNext(MoveType.After, x => x.MatchNewobj<ValueTuple<string, string, UnityEngine.Texture>>(), x => x.MatchStelemAny<ValueTuple<string, string, UnityEngine.Texture>>()) && c.Next.OpCode == OpCodes.Dup);
        }
    }*/
            if (c.TryGotoNext(MoveType.After, x => x.MatchStelemAny<ValueTuple<string, string, UnityEngine.Texture>>(), x => x.MatchLdloca(out _), x => x.MatchCallOrCallvirt(typeof(RoR2.UI.LogBook.PageBuilder).GetMethod("<StatsPanel>g__SetStats|26_13", (System.Reflection.BindingFlags)(-1)))))
            {
                if (c.TryGotoPrev(x => x.MatchLdloca(out _)))
                {
                    c.EmitDelegate<Func<ValueTuple<string, string, UnityEngine.Texture>[], ValueTuple<string, string, UnityEngine.Texture>[]>>(array =>
                    {
                        List<ValueTuple<string, string, UnityEngine.Texture>> newArray = new List<ValueTuple<string, string, UnityEngine.Texture>>(array);
                        newArray.Insert(17, (Language.GetString(totalTeleporterOverchargerUsed.displayToken), userProfile.statSheet.GetStatDisplayValue(totalTeleporterOverchargerUsed), null));
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
                    TeleporterInteraction.instance.holdoutZoneController.charge -= context.cost;
                }
                MultiShopCardUtils.OnNonMoneyPurchase(context);
            };
            teleporterCostType.colorIndex = ColorCatalog.ColorIndex.Teleporter;
            teleporterCostType.saturateWorldStyledCostString = true;
            teleporterCostType.darkenWorldStyledCostString = false;
            teleporterCostIndex = CostTypeCatalog.costTypeDefs.Length + obj.Count;
            obj.Add(teleporterCostType);
        }
    }
}
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.Misc
{
    class CostExtras
    {
        static CostTypeDef teleporterCostType;
        public static void Init()
        {
            CostTypeCatalog.modHelper.getAdditionalEntries += getAdditionalEntries;
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
            obj.Add(teleporterCostType);
        }
    }
}

using RoR2;

namespace TurboEdition.Items
{
    public static class BloodEconomyManager
    {
        private static BloodSiphonNearbyController bloodSiphonNearbyController;

        [SystemInitializer(typeof(PickupCatalog))]
        public static void Initialize()
        {
            On.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += PurchaseInteraction_CanBeAffordedByInteractor;
        }

        private static bool PurchaseInteraction_CanBeAffordedByInteractor(On.RoR2.PurchaseInteraction.orig_CanBeAffordedByInteractor orig, PurchaseInteraction self, Interactor activator)
        {
            //This stores the result of the original function, which if its not affordable, should be false.
            bool hookResult = orig(self, activator);

            //Make sure its money.
            if (CostTypeCatalog.GetCostTypeDef(self.costType) == CostTypeCatalog.GetCostTypeDef(CostTypeIndex.Money) && !hookResult)
            {
            }
            return hookResult;
        }

    }
}
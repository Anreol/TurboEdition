using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboEdition.Utils
{
    public class TurboUtils
    {
		public static PickupPickerController.Option[] GetOptionsFromPickupIndex(int numOptions, PickupIndex pickupIndex, Xoroshiro128Plus rng)
		{
			PickupIndex[] groupFromPickupIndex = PickupTransmutationManager.GetGroupFromPickupIndex(pickupIndex);
			if (groupFromPickupIndex == null)
			{
				return new PickupPickerController.Option[]
				{
					new PickupPickerController.Option
					{
						available = true,
						pickupIndex = pickupIndex
					}
				};
			}
			PickupPickerController.Option[] array = new PickupPickerController.Option[numOptions];
			for (int i = 0; i < numOptions; i++)
			{
				PickupIndex pickupIndex2 = groupFromPickupIndex[UnityEngine.Random.Range(0, groupFromPickupIndex.Length)];
				array[i] = new PickupPickerController.Option
				{
					available = Run.instance.IsPickupAvailable(pickupIndex2),
					pickupIndex = pickupIndex2
				};
			}
			return array;
		}

		public static PickupDef FindMostValuablePickupInOptions(PickupPickerController.Option[]  options)
        {
			PickupDef mostValuableSoFar = PickupCatalog.GetPickupDef(options[0].pickupIndex);
            foreach (var option in options)
            {
				PickupDef pickupDef = PickupCatalog.GetPickupDef(option.pickupIndex);
                if (pickupDef.itemTier > mostValuableSoFar.itemTier)
                {
					mostValuableSoFar = pickupDef;

				}
            }
			return mostValuableSoFar;
        }
	}
}

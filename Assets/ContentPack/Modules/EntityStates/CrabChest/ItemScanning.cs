using EntityStates;
using RoR2;
using RoR2.DirectionalSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.EntityStates.CrabChest.ItemScanner
{
    class ItemScanning : BaseState
    {
		public int numTimesToScan;
		private PickupSearch pickupSearch;


		//Shit from EquipmentSlot because for some reason its not static
		private GenericPickupController FindPickupController(Ray aimRay, float maxAngle, float maxDistance, bool requireLoS, bool requireTransmutable)
		{
			if (this.pickupSearch == null)
			{
				this.pickupSearch = new PickupSearch();
			}
			float num;
			aimRay = CameraRigController.ModifyAimRayIfApplicable(aimRay, base.gameObject, out num);
			this.pickupSearch.searchOrigin = aimRay.origin;
			this.pickupSearch.searchDirection = aimRay.direction;
			this.pickupSearch.minAngleFilter = 0f;
			this.pickupSearch.maxAngleFilter = maxAngle;
			this.pickupSearch.minDistanceFilter = 0f;
			this.pickupSearch.maxDistanceFilter = maxDistance + num;
			this.pickupSearch.filterByDistinctEntity = false;
			this.pickupSearch.filterByLoS = requireLoS;
			this.pickupSearch.sortMode = SortMode.DistanceAndAngle;
			this.pickupSearch.requireTransmutable = requireTransmutable;
			return this.pickupSearch.SearchCandidatesForSingleTarget<List<GenericPickupController>>(InstanceTracker.GetInstancesList<GenericPickupController>());
		}
	}
}

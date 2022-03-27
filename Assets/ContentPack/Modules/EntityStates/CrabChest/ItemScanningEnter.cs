using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace TurboEdition.EntityStates.CrabChest.ItemScanner
{
    internal class ItemScanningEnter : BaseItemStealState
    {
        public int numMaxStrayDropletsToSteal;
        public int numMaxItemStackToSteal;
        public int maxhHeightTolerance;

        private static void GetDropletsNearby(Vector3 origin, Vector3 aimDirection, float coneAngle, float maxDist, float heightTolerance)
        {
            float cone = Mathf.Cos(coneAngle * 0.5f * 0.017453292f);
            foreach (GenericPickupController item in InstanceTracker.GetInstancesList<GenericPickupController>())
            {
                if (Vector3.Distance(origin, item.transform.position) <= maxDist)
                {
                    Vector3 normalized = (origin - item.transform.position).normalized;
                    if (Vector3.Dot(-normalized, aimDirection) >= cone && heightTolerance >= Mathf.Abs(origin.y - item.transform.position.y))
                    {

                    }
                }
            }
        }

    }
}
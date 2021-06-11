using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace TurboEdition
{
    class ItemBehaviors : CharacterBody
    {
        private void Subscribe()
        {
            base.onInventoryChanged += AddBehaviors;
        }
        private void AddBehaviors()
        {
            if (NetworkServer.active)
	        {

	        }
        }

    }
}

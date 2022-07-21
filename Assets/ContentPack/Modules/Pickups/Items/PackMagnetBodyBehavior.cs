using RoR2;
using RoR2.Audio;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    //Check whenever it should run on client or server. It currently works... and the server is already calculating most of the stuff, so maybe let it get a free pass here?
    //All packs are sent through the net.
    public class PackMagnetBodyBehavior : BaseItemBodyBehavior
    {
        private static NetworkSoundEventDef grabSound = Assets.mainAssetBundle.LoadAsset<NetworkSoundEventDef>("nseItemProcPackMagnetGrab");
        private static NetworkSoundEventDef duplicateSound = Assets.mainAssetBundle.LoadAsset<NetworkSoundEventDef>("nseItemProcPackMagnetDuplicate");

        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.PackMagnet;
        }

        private SphereSearch sphereSearch;
        private List<Collider> colliders;

        private void OnEnable()
        {
            colliders = new List<Collider>();
            sphereSearch = new RoR2.SphereSearch()
            {
                mask = LayerIndex.pickups.mask,
                queryTriggerInteraction = UnityEngine.QueryTriggerInteraction.Collide
                //We do not need to filter by team as a gravitate pickup OnTriggerEnter already does it
            };
        }

        private void FixedUpdate()
        {
            if (sphereSearch == null || !body || body.transform.position == null) //Needs to be attatched to a body so we check if its null
                return;
            sphereSearch.origin = body.transform.position;
            sphereSearch.radius = 16f + ((stack - 1) * 8f);
            //GravitationControllers have sphere colliders to check whenever a player is in radius no matter what...
            colliders.Clear();
            sphereSearch.RefreshCandidates().OrderCandidatesByDistance().GetColliders(colliders);
            foreach (var item in colliders)
            {
                if (item == null)
                    return; //How did we get here
                GravitatePickup gravitatePickup = item.gameObject.GetComponent<GravitatePickup>();
                if (gravitatePickup && gravitatePickup.gravitateTarget == null) //It does not have a gravitation target, lets take it.
                {
                    if (body.gameObject.GetComponent<Collider>())
                    {
                        gravitatePickup.OnTriggerEnter(body.gameObject.GetComponent<Collider>()); //Simulate a touch
                        EntitySoundManager.EmitSoundServer(grabSound.index, gameObject);

                        if (gravitatePickup.rigidbody)
                        {
                            DuplicateGameObject(gravitatePickup.rigidbody.gameObject, item.transform);
                        }
                    }
                }
            }
        }

        private void DuplicateGameObject(GameObject gameObject, Transform transform)
        {
            if (Util.CheckRoll(5f + ((stack - 1)), body.master.luck))
            {
                TELog.LogI("Passed luck check, duplicating.");
                EntitySoundManager.EmitSoundServer(duplicateSound.index, gameObject);

                GameObject pickup = UnityEngine.Object.Instantiate<GameObject>(gameObject, transform);
                GravitatePickup clonedGravitator = pickup.GetComponentInChildren<GravitatePickup>();
                //Duplicates pickup and makes it gravitate towards you right away, blocking it from getting magnetized and duplicated yet again
                clonedGravitator.gravitateTarget = body.transform;
                //Speeds it up
                clonedGravitator.acceleration *= 2f;
                clonedGravitator.maxSpeed *= 2f;
            }
        }
    }
}
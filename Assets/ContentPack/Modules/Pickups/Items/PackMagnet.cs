using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//TODO: Consider turning this into a prefab and pack in the sphere search and a mesh renderer
namespace TurboEdition.Items
{
    public class PackMagnet : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("PackMagnet");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<PackMagnetBehavior>(stack);
        }

        internal class PackMagnetBehavior : CharacterBody.ItemBehavior
        {
            private SphereSearch sphereSearch;

            //private static readonly AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
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
                if (!NetworkServer.active || sphereSearch == null || !body || body.transform.position == null) //Needs to be attatched to a body so we check if its null
                {
                    return;
                }
                sphereSearch.origin = body.transform.position;
                sphereSearch.radius = 8f + ((stack - 1) * 4f);
                //GravitationControllers have sphere colliders to check whenever a player is in radius no matter what...
                colliders.Clear();
                sphereSearch.RefreshCandidates().OrderCandidatesByDistance().GetColliders(colliders);
                foreach (var item in colliders)
                {
                    if (item == null)
                        return; //How did we get here
                    GravitatePickup gravitatePickup = item.gameObject.GetComponent<GravitatePickup>();
                    if (gravitatePickup && gravitatePickup.gravitateTarget == null)
                    {
                        if (body.gameObject.GetComponent<Collider>())
                        {
                            gravitatePickup.OnTriggerEnter(body.gameObject.GetComponent<Collider>());
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
                float rawChance = Mathf.Min(stack / 25, 0.5f);
                if (Util.CheckRoll(rawChance, body.master.luck))
                {
                    Debug.LogWarning("Passed luck check, duplicating.");
                    GameObject pickup = UnityEngine.Object.Instantiate<GameObject>(gameObject, transform);
                    GravitatePickup clonedGravitator = pickup.GetComponentInChildren<GravitatePickup>();
                    clonedGravitator.gravitateTarget = body.transform; 
                    //Duplicates pickup and makes it gravitate towards you right away, blocking it from getting magnetized and duplicated yet again
                }
            }
        }
    }
}
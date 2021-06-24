using RoR2;
using System.Collections.Generic;
using UnityEngine;

//TODO: Consider turning this into a prefab and pack in the sphere search and a mesh renderer
namespace TurboEdition.Items
{
    internal class PackMagnetBehavior : CharacterBody.ItemBehavior
    {
        private SphereSearch SphereSearch;

        //private static readonly AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        private List<Collider> colliders;

        private void OnEnable()
        {
            colliders = new List<Collider>();
            SphereSearch = new SphereSearch();
            SphereSearch.origin = body.transform.position;
            SphereSearch.mask = LayerIndex.entityPrecise.mask;
            SphereSearch.queryTriggerInteraction = UnityEngine.QueryTriggerInteraction.Collide;
        }

        private void FixedUpdate()
        {
            SphereSearch.radius = 8f + ((stack - 1) * 4f);
            //GravitationControllers have sphere colliders to check whenever a player is in radius no matter what... so we can get the GO, then get the component.
            SphereSearch.RefreshCandidates().OrderCandidatesByDistance().GetColliders(colliders);
            foreach (var item in colliders)
            {
                if (item.gameObject.GetComponent<GravitatePickup>())
                {
                    if (body.gameObject.GetComponent<CapsuleCollider>()) //Could probably clean up to capsuleCollider ? capsule : sphere
                    {
                        item.gameObject.GetComponent<GravitatePickup>().OnTriggerEnter(body.gameObject.GetComponent<CapsuleCollider>());
                        if (item.gameObject.GetComponent<GravitatePickup>().rigidbody)
                        {
                            DuplicateGameObject(item.gameObject.GetComponent<GravitatePickup>().rigidbody.gameObject, item.transform);
                        }
                        continue;
                    }
                    else if (body.gameObject.GetComponent<SphereCollider>()) //Extra check done because the game does this when creating a body
                    {
                        item.gameObject.GetComponent<GravitatePickup>().OnTriggerEnter(body.gameObject.GetComponent<SphereCollider>());
                        if (item.gameObject.GetComponent<GravitatePickup>().rigidbody)
                        {
                            DuplicateGameObject(item.gameObject.GetComponent<GravitatePickup>().rigidbody.gameObject, item.transform);
                        }
                    }
                }
            }
        }

        private void DuplicateGameObject(GameObject gameObject, Transform transform)
        {
            if (Util.CheckRoll(5 * stack, body.master.luck))
            {
                UnityEngine.Object.Instantiate<GameObject>(gameObject, transform);
            }
        }
    }
}
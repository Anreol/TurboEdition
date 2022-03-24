#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using RoR2;

/// <summary>
/// Tools to help with re-rigging bullshit
/// go to the previous prefab, and on most gameobjects in the hierarchy, press shift+C to copy certain components, and shift+V to paste them on the objects in the new rig.
/// </summary>
public class ReriggingBullshitTools
{
    //public static Collider storedCollider;
    //public static Rigidbody storedRigidbody;
    //public static CharacterJoint storedJoint;
    public static ChildLocator storedChildLocator;

    //public static RagdollController storedRagdollController;

    [MenuItem("Edit/copy certain components #c")]
    public static void copyComponents()
    {
        //storedCollider = Selection.activeGameObject.GetComponent<Collider>();
        //storedRigidbody = Selection.activeGameObject.GetComponent<Rigidbody>();
        //storedJoint = Selection.activeGameObject.GetComponent<CharacterJoint>();
        storedChildLocator = Selection.activeGameObject.GetComponent<ChildLocator>();
        //storedRagdollController = Selection.activeGameObject.GetComponent<RagdollController>();

        if (//storedCollider == null &&
            //storedRigidbody == null &&
            //storedJoint == null &&
            storedChildLocator == null //&&
                                       //storedRagdollController == null
                )
        {
            Debug.Log("did not copy any components");
            return;
        }

        string copied = "copied components:";

        //if (storedCollider != null)
        //    copied += "Collider, ";
        //if (storedRigidbody != null)
        //    copied += "Rigidbody, ";
        //if (storedJoint != null)
        //    copied += "\nCharacterJoint, ";
        if (storedChildLocator != null)
            copied += " ChildLocator, ";
        //if (storedRagdollController != null)
        //    copied += "\nRagdollController, ";

        Debug.Log(copied);
    }

    [MenuItem("Edit/paste copied components #v")]
    public static void pasteComponents()
    {
        GameObject selected = Selection.activeGameObject;

        Undo.RecordObject(selected, "paste copied components");

        //if (storedCollider != null) {
        //    setStoredCollider(selected);
        //}

        //if (storedRigidbody != null) {
        //    setStoredRigidBody(selected);
        //}

        //if (storedJoint != null) {
        //    setStoredJoint(selected);
        //}

        if (storedChildLocator != null)
        {
            setStoredChildLocator(selected);
        }

        //if (storedChildLocator != null) {
        //    setStoredRagdollController(selected);
        //}

        //storedCollider = null;
        //storedJoint = null;
        //storedRigidbody = null;
        storedChildLocator = null;
        //storedRagdollController = null;
    }

    //private static void setStoredCollider(GameObject selected)
    //{
    //    if (storedCollider is SphereCollider)
    //    {
    //        SphereCollider col = selected.GetComponent<SphereCollider>();
    //        if (!col)
    //        {
    //            col = selected.AddComponent<SphereCollider>();
    //        }

    //        col.center = (storedCollider as SphereCollider).center;
    //        col.radius = (storedCollider as SphereCollider).radius;

    //    }
    //    else
    //    {
    //        CapsuleCollider col = selected.GetComponent<CapsuleCollider>();
    //        if (!col)
    //        {
    //            col = selected.AddComponent<CapsuleCollider>();
    //        }

    //        col.center = (storedCollider as CapsuleCollider).center;
    //        col.radius = (storedCollider as CapsuleCollider).radius;
    //        col.height = (storedCollider as CapsuleCollider).height;
    //        col.direction = (storedCollider as CapsuleCollider).direction;
    //    }
    //}

    //private static void setStoredRigidBody(GameObject selected)
    //{
    //    Rigidbody rig = selected.GetComponent<Rigidbody>();
    //    if (!rig)
    //    {
    //        rig = selected.AddComponent<Rigidbody>();
    //    }

    //    rig.drag = storedRigidbody.drag;
    //    rig.angularDrag = storedRigidbody.angularDrag;
    //    rig.isKinematic = storedRigidbody.isKinematic;
    //    rig.useGravity = storedRigidbody.useGravity;
    //}

    //private static void setStoredJoint(GameObject selected)
    //{
    //    CharacterJoint joint = selected.GetComponent<CharacterJoint>();
    //    if (!joint)
    //    {
    //        joint = selected.AddComponent<CharacterJoint>();
    //    }

    //    bool found = false;
    //    Transform check = selected.transform.parent;
    //    Transform body = null;

    //    while (!found)
    //    {
    //        if (storedJoint.connectedBody != null)
    //        {
    //            if (check.transform.name == storedJoint.connectedBody.transform.name)
    //            {
    //                body = check;
    //                found = true;
    //            }
    //            else
    //            {
    //                check = check.transform.parent;
    //            }
    //        }
    //        else
    //        {
    //            found = true;
    //        }

    //        if (check == null)
    //        {
    //            found = true;
    //        }
    //    }

    //    if (body != null)
    //    {
    //        joint.connectedBody = body.GetComponent<Rigidbody>();
    //        if (joint.connectedBody == null)
    //        {
    //            body.gameObject.AddComponent<Rigidbody>();
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"couldn't find connectedBody, [{storedJoint.connectedBody.transform.name}], for [{selected.name}]", selected);
    //    }
    //}

    private static void setStoredChildLocator(GameObject selected)
    {
        ChildLocator locator = selected.GetComponent<ChildLocator>();
        if (locator == null)
        {
            locator = selected.AddComponent<ChildLocator>();
        }

        List<Transform> children = selected.GetComponentsInChildren<Transform>(true).ToList();

        locator.transformPairs = new ChildLocator.NameTransformPair[storedChildLocator.transformPairs.Length];

        for (int i = 0; i < locator.transformPairs.Length; i++)
        {
            ChildLocator.NameTransformPair pair = locator.transformPairs[i];
            ChildLocator.NameTransformPair storedPair = storedChildLocator.transformPairs[i];

            pair.name = storedPair.name;

            if (storedPair.transform == null)
            {
                continue;
            };

            pair.transform = children.Find(tran =>
            {
                return tran.name == storedPair.transform.name;
            });
            if (pair.transform == null)
            {
                Debug.Log($"couldn't find {pair.name} to {storedPair.transform.name}");
            }

            locator.transformPairs[i] = pair;
        }
    }

    //private static void setStoredRagdollController(GameObject selected)
    //{
    //    RagdollController controller = selected.GetComponent<RagdollController>();
    //    if (controller == null)
    //    {
    //        controller = selected.AddComponent<RagdollController>();
    //    }

    //    List<Transform> children = selected.GetComponentsInChildren<Transform>().ToList();

    //    controller.bones = new Transform[storedRagdollController.bones.Length];

    //    for (int i = 0; i < controller.bones.Length; i++)
    //    {
    //        Transform bone = controller.bones[i];
    //        Transform storedBone = storedRagdollController.bones[i];

    //        if (storedBone == null)
    //            continue;

    //        bone = children.Find(tran => { return tran.name == storedBone.name; });
    //    }
    //}
}

#endif
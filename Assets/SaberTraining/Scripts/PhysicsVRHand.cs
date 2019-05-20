using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsVRHand : MonoBehaviour
{
    public OVRInput.Controller controllerKind;
    public Material ghostHandMterial;

    public static PhysicsVRHand Left;
    public static PhysicsVRHand Right;

    public GameObject originalHand;
    public GameObject copyHand;
    public Transform[] handParts;

    public FollowRealLifeObject grabbedObject;
    public SkinnedMeshRenderer originalHandMesh;
    public Material originalHandMaterial;
    public bool usingGhostMaterial;

    void InitializeCopyHand()
    {
        if (copyHand != null) return;
        if (controllerKind == OVRInput.Controller.LTouch)
        {
            PhysicsVRHand.Left = this;
            originalHand = GameObject.Find("hand_left_renderPart_0");
        }
        if (controllerKind == OVRInput.Controller.RTouch)
        {
            PhysicsVRHand.Right = this;
            originalHand = GameObject.Find("hand_right_renderPart_0");
        }
        if (originalHand == null) return;
        originalHandMesh = originalHand.GetComponent<SkinnedMeshRenderer>();
        if (originalHandMesh) originalHandMaterial = originalHandMesh.material;
         copyHand = Object.Instantiate(originalHand);
        copyHand.transform.parent = transform;
        copyHand.SetActive(false);
        Transform[] originalHandParts = originalHand.GetComponentsInChildren<Transform>(true);
        Transform[] copyHandParts = copyHand.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < copyHandParts.Length; i++) {
            var original = originalHandParts[i].gameObject;
            var copy = copyHandParts[i].gameObject;
            if(copy != copyHand)
            {
                copy.SetActive(true);
                var mirror = copy.AddComponent<MirrorLocalTransform>();
                mirror.target = original.transform;
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        InitializeCopyHand();

        // Check grabbed object
        if (!grabbedObject) return;
        if (!grabbedObject.isGrabbed || grabbedObject.controllerKind != controllerKind) {
            grabbedObject = null;
            copyHand.SetActive(false);
        } else
        {
            copyHand.SetActive(true);
        }

        if (!grabbedObject) return;

        // Enable physics world hand
        copyHand.SetActive(grabbedObject.isCollisioning || grabbedObject.isJoiningBack);

        // Track physics object
        copyHand.transform.position = grabbedObject.physicsVRGrabSpotTransform.position;
        copyHand.transform.rotation = grabbedObject.physicsVRGrabSpotTransform.rotation;


        // Change real avatar hand material
        if (grabbedObject.isCollisioning && ghostHandMterial != null)
        {
            originalHandMesh.material = ghostHandMterial;
            usingGhostMaterial = true;
        }
        else if (usingGhostMaterial) {
            usingGhostMaterial = false;
            originalHandMesh.material = originalHandMaterial;
        }
    }
}

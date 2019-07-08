using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceFeedbackSystem
{
    public class OVRVRHand : MonoBehaviour
    {
        public OVRInput.Controller controllerKind;
        public Material ghostHandMaterial;
        public bool displayCopyHand = true;

        [Header("Set dynamically")]
        public GameObject originalHand;
        public GameObject copyHand;
        public Transform[] handParts;

        public OVRGrabbableMover grabbedObject;
        public SkinnedMeshRenderer originalHandMesh;
        public Material originalHandMaterial;
        public bool usingGhostMaterial;
        public static OVRVRHand Left;
        public static OVRVRHand Right;

        void InitializeCopyHand()
        {
            if (copyHand != null) return;
            if (controllerKind == OVRInput.Controller.LTouch)
            {
                OVRVRHand.Left = this;
                originalHand = GameObject.Find("hand_left_renderPart_0");
            }
            if (controllerKind == OVRInput.Controller.RTouch)
            {
                OVRVRHand.Right = this;
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
            for (int i = 0; i < copyHandParts.Length; i++)
            {
                var original = originalHandParts[i].gameObject;
                var copy = copyHandParts[i].gameObject;
                if (copy != copyHand)
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
            if (!grabbedObject.isGrabbed || grabbedObject.grabbingControllerKind != controllerKind)
            {
                grabbedObject = null;
                copyHand.SetActive(false);
            }
            else
            {
                copyHand.SetActive(displayCopyHand);
            }

            if (!grabbedObject) return;

            bool displayVRHandNeeded = grabbedObject.forceFeedbackSystem.status == VRForceFeedbackSystem.FFSStatus.TryingSynchronisation
                || grabbedObject.forceFeedbackSystem.status == VRForceFeedbackSystem.FFSStatus.JoiningBack;
            
            // Enable physics world hand
            copyHand.SetActive(displayCopyHand && displayVRHandNeeded);

            // Track physics object
            copyHand.transform.position = grabbedObject.forceFeedbackSystem.vrWorldObject.transform.position + grabbedObject.grabSpotLocalPosition;
            copyHand.transform.rotation = grabbedObject.grabSpotLocalRotation * grabbedObject.forceFeedbackSystem.vrWorldObject.transform.rotation ;


            // Change real avatar hand material
            if (displayVRHandNeeded && ghostHandMaterial != null)
            {
                originalHandMesh.material = ghostHandMaterial;
                usingGhostMaterial = true;
            }
            else if (usingGhostMaterial)
            {
                usingGhostMaterial = false;
                originalHandMesh.material = originalHandMaterial;
            }
        }
    } 
}

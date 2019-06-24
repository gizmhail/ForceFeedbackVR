using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceFeedbackSystem
{
    [RequireComponent(typeof(Rigidbody))]

    public class VRForceFeedbackSystem : MonoBehaviour, ForceFollowObject.ICollisionHandler
    {
        public Material ghostMetarial;
        public GameObject vrWorldObject;

        public bool isGrabbed = false;
        public enum FFSStatus
        {
            Ungrabbed,
            Grabbed,
            TryingSynchronisation,
            JoiningBack,
            JoinedBack
        }

        public FFSStatus status;

        [Header("Layer masks (to avoid collisions for instance)")]
        public string irlLayerName = "RealLifeVR";
        public string vrWorldLayerName = "GameVR";
        
        MeshRenderer[] irlMeshes;
        ForceFollowObject vrFollow;

        // Start is called before the first frame update
        void Start()
        {
            ActivateFFS();
        }

        // Update is called once per frame
        void Update()
        {
            if (isGrabbed && status == FFSStatus.Ungrabbed) ActivateGrabbedState();
        }

        void ActivateFFS()
        {
            vrWorldObject = GameObject.Instantiate(gameObject);

            // We remove this component to avoid loops
            Destroy(vrWorldObject.GetComponent<VRForceFeedbackSystem>());

            vrWorldObject.transform.position = transform.position;
            vrWorldObject.transform.rotation = transform.rotation;
            vrWorldObject.transform.parent = null;

            vrWorldObject.name = name + " (VR)";
            name = name + " (IRL)";

            foreach (Transform transform in gameObject.GetComponentsInChildren<Transform>(true))
            {
                transform.gameObject.layer = LayerMask.NameToLayer(irlLayerName);
            }
            foreach (Transform transform in vrWorldObject.GetComponentsInChildren<Transform>(true))
            {
                transform.gameObject.layer = LayerMask.NameToLayer(vrWorldLayerName);
            }

            ApplyGhostEffect();

            vrFollow = vrWorldObject.AddComponent<ForceFollowObject>();
            vrFollow.target = gameObject;
            vrFollow.collisionHandler = this;//TODO: Move to a collision proxy component

            ActivateUngrabbedState();
        }

        #region Ghost effect
        void ApplyGhostEffect()
        {
            irlMeshes = GetComponentsInChildren<MeshRenderer>();
            if (ghostMetarial == null) return;
            foreach (var mesh in irlMeshes)
            {
                mesh.material = ghostMetarial;
            }
        }

        void SetGhostActivation(bool enable)
        {
            foreach (var mesh in irlMeshes)
            {
                mesh.enabled = enable;
            }
        }
        #endregion

        #region Tracking states

        void ActivateUngrabbedState()
        {
            status = FFSStatus.Ungrabbed;
            vrFollow.mode = ForceFollowObject.Mode.TargetInstantlyMatchObjectPosition;// IRL ghost follows VR
            SetGhostActivation(false);
        }

        void ActivateGrabbedState()
        {
            status = FFSStatus.Grabbed;
            vrFollow.mode = ForceFollowObject.Mode.ObjectInstantlyMatchTargetPosition;// VR follows IRL ghost
            SetGhostActivation(false);
        }

        void ActivateTryingSynchronisationState()
        {
            status = FFSStatus.TryingSynchronisation;
            vrFollow.mode = ForceFollowObject.Mode.ObjectForceFollowTarget;// VR tries to join IRL position back
            SetGhostActivation(true);
        }

        void ActivateJoiningBackState()
        {
            status = FFSStatus.JoiningBack;
            vrFollow.mode = ForceFollowObject.Mode.ObjectProgressiveReturnToTargetposition;// VR tries to join IRL position back
            SetGhostActivation(true);
        }

        void ActivateJoinedBackState()
        {
            status = FFSStatus.JoinedBack;
            if (isGrabbed)
            {
                ActivateGrabbedState();
            }
            else
            {
                ActivateUngrabbedState();
            }
        }

        #endregion

        #region VR world object collision
        public void OnCollisionEnter(Collision collision, ForceFollowObject forceFollowObject)
        {
            ActivateTryingSynchronisationState();
        }

        public void OnCollisionStay(Collision collision, ForceFollowObject forceFollowObject)
        {
        }

        public void OnCollisionExit(Collision collision, ForceFollowObject forceFollowObject)
        {
            ActivateJoiningBackState();
        }
        #endregion
    } 
}

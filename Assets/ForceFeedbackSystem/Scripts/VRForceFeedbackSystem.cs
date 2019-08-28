using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceFeedbackSystem
{
    public interface IIRLMover { }

    public interface IHapticActioner {
        void SetVibrationLevel(float level);
    }


    public interface IIRLMoveHandler
    {
        void OnIRLMoveStart(GameObject movingObject, Vector3 grabSpotLocalPosition, Quaternion grabSpotLocalRotation);
        void OnIRLMove(GameObject movingObject);
        void OnIRLMoveEnd(GameObject movingObject);
    }

    [RequireComponent(typeof(Rigidbody))]
    public class VRForceFeedbackSystem : MonoBehaviour, ForceFollowObject.ICollisionHandler, IIRLMoveHandler
    {
        public Material ghostMaterial;

        public enum FFSStatus
        {
            Free,
            MovedInRealLife,
            TryingSynchronisation,
            JoiningBack,
            JoinedBack
        }

        /// <summary>
        /// Under this distance, VR and IRL object are considered in sync
        /// </summary>
        public float synchronizedMinDistance = 0.03f;

        [Header("Layer masks (to avoid collisions for instance)")]
        public string irlLayerName = "RealLifeVR";
        public string vrWorldLayerName = "GameVR";

        MeshRenderer[] irlMeshes;
        ForceFollowObject vrFollow;

        [Header("Haptic trigger level")]
        float outOfSyncDistance = 0;
        public float maxOutOfSyncDistance = 0.2f;
        IHapticActioner hapticActioner;

        [Header("Set dynamically")]
        public GameObject vrWorldObject;
        public FFSStatus status;
        [SerializeField]
        bool isMovedInRealLife = false;


        // Start is called before the first frame update
        void Start()
        {
            ActivateFFS();
        }

        // Update is called once per frame
        void Update()
        {
            if (isMovedInRealLife && status == FFSStatus.Free) ActivateMovedInRealLifeState();
            if (!isMovedInRealLife && status == FFSStatus.TryingSynchronisation) ActivateFreeState();
            if (status == FFSStatus.JoiningBack || status == FFSStatus.TryingSynchronisation)
            {
                outOfSyncDistance = Vector3.Distance(vrFollow.mainObjectAttractionPoint.position, vrFollow.mainTargetAttractionPoint.position);
            }
            else
            {
                outOfSyncDistance = 0f;
            }
            if (status == FFSStatus.JoiningBack)
            {
                if(outOfSyncDistance < synchronizedMinDistance)
                {
                    ActivateJoinedBackState();
                }
            }
            SetVibrationLevel(outOfSyncDistance / maxOutOfSyncDistance);
        }

        void ActivateFFS()
        {
            hapticActioner = GetComponent<IHapticActioner>();

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

            ActivateFreeState();

            // Remove unrelevant components on Vr copy
            foreach (IIRLMover mover in vrWorldObject.GetComponentsInChildren<IIRLMover>(true))
            {
                Destroy((Component)mover);
            }
        }

        #region Ghost effect
        void ApplyGhostEffect()
        {
            irlMeshes = GetComponentsInChildren<MeshRenderer>();
            if (ghostMaterial == null) return;
            foreach (var mesh in irlMeshes)
            {
                mesh.material = ghostMaterial;
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

        void ActivateFreeState()
        {
            status = FFSStatus.Free;
            vrFollow.mode = ForceFollowObject.Mode.TargetInstantlyMatchObjectPosition;// IRL ghost follows VR
            SetGhostActivation(false);
        }

        void ActivateMovedInRealLifeState()
        {
            status = FFSStatus.MovedInRealLife;
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
            vrFollow.StartJoiningBack();// VR tries to join IRL position back
            SetGhostActivation(isMovedInRealLife);
        }

        void ActivateJoinedBackState()
        {
            status = FFSStatus.JoinedBack;
            if (isMovedInRealLife)
            {
                ActivateMovedInRealLifeState();
            }
            else
            {
                ActivateFreeState();
            }
        }

        #endregion

        #region VR world object collision
        public void OnFollowCollisionEnter(Collision collision, ForceFollowObject forceFollowObject)
        {
            if (!isMovedInRealLife) return;
            ActivateTryingSynchronisationState();

            Vector3 localPosition = forceFollowObject.transform.InverseTransformPoint(collision.GetContact(0).point);
            vrFollow.secondaryObjectAttractionPoint.localPosition = localPosition;
            vrFollow.secondaryTargetAttractionPoint.localPosition = localPosition;
        }

        public void OnFollowCollisionStay(Collision collision, ForceFollowObject forceFollowObject)
        {
        }

        public void OnFollowCollisionExit(Collision collision, ForceFollowObject forceFollowObject)
        {
            if (isMovedInRealLife)
            {
                ActivateJoiningBackState();
            }
            else
            {
                ActivateFreeState();
            }
        }
        #endregion

        #region IRL Move
        public void OnIRLMove(GameObject movingObject)
        {
            this.isMovedInRealLife = true;
        }

        public void OnIRLMoveStart(GameObject movingObject, Vector3 grabSpotLocalPosition, Quaternion grabSpotLocalRotation)
        {
            this.isMovedInRealLife = true;
            vrFollow.mainObjectAttractionPoint.localPosition = grabSpotLocalPosition;
            vrFollow.mainObjectAttractionPoint.localRotation = grabSpotLocalRotation;
            vrFollow.mainTargetAttractionPoint.localPosition = grabSpotLocalPosition;
            vrFollow.mainTargetAttractionPoint.localRotation = grabSpotLocalRotation;
        }

        public void OnIRLMoveEnd(GameObject movingObject)
        {
            this.isMovedInRealLife = false;
        }
        #endregion

        #region Haptic 

        void SetVibrationLevel(float level)
        {
            if (hapticActioner == null) return;
            hapticActioner.SetVibrationLevel(level);
        }
        #endregion
    }
}

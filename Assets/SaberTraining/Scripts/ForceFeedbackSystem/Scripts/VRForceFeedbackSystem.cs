using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceFeedbackSystem
{
    public interface IIRLMover { }


    public interface IIRLMoveHandler
    {
        void OnIRLMoveStart(GameObject movingObject);
        void OnIRLMove(GameObject movingObject, IIRLMoveHandler h);
        void OnIRLMoveEnd(GameObject movingObject);
    }

    [RequireComponent(typeof(Rigidbody))]
    public class VRForceFeedbackSystem : MonoBehaviour, ForceFollowObject.ICollisionHandler, IIRLMoveHandler
    {
        public Material ghostMetarial;
        public GameObject vrWorldObject;

        public bool isMovedInRealLife = false;
        public enum FFSStatus
        {
            Free,
            MovedInRealLife,
            TryingSynchronisation,
            JoiningBack,
            JoinedBack
        }

        public FFSStatus status;
        /// <summary>
        /// Under this distance, VR and IRL object are considered in sync
        /// </summary>
        public float synchronizedMinDistance = 0.03f;

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
            if (isMovedInRealLife && status == FFSStatus.Free) ActivateMovedInRealLifeState();
            if(status == FFSStatus.JoiningBack)
            {
                var distance = Vector3.Distance(transform.position, vrWorldObject.transform.position);
                if(distance < synchronizedMinDistance)
                {
                    ActivateJoinedBackState();
                }
            }
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

        void ActivateFreeState()
        {
            Debug.Log("ActivateFreeState");
            status = FFSStatus.Free;
            vrFollow.mode = ForceFollowObject.Mode.TargetInstantlyMatchObjectPosition;// IRL ghost follows VR
            SetGhostActivation(false);
        }

        void ActivateMovedInRealLifeState()
        {
            Debug.Log("ActivateMovedInRealLifeState");
            status = FFSStatus.MovedInRealLife;
            vrFollow.mode = ForceFollowObject.Mode.ObjectInstantlyMatchTargetPosition;// VR follows IRL ghost
            SetGhostActivation(true);
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
            SetGhostActivation(true);
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
            ActivateTryingSynchronisationState();
        }

        public void OnFollowCollisionStay(Collision collision, ForceFollowObject forceFollowObject)
        {
        }

        public void OnFollowCollisionExit(Collision collision, ForceFollowObject forceFollowObject)
        {
            ActivateJoiningBackState();
        }
        #endregion

        #region IRL Move
        public void OnIRLMove(GameObject movingObject, IIRLMoveHandler m)
        {
            Debug.Log("OnIRLMove "+this+"/"+ m);
            this.isMovedInRealLife = true;
        }

        public void OnIRLMoveStart(GameObject movingObject)
        {
            Debug.Log("OnIRLMoveStart");
            this.isMovedInRealLife = true;
        }

        public void OnIRLMoveEnd(GameObject movingObject)
        {
            Debug.Log("OnIRLMoveEnd");
            this.isMovedInRealLife = false;
        }
        #endregion
    }
}

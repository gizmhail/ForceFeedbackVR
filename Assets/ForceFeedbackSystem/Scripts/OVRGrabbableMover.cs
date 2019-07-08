using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceFeedbackSystem
{
    public class OVRGrabbableMover : OVRGrabbable, IIRLMover, IHapticActioner
    {
        IIRLMoveHandler _moveHandler;
        IIRLMoveHandler MoveHandler
        {
            get
            {
                if (_moveHandler == null) _moveHandler = GetComponent<IIRLMoveHandler>();
                return _moveHandler;
            }
        }
        public bool moving = false;
        public OVRInput.Controller grabbingControllerKind = OVRInput.Controller.None;
        float lastVibrationLevel = 0;

        [Header("Force feedback system")]
        public bool installForceFeedbackSystem = true;
        public Material ghostMaterial;
        public string irlLayerName = "RealLifeVR";
        public string vrWorldLayerName = "GameVR";
        public string grabbingIrlLayerName = "GrabbingRealLife";

        [Header("Set dynamically")]
        public VRForceFeedbackSystem forceFeedbackSystem;
        public Vector3 grabSpotLocalPosition;
        public Quaternion grabSpotLocalRotation;

        private void Awake()
        {
            
            if (m_grabPoints.Length == 0)
            {
                // Get the collider from the grabbable
                Collider collider = this.GetComponent<Collider>();
                if (collider == null)
                {
                    throw new ArgumentException("Grabbables cannot have zero grab points and no collider -- please add a grab point or collider.");
                }

                // Create a default grab point
                m_grabPoints = new Collider[1] { collider };
            }
        }

        protected override void Start()
        {
            base.Start();
            if (installForceFeedbackSystem && GetComponent<VRForceFeedbackSystem>() == null)
            {
                forceFeedbackSystem = gameObject.AddComponent<VRForceFeedbackSystem>();
                forceFeedbackSystem.ghostMaterial = ghostMaterial;
                forceFeedbackSystem.irlLayerName = irlLayerName;
                forceFeedbackSystem.vrWorldLayerName = vrWorldLayerName;
            }
            else
            {
                forceFeedbackSystem = GetComponent<VRForceFeedbackSystem>();
            }
        }

        private void Update()
        {
            if (MoveHandler != null && moving)
            {
                MoveHandler.OnIRLMove(gameObject);
            }
        }

        public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
        {
            base.GrabBegin(hand, grabPoint);

            if(hand.gameObject.layer != LayerMask.NameToLayer(grabbingIrlLayerName))
            {
                Debug.LogError("To behave properly, the grabbing object should be in a dedicated layer, the only one interacting with the " + irlLayerName + " layer");
            }

            grabSpotLocalPosition = grabbedBy.transform.position - transform.position;
            grabSpotLocalRotation = Quaternion.Inverse(transform.rotation) * grabbedBy.transform.rotation;

            DetectGrabbingController();
            if (MoveHandler != null)
            {
                MoveHandler.OnIRLMoveStart(gameObject, grabSpotLocalPosition, grabSpotLocalRotation);
                moving = true;
            }

            // warn VR hands
            if (grabbingControllerKind == OVRInput.Controller.LTouch && OVRVRHand.Left != null)
            {
                OVRVRHand.Left.grabbedObject = this;
            }
            if (grabbingControllerKind == OVRInput.Controller.RTouch && OVRVRHand.Right != null)
            {
                OVRVRHand.Right.grabbedObject = this;
            }
        }

        public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
        {

            base.GrabEnd(linearVelocity, angularVelocity);
            if (MoveHandler != null)
            {
                MoveHandler.OnIRLMoveEnd(gameObject);
                moving = false;
            }
        }

        #region Haptic 

        void DetectGrabbingController()
        {
            //Note: instead of parsing name, we could subclass OVRGrabber to make the controller kind public
            grabbingControllerKind = OVRInput.Controller.None;
            if (grabbedBy != null)
            {
                if (grabbedBy.name.Contains("Left"))
                {
                    grabbingControllerKind = OVRInput.Controller.LTouch;
                }
                if (grabbedBy.name.Contains("Right"))
                {
                    grabbingControllerKind = OVRInput.Controller.RTouch;
                }
            }
        }

        public void SetVibrationLevel(float level)
        {
            if (grabbingControllerKind == OVRInput.Controller.None) return;
            if (level == 0 && lastVibrationLevel == 0) return;
            lastVibrationLevel = level;

            if (level == 0)
            {
                OVRInput.SetControllerVibration(0, 0, grabbingControllerKind);
            }
            else
            {
                OVRInput.SetControllerVibration(frequency: level, amplitude: level, grabbingControllerKind);
            }
        }
        #endregion
    }
}


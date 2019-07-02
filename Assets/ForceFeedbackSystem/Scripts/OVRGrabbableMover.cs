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
        public bool installForceFeedbackSystem = false;
        public Material ghostMaterial;
        public string irlLayerName = "RealLifeVR";
        public string vrWorldLayerName = "GameVR";

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
                var ffs = gameObject.AddComponent<VRForceFeedbackSystem>();
                ffs.ghostMaterial = ghostMaterial;
                ffs.irlLayerName = irlLayerName;
                ffs.vrWorldLayerName = vrWorldLayerName;
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

            var grabSpotLocalPosition = grabbedBy.transform.position - transform.position;
            var grabSpotLocalRotation = Quaternion.Inverse(transform.rotation) * grabbedBy.transform.rotation;

            DetectGrabbingController();
            if (MoveHandler != null)
            {
                MoveHandler.OnIRLMoveStart(gameObject, grabSpotLocalPosition, grabSpotLocalRotation);
                moving = true;
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


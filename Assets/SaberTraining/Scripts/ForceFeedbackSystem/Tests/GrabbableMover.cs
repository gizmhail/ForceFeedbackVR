using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceFeedbackSystem
{
    public class GrabbableMover : OVRGrabbable, IIRLMover
    {
        IIRLMoveHandler moveHandler;
        bool moving = false;

        private void Awake()
        {
            moveHandler = GetComponent<IIRLMoveHandler>();
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

        private void Update()
        {
            if (moveHandler != null && moving)
            {
                moveHandler.OnIRLMove(gameObject);
            }
        }

        public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
        {
            base.GrabBegin(hand, grabPoint);
            if (moveHandler != null)
            {
                moveHandler.OnIRLMoveStart(gameObject);
                moving = true;
            }
        }

        public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
        {

            base.GrabEnd(linearVelocity, angularVelocity);
            if (moveHandler != null)
            {
                moveHandler.OnIRLMoveEnd(gameObject);
                moving = false;
            }
        }
    }
}


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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ForceFeedbackSystem
{
    public class KeyboardMove : MonoBehaviour, IIRLMover
    {
        public float speed = 0.05f;
        IIRLMoveHandler moveHandler;
        bool moving = false;

        void Awake()
        {
            moveHandler = GetComponent<IIRLMoveHandler>();
        }

        // Update is called once per frame
        void Update()
        {
            var movingDuringThisFrame = false;
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.position -= speed * transform.right;
                movingDuringThisFrame = true;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.position += speed * transform.right;
                movingDuringThisFrame = true;
            }
            else if (Input.GetKey(KeyCode.UpArrow))
            {
                transform.position += speed * transform.forward;
                movingDuringThisFrame = true;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                transform.position -= speed * transform.forward;
                movingDuringThisFrame = true;
            }

            if (movingDuringThisFrame)
            {
                if (moveHandler != null && !moving)
                {
                    moveHandler.OnIRLMoveStart(gameObject, Vector3.zero, Quaternion.identity);
                }
                if (moveHandler != null && moving) moveHandler.OnIRLMove(gameObject);
            } else
            {
                if (moveHandler != null && moving)
                {
                    moveHandler.OnIRLMoveEnd(gameObject);
                }
            }
            moving = movingDuringThisFrame;
        }
    }

}
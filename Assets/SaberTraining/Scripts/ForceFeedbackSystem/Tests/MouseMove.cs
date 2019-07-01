using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceFeedbackSystem
{
    public class MouseMove : MonoBehaviour, IIRLMover
    {
        Vector3 initialDelta;
        float depth;
        IIRLMoveHandler moveHandler;

        private void Start()
        {
            foreach(var c in GetComponents(typeof(Component)))
            {
                if (c is IIRLMoveHandler) moveHandler = (IIRLMoveHandler)c;
            }

            Debug.Log(name+" MH:" + moveHandler);
            Debug.Log(name+" MH<>:" + GetComponent<IIRLMoveHandler>());

        }

        private void Update()
        {

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log(" Input.mousePosition.x: " + Input.mousePosition.x);
                Debug.Log(" Input.mousePosition.y: " + Input.mousePosition.y);
            }
        }

        void OnMouseDown()
        {
            var screenPosition = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            var depth = screenPosition.z;
            var clickPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth));
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = clickPosition;
            sphere.transform.localScale = new Vector3(0.1f,0.1f,0.1f);

            initialDelta = gameObject.transform.position - clickPosition;
            if (moveHandler != null) moveHandler.OnIRLMoveStart(gameObject);
        }

        void OnMouseDrag()
        {
            var screenPosition = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            var depth = screenPosition.z;
            var cursorScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth);
            Debug.Log("cursorScreenPosition: " + cursorScreenPosition);
            Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorScreenPosition) + initialDelta;
            Debug.Log("cursorPosition: " + cursorPosition);
            transform.position = cursorPosition;
            Debug.Log("MH:" + moveHandler);
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.position = Camera.main.ScreenToWorldPoint(cursorScreenPosition) + initialDelta;
            cylinder.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            //if (moveHandler != null) moveHandler.OnIRLMove(gameObject, moveHandler);
        }

        private void OnMouseUp()
        {
            if (moveHandler != null) moveHandler.OnIRLMoveEnd(gameObject);
        }
    } 
}

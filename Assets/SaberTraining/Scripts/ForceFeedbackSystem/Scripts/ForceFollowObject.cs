using ForceBasedMove;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceFeedbackSystem
{
    public class ForceFollowObject : MonoBehaviour
    {
        public interface ICollisionHandler
        {
            void OnCollisionEnter(Collision collision, ForceFollowObject forceFollowObject);
            void OnCollisionStay(Collision collision, ForceFollowObject forceFollowObject);
            void OnCollisionExit(Collision collision, ForceFollowObject forceFollowObject);
        }

        public enum Mode {
            NoAttraction,
            TargetInstantlyMatchObjectPosition,
            ObjectInstantlyMatchTargetPosition,
            ObjectForceFollowTarget,
            ObjectProgressiveReturnToTargetposition,
        }


        public GameObject target;
        public Mode mode;
        public ICollisionHandler collisionHandler;

        public Rigidbody objectRigidBody;
        public Rigidbody targetRigidBody;

        [Header("Force attraction")]
        public float forceCatchupScale = 14f;
        public float torqueCatchupScale = 14f;
        public float isJoiningBackForceMultiplicator = 4;
        public float isJoiningBackTorqueMultiplicator = 4;
        public float secondaryAttractionPointForceMultiplicator = 0.5f;

        [Header("Attraction points")]
        public Transform mainTargetAttractionPoint;
        public Transform mainObjectAttractionPoint;
        public Transform secondaryTargetAttractionPoint;
        public Transform secondaryObjectAttractionPoint;


        // Start is called before the first frame update
        void Start()
        {
            SetUpFollowingsystem();
        }

        private void Update()
        {
            switch (mode)
            {
                case Mode.ObjectInstantlyMatchTargetPosition:
                    transform.position = target.transform.position;
                    transform.rotation = target.transform.rotation;
                    break;
                case Mode.TargetInstantlyMatchObjectPosition:
                    target.transform.position = transform.position;
                    target.transform.rotation = transform.rotation;
                    break;
            }
        }

        void FixedUpdate()
        {
            switch (mode)
            {
                case Mode.ObjectForceFollowTarget:
                    ForceBaseClosing();
                    break;
            }
        }

        #region Set up

        void SetUpFollowingsystem()
        {
            if (target == null) return;
            if(objectRigidBody == null) objectRigidBody = GetComponent<Rigidbody>();
            if(targetRigidBody == null) targetRigidBody = target.GetComponent<Rigidbody>();
            SetupAttractionPoints();
        }

        Transform CreateAttractionPoint(string name, Transform storageTransform) {
            GameObject attractionPoint = new GameObject(name);
            attractionPoint.transform.position = storageTransform.position;
            attractionPoint.transform.rotation = storageTransform.rotation;
            attractionPoint.transform.parent = storageTransform;
            return attractionPoint.transform;
        }

        void SetupAttractionPoints()
        {
            if (mainTargetAttractionPoint == null)
            {
                mainTargetAttractionPoint = CreateAttractionPoint("FFS.mainTargetAttractionPoint", target.transform);
            }
            if (mainObjectAttractionPoint == null)
            {
                mainObjectAttractionPoint = CreateAttractionPoint("FFS.mainObjectAttractionPoint", transform);
            }
            if (secondaryTargetAttractionPoint == null)
            {
                secondaryTargetAttractionPoint = CreateAttractionPoint("FFS.secondaryTargetAttractionPoint", target.transform);
            }
            if (secondaryObjectAttractionPoint == null)
            {
                secondaryObjectAttractionPoint = CreateAttractionPoint("FFS.secondaryObjectAttractionPoint", transform);
            }
        }
        #endregion


        #region Movements

        void ForceBaseClosing(float forceMultiplier = 1.0f, float torqueMultiplier = 1.0f)
        {
            if (mode != Mode.ObjectForceFollowTarget) return;

            var forcScale = this.forceCatchupScale;
            var torqueScale = this.torqueCatchupScale;
            forcScale *= forceMultiplier;
            torqueScale *= torqueMultiplier;

            objectRigidBody.AddForceTowards(mainObjectAttractionPoint.position, mainTargetAttractionPoint.position, frequency: forcScale);
            objectRigidBody.AddTorqueTowards(mainObjectAttractionPoint.rotation, mainTargetAttractionPoint.rotation, frequency: torqueCatchupScale);
            // Small additional startup force, if the object is in the right position, but rotated
            objectRigidBody.AddForceTowards(secondaryObjectAttractionPoint.position, secondaryTargetAttractionPoint.position, frequency: forceCatchupScale* secondaryAttractionPointForceMultiplicator);

        }
        /*
        void PositionBasedClosing()
        {
            var returnStart = joiningBackEndTime - joiningBackDuration;
            var returnProgress = (Time.time - returnStart) / joiningBackDuration;

            physicsVRObject.transform.position = Vector3.Lerp(physicsVRObject.transform.position, realLifeObject.transform.position, returnProgress);
            physicsVRObject.transform.rotation = Quaternion.Lerp(physicsVRObject.transform.rotation, realLifeObject.transform.rotation, returnProgress);
            physicsVRObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            physicsVRObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }*/

        #endregion

        #region Collision

        private void OnCollisionEnter(Collision collision)
        {
            if (collisionHandler != null) collisionHandler.OnCollisionEnter(collision, this);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collisionHandler != null) collisionHandler.OnCollisionStay(collision, this);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collisionHandler != null) collisionHandler.OnCollisionExit(collision, this);
        }
        #endregion
    }

}
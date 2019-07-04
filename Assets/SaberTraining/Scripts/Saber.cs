using ForceFeedbackSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saber : MonoBehaviour
{
    Animator animator;
    OVRGrabbableMover grabbable;
    public bool isSabberButtonOn = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        grabbable = GetComponent<OVRGrabbableMover>();
    }

    // Update is called once per frame
    void Update()
    {
        if (grabbable.isGrabbed)
        {
            var triggerAxis = grabbable.grabbingControllerKind == OVRInput.Controller.LTouch ? "Oculus_CrossPlatform_PrimaryIndexTrigger" : "Oculus_CrossPlatform_SecondaryIndexTrigger";
            if (Input.GetAxis(triggerAxis) > 0.80)
            {
                if (!isSabberButtonOn) SaberToggle();
                isSabberButtonOn = true;
            } else
            {
                isSabberButtonOn = false;
            }
        } else
        {
            isSabberButtonOn = false;
        }
    }

    public void SaberToggle()
    {
        animator.SetTrigger("SaberTrigger");
        if(grabbable.forceFeedbackSystem.vrWorldObject != null)
        {
            // We are the IRL saber
            grabbable.forceFeedbackSystem.vrWorldObject.GetComponent<Saber>().SaberToggle();
        }
    }
}

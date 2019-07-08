using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorForce : MonoBehaviour
{
    public Rigidbody rb;
    public Rigidbody mirrorRb;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = mirrorRb.velocity;
        rb.angularVelocity = mirrorRb.angularVelocity;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassCenterPositioning : MonoBehaviour
{
    public Transform massCenter;
    public Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = massCenter.localPosition;
    }
}

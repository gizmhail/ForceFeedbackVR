using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorLocalTransform : MonoBehaviour
{
    public Transform target;

    void Update()
    {
        transform.localPosition = target.localPosition;
        transform.localRotation = target.localRotation;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStrike : MonoBehaviour
{
    public float lastStrikeStart = 0;
    public float lastStrikePause = -2;
    public bool isStriking = false;
    public Rigidbody strikingBody;

    void Awake()
    {
        if (strikingBody == null) {
            strikingBody = GetComponent<Rigidbody>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStriking && (Time.time - lastStrikePause > 5))
        {
            isStriking = true;
            lastStrikeStart = Time.time;
        }
        if (isStriking && (Time.time - lastStrikeStart > 3))
        {
            isStriking = false;
            lastStrikePause = Time.time;
        }
    }

    private void FixedUpdate()
    {
        if (isStriking) {
            strikingBody.AddRelativeTorque(new Vector3(0, 0, -20));
        }
    }
}

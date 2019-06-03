using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStrike : MonoBehaviour
{
    public float lastStrikeStart = 0;
    public float lastStrikePause = -2;
    public bool isStriking = false;
    public Rigidbody strikingBody;

    public float lastStrengthChange = 0;
    public float nominalStrengthLevelDuration = 1.5f;
    public float strengthLevelDuration = 1.5f;
    public float nominalStrength = 20f;
    public float strength;
    void Awake()
    {
        if (strikingBody == null) {
            strikingBody = GetComponent<Rigidbody>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (strength == 0 || Time.time - lastStrengthChange > strengthLevelDuration) {
            strength = Random.Range(nominalStrength * 0.75f, nominalStrength * 1.5f);
            strengthLevelDuration = Random.Range(nominalStrengthLevelDuration * 0.5f, nominalStrengthLevelDuration * 1.5f);
        }

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
            strikingBody.AddRelativeTorque(new Vector3(0, 0, -strength));
        }
    }
}

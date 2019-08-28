using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStrike : MonoBehaviour
{
    public float lastStrikeStart = 0;
    public float lastStrikePause = -2;
    public bool isStriking = false;
    public Rigidbody strikingBody;

    public float strikeDuration = 3;
    public float minTimeBetweenStrikes = 1;
    public float lastStrengthChange = 0;
    public float nominalStrengthLevelDuration = 1.5f;
    public float strengthLevelDuration = 1.5f;
    public float nominalStrength = 20f;
    public float strength;
    public int remainingStrikes = 1;//-1 for looping indefinitively
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

        if (!isStriking && (Time.time - lastStrikePause > minTimeBetweenStrikes) &&(remainingStrikes > 0 || remainingStrikes == -1))
        {
            isStriking = true;
            if (remainingStrikes > 0) remainingStrikes--;
            lastStrikeStart = Time.time;
        }
        if (isStriking && (Time.time - lastStrikeStart > strikeDuration))
        {
            isStriking = false;
            lastStrikePause = Time.time;
        }
    }

    public void DelayedReload(int delay = 2) {
        if (remainingStrikes == 0) StartCoroutine(DelayedReloadCoroutine(delay));
    }

    IEnumerator DelayedReloadCoroutine(int delay)
    {
        yield return new WaitForSeconds(delay);
        remainingStrikes++;
    }

    private void FixedUpdate()
    {
        if (isStriking)
        {
            //Debug.Log(transform.localRotation.eulerAngles);
            strikingBody.AddRelativeTorque(new Vector3(0, 0, -strength));
        }
    }
}

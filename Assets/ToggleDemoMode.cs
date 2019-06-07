using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleDemoMode : MonoBehaviour
{
    public GameObject demoZone;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            demoZone.SetActive(true);
        }
    }
}

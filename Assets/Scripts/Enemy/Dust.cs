using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dust : MonoBehaviour
{

    private float destroyTime = 3f;
    private float startTime;

    // Start is called before the first frame update
    void Start()
    {
         startTime = Time.time;
    }
    private void FixedUpdate()
    {
        if(Time.time > destroyTime +startTime) {
            Destroy(this.gameObject);
        }
    }
}

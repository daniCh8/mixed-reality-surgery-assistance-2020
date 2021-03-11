using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogPosition : MonoBehaviour
{

    private void Start()
    {
        transform.position = transform.position - transform.gameObject.GetComponentInChildren<Renderer>().bounds.center;
        
        Vector3 p1 = new Vector3((float)0.2897712, (float)0.13128, (float)1.780701);
        Vector3 p2 = new Vector3((float)-1.489732, (float)0.230065, (float)1.953033);

        transform.position = transform.position + p1;

        /*
        if (transform.name == "Patient1")
        {
            transform.position = transform.position - p1;
        }
        else
        {
            transform.position = transform.position - p2;
        }
        */
    }

    void Update()
    {
        Vector3 pos = transform.gameObject.GetComponentInChildren<Renderer>().bounds.center;
        Debug.Log(transform.name + " position is: " + pos.x + " - " + pos.y + " - " + pos.z);
    }
}

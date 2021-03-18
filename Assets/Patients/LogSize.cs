using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogSize : MonoBehaviour
{
    private Renderer objRenderer;
    private Vector3 p1Old = new Vector3((float)0.8, (float)0.2, (float)0.1);
    private Vector3 p2Old = new Vector3((float)0.5, (float)0.1, (float)0.1);

    private Vector3 p1 = new Vector3((float)0.001881594, (float)0.0005296624, (float)0.0003717189);
    private Vector3 p2 = new Vector3((float)0.001136337, (float)0.0003595751, (float)0.0003358721);

    void Start()
    {
        //Debug.Log(transform.name + " scale is: " + transform.localScale);
        objRenderer = transform.gameObject.GetComponentInChildren<Renderer>();
        //float newScaleX = transform.localScale.x * p2.x / p1.x;
        //transform.localScale = new Vector3(newScaleX, transform.localScale.y, transform.localScale.z);

    }

    void Update()
    {
        // slider.x : p1.x = newslider.x : p2.x
        // Debug.Log(transform.name + " size is: " + objRenderer.bounds.size);
        // Debug.Log(transform.name + " scale is: " + transform.localScale);

        Debug.Log(transform.name + " x-scale is: " + transform.lossyScale.x);
        Debug.Log(transform.name + " y-scale is: " + transform.lossyScale.y);
        Debug.Log(transform.name + " z-scale is: " + transform.lossyScale.z);

        Debug.Log(transform.name + " x-size is: " + objRenderer.bounds.size.x);
        Debug.Log(transform.name + " y-size is: " + objRenderer.bounds.size.y);
        Debug.Log(transform.name + " z-size is: " + objRenderer.bounds.size.z);
    }
}

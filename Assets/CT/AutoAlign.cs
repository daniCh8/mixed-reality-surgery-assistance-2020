using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAlign : MonoBehaviour
{
    public GameObject target;
    public Vector3 offset;

    void Start()
    {
        if (offset == null)
        {
            offset = new Vector3(-888, 0, 0);
        }
    }

    void Update()
    {
        if (offset.x != -888)
        {
            transform.position = target.transform.position + offset;
        }
    }

    public void ComputeOffset()
    {
        offset = target.transform.position - transform.position;
        offset *= -1;
    }
}

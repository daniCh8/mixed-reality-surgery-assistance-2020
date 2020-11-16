using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionConstraint : MonoBehaviour
{
    public Vector3 screwPosition;

    private void Start()
    {
        screwPosition = transform.position;
    }

    void Update()
    {
        transform.position = screwPosition;
    }
}

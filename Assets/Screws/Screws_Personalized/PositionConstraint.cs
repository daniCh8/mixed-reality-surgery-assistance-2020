using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionConstraint : MonoBehaviour
{
    public Vector3 screwPosition;

    void Update()
    {
        transform.position = screwPosition;
    }
}

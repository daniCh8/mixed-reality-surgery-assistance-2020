using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrewRotationUpdate : MonoBehaviour
{
    public GameObject rotationHandler;

    private void Update()
    {
        if(rotationHandler.transform.localEulerAngles != transform.localEulerAngles)
        {
            transform.eulerAngles = rotationHandler.transform.eulerAngles;
        }
    }
}

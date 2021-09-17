using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrewRotationUpdate : MonoBehaviour
{
    public GameObject rotationHandler;
    public Vector3 previousRotationHandlerAngles;

    private void Update()
    {
        if(false)
        //if(previousRotationHandlerAngles != rotationHandler.transform.eulerAngles)
        {
            Vector3 firstEndPoint = gameObject.transform.Find(
                ScrewConstants.FIRST_POINT_NAME).position;
            transform.RotateAround(firstEndPoint,
                (rotationHandler.transform.eulerAngles - previousRotationHandlerAngles),
                Vector3.Angle(rotationHandler.transform.eulerAngles, previousRotationHandlerAngles));
            previousRotationHandlerAngles = rotationHandler.transform.eulerAngles;
        }
    }
}

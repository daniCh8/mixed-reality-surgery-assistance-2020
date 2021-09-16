using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class ScrewSizeUpdate : MonoBehaviour
{

    public double screwSize;
    public float previousPinchSliderVal;
    public ScrewSceneController screwSceneController;
    public PinchSlider pinchSlider;

    void Update()
    {
        if(pinchSlider.SliderValue != previousPinchSliderVal && pinchSlider.SliderValue > 0)
        {
            Vector3 firstEndPoint = gameObject.transform.Find(
                ScrewConstants.FIRST_POINT_NAME).position,
                    secondEndPoint = gameObject.transform.Find(
                ScrewConstants.SECOND_POINT_NAME).position;
            previousPinchSliderVal = pinchSlider.SliderValue;
            float old_distance = Vector3.Distance(secondEndPoint, firstEndPoint),
                old_size = (float)screwSize,
                new_size = previousPinchSliderVal * ScrewConstants.MAX_LENGTH_SCREW;
            float new_distance = ((new_size * old_distance) / old_size);
            Vector3 newEndPoint =
                LerpByDistance(firstEndPoint, secondEndPoint, new_distance);
            screwSceneController.AlignCylinder(gameObject, firstEndPoint, newEndPoint);
            screwSize = new_size;
            screwSceneController.SetScrewSizeText(new_size);
        }
    }

    private Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
    {
        Vector3 P = x * Vector3.Normalize(B - A) + A;
        return P;
    }
}

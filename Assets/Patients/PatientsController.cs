using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientsController : MonoBehaviour
{
    public GameObject newPatient, newBone;
    public GameObject referenceBone, referencePatient;

    public GameObject pinchSliderHor, visualTrackHor;
    public GameObject pinchSliderVer, visualTrackVer;

    public CTReader cTReader;
    public TextAsset newScans, referenceScans;

    private Vector3 referencePatientPosition, referenceBonePosition;
    private Vector3 referenceBoneSize;


    void Start()
    {
        referencePatientPosition = referencePatient.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center;
        referenceBonePosition = referenceBone.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center;
        referenceBoneSize = referenceBone.transform.gameObject.GetComponentInChildren<Renderer>().bounds.size;

        XCenterToRef(pinchSliderHor, referenceBonePosition);
        ZCenterToRef(pinchSliderVer, referenceBonePosition);
        TunePinchSlider(referenceBoneSize);
    }

    public GameObject SwitchPatient()
    {
        CenterToRef(newPatient, referencePatientPosition);
        TunePinchSlider(newBone.transform.gameObject.GetComponentInChildren<Renderer>().bounds.size);
        cTReader.ct = newScans;

        GameObject boxBone = referenceBone;
        GameObject boxPatient = referencePatient;
        TextAsset boxCt = referenceScans;
        referenceBone = newBone;
        referencePatient = newPatient;
        referenceScans = newScans;
        newBone = boxBone;
        newPatient = boxPatient;
        newScans = boxCt;
        

        newPatient.SetActive(false);
        referencePatient.SetActive(true);

        return referencePatient;
    }


    public static void CenterToRef(GameObject obj, Vector3 referencePosition)
    {
        obj.transform.position = obj.transform.position -
            obj.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center +
            referencePosition;
    }

    public static void ResizeToRef(GameObject obj, Vector3 referenceSize)
    {
        Vector3 oldLocalScale = obj.transform.localScale;
        Vector3 oldSize = obj.GetComponentInChildren<Renderer>().bounds.size;

        float newX = oldLocalScale.x / oldSize.x * referenceSize.x;

        obj.transform.localScale = new Vector3(newX, newX, newX);
    }

    private void XCenterToRef(GameObject obj, Vector3 referencePosition)
    {
        Vector3 startingPosition = obj.transform.position;
        float newX = startingPosition.x -
            obj.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center.x +
            referencePosition.x;

        obj.transform.position = new Vector3(newX, startingPosition.y, startingPosition.z);
    }

    private void ZCenterToRef(GameObject obj, Vector3 referencePosition)
    {
        Vector3 startingPosition = obj.transform.position;
        float newZ = startingPosition.z -
            obj.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center.z +
            referencePosition.z;

        obj.transform.position = new Vector3(startingPosition.x, startingPosition.y, newZ);
    }

    private void TunePinchSlider(Vector3 referenceSize)
    {
        PinchSlider pinchSliderCompHor = pinchSliderHor.GetComponent<PinchSlider>();
        float distanceHor = referenceSize.x / (pinchSliderHor.transform.lossyScale.x * 2);
        pinchSliderCompHor.SliderStartDistance = -distanceHor;
        pinchSliderCompHor.SliderEndDistance = distanceHor;

        Vector3 oldLocalScaleHor = visualTrackHor.transform.localScale;
        float newTrackLocalScaleHor = oldLocalScaleHor.x /
            visualTrackHor.transform.gameObject.GetComponentInChildren<Renderer>().bounds.size.x *
            referenceSize.x;

        visualTrackHor.transform.localScale = new Vector3(newTrackLocalScaleHor, oldLocalScaleHor.y, oldLocalScaleHor.z);

        PinchSlider pinchSliderCompVer = pinchSliderVer.GetComponent<PinchSlider>();
        float distanceVer = referenceSize.z / (pinchSliderVer.transform.lossyScale.x * 2);
        pinchSliderCompVer.SliderStartDistance = -distanceVer;
        pinchSliderCompVer.SliderEndDistance = distanceVer;

        Vector3 oldLocalScaleVer = visualTrackVer.transform.localScale;
        float newTrackLocalScaleVer = oldLocalScaleVer.x /
            visualTrackVer.transform.gameObject.GetComponentInChildren<Renderer>().bounds.size.z *
            referenceSize.z;

        visualTrackVer.transform.localScale = new Vector3(newTrackLocalScaleVer, oldLocalScaleVer.y, oldLocalScaleVer.z);
    }
}

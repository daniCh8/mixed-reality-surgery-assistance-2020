using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientsController : MonoBehaviour
{
    // Manipulation Scene References
    public GameObject newPatientManip, newBoneManip;
    public GameObject referenceBoneManip, referencePatientManip;

    public GameObject pinchSliderHor, visualTrackHor;
    public GameObject pinchSliderVer, visualTrackVer;

    public CTReader cTReader;
    public TextAsset newScans, referenceScans;

    private Vector3 referencePatientPositionManip, referenceBonePositionManip;
    private Vector3 referenceBoneSizeManip;

    // Screw Scene References
    public GameObject referencePatientScrew, newPatientScrew;

    void Start()
    {
        referencePatientPositionManip = referencePatientManip.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center;
        referenceBonePositionManip = referenceBoneManip.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center;
        referenceBoneSizeManip = referenceBoneManip.transform.gameObject.GetComponentInChildren<Renderer>().bounds.size;

        XCenterToRef(pinchSliderHor, referenceBonePositionManip);
        ZCenterToRef(pinchSliderVer, referenceBonePositionManip);
        TunePinchSlider(referenceBoneSizeManip);
    }

    public Tuple<GameObject, GameObject> SwitchPatient()
    {
        referencePatientPositionManip = 
            referencePatientManip.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center;
        CenterToRef(newPatientManip, referencePatientPositionManip);
        TunePinchSlider(newBoneManip.transform.gameObject.GetComponentInChildren<Renderer>().bounds.size);
        cTReader.ct = newScans;

        GameObject boxBone = referenceBoneManip;
        GameObject boxPatient = referencePatientManip;
        TextAsset boxCt = referenceScans;
        referenceBoneManip = newBoneManip;
        referencePatientManip = newPatientManip;
        referenceScans = newScans;
        newBoneManip = boxBone;
        newPatientManip = boxPatient;
        newScans = boxCt;

        newPatientManip.SetActive(false);
        referencePatientManip.SetActive(true);

        CenterToRef(newPatientScrew,
            referencePatientScrew.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center);
        boxPatient = referencePatientScrew;
        referencePatientScrew = newPatientScrew;
        newPatientScrew = boxPatient;

        newPatientScrew.SetActive(false);
        referencePatientScrew.SetActive(true);

        return new Tuple<GameObject, GameObject>(referencePatientManip, referencePatientScrew);
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
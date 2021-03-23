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

    // Screw Scene References
    public GameObject referencePatientScrew, newPatientScrew;

    private enum Axis
    {
        x,
        y,
        z
    }

    void Start()
    {
        referencePatientPositionManip = referencePatientManip.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center;
        referenceBonePositionManip = referenceBoneManip.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center;

        Vector3 offset = ComputeOffsetFromOriginalCentres(GetPatientOriginalPosition(referencePatientManip), cTReader.ctCenter, referencePatientManip);
        CenterToAxWithOffset(pinchSliderHor, referenceBonePositionManip, offset, Axis.x);
        CenterToAxWithOffset(pinchSliderVer, referenceBonePositionManip, offset, Axis.z);
        TunePinchSliders(RetrieveCombinedBounds(referencePatientManip).size,
            RetrieveBoneSize(referencePatientManip),
            cTReader.ctLength,
            cTReader.ctDepth);
    }

    private Bounds RetrieveCombinedBounds(GameObject parent)
    {
        Renderer renderer = parent.GetComponentInChildren<Renderer>();
        Bounds combinedBounds = renderer.bounds;
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
        foreach (Renderer render in renderers)
        {
            if (render != renderer) combinedBounds.Encapsulate(render.bounds);
        }
        return combinedBounds;
    }

    private Vector3 RetrieveBoneSize(GameObject patient)
    {
        Vector3 rotationFactor = patient.transform.eulerAngles;
        Vector3 rotationBox = patient.transform.localEulerAngles;

        Vector3 scaleFactor = patient.transform.lossyScale;
        Vector3 scaleBox = patient.transform.localScale;
        
        patient.transform.localEulerAngles = new Vector3((-1 * rotationFactor.x), (-1 * rotationFactor.y), (-1 * rotationFactor.z));
        patient.transform.localScale = new Vector3(
            (1 / scaleFactor.x), (1 / scaleFactor.y), (1 / scaleFactor.z));

        Vector3 res = RetrieveCombinedBounds(patient).size;

        patient.transform.localScale = scaleBox;
        patient.transform.localEulerAngles = rotationBox;

        return res;
    }

    public Tuple<GameObject, GameObject> SwitchPatient()
    {
        cTReader.ct = newScans;
        cTReader.Init();
        referencePatientPositionManip = 
            referencePatientManip.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center;
        CenterToRef(newPatientManip, referencePatientPositionManip);

        Vector3 offset = ComputeOffsetFromOriginalCentres(GetPatientOriginalPosition(newPatientManip), cTReader.ctCenter, newPatientManip);
        CenterToAxWithOffset(pinchSliderHor, referenceBonePositionManip, offset, Axis.x);
        CenterToAxWithOffset(pinchSliderVer, referenceBonePositionManip, offset, Axis.z);

        TunePinchSliders(RetrieveCombinedBounds(newPatientManip).size,
            RetrieveBoneSize(newPatientManip),
            cTReader.ctLength,
            cTReader.ctDepth);

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

    public static void SameResizeToRef(GameObject obj, Vector3 referenceSize)
    {
        Vector3 oldLocalScale = obj.transform.localScale;
        Vector3 oldSize = obj.GetComponentInChildren<Renderer>().bounds.size;

        float newX = oldLocalScale.x / oldSize.x * referenceSize.x;

        obj.transform.localScale = new Vector3(newX, newX, newX);
    }

    /*
    private void OldTunePinchSliders(Vector3 referenceSize)
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
    */

    private void CenterToAxWithOffset(GameObject obj, Vector3 referencePos, Vector3 offset, Axis axis)
    {
        if(offset == null)
        {
            offset = new Vector3(0, 0, 0);
        }
        Vector3 startingPosition = obj.transform.position;
        float newAxis = startingPosition[(int)axis] -
            obj.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center[(int)axis] +
            referencePos[(int)axis] + 
            offset[(int)axis];

        startingPosition[(int)axis] = newAxis;
        obj.transform.position = startingPosition;
    }

    private Vector3 ComputeOffsetFromOriginalCentres(Vector3 refOrigin, Vector3 offsetOrigin, GameObject refObj)
    {
        Quaternion refRotation = refObj.transform.rotation;
        Vector3 realOffset = new Vector3(
            offsetOrigin.x - refOrigin.x,
            offsetOrigin.y - refOrigin.y,
            offsetOrigin.z - refOrigin.z),
            rotatedRealOffset = refRotation * realOffset,
            lossyScale = refObj.transform.lossyScale,
            scaledRealOffset = new Vector3(
                lossyScale.x * rotatedRealOffset.x,
                lossyScale.y * rotatedRealOffset.y,
                lossyScale.z * rotatedRealOffset.z);

        return scaledRealOffset;
    }

    private Vector3 GetPatientOriginalPosition(GameObject patient)
    {
        Vector3 backupScale = patient.transform.localScale,
            backupPosition = patient.transform.localPosition,
            backupRotation = patient.transform.localEulerAngles,
            zeroVector = new Vector3(0, 0, 0),
            unitVector = new Vector3(1, 1, 1);
        Transform backupParent = patient.transform.parent;

        patient.transform.parent = null;
        patient.transform.localScale = unitVector;
        patient.transform.localPosition = zeroVector;
        patient.transform.localEulerAngles = zeroVector;

        Vector3 patientCenter = RetrieveCombinedBounds(patient).center;

        patient.transform.parent = backupParent;
        patient.transform.localScale = backupScale;
        patient.transform.localPosition = backupPosition;
        patient.transform.localEulerAngles = backupRotation;


        return patientCenter;
    }

    private void TunePinchSliders(Vector3 referenceSize, Vector3 actualSize, float ctLength, float ctDepth)
    {
        // referenceSize.x : actualSize.z = x : ctLength
        // referenceSize.z : actualSize.y = x : ctDepth

        float ctLengthRef = ctLength * referenceSize.x / actualSize.z;

        PinchSlider pinchSliderCompHor = pinchSliderHor.GetComponent<PinchSlider>();
        float distanceHor = ctLengthRef / (pinchSliderHor.transform.lossyScale.x * 2);
        pinchSliderCompHor.SliderStartDistance = -distanceHor;
        pinchSliderCompHor.SliderEndDistance = distanceHor;

        Vector3 oldLocalScaleHor = visualTrackHor.transform.localScale;
        float newTrackLocalScaleHor = oldLocalScaleHor.x /
            visualTrackHor.transform.gameObject.GetComponentInChildren<Renderer>().bounds.size.x *
            ctLengthRef;

        visualTrackHor.transform.localScale = new Vector3(newTrackLocalScaleHor, oldLocalScaleHor.y, oldLocalScaleHor.z);

        float ctDepthRef = ctDepth * referenceSize.z / actualSize.y;
        
        PinchSlider pinchSliderCompVer = pinchSliderVer.GetComponent<PinchSlider>();
        float distanceVer = ctDepthRef / (pinchSliderVer.transform.lossyScale.x * 2);
        pinchSliderCompVer.SliderStartDistance = -distanceVer;
        pinchSliderCompVer.SliderEndDistance = distanceVer;

        Vector3 oldLocalScaleVer = visualTrackVer.transform.localScale;
        float newTrackLocalScaleVer = oldLocalScaleVer.x /
            visualTrackVer.transform.gameObject.GetComponentInChildren<Renderer>().bounds.size.z *
            ctDepthRef;

        visualTrackVer.transform.localScale = new Vector3(newTrackLocalScaleVer, oldLocalScaleVer.y, oldLocalScaleVer.z);
    }
}
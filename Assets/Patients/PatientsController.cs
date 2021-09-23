using Dummiesman;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
#endif

public class PatientsController : MonoBehaviour
{
    // Manipulation Scene References
    public GameObject newPatientManip, referencePatientManip, onScreenPatientManip;
    public GameObject pinchSliderHor, pinchSliderVer;
    public CTReader cTReader;
    public TextAsset newScans, referenceScans, onScreenScans;
    private byte[] newScansB, referenceScansB, onScreenScansB;

    // Dummy Object at (0,0,0)
    public GameObject dummyObject;

    void Start()
    {
        // TutunePinchSliders();
    }

    public void Step1()
    {
        cTReader.Init();
    }

    public void Step2()
    {
        TutunePinchSliders();
        cTReader.ComputeOffsets();
    }

    private void TutuneTranslation()
    {
        Vector3 refCenter = cTReader.GetCenterOfCt(referenceScansB);
        Vector3 newCenter = cTReader.GetCenterOfCt(newScansB);
        float xTranslation = refCenter.x - newCenter.x,
            yTranslation = refCenter.y - newCenter.y,
            zTranslation = refCenter.z - newCenter.z;
        newPatientManip.transform.localPosition = new Vector3(xTranslation, yTranslation, zTranslation);
        
        Vector3 pshPos = pinchSliderHor.transform.localPosition,
            psvPos = pinchSliderVer.transform.localPosition;
        pinchSliderHor.transform.localPosition = new Vector3(
            pshPos.x + xTranslation, pshPos.y + yTranslation, pshPos.z + zTranslation);
        pinchSliderVer.transform.localPosition = new Vector3(
            psvPos.x + xTranslation, psvPos.y + yTranslation, psvPos.z + zTranslation);

        foreach (GameObject go in cTReader.GetPoints())
        {
            Vector3 goPos = go.transform.localPosition;
            go.transform.localPosition = new Vector3(
                goPos.x + xTranslation, goPos.y + yTranslation, goPos.z + zTranslation);
        }
    }

    private void TutunePinchSliders()
    {
        pinchSliderHor.transform.localPosition = cTReader.center.transform.localPosition;
        pinchSliderVer.transform.localPosition = cTReader.center.transform.localPosition;

        pinchSliderHor.transform.localScale = new Vector3(400f, 400f, 400f);
        pinchSliderHor.transform.localEulerAngles= new Vector3(0f, 90f, 0f);
        pinchSliderVer.transform.localScale = new Vector3(400f, 400f, 400f);
        pinchSliderVer.transform.localEulerAngles = new Vector3(180f, 0f, 0f);

        PinchSlider psH = pinchSliderHor.GetComponentInChildren<PinchSlider>();
        float sliderLength = (Math.Abs(cTReader.bottomBackRight.transform.localPosition.z - cTReader.bottomBackLeft.transform.localPosition.z))
            / (2 * pinchSliderHor.transform.localScale.z);
        psH.SliderStartDistance = sliderLength;
        psH.SliderEndDistance = -sliderLength;
        // 1 : .250 = x : 2*sliderLength --> x = .9725 / .250
        float newScaleX = (2 * sliderLength) / 0.250f;
        psH.transform.GetChild(0).transform.localScale = new Vector3(newScaleX, 1f, 1f);
        psH.transform.localPosition = new Vector3(cTReader.bottomFrontLeft.transform.localPosition.x,
            cTReader.bottomFrontLeft.transform.localPosition.y,
            psH.transform.localPosition.z);

        PinchSlider psV = pinchSliderVer.GetComponentInChildren<PinchSlider>();
        sliderLength = (Math.Abs(cTReader.bottomFrontLeft.transform.localPosition.x - cTReader.bottomBackLeft.transform.localPosition.x))
            / (2 * pinchSliderVer.transform.localScale.z);
        psV.SliderStartDistance = sliderLength;
        psV.SliderEndDistance = -sliderLength;
        newScaleX = (2 * sliderLength) / 0.250f;
        psV.transform.GetChild(0).transform.localScale = new Vector3(newScaleX, 1f, 1f);
        psV.transform.localPosition = new Vector3(psV.transform.localPosition.x,
            cTReader.bottomFrontRight.transform.localPosition.y,
            cTReader.bottomFrontRight.transform.localPosition.z);
    }

    public void SwitchPatient()
    {
        /*
        foreach (GameObject go in cTReader.GetPoints())
        {
            Destroy(go);
        }

        cTReader.ct_bytes = newScansB;
        cTReader.Init();

        TutunePinchSliders();
        TutuneTranslation();
        cTReader.ComputeOffsets();

        globalController.patient = newPatientManip;
        byte[] boxCtB = newScansB;
        newPatientManip = onScreenPatientManip;
        newScansB = onScreenScansB;
        onScreenPatientManip = globalController.patient;
        onScreenScansB = boxCtB;

        newPatientManip.SetActive(false);
        onScreenPatientManip.SetActive(true);

        foreach (var item in new GameObject[] {pinchSliderHor, pinchSliderVer})
        {
            foreach (SliderSlice sliderSlice in item.GetComponentsInChildren<SliderSlice>())
            {
                sliderSlice.UpdateHelper();
            }
        }
    }

    private GameObject findChildrenWithName(Transform parent, String name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Equals(name))
            {
                return child.gameObject;
            }
            GameObject res = findChildrenWithName(child, name);
            if (res != null)
            {
                return res;
            }
        }

        return null;
    }

    public static void CenterToRef(GameObject obj, Vector3 referencePosition)
    {
        obj.transform.position = obj.transform.position -
            obj.transform.gameObject.GetComponentInChildren<Renderer>().bounds.center +
            referencePosition;
    }
}
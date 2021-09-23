using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Debug = UnityEngine.Debug;
using System;

#if !UNITY_EDITOR && UNITY_WSA
using Windows.ApplicationModel.Core;
#endif

public enum ColorState { Select, Edit };
public enum ScaleState { Half, Original, Double };
public enum BoneState { ShowAll, HidePlates, HideAll };

public class GlobalController : MonoBehaviour//, IMixedRealitySpeechHandler
{
    // Transform and positions
    class TransformInfo
    {
        public Vector3 pos;
        public Quaternion rotate;
        public Vector3 scale;
    }

    private static List<GameObject> bones = new List<GameObject>();
    private static List<TransformInfo> originalTransform = new List<TransformInfo>();


    // Colors and opacities
    public static ColorState gColorState { get; set; }

    public static int numberOfBones = 0;
    // adjusted bones
    public static BoneState gBoneState { get; set; }

    // Slider
    public GameObject slider, slider2;
    public GameObject[] quads;
    public Material quadTransparentMat, quadCyanMat, quadYellowMat;

    // Menu
    public GameObject nearMenu;

    // Bone handlers
    public GameObject boneGroup;

    // Patient
    public GameObject patient;

    // Patient Switch Controller
    public PatientsController patientsController;

    // CT handlers
    public GameObject ctGroup;

    // Hand CT Plane
    public GameObject ctPlane3;
    private HandSlice ctPlane;

    // Group handlers
    public GameObject boneRef;

    //Document slate
    public GameObject docSlate;

    // Scale
    public static ScaleState gScaleState { get; set; }

    // Screw Scene References
    public GameObject manipulationScene;
    public ScrewSceneController screwSceneController;

    // Start is called before the first frame update
    void Start()
    {
        // init bone references
        InitBoneReferences();

        ctPlane = ctPlane3.GetComponent<HandSlice>();
    }

    public void GoToManipScene()
    {
        startingScene.SetActive(false);
        screwScene.SetActive(false);
        manipulationScene.SetActive(true);
    }

    public void GoToScrewScene()
    {
        startingScene.SetActive(false);
        manipulationScene.SetActive(false);
        screwScene.SetActive(true);
    }

    public void GoToStartingScene()
    {
        manipulationScene.SetActive(false);
        screwScene.SetActive(false);
        startingScene.SetActive(true);
    }

    public void InitBoneReferences()
    {
        InitBoneReferencesHelper(patient.transform);
    }

    private void InitBoneReferencesHelper(Transform parent)
    {
        foreach (Transform child in parent)
        {
            InitBoneReferencesHelper(child);
            if (child.name.StartsWith("b"))
            {
                bones.Add(child.gameObject);
                TransformInfo ti = new TransformInfo
                {
                    pos = child.transform.localPosition,
                    rotate = child.transform.localRotation,
                    scale = child.transform.localScale
                };
                originalTransform.Add(ti);
            }
        }
    }

    public void DeactivateHandSlicer()
    {
        TextMeshPro[] texts = GameObject.Find("ChangeHandState").GetComponentsInChildren<TextMeshPro>();
        HandSlice ctplane = GameObject.Find("CTPlane3").GetComponent<HandSlice>();
        ctplane.active = false;
        foreach (TextMeshPro tmp in texts)
        {
            tmp.text = "Activate Hand Slicer";
        }
    }

    public void ActivateHandSlicer()
    {
        TextMeshPro[] texts = GameObject.Find("ChangeHandState").GetComponentsInChildren<TextMeshPro>();
        HandSlice ctplane = GameObject.Find("CTPlane3").GetComponent<HandSlice>();
        ctplane.active = true;
        foreach (TextMeshPro tmp in texts)
        {
            tmp.text = "Deactivate Hand Slicer";
        }
    }

    public void ScanToPlanes()
    {
        foreach (var quad in quads)
        {
            Renderer r = quad.GetComponent<Renderer>();
            SliderSlice s = quad.GetComponent<SliderSlice>();
            if (s.enabled)
            {
                s.enabled = false;
                r.material = s.colorTexture == SliderSlice.ColorFlag.Cyan ? quadCyanMat : quadYellowMat;

            }
            else
            {
                s.enabled = true;
                r.material = quadTransparentMat;
                s.Init();
            }
        }
    }

    public void ChangeHandSlicerState()
    {
        Debug.Log("Change Hand State Pressed");

        if (ctPlane.active == true)
        {
            DeactivateHandSlicer();
            ShowSlider();
        }
        else
        {
            HideSlider();
            ActivateHandSlicer();
        }
    }

    private void ShowSlider()
    {
        TextMeshPro[] texts = GameObject.Find("ShowSlider").GetComponentsInChildren<TextMeshPro>();
        slider.SetActive(true);
        slider2.SetActive(true);
        foreach (TextMeshPro tmp in texts)
        {
            tmp.text = "Hide Slider";
        }
    }

    private void HideSlider()
    {
        TextMeshPro[] texts = GameObject.Find("ShowSlider").GetComponentsInChildren<TextMeshPro>();
        slider.SetActive(false);
        slider2.SetActive(false);
        foreach (TextMeshPro tmp in texts)
        {
            tmp.text = "Show Slider";
        }
    }

    public void ShowOrHideSlider()
    {
        Debug.Log("Slider Button Pressed");

        if (ctPlane.active == true)
        {
            return;
        }

        if (slider.activeInHierarchy)
        {
            HideSlider();
        }
        else
        {
            ShowSlider();
        }
    }
    public async void RestartApp()
    {
#if !UNITY_EDITOR && UNITY_WSA
        // Attempt restart, with arguments.
        AppRestartFailureReason result =
            await CoreApplication.RequestRestartAsync("");

        if (result == AppRestartFailureReason.NotInForeground
            || result == AppRestartFailureReason.Other)
        {
            Debug.Log("Please manually restart.");
        }
#endif
    }
}

static class GlobalConstants
{
    public const String SCREW_GROUP = "Screws";
    public const String PLATE_GROUP = "Plates";
    public const String BONE_GROUP = "Bone";
}

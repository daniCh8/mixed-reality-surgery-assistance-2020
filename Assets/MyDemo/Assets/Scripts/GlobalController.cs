using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Debug = UnityEngine.Debug;
using BoundingBox = Microsoft.MixedReality.Toolkit.UI.BoundingBox;
using ManipulationHandler = Microsoft.MixedReality.Toolkit.UI.ManipulationHandler;

public enum ColorState { Select, Edit };
public enum ScaleState { Half, Original, Double };
public enum BoneState { ShowAll, HidePlates, HideAll };

public class GlobalController : MonoBehaviour
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
    private static List<TransformInfo> originalTransformAdjusted = new List<TransformInfo>();


    // Colors and opacities
    public static ColorState gColorState { get; set; }

    public static int numberOfBones = 0;//, numberOfAdjustedBones = 1;
    // adjusted bones
    public static BoneState gBoneState { get; set; }

    // Slider
    private static GameObject slider, slider2;

    // Menu
    private static GameObject nearMenu, handMenu;

    // Bone handlers
    private static BoundingBox boneBoundingBox;
    private static ManipulationHandler manipulationHandler;

    // Scale
    public static ScaleState gScaleState { get; set; }

    // Start is called before the first frame update
    void Start()
    {

        numberOfBones = 6;

        // init bone references
        for (int i = 1; i <= numberOfBones; i++)
        {
            GameObject t = GameObject.Find("Bone_" + i);
            bones.Add(t);
            Transform tr = t.GetComponent<Transform>();
            TransformInfo ti = new TransformInfo
            {
                pos = tr.localPosition,
                rotate = tr.localRotation,
                scale = tr.localScale
            };
            originalTransform.Add(ti);
        }

        gBoneState = BoneState.ShowAll;

        // slider enable/disable
        slider = GameObject.Find("PinchSlider");
        slider2 = GameObject.Find("PinchSliderHor");

        // scale functions
        gScaleState = ScaleState.Original;

        // menus
        nearMenu = GameObject.Find("NearMenu");
        handMenu = GameObject.Find("HandMenu");

        boneBoundingBox = GameObject.Find("Bone_1").GetComponent<BoundingBox>();
        manipulationHandler = GameObject.Find("Bone_1").GetComponent<ManipulationHandler>();
        
        handMenu.SetActive(false);

    }

    public void EnableBoundingBox()
    {
        Debug.Log("Enable Bounding Box pressed");

        TextMeshPro[] texts = GameObject.Find("EnableBoundingBox").GetComponentsInChildren<TextMeshPro>();

        if (boneBoundingBox.enabled)
        {
            boneBoundingBox.enabled = false;
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Enable BoundingBox";
            }
        }
        else
        {
            boneBoundingBox.enabled = true;
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Disable BoundingBox";
            }
        }
    }

    public void EnableManipulation()
    {
        Debug.Log("Enable Manipulation pressed");

        TextMeshPro[] texts = GameObject.Find("EnableManipulation").GetComponentsInChildren<TextMeshPro>();

        if (manipulationHandler.enabled)
        {
            manipulationHandler.enabled = false;
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Enable Manipulation";
            }
        }
        else
        {
            manipulationHandler.enabled = true;
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Disable Manipulation";
            }
        }
    }

    public void ChangeHandness()
    {
        Debug.Log("Change Handness pressed");

        TextMeshPro[] texts = GameObject.Find("ChangeHandness").GetComponentsInChildren<TextMeshPro>();
        HandSlice ctplane = GameObject.Find("CTPlane3").GetComponent<HandSlice>();

        if (ctplane.leftHanded == true)
        {
            ctplane.leftHanded = false;
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Use Left Hand";
            }
        }
        else
        {
            ctplane.leftHanded = true;
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Use Right Hand";
            }
        }
    }

    public void ChangeHandSlicerState()
    {
        Debug.Log("Change Hand State Pressed");

        TextMeshPro[] texts = GameObject.Find("ChangeHandState").GetComponentsInChildren<TextMeshPro>();
        HandSlice ctplane = GameObject.Find("CTPlane3").GetComponent<HandSlice>();

        if (ctplane.active == true)
        {
            ctplane.active = false;
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Activate Hand Slicer";
            }
        }
        else
        {
            ctplane.active = true;
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Deactivate Hand Slicer";
            }
        }
    }

    public void ResetPositions()
    {
        Debug.Log("Reset Button Pressed");

        for (int i = 1; i < originalTransform.Count; i++)
        {
            bones[i].transform.localPosition = originalTransform[i].pos;
            bones[i].transform.localRotation = originalTransform[i].rotate;
            bones[i].transform.localScale = originalTransform[i].scale;
        }

        //adjustedBones[0].transform.localPosition = originalTransformAdjusted[0].pos;
        //adjustedBones[0].transform.localRotation = originalTransformAdjusted[0].rotate;
        //adjustedBones[0].transform.localScale = originalTransformAdjusted[0].scale;


        Vector3 scale = originalTransform[0].scale;
        Vector3 localScale = (gScaleState == ScaleState.Original) ? scale :
            (gScaleState == ScaleState.Double) ? scale * 2 : scale * 0.5f;
        bones[0].transform.localScale = localScale;
    }

    public void ShowOrHideAdjustments()
    {
        TextMeshPro[] texts = GameObject.Find("ShowAdjustment").GetComponentsInChildren<TextMeshPro>();

        switch (gBoneState)
        {
            case BoneState.ShowAll:
                {
                    //adjustedBones[6].SetActive(false);
                    foreach (TextMeshPro tmp in texts)
                    {
                        tmp.text = "Hide Adjustments";
                    }
                    gBoneState = BoneState.HidePlates;
                    break;
                }
            case BoneState.HidePlates:
                {
                    //adjustedBones[6].SetActive(true);
                    //adjustedBones[0].SetActive(false);
                    foreach (TextMeshPro tmp in texts)
                    {
                        tmp.text = "Show Adjustments";
                    }
                    gBoneState = BoneState.HideAll;
                    break;
                }
            case BoneState.HideAll:
                {
                    //adjustedBones[0].SetActive(true);
                    foreach (TextMeshPro tmp in texts)
                    {
                        tmp.text = "Hide Bone Plates";
                    }
                    gBoneState = BoneState.ShowAll;
                    break;
                }
        }
    }

    public void ChangeMenuType()
    {
        Debug.Log("Change Menu Type Pressed");


        if (nearMenu.activeInHierarchy)
        {
            nearMenu.SetActive(false);
            handMenu.SetActive(true);
        }
        else
        {
            nearMenu.SetActive(true);
            handMenu.SetActive(false);
        }
    }

    public void ShowOrHideSlider()
    {
        TextMeshPro[] texts = GameObject.Find("ShowSlider").GetComponentsInChildren<TextMeshPro>();

        Debug.Log("Slider Button Pressed");


        if (slider.activeInHierarchy)
        {
            slider.SetActive(false);
            slider2.SetActive(false);
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Show Slider";
            }
            ResetPositions();
            //ResetColorForAll();
        }
        else
        {
            slider.SetActive(true);
            slider2.SetActive(true);
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Hide Slider";
            }
        }
    }

    public void ScaleUpStep()
    {
        Vector3 scale = originalTransform[0].scale;
        float factor = bones[0].transform.localScale.x / scale.x;
        factor += 0.005f;
        bones[0].transform.localScale = scale * factor;
    }

    public void ScaleDownStep()
    {
        Vector3 scale = originalTransform[0].scale;
        float factor = bones[0].transform.localScale.x / scale.x;
        factor -= 0.015f;
        bones[0].transform.localScale = scale * factor;
    }

    public void ChangeScaleMode()
    {
        TextMeshPro[] texts = GameObject.Find("ScaleButton").GetComponentsInChildren<TextMeshPro>();

        Debug.Log("Change Scale Button Pressed");

        Vector3 scale = originalTransform[0].scale;
        //Vector3 pos = bones[0].transform.localPosition;
        //Vector3 scaledPosition = new Vector3(pos.x / scale.x, pos.y / scale.y, pos.z / scale.z);
        //bones[0].transform.localPosition = scaledPosition;
        switch (gScaleState)
        {
            case ScaleState.Half:
                {
                    gScaleState = ScaleState.Original;
                    for (int i = 1; i < 100; i++)
                    {
                        Invoke("ScaleUpStep", 0.5f * i / 100);
                    }
                    //bones[0].transform.localScale = scale;

                    foreach (TextMeshPro tmp in texts)
                    {
                        tmp.text = "Zoom: 2x";
                    }
                    break;
                }
            case ScaleState.Original:
                {
                    gScaleState = ScaleState.Double;
                    for (int i = 1; i < 200; i++)
                    {
                        Invoke("ScaleUpStep", 0.5f * i / 200);
                    }
                    foreach (TextMeshPro tmp in texts)
                    {
                        tmp.text = "Zoom: 0.5x";
                    }
                    break;
                }
            case ScaleState.Double:
                {
                    gScaleState = ScaleState.Half;
                    for (int i = 1; i < 100; i++)
                    {
                        Invoke("ScaleDownStep", 0.5f * i / 100);
                    }
                    foreach (TextMeshPro tmp in texts)
                    {
                        tmp.text = "Zoom: 1x";
                    }
                    break;
                }
            default:
                break;
        }
    }
}

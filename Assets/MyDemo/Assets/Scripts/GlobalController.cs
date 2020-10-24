using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

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
    //private static List<GameObject> adjustedBones = new List<GameObject>();
    private static List<TransformInfo> originalTransformAdjusted = new List<TransformInfo>();


    // Colors and opacities
    public static ColorState gColorState { get; set; }

    public static int numberOfBones = 0;//, numberOfAdjustedBones = 1;
    // adjusted bones
    public static BoneState gBoneState { get; set; }

    // Slider
    private static GameObject slider, slider2;

    // Scale
    public static ScaleState gScaleState { get; set; }

    // Start is called before the first frame update
    void Start()
    {

        //DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/MyDemo/Assets/Meshes");
        //FileInfo[] fis = di.GetFiles();

        //foreach (FileInfo fi in fis)
        //{
        //    // File Name Convensions : 
        //    // Bone_1.obj for base bones,
        //    // Bone_#.obj for fragments,
        //    // Bone_#_aligned.obj for adjusted fragments,
        //    // Bone_{#+1}_aligned.obj for addition artifitial structures.

        //    //Debug.Log(fi.Name);
        //    if (fi.Extension.Contains("obj"))
        //    {
        //        if (fi.Name.Contains("aligned"))
        //            numberOfAdjustedBones++;
        //        else
        //            numberOfBones++;
        //    }
        //}

        numberOfBones = 6;
        //numberOfAdjustedBones = 7;

        Debug.Log("Number of bones loaded: " + numberOfBones);
        //Debug.Log("Number of adjusted bones loaded: " + numberOfAdjustedBones);

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

        /*
        for (int i = 1; i <= numberOfAdjustedBones; i++)
        {
            GameObject t = GameObject.Find("Bone_" + i + "_aligned");
            adjustedBones.Add(t);
            Transform tr = t.GetComponent<Transform>();
            TransformInfo ti = new TransformInfo
            {
                pos = tr.localPosition,
                rotate = tr.localRotation,
                scale = tr.localScale
            };
            originalTransformAdjusted.Add(ti);
        }
        */

        gBoneState = BoneState.ShowAll;

        // color mode init
        //gColorState = ColorState.Select;

        // slider enable/disable
        slider = GameObject.Find("PinchSlider");
        slider2 = GameObject.Find("PinchSliderHor");

        // scale functions
        gScaleState = ScaleState.Original;

    }

    public void ResetPositions()
    {
        Debug.Log("Reset Botton Pressed");
        Debug.Log(originalTransform.Count);

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

    /*
    public void ResetColorForAll()
    {
        foreach (GameObject o in bones)
        {
            o.GetComponent<AdjustBoneColor>().ResetColor();
        }
    }
    */

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

    public void ShowOrHideSlider()
    {
        TextMeshPro[] texts = GameObject.Find("ShowSlider").GetComponentsInChildren<TextMeshPro>();


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

    /*
    public void ChangeColoringMode()
    {
        gColorState = 1 - gColorState;
        TextMeshPro[] texts = GameObject.Find("EditOpacityButton").GetComponentsInChildren<TextMeshPro>();
        foreach (TextMeshPro tmp in texts)
        {
            if (gColorState == ColorState.Select)
                tmp.text = "Edit Opacity";
            else
                tmp.text = "Fix Opacity";
        }
    }
    */

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

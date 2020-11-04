using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Debug = UnityEngine.Debug;
using BoundingBox = Microsoft.MixedReality.Toolkit.UI.BoundingBox;
using ManipulationHandler = Microsoft.MixedReality.Toolkit.UI.ManipulationHandler;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Linq;
using Microsoft.MixedReality.Toolkit;

public enum ColorState { Select, Edit };
public enum ScaleState { Half, Original, Double };
public enum BoneState { ShowAll, HidePlates, HideAll };
public enum MenuState { Hand, Near }

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
    private static List<TransformInfo> originalTransformAdjusted = new List<TransformInfo>();


    // Colors and opacities
    public static ColorState gColorState { get; set; }

    public static int numberOfBones = 0;
    // adjusted bones
    public static BoneState gBoneState { get; set; }

    // Slider
    private static GameObject slider, slider2;

    // Menu
    private static GameObject nearMenu, handMenu;

    // Bone handlers
    private static BoundingBox boneBoundingBox;
    private static ManipulationHandler boneManipulationHandler;

    // CT handlers
    private static BoundingBox ctBoundingBox;
    private static ManipulationHandler ctManipulationHandler;

    // Group handlers
    private static BoundingBox allBoundingBox;
    private static ManipulationHandler allManipulationHandler;

    //Document slate
    private static GameObject docSlate;

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

        //document slate
        docSlate = GameObject.Find("Slate");

        GameObject bone_1_ref = GameObject.Find("Bone_1");

        // bone manipulation components
        boneBoundingBox = bone_1_ref.transform.Find("BoneGroup").gameObject.GetComponent<BoundingBox>();
        boneManipulationHandler = bone_1_ref.transform.Find("BoneGroup").gameObject.GetComponent<ManipulationHandler>();

        // CT manipulation components
        ctBoundingBox = bone_1_ref.transform.Find("CTGroup").gameObject.GetComponent<BoundingBox>();
        ctManipulationHandler = bone_1_ref.transform.Find("CTGroup").gameObject.GetComponent<ManipulationHandler>();

        // group manipulation components
        allBoundingBox = bone_1_ref.GetComponent<BoundingBox>();
        allManipulationHandler = bone_1_ref.GetComponent<ManipulationHandler>();

        handMenu.SetActive(false);
    }

    public void SwitchGroupManipulation()
    {
        Debug.Log("All Manipulation Button pressed");
        TextMeshPro[] texts = GameObject.Find("AllManipulationButton").GetComponentsInChildren<TextMeshPro>();

        if (allBoundingBox.enabled)
        {
            DisableGroupManipulation(texts);
        }
        else
        {
            if (boneBoundingBox.enabled)
            {
                TextMeshPro[] boneTexts = GameObject.Find("BoneManipulationButton").GetComponentsInChildren<TextMeshPro>();
                DisableBoneManipulation(boneTexts);
            }

            if (ctBoundingBox.enabled)
            {
                TextMeshPro[] ctTexts = GameObject.Find("ScansManipulationButton").GetComponentsInChildren<TextMeshPro>();
                DisableScansManipulation(ctTexts);
            }

            EnableGroupManipulation(texts);
        }
    }

    private void EnableGroupManipulation(TextMeshPro[] texts)
    {
        allBoundingBox.enabled = true;
        allManipulationHandler.enabled = true;
        foreach (TextMeshPro tmp in texts)
        {
            tmp.text = "Disable All Manipulation";
        }
    }

    private void DisableGroupManipulation(TextMeshPro[] texts)
    {
        allBoundingBox.enabled = false;
        allManipulationHandler.enabled = false;
        foreach (TextMeshPro tmp in texts)
        {
            tmp.text = "Enable All Manipulation";
        }
    }

    public void SwitchScansManipulation()
    {
        Debug.Log("Scans Manipulation Button pressed");
        TextMeshPro[] texts = GameObject.Find("ScansManipulationButton").GetComponentsInChildren<TextMeshPro>();

        if (ctBoundingBox.enabled)
        {
            DisableScansManipulation(texts);
        }
        else
        {
            if(allBoundingBox.enabled)
            {
                TextMeshPro[] allTexts = GameObject.Find("AllManipulationButton").GetComponentsInChildren<TextMeshPro>();
                DisableGroupManipulation(allTexts);
            }

            EnableScansManipulation(texts);
        }
    }

    private void EnableScansManipulation(TextMeshPro[] texts)
    {
        ctBoundingBox.enabled = true;
        ctManipulationHandler.enabled = true;
        foreach (TextMeshPro tmp in texts)
        {
            tmp.text = "Disable Planes Manipulation";
        }
    }

    private void DisableScansManipulation(TextMeshPro[] texts)
    {
        ctBoundingBox.enabled = false;
        ctManipulationHandler.enabled = false;
        foreach (TextMeshPro tmp in texts)
        {
            tmp.text = "Enable Planes Manipulation";
        }
    }

    public void SwitchBoneManipulation()
    {
        Debug.Log("Bone Manipulation Button pressed");
        TextMeshPro[] texts = GameObject.Find("BoneManipulationButton").GetComponentsInChildren<TextMeshPro>();

        if (boneBoundingBox.enabled)
        {
            DisableBoneManipulation(texts);
        }
        else
        {
            if (allBoundingBox.enabled)
            {
                TextMeshPro[] allTexts = GameObject.Find("AllManipulationButton").GetComponentsInChildren<TextMeshPro>();
                DisableGroupManipulation(allTexts);
            }

            EnableBoneManipulation(texts);
        }
    }

    private void EnableBoneManipulation(TextMeshPro[] texts)
    {
        boneBoundingBox.enabled = true;
        boneManipulationHandler.enabled = true;
        foreach (TextMeshPro tmp in texts)
        {
            tmp.text = "Disable Bone Manipulation";
        }
    }

    private void DisableBoneManipulation(TextMeshPro[] texts)
    {
        boneBoundingBox.enabled = false;
        boneManipulationHandler.enabled = false;
        foreach (TextMeshPro tmp in texts)
        {
            tmp.text = "Enable Bone Manipulation";
        }
    }

    public void ManageManipulation()
    {
        Debug.Log("Manage Manipulation pressed");
        
        if(nearMenu.activeInHierarchy)
        {
            EnableManipulationMenu(nearMenu);
        }
        else
        {
            EnableManipulationMenu(handMenu);
        }
    }

    private void EnableManipulationMenu(GameObject menu)
    {
        GameObject buttonCollection = menu.transform.Find("ButtonCollection").gameObject;
        GameObject mainMenuCollection = buttonCollection.transform.Find("MainMenuCollection").gameObject;
        GameObject manipulationMenuCollection = buttonCollection.transform.Find("ManipulationMenuCollection").gameObject;

        mainMenuCollection.SetActive(false);
        manipulationMenuCollection.SetActive(true);
    }

    public void GoBackToMainMenu()
    {
        Debug.Log("Back to Main pressed");

        if (nearMenu.activeInHierarchy)
        {
            DisableManipulationMenu(nearMenu);
        }
        else
        {
            DisableManipulationMenu(handMenu);
        }

    }

    private void DisableManipulationMenu(GameObject menu)
    {
        GameObject buttonCollection = menu.transform.Find("ButtonCollection").gameObject;
        GameObject mainMenuCollection = buttonCollection.transform.Find("MainMenuCollection").gameObject;
        GameObject manipulationMenuCollection = buttonCollection.transform.Find("ManipulationMenuCollection").gameObject;

        manipulationMenuCollection.SetActive(false);
        mainMenuCollection.SetActive(true);
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

    public void ChangeHandSlicerState()
    {
        Debug.Log("Change Hand State Pressed");

        HandSlice ctplane = GameObject.Find("CTPlane3").GetComponent<HandSlice>();

        if (ctplane.active == true)
        {
            DeactivateHandSlicer();
        }
        else
        {
            ActivateHandSlicer();
        }
    }

    /*
    void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        if (eventData.Command.Keyword == "Stop Tracking")
        {
            DeactivateHandSlicer();
        }
        else if (eventData.Command.Keyword == "Track My Hand")
        {
            ActivateHandSlicer();
        }
    }
    */

    public void ResetPositions()
    {
        Debug.Log("Reset Button Pressed");

        for (int i = 1; i < originalTransform.Count; i++)
        {
            bones[i].transform.localPosition = originalTransform[i].pos;
            bones[i].transform.localRotation = originalTransform[i].rotate;
            bones[i].transform.localScale = originalTransform[i].scale;
        }


        Vector3 scale = originalTransform[0].scale;
        Vector3 localScale = (gScaleState == ScaleState.Original) ? scale :
            (gScaleState == ScaleState.Double) ? scale * 2 : scale * 0.5f;
        bones[0].transform.localScale = localScale;
    }

    private bool isASharedButton(PressableButton button)
    {
        return button.name != "ButtonPin" && button.name != "ChangeMenuType";
    }

    private void resetButtonTexts(MenuState oldMenu)
    {
        PressableButton[] nearButtons = Array.FindAll(nearMenu.GetComponentsInChildren<PressableButton>(true), isASharedButton).ToArray();
        PressableButton[] handButtons = Array.FindAll(handMenu.GetComponentsInChildren<PressableButton>(true), isASharedButton).ToArray();

        IDictionary<string, string> buttonsText = new Dictionary<string, string>();
        PressableButton[] oldButtons;
        PressableButton[] newButtons;

        oldButtons = oldMenu == MenuState.Hand ? handButtons : nearButtons;
        newButtons = oldMenu == MenuState.Hand ? nearButtons : handButtons;

        foreach (var button in oldButtons)
        {
            buttonsText.Add(button.name, button.GetComponentInChildren<ButtonConfigHelper>().MainLabelText);
        }

        foreach (var button in newButtons)
        {
            TextMeshPro[] texts = button.GetComponentsInChildren<TextMeshPro>();
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = buttonsText[button.name];
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

            handMenu.SetChildrenActive(true);
            handMenu.transform.Find("ButtonCollection").Find("ManipulationMenuCollection").gameObject.SetActive(false);
            
            resetButtonTexts(MenuState.Near);
        }
        else
        {
            nearMenu.SetActive(true);
            handMenu.SetActive(false);

            nearMenu.SetChildrenActive(true);
            nearMenu.transform.Find("ButtonCollection").Find("ManipulationMenuCollection").gameObject.SetActive(false);

            resetButtonTexts(MenuState.Hand);
        }
    }

    public void VisualizeDocuments()
    {
        TextMeshPro[] texts = GameObject.Find("VisualizeDocuments").GetComponentsInChildren<TextMeshPro>();

        Debug.Log("Visualize Documents Pressed");

        if (docSlate.activeInHierarchy)
        {
            docSlate.SetActive(false);

            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Visualize Documents";
            }
        }
        else
        {
            docSlate.SetActive(true);

            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Hide Documents";
            }
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
        switch (gScaleState)
        {
            case ScaleState.Half:
                {
                    gScaleState = ScaleState.Original;
                    for (int i = 1; i < 100; i++)
                    {
                        Invoke("ScaleUpStep", 0.5f * i / 100);
                    }

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

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
    private BoundingBox boneBoundingBox;
    private ManipulationHandler boneManipulationHandler;

    // Patient
    public GameObject patient;

    // Patient Switch Controller
    public PatientsController patientsController;

    // CT handlers
    public GameObject ctGroup;
    private BoundingBox ctBoundingBox;
    private ManipulationHandler ctManipulationHandler;

    // Hand CT Plane
    public GameObject ctPlane3;
    private HandSlice ctPlane;

    // Group handlers
    public GameObject boneRef;
    private BoundingBox allBoundingBox;
    private ManipulationHandler allManipulationHandler;

    //Document slate
    public GameObject docSlate;

    // Scale
    public static ScaleState gScaleState { get; set; }

    // Screw Scene References
    public GameObject screwScene, manipulationScene, startingScene;
    public ScrewSceneController screwSceneController;

    // Start is called before the first frame update
    void Start()
    {
        /*
        // init bone references
        InitBoneReferences();

        gBoneState = BoneState.ShowAll;

        // scale functions
        gScaleState = ScaleState.Original;

        //document slate
        Material firstPage = docSlate.transform.Find("ContentQuad").gameObject.GetComponent<Pages>().getMat();
        docSlate.transform.Find("ContentQuad").gameObject.GetComponent<Renderer>().material = firstPage;
        docSlate.SetActive(false);
        
        // bone manipulation components
        boneBoundingBox = boneGroup.gameObject.GetComponent<BoundingBox>();
        boneManipulationHandler = boneGroup.gameObject.GetComponent<ManipulationHandler>();

        // CT manipulation components
        ctBoundingBox = ctGroup.gameObject.GetComponent<BoundingBox>();
        ctManipulationHandler = ctGroup.gameObject.GetComponent<ManipulationHandler>();

        // group manipulation components
        allBoundingBox = boneRef.GetComponent<BoundingBox>();
        allManipulationHandler = boneRef.GetComponent<ManipulationHandler>();

        ctPlane = ctPlane3.GetComponent<HandSlice>();
        */

        screwSceneController.Init();
        patientsController.Init();
        FilePicker.CreateFolders();

        GoToStartingScene();
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
        
        EnableManipulationMenu(nearMenu);
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

        DisableManipulationMenu(nearMenu);
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

    public void NextSlatePage()
    {
        Material nextPage = docSlate.transform.Find("ContentQuad").gameObject.GetComponent<Pages>().nextMat();
        docSlate.transform.Find("ContentQuad").gameObject.GetComponent<Renderer>().material = nextPage;
    }

    public void PrevSlatePage()
    {
        Material prevPage = docSlate.transform.Find("ContentQuad").gameObject.GetComponent<Pages>().prevMat();
        docSlate.transform.Find("ContentQuad").gameObject.GetComponent<Renderer>().material = prevPage;
    }

    public void ChangeScene()
    {
        Debug.Log("Change Scene Button Pressed");

        if (screwScene.activeInHierarchy)
        {
            manipulationScene.SetActive(true);

            /*UpdateScenePosition(screwSceneController.allGroup.transform,
                screwSceneController.nearMenu.transform,
                boneRef.transform,
                nearMenu.transform);*/
            screwScene.SetActive(false);
        }
        else
        {
            screwScene.SetActive(true);

            /*UpdateScenePosition(boneRef.transform,
                nearMenu.transform,
                screwSceneController.allGroup.transform,
                screwSceneController.nearMenu.transform);*/

            manipulationScene.SetActive(false);
        }
    }

    public void ChangePatient()
    {
        patientsController.PickNewPatient(true);
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
                
            } else
            {
                s.enabled = true;
                r.material = quadTransparentMat;
                s.Init();
            }
        }
    }
}

static class GlobalConstants
{
    public const String SCREW_GROUP = "Screws";
    public const String PLATE_GROUP = "Plates";
    public const String BONE_GROUP = "Bone";
}

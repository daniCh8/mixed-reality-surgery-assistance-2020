using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScrewSceneController : MonoBehaviour
{
    // Menu State
    public enum MenuState { Hand, Near }

    // Plates Visibility State
    public enum PlatesState { Both, Lat, Med, None }

    // Screw Prefab to Instantiate
    public GameObject screwPrefab;

    // Menus
    public GameObject nearMenu, handMenu;

    // Groups
    public GameObject screwGroup, plateGroup, boneGroup, allGroup;

    // Screws Materials
    public Material newScrewMaterial, medScrewMaterial, latScrewMaterial, selectedScrewMaterial, boneMaterial;

    // Screw Button Handler
    public GameObject screwButton;

    // Scene
    public GameObject scene;

    // Screw Size Window
    public GameObject screwSizeWindow;

    // Lists
    public static List<GameObject> screws, bones;

    // Screw List Index
    private static int screwIndex;

    // Slate state
    private static PlatesState gPlatesState { get; set; }

    // Slates Characterized
    private GameObject latPlate, medPlate;

    // Manipulating Screws
    private bool manipulating;

    // Original Screw transforms
    private Dictionary<String, Vector3> originalScrewPositions, originalScrewScales;
    private Dictionary<String, Quaternion> originalScrewRotations;

    // All Group Starting transform
    private Vector3 allGroupStartingPosition, allGroupStartingScale;
    private Quaternion allGroupStartingRotation;

    // Screw Adding p
    public static bool AddingScrewFirstIndicator = false;
    public static bool AddingScrewSecondIndicator = false;
    public static Vector3 AddScrewPoint;
    private GameObject PointIndicator;
    public static bool ScrewAddModePlanar = false;
    // Screw Size text
    private TextMesh screwSizeText;


    void Start()
    {
        screws = new List<GameObject>();
        bones = new List<GameObject>();

        originalScrewPositions = new Dictionary<string, Vector3>();
        originalScrewScales = new Dictionary<string, Vector3>();
        originalScrewRotations = new Dictionary<string, Quaternion>();

        allGroupStartingPosition = allGroup.transform.position;
        allGroupStartingScale = allGroup.transform.localScale;
        allGroupStartingRotation = allGroup.transform.rotation;

        // Initialize Screw List
        foreach (Transform screw in screwGroup.transform)
        {
            foreach(Transform real_screw in screw.gameObject.transform)
            {
                screws.Add(real_screw.gameObject);
                originalScrewPositions.Add(real_screw.gameObject.name, real_screw.position);
                originalScrewScales.Add(real_screw.gameObject.name, real_screw.localScale);
                originalScrewRotations.Add(real_screw.gameObject.name, real_screw.rotation);
            }
        }

        // Initialize Plate List
        foreach (Transform plate in plateGroup.transform)
        {
            if (plate.gameObject.name.StartsWith(Constants.LAT_SCREW_TAG))
            {
                latPlate = plate.gameObject;
            }
            else
            {
                medPlate = plate.gameObject;
            }
        }

        screwIndex = 0;

        gPlatesState = PlatesState.Both;

        manipulating = false;

        screwSizeText = screwSizeWindow.GetComponentInChildren<TextMesh>(true);
    }

    private void SetTexts(TextMeshPro[] texts, String text)
    {
        foreach (TextMeshPro tmp in texts)
        {
            tmp.text = text;
        }
    }

    public void ChangeMenuType()
    {
        Debug.Log(Constants.CHANGE_MENU_BUTTON);

        if (nearMenu.activeInHierarchy)
        {
            nearMenu.SetActive(false);
            handMenu.SetActive(true);
            resetButtonTexts(MenuState.Near);
        }
        else
        {
            nearMenu.SetActive(true);
            handMenu.SetActive(false);
            resetButtonTexts(MenuState.Hand);
        }
    }

    public void ChangeBoneVisibility()
    {
        TextMeshPro[] texts = GameObject.Find(Constants.BONE_VISIBILITY_BUTTON).GetComponentsInChildren<TextMeshPro>();

        if (boneGroup.activeInHierarchy)
        {
            boneGroup.SetActive(false);
            SetTexts(texts, Constants.SHOW_BONE);
        }
        else
        {
            boneGroup.SetActive(true);
            SetTexts(texts, Constants.HIDE_BONE);
        }
    }

    public void ChangePlatesVisibility()
    {
        TextMeshPro[] texts = GameObject.Find(Constants.CHANGE_PLATES_VISIBILITY).GetComponentsInChildren<TextMeshPro>();

        switch (gPlatesState)
        {
            case PlatesState.Both:
                latPlate.SetActive(true);
                medPlate.SetActive(false);
                SetLatScrewsActive(true);
                SetMedScrewsActive(false);
                gPlatesState = PlatesState.Lat;
                SetTexts(texts, Constants.SHOW_MED_PLATE);
                break;
            case PlatesState.Lat:
                latPlate.SetActive(false);
                medPlate.SetActive(true);
                SetLatScrewsActive(false);
                SetMedScrewsActive(true);
                gPlatesState = PlatesState.Med;
                SetTexts(texts, Constants.SHOW_NO_PLATES);
                break;
            case PlatesState.Med:
                latPlate.SetActive(false);
                medPlate.SetActive(false);
                SetLatScrewsActive(true);
                SetMedScrewsActive(true);
                gPlatesState = PlatesState.None;
                SetTexts(texts, Constants.SHOW_BOTH_PLATES);
                break;
            case PlatesState.None:
                latPlate.SetActive(true);
                medPlate.SetActive(true);
                SetLatScrewsActive(true);
                SetMedScrewsActive(true);
                gPlatesState = PlatesState.Both;
                SetTexts(texts, Constants.SHOW_LAT_PLATE);
                break;
            default:
                break;
        }
    }

    private bool ScrewIsNotTag(GameObject screw, String flag)
    {
        return !ScrewIsTag(screw, flag);
    }

    private bool ScrewIsTag(GameObject screw, String flag)
    {
        return screw.CompareTag(flag);
    }

    private void SetLatScrewsActive(bool active)
    {
        foreach (GameObject screw in screws)
        {
            if (ScrewIsTag(screw, Constants.LAT_SCREW_TAG))
            {
                screw.SetActive(active);
            }
        }
    }

    private void SetMedScrewsActive(bool active)
    {
        foreach (GameObject screw in screws)
        {
            if (ScrewIsTag(screw, Constants.MED_SCREW_TAG))
            {
                screw.SetActive(active);
            }
        }
    }

    public void NextScrew()
    {
        DeactivateScrew(screws[screwIndex]);

        FindNextIndex();

        ActivateScrew(screws[screwIndex]);
    }

    public void PrevScrew()
    {
        DeactivateScrew(screws[screwIndex]);

        FindPrevIndex();

        ActivateScrew(screws[screwIndex]);
    }

    private void FindNextIndex()
    {
        if (gPlatesState == PlatesState.Both || gPlatesState == PlatesState.None)
        {
            NextIndex();
            while(IsDeletedScrew(screws[screwIndex]))
            {
                NextIndex();
            }
            return;
        }

        String flag = gPlatesState == PlatesState.Lat ? Constants.LAT_SCREW_TAG : Constants.MED_SCREW_TAG;
        NextIndex();
        while (ScrewIsNotTag(screws[screwIndex], flag) || IsDeletedScrew(screws[screwIndex]))
        {
            NextIndex();
        }
    }

    private void FindPrevIndex()
    {
        if (gPlatesState == PlatesState.Both || gPlatesState == PlatesState.None)
        {
            PrevIndex();
            while(IsDeletedScrew(screws[screwIndex]))
            {
                PrevIndex();
            }
            return;
        }

        String flag = gPlatesState == PlatesState.Lat ? Constants.LAT_SCREW_TAG : Constants.MED_SCREW_TAG;
        PrevIndex();
        while (ScrewIsNotTag(screws[screwIndex], flag) || IsDeletedScrew(screws[screwIndex]))
        {
            PrevIndex();
        }
    }

    private void PrevIndex()
    {
        screwIndex = (screwIndex == 0) ? (screws.Count - 1) : (screwIndex - 1);
    }

    private void NextIndex()
    {
        screwIndex = (screwIndex + 1 == screws.Count) ? 0 : (screwIndex + 1);
    }

    private void DeactivateScrew(GameObject screw)
    {
        switch (screw.tag)
        {
            case Constants.LAT_SCREW_TAG:
                screw.GetComponentInChildren<Renderer>().material = latScrewMaterial;
                break;
            case Constants.MED_SCREW_TAG:
                screw.GetComponentInChildren<Renderer>().material = medScrewMaterial;
                break;
            case Constants.NEW_SCREW_TAG:
                screw.GetComponentInChildren<Renderer>().material = newScrewMaterial;
                break;
        }

        SetCurrObjectManipulator(screw, false);
        screw.GetComponentInChildren<BoundsControl>(true).enabled = false;
        screw.GetComponentInChildren<ScaleConstraint>(true).enabled = false;
        screw.GetComponentInChildren<PositionConstraint>(true).enabled = false;
    }

    private void ActivateScrew(GameObject screw)
    {
        screw.GetComponentInChildren<BoundsControl>(true).enabled = true;
        screw.GetComponentInChildren<Renderer>().material = selectedScrewMaterial;
        SetCurrObjectManipulator(screw, manipulating);
        SetScrewSizeText(screw);
    }

    private void SetScrewSizeText(GameObject screw)
    {
        screwSizeText.text = Constants.SCREW_SIZE_STUB_START + ComputeScrewSize(screw) + Constants.SCREW_SIZE_STUB_END;
    }

    private double ComputeScrewSize(GameObject screw)
    {
        float scale = screw.transform.localScale.y * scene.transform.localScale.y * 100;
        return Math.Round(scale, 2);
    }

    public void ChangeScrewState()
    {
        TextMeshPro[] texts = GameObject.Find(Constants.SCREW_STATE_BUTTON).GetComponentsInChildren<TextMeshPro>();

        if (screwButton.activeInHierarchy)
        {
            screwButton.SetActive(false);
            foreach (GameObject screw in screws)
            {
                DeactivateScrew(screw);
            }
            SetTexts(texts, Constants.START_MANIPULATING_SCREWS);
        }
        else
        {
            screwButton.SetActive(true);
            FindNextIndex();
            ActivateScrew(screws[screwIndex]);
            SetTexts(texts, Constants.STOP_MANIPULATING_SCREWS);
        }
    }

    public void ChangeBoundsControlState()
    {
        TextMeshPro[] texts = GameObject.Find(Constants.CHANGE_BOUNDS_CONTROL).GetComponentsInChildren<TextMeshPro>();
        BoundsControl boundsControl = allGroup.GetComponentInChildren<BoundsControl>(true);
        ObjectManipulator objectManipulator = allGroup.GetComponentInChildren<ObjectManipulator>(true);
        BoxCollider boxCollider = allGroup.GetComponentInChildren<BoxCollider>(true);

        if (boundsControl.enabled)
        {
            boxCollider.enabled = false;
            boundsControl.enabled = false;
            objectManipulator.enabled = false;
            SetTexts(texts, Constants.ALLOW_MANIPULATION);
        }
        else
        {
            boxCollider.enabled = true;
            boundsControl.enabled = true;
            objectManipulator.enabled = true;
            SetTexts(texts, Constants.DISALLOW_MANIPULATION);
        }
    }

    private bool isASharedButton(PressableButton button)
    {
        return button.name != Constants.BUTTON_PIN_NAME && button.name != Constants.CHANGE_MENU_BUTTON_NAME;
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

    public void ResetScrews()
    {
        Vector3 allGroupPositionBackup = allGroup.transform.position;
        Vector3 allGroupScaleBackup = allGroup.transform.localScale;
        Quaternion allGroupRotationBackup = allGroup.transform.rotation;

        allGroup.transform.position = allGroupStartingPosition;
        allGroup.transform.localScale = allGroupStartingScale;
        allGroup.transform.rotation = allGroupStartingRotation;

        foreach (GameObject screw in screws)
        {
            if(IsOriginalScrew(screw))
            {
                if(IsDeletedScrew(screw))
                {
                    ChangeDeleteScrewTag(screw);
                }

                if(ShouldBeActiveScrew(screw))
                {
                    screw.SetActive(true);
                }
                
                screw.transform.position = originalScrewPositions[screw.name];
                screw.transform.localScale = originalScrewScales[screw.name];
                screw.transform.rotation = originalScrewRotations[screw.name];
            }
        }

        allGroup.transform.position = allGroupPositionBackup;
        allGroup.transform.localScale = allGroupScaleBackup;
        allGroup.transform.rotation = allGroupRotationBackup;
    }

    private bool IsDeletedScrew(GameObject screw)
    {
        return screw.CompareTag(Constants.LAT_DELETED_SCREW_TAG) ||
               screw.CompareTag(Constants.MED_DELETED_SCREW_TAG) ||
               screw.CompareTag(Constants.NEW_DELETED_SCREW_TAG);
    }

    private bool ShouldBeActiveScrew(GameObject screw)
    {
        switch (gPlatesState)
        {
            case PlatesState.Lat:
                return ScrewIsTag(screw, Constants.LAT_SCREW_TAG);
            case PlatesState.Med:
                return ScrewIsTag(screw, Constants.MED_SCREW_TAG);
            default:
                return true;
        }
    }

    private bool IsOriginalScrew(GameObject screw)
    {
        return screw.tag != Constants.NEW_SCREW_TAG;
    }


    public void NewScrew()
    {
        AddingScrewFirstIndicator = true;
        boneGroup.GetComponent<PointerHandler>().enabled = true;
        AddingScrewSecondIndicator = false;
        ScrewAddModePlanar = false;
    }

    public void NewScrewPlanar()
    {
        AddingScrewFirstIndicator = true;
        boneGroup.GetComponent<PointerHandler>().enabled = true;
        AddingScrewSecondIndicator = false;
        ScrewAddModePlanar = true;

    }

    public void Screwpointregister(MixedRealityPointerEventData eventData)
    {
        if(AddingScrewFirstIndicator && !AddingScrewSecondIndicator)
        {
            ScrewAddfirstpoint(eventData);
            AddingScrewSecondIndicator = true;
            AddingScrewFirstIndicator = false;
            boneGroup.GetComponent<FocusHandlerVisualizer>().enabled = true;
            if(!ScrewAddModePlanar)
            {
                boneGroup.GetComponent<FocusHandlerVisualizer>().enabled = true;
            }
            else
            {
                boneGroup.GetComponent<FocusHandlerOrientation>().enabled = true;
                allGroup.transform.Find("HandPlaneScrew").gameObject.SetActive(true);
            }
        }
        else if(AddingScrewSecondIndicator && !AddingScrewFirstIndicator && !ScrewAddModePlanar)
        {
            ScrewAddsecondpoint(eventData);
            AddingScrewFirstIndicator = false;
            AddingScrewSecondIndicator = false;
        }
    }

    public void TerminateAddingPoint()
    {
            Destroy(PointIndicator);
            boneGroup.GetComponent<PointerHandler>().enabled = false;
            boneGroup.GetComponent<FocusHandlerVisualizer>().enabled = false;
            boneGroup.GetComponent<FocusHandlerOrientation>().enabled = false;
            allGroup.transform.Find("HandPlaneScrew").gameObject.SetActive(false);
            boneMaterial.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }

    private void ScrewAddfirstpoint(MixedRealityPointerEventData eventData)
    {
        {
            PointIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            PointIndicator.transform.localScale = Vector3.one * 0.01f;
            Color translucentred = Color.red;
            translucentred.a = 0.3f;
            PointIndicator.GetComponent<Renderer>().material.color = translucentred;

            Vector3 pos = eventData.Pointer.Result.Details.Point;
            PointIndicator.transform.position = pos;
            PointIndicator.SetActive(true);
            AddScrewPoint = pos;

            boneMaterial.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);

        }
    }
    private void ScrewAddsecondpoint(MixedRealityPointerEventData eventData)
    {
        if(AddingScrewSecondIndicator && !AddingScrewFirstIndicator)
        {
            Vector3 pos = eventData.Pointer.Result.Details.Point;
            Vector3 p1 = LerpByDistance(AddScrewPoint, pos, -0.1f);
            Vector3 p2 = LerpByDistance(pos, AddScrewPoint, -0.1f);

            Debug.Log("New Screw Added");

            Destroy(PointIndicator);
            boneGroup.GetComponent<PointerHandler>().enabled = false;
            boneGroup.GetComponent<FocusHandlerVisualizer>().enabled = false;
            boneMaterial.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            NewScrewregister(p1, p2);
        }
    } 
    // public void AddScrewFirstPoint(MixedRealityPointerEventData eventData)
    // {
    //     Debug.Log(AddingScrewSecondIndicator);
    //     if (AddingScrewFirstIndicator){
    //         Debug.Log(AddScrewPoint);
    //         Vector3 pos = eventData.Pointer.Result.Details.Point;
    //         PointIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //         PointIndicator.transform.localScale = Vector3.one * 0.01f;
    //         Color translucentred = Color.red;
    //         translucentred.a = 0.3f;
    //         PointIndicator.GetComponent<Renderer>().material.color = translucentred;
    //         PointIndicator.transform.position = pos;
    //         PointIndicator.SetActive(true);
    //         AddScrewPoint = pos;
    //         AddingScrewSecondIndicator = true;
    //         AddingScrewFirstIndicator = false;
    //         boneGroup.GetComponent<FocusHandlerVisualizer>().enabled = true;
    //         boneMaterial.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
    //     }
    //     else if (AddingScrewSecondIndicator)
    //     {
    //         Vector3 pos = eventData.Pointer.Result.Details.Point;
    //         Vector3 p1 = LerpByDistance(AddScrewPoint, pos, -0.1f);
    //         Vector3 p2 = LerpByDistance(pos, AddScrewPoint, -0.1f);

    //         Debug.Log("New Screw Added");
    //         AddingScrewFirstIndicator = false;
    //         AddingScrewSecondIndicator = false;
    //         Destroy(PointIndicator);
    //         boneGroup.GetComponent<PointerHandler>().enabled = false;
    //         boneGroup.GetComponent<FocusHandlerVisualizer>().enabled = false;
    //         boneMaterial.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    //         NewScrewregister(p1, p2);
    //     }

    // }

    public static Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
    {
        Vector3 P = x * (B - A) + A;
        return P;
    }

    public void NewScrewregister(Vector3 pos1, Vector3 pos2)
    {
        var newScrew = CreateCylinderBetweenPoints(pos1,pos2);
        newScrew.tag = Constants.NEW_SCREW_TAG;
        newScrew.name = $"Screw_{screws.Count+1}";

        screws.Add(newScrew);
        newScrew.transform.parent = screwGroup.transform;
        DeactivateScrew(screws[screwIndex]);
        screwIndex = screws.Count - 1;
        ActivateScrew(screws[screwIndex]);

        // For some reason, this is the only way to get the BoundsControl going the first time. 
        screws[screwIndex].GetComponentInChildren<BoundsControl>(true).enabled = false;
        screws[screwIndex].GetComponentInChildren<BoundsControl>(true).enabled = true;
    }

    public GameObject CreateCylinderBetweenPoints(Vector3 start, Vector3 end)
    {
        var offset = end - start;
        var scale = new Vector3(0.01F, offset.magnitude / 2.0f, 0.01F);
        var position = start + (offset / 2.0f);

        var cylinder = Instantiate(screwPrefab, position, Quaternion.identity);
        
        cylinder.transform.up = offset;
        cylinder.transform.localScale = scale;

        return cylinder;
    }

    public void ManipulateScrew()
    {
        ButtonConfigHelper buttonConfig = GameObject.Find(Constants.SCREW_MANIPULATE_BUTTON).GetComponentInChildren<ButtonConfigHelper>();
        if(manipulating)
        {
            manipulating = false;
            buttonConfig.SetQuadIconByName(Constants.ICON_HAND_GESTURE);
        }
        else
        {
            manipulating = true;
            buttonConfig.SetQuadIconByName(Constants.ICON_STOP_HAND_GESTURE);
        }

        SetCurrObjectManipulator(screws[screwIndex], manipulating);
    }

    private void SetCurrObjectManipulator(GameObject screw, bool activate)
    {
        screw.GetComponentInChildren<ObjectManipulator>(true).enabled = activate;
        screw.GetComponentInChildren<NearInteractionGrabbable>(true).enabled = activate;
        screw.GetComponentInChildren<WholeScaleConstraint>(true).enabled = activate;
        screw.GetComponentInChildren<ScaleConstraint>(true).enabled = !activate;
        screw.GetComponentInChildren<PositionConstraint>(true).enabled = !activate;
    }

    public void DeleteScrew()
    {
        GameObject screwToDelete = screws[screwIndex];
        NextScrew();
        ChangeDeleteScrewTag(screwToDelete);
        screwToDelete.SetActive(false);
    }

    private void ChangeDeleteScrewTag(GameObject screw)
    {
        switch (screw.tag)
        {
            case Constants.LAT_SCREW_TAG:
                screw.tag = Constants.LAT_DELETED_SCREW_TAG;
                break;
            case Constants.MED_SCREW_TAG:
                screw.tag = Constants.MED_DELETED_SCREW_TAG;
                break;
            case Constants.NEW_SCREW_TAG:
                screw.tag = Constants.NEW_DELETED_SCREW_TAG;
                break;
            case Constants.LAT_DELETED_SCREW_TAG:
                screw.tag = Constants.LAT_SCREW_TAG;
                break;
            case Constants.MED_DELETED_SCREW_TAG:
                screw.tag = Constants.MED_SCREW_TAG;
                break;
            default:
                break;
        }
    }
}

static class Constants
{
    public const String LAT_SCREW_TAG = "Lat";
    public const String MED_SCREW_TAG = "Med";
    public const String NEW_SCREW_TAG = "New";
    public const String NEW_DELETED_SCREW_TAG = "NewDeleted";
    public const String LAT_DELETED_SCREW_TAG = "LatDeleted";
    public const String MED_DELETED_SCREW_TAG = "MedDeleted";
    public const String CHANGE_MENU_BUTTON = "Change Menu Button pressed";
    public const String CHANGE_MENU_BUTTON_NAME = "ChangeMenuType";
    public const String BUTTON_PIN_NAME = "ButtonPin";
    public const String DISALLOW_MANIPULATION = "Disallow Manipulation";
    public const String ALLOW_MANIPULATION = "Allow Manipulation";
    public const String CHANGE_BOUNDS_CONTROL = "ChangeBoundsControl";
    public const String STOP_MANIPULATING_SCREWS = "Stop Manipulating Screws";
    public const String START_MANIPULATING_SCREWS = "Manipulate Screws";
    public const String SCREW_STATE_BUTTON = "ChangeScrewState";
    public const String CHANGE_PLATES_VISIBILITY = "ChangePlatesVisibility";
    public const String SHOW_MED_PLATE = "Show Only Med Plate";
    public const String SHOW_LAT_PLATE = "Show Only Lat Plate";
    public const String SHOW_NO_PLATES = "Hide Plates";
    public const String SHOW_BOTH_PLATES = "Show Plates";
    public const String BONE_VISIBILITY_BUTTON = "ChangeBoneVisibility";
    public const String SHOW_BONE = "Show Bone";
    public const String HIDE_BONE = "Hide Bone";
    public const String SCREW_MANIPULATE_BUTTON = "ManipulateButton";
    public const String ICON_HAND_GESTURE = "hand-gest";
    public const String ICON_STOP_HAND_GESTURE = "stop-hand-gest";
    public const String SCREW_SIZE_STUB_START = "Screw Length: ";
    public const String SCREW_SIZE_STUB_END = " cm.";
}

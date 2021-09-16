using MathNet.Numerics.LinearAlgebra;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using static Microsoft.MixedReality.Toolkit.UI.ObjectManipulator;

public class ScrewSceneController : MonoBehaviour
{

    // Screw Positions
    public TextAsset screwLatPositions, screwDistPositions, screwMedPositions;

    // Plates Visibility State
    public enum PlatesState { All, Lat, Med, Dist, None }

    // Screw Prefab to Instantiate
    public GameObject screwPrefab;

    // Groups
    public GameObject patient, screwGroup, plateGroup, boneGroup, allGroup;

    // Screws Materials
    public Material newScrewMaterial, medScrewMaterial, latScrewMaterial, distScrewMaterial, selectedScrewMaterial, boneMaterial;

    // Screw Button Handler
    public GameObject screwButton, screwSizeSliderObject, rotationHandlerObject;
    private PinchSlider screwSizeSlider;

    // Scene
    public GameObject scene;

    // Screw Size Window
    public GameObject screwSizeWindow;

    // Near Menu
    public GameObject nearMenu;

    // List of screws
    public static List<GameObject> screws;

    // Screw List Index
    private static int screwIndex;

    // Slate state
    private static PlatesState gPlatesState { get; set; }

    // Slates Characterized
    private GameObject latPlate, medPlate, distPlate;

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

    public void Init()
    {
        foreach (Transform child in patient.transform)
        {
            if (String.Compare(child.name, ScrewConstants.BONE) == 0)
            {
                boneGroup = child.gameObject;
            }
            else if (String.Compare(child.name, ScrewConstants.PLATES) == 0)
            {
                plateGroup = child.gameObject;
            }
            else if (String.Compare(child.name, ScrewConstants.SCREWS) == 0)
            {
                screwGroup = child.gameObject;
            }
        }

        // Initialize Screws and Bones
        InitScrews();
        // Initialize Plate List
        InitPlates();

        gPlatesState = PlatesState.All;
        manipulating = false;
        screwSizeText = screwSizeWindow.GetComponentInChildren<TextMesh>(true);
        allGroup.AddComponent<IgnoreBoxCollider>();
        screwSizeSlider = screwSizeSliderObject.GetComponentInChildren<PinchSlider>(true);
    }

    private List<Tuple<Vector3, Vector3>> ProcessScrewPosition(string textScrewPositions)
    {
        String textAsset = textScrewPositions.Replace("\"", "");
        String[] lines = Regex.Split(textAsset, "\n|\r|\r\n");
        List<Tuple<Vector3, Vector3>> points = new List<Tuple<Vector3, Vector3>>();
        Vector3 dummyVector = new Vector3(0, 0, 0);
        Vector3 item1 = dummyVector;

        foreach(String line in lines)
        {
            if(line.Contains(",")) 
            {
                String[] vector = Regex.Split(line, ",");
                Vector3 newPoint = new Vector3(-1f * float.Parse(vector[0], CultureInfo.InvariantCulture.NumberFormat), 
                    float.Parse(vector[1], CultureInfo.InvariantCulture.NumberFormat), 
                    float.Parse(vector[2], CultureInfo.InvariantCulture.NumberFormat));

                if (item1 == dummyVector)
                {
                    item1 = newPoint;
                }
                else
                {
                    points.Add(new Tuple<Vector3, Vector3>(item1, newPoint));
                    item1 = dummyVector;
                }
            }
        }

        return points;
    }

    public void GenerateScrewsFromTextFiles()
    {
        bool backupSceneActive = scene.activeSelf;
        scene.SetActive(true);

        if (screwLatPositions != null)
        {
            GenerateScrewsFromTextFileHelper(screwLatPositions.text, ScrewConstants.LAT_SCREW_TAG, latScrewMaterial);
        }

        if (screwMedPositions != null)
        {
            GenerateScrewsFromTextFileHelper(screwMedPositions.text, ScrewConstants.MED_SCREW_TAG, medScrewMaterial);
        }

        if (screwDistPositions != null)
        {
            GenerateScrewsFromTextFileHelper(screwDistPositions.text, ScrewConstants.DIST_SCREW_TAG, distScrewMaterial);
        }

        scene.SetActive(backupSceneActive);
    }

    private void GenerateScrewsFromTextFileHelper(string textScrewPositions, string tag, Material material)
    {
        List<Tuple<Vector3, Vector3>> textScrewPoints = ProcessScrewPosition(textScrewPositions);
        int i = 0;
        foreach (var textScrew in textScrewPoints)
        {
            Vector3 startPos = screwGroup.transform.TransformPoint(textScrew.Item1),
                endPos = screwGroup.transform.TransformPoint(textScrew.Item2);
            GameObject cylinderScrew = CreateCylinderBetweenPoints(startPos, endPos);
            cylinderScrew.transform.parent = screwGroup.transform;
            cylinderScrew.tag = tag;
            cylinderScrew.name = tag + i++;
            cylinderScrew.GetComponent<MeshRenderer>().material = material;
            cylinderScrew.GetComponent<ObjectManipulator>().enabled = false;
            cylinderScrew.GetComponent<NearInteractionGrabbable>().enabled = false;

            // cylinderScrew.GetComponent<BoundsControl>().enabled = false;
            // cylinderScrew.GetComponent<ScaleConstraint>().enabled = false;
            // cylinderScrew.GetComponent<WholeScaleConstraint>().enabled = false;
            // cylinderScrew.GetComponent<PositionConstraint>().enabled = false;

            screws.Add(cylinderScrew);
            originalScrewPositions.Add(cylinderScrew.name, cylinderScrew.transform.position);
            originalScrewScales.Add(cylinderScrew.name, cylinderScrew.transform.localScale);
            originalScrewRotations.Add(cylinderScrew.name, cylinderScrew.transform.rotation);
        }
    }

    private void InitScrews()
    {
        screws = new List<GameObject>();

        originalScrewPositions = new Dictionary<string, Vector3>();
        originalScrewScales = new Dictionary<string, Vector3>();
        originalScrewRotations = new Dictionary<string, Quaternion>();

        allGroupStartingPosition = allGroup.transform.position;
        allGroupStartingScale = allGroup.transform.localScale;
        allGroupStartingRotation = allGroup.transform.rotation;

        GenerateScrewsFromTextFiles();
        SetScrewTags();

        screwIndex = 0;
    }

    private void InitPlates()
    {
        if(plateGroup == null)
        {
            return;
        }
        foreach (Transform plate in plateGroup.transform)
        {
            if (plate.gameObject.name.StartsWith(ScrewConstants.LAT_SCREW_TAG))
            {
                latPlate = plate.gameObject;
            }
            else if (plate.gameObject.name.StartsWith(ScrewConstants.MED_SCREW_TAG))
            {
                medPlate = plate.gameObject;
            }
            else
            {
                distPlate = plate.gameObject;
            }
        }
    }

    private void SetScrewTags()
    {
        foreach (GameObject screw in screws)
        {
            if (IsOriginalScrew(screw))
            {
                if(screw.name.Contains(ScrewConstants.LAT_SCREW_TAG, StringComparison.OrdinalIgnoreCase) ||
                    screw.transform.parent.name.Contains(ScrewConstants.LAT_SCREW_TAG, StringComparison.OrdinalIgnoreCase) ||
                    (screw.transform.childCount > 0 && screw.transform.GetChild(0).name.Contains(ScrewConstants.LAT_SCREW_TAG, StringComparison.OrdinalIgnoreCase)))
                {
                    screw.tag = ScrewConstants.LAT_SCREW_TAG;
                } else if(screw.name.Contains(ScrewConstants.MED_SCREW_TAG, StringComparison.OrdinalIgnoreCase) ||
                    screw.transform.parent.name.Contains(ScrewConstants.MED_SCREW_TAG, StringComparison.OrdinalIgnoreCase) ||
                    (screw.transform.childCount > 0 && screw.transform.GetChild(0).name.Contains(ScrewConstants.MED_SCREW_TAG, StringComparison.OrdinalIgnoreCase)))
                {
                    screw.tag = ScrewConstants.MED_SCREW_TAG;
                } else
                {
                    screw.tag = ScrewConstants.DIST_SCREW_TAG;
                }
            }
        }
    }

    private TextMeshPro[] RetrieveButtonText(String buttonName)
    {
        return RetrieveButtonFromHierarchy(buttonName).GetComponentsInChildren<TextMeshPro>();
    }

    private GameObject RetrieveButtonFromHierarchy(String objectName)
    {
        return RetrieveButtonFromHierarchyHelper(objectName, nearMenu.transform);
    }

    private GameObject RetrieveButtonFromHierarchyHelper(String buttonName, Transform parent)
    {
        foreach(Transform child in parent)
        {
            if(child.name.Equals(buttonName))
            {
                return child.gameObject;
            }
            GameObject res = RetrieveButtonFromHierarchyHelper(buttonName, child);
            if(res != null)
            {
                return res;
            }
        }
        return null;
    } 

    private void SetTexts(TextMeshPro[] texts, String text)
    {
        foreach (TextMeshPro tmp in texts)
        {
            tmp.text = text;
        }
    }

    public void ChangeBoneVisibility()
    {
        TextMeshPro[] texts = RetrieveButtonText(ScrewConstants.BONE_VISIBILITY_BUTTON);

        if (boneGroup.activeSelf)
        {
            boneGroup.SetActive(false);
            SetTexts(texts, ScrewConstants.SHOW_BONE);
        }
        else
        {
            boneGroup.SetActive(true);
            SetTexts(texts, ScrewConstants.HIDE_BONE);
        }
    }

    public void Debuggie()
    {
        return;
    }

    private void TrySettingPlate(PlatesState plate, bool flag)
    {
        switch (plate)
        {
            case PlatesState.Lat:
                if (latPlate != null)
                {
                    latPlate.SetActive(flag);
                }
                break;
            case PlatesState.Med:
                if (medPlate != null)
                {
                    medPlate.SetActive(flag);
                }
                break;
            case PlatesState.Dist:
                if (distPlate != null)
                {
                    distPlate.SetActive(flag);
                }
                break;
            default:
                break;
        }
    }

    private void ActivatePlate(PlatesState plate)
    {
        SetLatScrewsActive(false);
        SetMedScrewsActive(false);
        SetDistScrewsActive(false);

        TrySettingPlate(PlatesState.Lat, false);
        TrySettingPlate(PlatesState.Med, false);
        TrySettingPlate(PlatesState.Dist, false);

        gPlatesState = plate;

        switch (plate)
        {
            case PlatesState.All:
                SetLatScrewsActive(true);
                SetMedScrewsActive(true);
                SetDistScrewsActive(true);
                TrySettingPlate(PlatesState.Lat, true);
                TrySettingPlate(PlatesState.Med, true);
                TrySettingPlate(PlatesState.Dist, true);
                break;
            case PlatesState.Lat:
                SetLatScrewsActive(true);
                TrySettingPlate(PlatesState.Lat, true);
                break;
            case PlatesState.Med:
                SetMedScrewsActive(true);
                TrySettingPlate(PlatesState.Med, true);
                break;
            case PlatesState.Dist:
                SetDistScrewsActive(true);
                TrySettingPlate(PlatesState.Dist, true);
                break;
            case PlatesState.None:
                SetLatScrewsActive(true);
                SetMedScrewsActive(true);
                SetDistScrewsActive(true);
                break;
            default:
                break;
        }
    }

    public void ChangePlatesVisibility()
    {
        TextMeshPro[] texts = RetrieveButtonText(ScrewConstants.CHANGE_PLATES_VISIBILITY);

        switch (gPlatesState)
        {
            case PlatesState.All:
                if(AreThereLatScrews())
                {
                    ActivatePlate(PlatesState.Lat);
                    if(AreThereMedScrews()) SetTexts(texts, ScrewConstants.SHOW_MED_PLATE);
                    else if(AreThereDistScrews()) SetTexts(texts, ScrewConstants.SHOW_DIST_PLATE);
                    else SetTexts(texts, ScrewConstants.SHOW_NO_PLATES);
                }
                else if(AreThereMedScrews())
                {
                    ActivatePlate(PlatesState.Med);
                    if (AreThereDistScrews()) SetTexts(texts, ScrewConstants.SHOW_DIST_PLATE);
                    else SetTexts(texts, ScrewConstants.SHOW_NO_PLATES);
                }
                else if (AreThereDistScrews())
                {
                    ActivatePlate(PlatesState.Dist);
                    SetTexts(texts, ScrewConstants.SHOW_NO_PLATES);
                }
                else
                {
                    ActivatePlate(PlatesState.None);
                    SetTexts(texts, ScrewConstants.SHOW_ALL_PLATES);
                }
                break;
            case PlatesState.Lat:
                if (AreThereMedScrews())
                {
                    ActivatePlate(PlatesState.Med);
                    if (AreThereDistScrews()) SetTexts(texts, ScrewConstants.SHOW_DIST_PLATE);
                    else SetTexts(texts, ScrewConstants.SHOW_NO_PLATES);
                }
                else if (AreThereDistScrews())
                {
                    ActivatePlate(PlatesState.Dist);
                    SetTexts(texts, ScrewConstants.SHOW_NO_PLATES);
                }
                else
                {
                    ActivatePlate(PlatesState.None);
                    SetTexts(texts, ScrewConstants.SHOW_ALL_PLATES);
                }
                break;
            case PlatesState.Med:
                if (AreThereDistScrews())
                {
                    ActivatePlate(PlatesState.Dist);
                    SetTexts(texts, ScrewConstants.SHOW_NO_PLATES);
                }
                else
                {
                    ActivatePlate(PlatesState.None);
                    SetTexts(texts, ScrewConstants.SHOW_ALL_PLATES);
                }
                break;
            case PlatesState.Dist:
                ActivatePlate(PlatesState.None);
                SetTexts(texts, ScrewConstants.SHOW_ALL_PLATES);
                break;
            case PlatesState.None:
                ActivatePlate(PlatesState.All);
                if (AreThereLatScrews()) SetTexts(texts, ScrewConstants.SHOW_LAT_PLATE);
                else if (AreThereMedScrews()) SetTexts(texts, ScrewConstants.SHOW_MED_PLATE);
                else if (AreThereDistScrews()) SetTexts(texts, ScrewConstants.SHOW_DIST_PLATE);
                else SetTexts(texts, ScrewConstants.SHOW_NO_PLATES);
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
            if (ScrewIsTag(screw, ScrewConstants.LAT_SCREW_TAG))
            {
                screw.SetActive(active);
            }
        }
    }

    private void SetDistScrewsActive(bool active)
    {
        foreach (GameObject screw in screws)
        {
            if (ScrewIsTag(screw, ScrewConstants.DIST_SCREW_TAG))
            {
                screw.SetActive(active);
            }
        }
    }

    private void SetMedScrewsActive(bool active)
    {
        foreach (GameObject screw in screws)
        {
            if (ScrewIsTag(screw, ScrewConstants.MED_SCREW_TAG))
            {
                screw.SetActive(active);
            }
        }
    }

    private bool AreThereDistScrews()
    {
        foreach (GameObject screw in screws)
        {
            if (ScrewIsTag(screw, ScrewConstants.DIST_SCREW_TAG))
            {
                return true;
            }
        }
        return false;
    }

    private bool AreThereMedScrews()
    {
        foreach (GameObject screw in screws)
        {
            if (ScrewIsTag(screw, ScrewConstants.MED_SCREW_TAG))
            {
                return true;
            }
        }
        return false;
    }

    private bool AreThereLatScrews()
    {
        foreach (GameObject screw in screws)
        {
            if (ScrewIsTag(screw, ScrewConstants.LAT_SCREW_TAG))
            {
                return true;
            }
        }
        return false;
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
        if (gPlatesState == PlatesState.All || gPlatesState == PlatesState.None)
        {
            NextIndex();
            while(IsDeletedScrew(screws[screwIndex]))
            {
                NextIndex();
            }
            return;
        }

        String flag = RetrievePlatesFlag();
        NextIndex();
        while (ScrewIsNotTag(screws[screwIndex], flag) || IsDeletedScrew(screws[screwIndex]))
        {
            NextIndex();
        }
    }

    private String RetrievePlatesFlag()
    {
        switch (gPlatesState)
        {
            case PlatesState.Lat:
                return ScrewConstants.LAT_SCREW_TAG;
            case PlatesState.Med:
                return ScrewConstants.MED_SCREW_TAG;
            case PlatesState.Dist:
                return ScrewConstants.DIST_SCREW_TAG;
            default:
                return null;
        }
    }

    private void FindPrevIndex()
    {
        if (gPlatesState == PlatesState.All || gPlatesState == PlatesState.None)
        {
            PrevIndex();
            while(IsDeletedScrew(screws[screwIndex]))
            {
                PrevIndex();
            }
            return;
        }

        String flag = RetrievePlatesFlag();
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
            case ScrewConstants.LAT_SCREW_TAG:
                screw.GetComponentInChildren<Renderer>().material = latScrewMaterial;
                break;
            case ScrewConstants.MED_SCREW_TAG:
                screw.GetComponentInChildren<Renderer>().material = medScrewMaterial;
                break;
            case ScrewConstants.NEW_SCREW_TAG:
                screw.GetComponentInChildren<Renderer>().material = newScrewMaterial;
                break;
        }

        SetCurrObjectManipulator(screw, false);
        screw.GetComponentInChildren<ScrewSizeUpdate>(true).enabled = false;
        screw.GetComponentInChildren<ScrewRotationUpdate>(true).enabled = false;
    }

    private void ActivateScrew(GameObject screw)
    {
        screw.GetComponentInChildren<Renderer>().material = selectedScrewMaterial;
        UpdateScrewSizeSlider(screw);
        UpdateScrewRotationHandler(screw);
        screw.GetComponentInChildren<ScrewSizeUpdate>(true).enabled = true;
        SetCurrObjectManipulator(screw, true);
        SetScrewSizeText(screw.GetComponentInChildren<ScrewSizeUpdate>(true).screwSize);
    }

    private void UpdateScrewRotationHandler(GameObject screw)
    {
        rotationHandlerObject.transform.eulerAngles = screw.transform.eulerAngles;
        var screwRotationUpdate = screw.GetComponentInChildren<ScrewRotationUpdate>(true);
        screwRotationUpdate.rotationHandler = rotationHandlerObject;
        screwRotationUpdate.enabled = true;
    }

    private void UpdateScrewSizeSlider(GameObject screw)
    {
        var size = ComputeScrewSize(screw);
        var screwSizeUpdate = screw.GetComponentInChildren<ScrewSizeUpdate>(true);

        screwSizeUpdate.screwSize = size;
        screwSizeUpdate.pinchSlider = screwSizeSlider;
        screwSizeUpdate.screwSceneController = this;

        screwSizeSlider.SliderValue = (float)(size / ScrewConstants.MAX_LENGTH_SCREW);
        screwSizeUpdate.previousPinchSliderVal = screwSizeSlider.SliderValue;
    }

    public void SetScrewSizeText(double size)
    {
        screwSizeText.text = ScrewConstants.SCREW_SIZE_STUB_START +
            Math.Round(size, 2) +
            ScrewConstants.SCREW_SIZE_STUB_END;
    }

    private double ComputeScrewSize(GameObject screw)
    {
        float scale = screw.transform.localScale.y * scene.transform.localScale.y * 100;
        return scale;
    }

    public void ChangeScrewState()
    {
        TextMeshPro[] texts = RetrieveButtonText(ScrewConstants.SCREW_STATE_BUTTON);

        if (screwButton.activeInHierarchy)
        {
            screwButton.SetActive(false);
            screwSizeSliderObject.SetActive(false);
            rotationHandlerObject.SetActive(false);
            foreach (GameObject screw in screws)
            {
                DeactivateScrew(screw);
            }
            SetTexts(texts, ScrewConstants.START_MANIPULATING_SCREWS);
        }
        else
        {
            screwButton.SetActive(true);
            screwSizeSliderObject.SetActive(true);
            rotationHandlerObject.SetActive(true);
            FindNextIndex();
            ActivateScrew(screws[screwIndex]);
            DeactivateAllBounds();
            SetTexts(texts, ScrewConstants.STOP_MANIPULATING_SCREWS);
        }
    }

    private void DeactivateAllBounds()
    {
        while(allGroup.GetComponentInChildren<BoxCollider>(true).enabled == true)
        {
            ChangeBoundsControlState();
        }
    }

    public void ChangeBoundsControlState()
    {
        TextMeshPro[] texts = RetrieveButtonText(ScrewConstants.CHANGE_BOUNDS_CONTROL);
        BoundingBox boundingBox = allGroup.GetComponentInChildren<BoundingBox>(true);
        ManipulationHandler manipulationHandler = allGroup.GetComponentInChildren<ManipulationHandler>(true);
        NearInteractionGrabbable nearInteractionGrabbable = allGroup.GetComponentInChildren<NearInteractionGrabbable>(true);
        BoxCollider boxCollider = allGroup.GetComponentInChildren<BoxCollider>(true);

        if (boundingBox.enabled)
        {
            boxCollider.enabled = false;
            boundingBox.enabled = false;
            manipulationHandler.enabled = false;
            nearInteractionGrabbable.enabled = false;
            SetTexts(texts, ScrewConstants.ALLOW_MANIPULATION);
        }
        else
        {
            boxCollider.enabled = true;
            boundingBox.enabled = true;
            manipulationHandler.enabled = true;
            nearInteractionGrabbable.enabled = true;
            SetTexts(texts, ScrewConstants.DISALLOW_MANIPULATION);
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
                
                screw.transform.position = originalScrewPositions[screw.transform.name];
                screw.transform.localScale = originalScrewScales[screw.transform.name];
                screw.transform.rotation = originalScrewRotations[screw.transform.name];
            }
        }

        allGroup.transform.position = allGroupPositionBackup;
        allGroup.transform.localScale = allGroupScaleBackup;
        allGroup.transform.rotation = allGroupRotationBackup;
    }

    private bool IsDeletedScrew(GameObject screw)
    {
        return screw.CompareTag(ScrewConstants.LAT_DELETED_SCREW_TAG) ||
               screw.CompareTag(ScrewConstants.MED_DELETED_SCREW_TAG) ||
               screw.CompareTag(ScrewConstants.NEW_DELETED_SCREW_TAG);
    }

    private bool ShouldBeActiveScrew(GameObject screw)
    {
        switch (gPlatesState)
        {
            case PlatesState.Lat:
                return ScrewIsTag(screw, ScrewConstants.LAT_SCREW_TAG);
            case PlatesState.Med:
                return ScrewIsTag(screw, ScrewConstants.MED_SCREW_TAG);
            default:
                return true;
        }
    }

    private bool IsOriginalScrew(GameObject screw)
    {
        return screw.tag != ScrewConstants.NEW_SCREW_TAG && screw.tag != ScrewConstants.NEW_DELETED_SCREW_TAG;
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

    public static Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
    {
        Vector3 P = x * (B - A) + A;
        return P;
    }

    public void NewScrewregister(Vector3 pos1, Vector3 pos2)
    {
        var newScrew = CreateCylinderBetweenPoints(pos1,pos2);
        newScrew.tag = ScrewConstants.NEW_SCREW_TAG;
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

    public void AlignCylinder(GameObject cylinder, Vector3 start, Vector3 end)
    {
        Transform parentBackup = cylinder.transform.parent;
        cylinder.transform.parent = null;

        var offset = end - start;
        var scale = new Vector3(0.01F, offset.magnitude / 2.0f, 0.01F);
        var position = start + (offset / 2.0f);
        cylinder.transform.localPosition = position;
        cylinder.transform.localRotation = Quaternion.identity;
        cylinder.transform.up = offset;
        cylinder.transform.localScale = scale;
        cylinder.transform.parent = parentBackup;
    }

    public GameObject CreateCylinderBetweenPoints(Vector3 start, Vector3 end)
    {
        var cylinder = Instantiate(screwPrefab);
        AlignCylinder(cylinder, start, end);

        cylinder.AddComponent<Rigidbody>();
        cylinder.AddComponent<CapsuleCollider>();
        cylinder.AddComponent<OnTrigger>();
        cylinder.GetComponent<OnTrigger>().selectedScrewMaterial = selectedScrewMaterial;
        cylinder.GetComponent<OnTrigger>().selectedFlag = false;
        cylinder.GetComponent<CapsuleCollider>().isTrigger = true;
        cylinder.GetComponent<Rigidbody>().useGravity = false;
        cylinder.GetComponent<Rigidbody>().isKinematic = true;

        var startPt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        startPt.transform.position = start;
        startPt.transform.localScale = new Vector3();
        startPt.transform.name = ScrewConstants.FIRST_POINT_NAME;
        startPt.transform.parent = cylinder.transform;

        var endPt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        endPt.transform.position = end;
        endPt.transform.localScale = new Vector3();
        endPt.transform.name = ScrewConstants.SECOND_POINT_NAME;
        endPt.transform.parent = cylinder.transform;

        return cylinder;
    }

    public void ManipulateScrew()
    {
        ButtonConfigHelper buttonConfig = RetrieveButtonFromHierarchy(ScrewConstants.SCREW_MANIPULATE_BUTTON).GetComponentInChildren<ButtonConfigHelper>();
        if(manipulating)
        {
            manipulating = false;
            buttonConfig.SetQuadIconByName(ScrewConstants.ICON_HAND_GESTURE);
        }
        else
        {
            manipulating = true;
            buttonConfig.SetQuadIconByName(ScrewConstants.ICON_STOP_HAND_GESTURE);
        }

        SetCurrObjectManipulator(screws[screwIndex], manipulating);
    }

    private void SetCurrObjectManipulator(GameObject screw, bool activate)
    {
        screw.GetComponentInChildren<ObjectManipulator>(true).enabled = activate;
        screw.GetComponentInChildren<NearInteractionGrabbable>(true).enabled = activate;
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
            case ScrewConstants.LAT_SCREW_TAG:
                screw.tag = ScrewConstants.LAT_DELETED_SCREW_TAG;
                break;
            case ScrewConstants.MED_SCREW_TAG:
                screw.tag = ScrewConstants.MED_DELETED_SCREW_TAG;
                break;
            case ScrewConstants.NEW_SCREW_TAG:
                screw.tag = ScrewConstants.NEW_DELETED_SCREW_TAG;
                break;
            case ScrewConstants.LAT_DELETED_SCREW_TAG:
                screw.tag = ScrewConstants.LAT_SCREW_TAG;
                break;
            case ScrewConstants.MED_DELETED_SCREW_TAG:
                screw.tag = ScrewConstants.MED_SCREW_TAG;
                break;
            default:
                break;
        }
    }
}

public static class ScrewConstants
{
    public const String LAT_SCREW_TAG = "Lat";
    public const String DIST_SCREW_TAG = "Dist";
    public const String MED_SCREW_TAG = "Med";
    public const String NEW_SCREW_TAG = "New";
    public const String DIST_DELETED_SCREW_TAG = "DistDeleted";
    public const String NEW_DELETED_SCREW_TAG = "NewDeleted";
    public const String LAT_DELETED_SCREW_TAG = "LatDeleted";
    public const String MED_DELETED_SCREW_TAG = "MedDeleted";
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
    public const String SHOW_DIST_PLATE = "Show Only Dist Plate";
    public const String SHOW_NO_PLATES = "Hide Plates";
    public const String SHOW_ALL_PLATES = "Show Plates";
    public const String BONE_VISIBILITY_BUTTON = "ChangeBoneVisibility";
    public const String SHOW_BONE = "Show Bone";
    public const String HIDE_BONE = "Hide Bone";
    public const String SCREW_MANIPULATE_BUTTON = "ManipulateButton";
    public const String ICON_HAND_GESTURE = "hand-gest";
    public const String ICON_STOP_HAND_GESTURE = "stop-hand-gest";
    public const String SCREW_SIZE_STUB_START = "Screw Length: ";
    public const String SCREW_SIZE_STUB_END = " cm.";
    public const String SCREWS = "Screws";
    public const String PLATES = "Plates";
    public const String BONE = "Bone";
    public const String FIRST_POINT_NAME = "StartPoint";
    public const String SECOND_POINT_NAME = "EndPoint";
    public const float MAX_LENGTH_SCREW = 15f;
}

public static class StringExtensions
{
    public static bool Contains(this string source, string toCheck, StringComparison comp)
    {
        return source?.IndexOf(toCheck, comp) >= 0;
    }
}

﻿using MathNet.Numerics.LinearAlgebra;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using static Microsoft.MixedReality.Toolkit.UI.ObjectManipulator;

public class ScrewSceneController : MonoBehaviour
{

    // Screw Positions
    public TextAsset screwLatPositions, screwDistPositions, screwMedPositions;
    [HideInInspector]
    public string screwLatPositionsS, screwDistPositionsS, screwMedPositionsS;

    // Plates Visibility State
    public enum PlatesState { Both, Lat, Med, None }

    // Screw Prefab to Instantiate
    public GameObject screwPrefab;

    // Groups
    public GameObject screwGroup, plateGroup, boneGroup, allGroup;

    // Screws Materials
    public Material newScrewMaterial, medScrewMaterial, latScrewMaterial, distScrewMaterial, selectedScrewMaterial, boneMaterial;

    // Screw Button Handler
    public GameObject screwButton;

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

    public void Init()
    {
        screwLatPositionsS = screwLatPositions == null ? null : screwLatPositions.text;
        screwMedPositionsS = screwMedPositions == null ? null : screwMedPositions.text;
        screwDistPositionsS = screwDistPositions == null ? null : screwDistPositions.text;
        // Initialize Screws and Bones
        InitScrews();
        // Initialize Plate List
        InitPlates();

        gPlatesState = PlatesState.Both;
        manipulating = false;
        screwSizeText = screwSizeWindow.GetComponentInChildren<TextMesh>(true);
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
                Vector3 newPoint = new Vector3(-1f * float.Parse(vector[0]), float.Parse(vector[1]), float.Parse(vector[2]));


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

        if (screwLatPositionsS != null)
        {
            GenerateScrewsFromTextFileHelper(screwLatPositionsS, ScrewConstants.LAT_SCREW_TAG, latScrewMaterial);
        }

        if (screwMedPositionsS != null)
        {
            GenerateScrewsFromTextFileHelper(screwMedPositionsS, ScrewConstants.MED_SCREW_TAG, medScrewMaterial);
        }

        if (screwDistPositionsS != null)
        {
            GenerateScrewsFromTextFileHelper(screwDistPositionsS, ScrewConstants.DIST_SCREW_TAG, distScrewMaterial);
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
            cylinderScrew.GetComponent<BoundsControl>().enabled = false;
            cylinderScrew.GetComponent<ObjectManipulator>().enabled = false;
            cylinderScrew.GetComponent<ScaleConstraint>().enabled = false;
            cylinderScrew.GetComponent<WholeScaleConstraint>().enabled = false;
            cylinderScrew.GetComponent<PositionConstraint>().enabled = false;
            cylinderScrew.GetComponent<NearInteractionGrabbable>().enabled = false;

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
            else
            {
                medPlate = plate.gameObject;
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

    public void ResetState()
    {
        while(manipulating)
        {
            ManipulateScrew();
        }
        while(gPlatesState != PlatesState.Both)
        {
            ChangePlatesVisibility();
        }
        while (!boneGroup.activeSelf)
        {
            ChangeBoneVisibility();
        }
        ResetScrews();
        SetScrewTags();
    }

    public void ReInit()
    {
        InitScrews();
        InitPlates();
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

    public void ChangePlatesVisibility()
    {
        TextMeshPro[] texts = RetrieveButtonText(ScrewConstants.CHANGE_PLATES_VISIBILITY);
        bool latPlateActivation = true, medPlateActivation = true;

        switch (gPlatesState)
        {
            case PlatesState.Both:
                latPlateActivation = true;
                medPlateActivation = false;
                SetLatScrewsActive(true);
                SetMedScrewsActive(false);
                gPlatesState = PlatesState.Lat;
                SetTexts(texts, ScrewConstants.SHOW_MED_PLATE);
                break;
            case PlatesState.Lat:
                latPlateActivation = false;
                medPlateActivation = true;
                SetLatScrewsActive(false);
                SetMedScrewsActive(true);
                gPlatesState = PlatesState.Med;
                SetTexts(texts, ScrewConstants.SHOW_NO_PLATES);
                break;
            case PlatesState.Med:
                latPlateActivation = false;
                medPlateActivation = false;
                SetLatScrewsActive(true);
                SetMedScrewsActive(true);
                gPlatesState = PlatesState.None;
                SetTexts(texts, ScrewConstants.SHOW_BOTH_PLATES);
                break;
            case PlatesState.None:
                latPlateActivation = true;
                medPlateActivation = true;
                SetLatScrewsActive(true);
                SetMedScrewsActive(true);
                gPlatesState = PlatesState.Both;
                SetTexts(texts, ScrewConstants.SHOW_LAT_PLATE);
                break;
            default:
                break;
        }

        if(latPlate != null && medPlate != null)
        {
            latPlate.SetActive(latPlateActivation);
            medPlate.SetActive(medPlateActivation);
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

        String flag = gPlatesState == PlatesState.Lat ? ScrewConstants.LAT_SCREW_TAG : ScrewConstants.MED_SCREW_TAG;
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

        String flag = gPlatesState == PlatesState.Lat ? ScrewConstants.LAT_SCREW_TAG : ScrewConstants.MED_SCREW_TAG;
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
        screwSizeText.text = ScrewConstants.SCREW_SIZE_STUB_START + ComputeScrewSize(screw) + ScrewConstants.SCREW_SIZE_STUB_END;
    }

    private double ComputeScrewSize(GameObject screw)
    {
        float scale = screw.transform.localScale.y * scene.transform.localScale.y * 100;
        return Math.Round(scale, 2);
    }

    public void ChangeScrewState()
    {
        TextMeshPro[] texts = RetrieveButtonText(ScrewConstants.SCREW_STATE_BUTTON);

        if (screwButton.activeInHierarchy)
        {
            screwButton.SetActive(false);
            foreach (GameObject screw in screws)
            {
                DeactivateScrew(screw);
            }
            SetTexts(texts, ScrewConstants.START_MANIPULATING_SCREWS);
        }
        else
        {
            screwButton.SetActive(true);
            FindNextIndex();
            ActivateScrew(screws[screwIndex]);
            SetTexts(texts, ScrewConstants.STOP_MANIPULATING_SCREWS);
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

public static class StringExtensions
{
    public static bool Contains(this string source, string toCheck, StringComparison comp)
    {
        return source?.IndexOf(toCheck, comp) >= 0;
    }
}

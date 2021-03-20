using MathNet.Numerics.LinearAlgebra;
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

    // Plates Visibility State
    public enum PlatesState { Both, Lat, Med, None }

    // Screw Prefab to Instantiate
    public GameObject screwPrefab;

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


    void Start()
    {
        // Initialize Screws and Bones
        InitScrews();
        // Initialize Plate List
        InitPlates();

        gPlatesState = PlatesState.Both;
        manipulating = false;
        screwSizeText = screwSizeWindow.GetComponentInChildren<TextMesh>(true);
    }

    /*
    private void DecorateScrew(GameObject screw)
    {
        if (screw.GetComponentInChildren<BoundsControl>(true) == null)
        {
            BoundsControl boundsControl = screw.AddComponent<BoundsControl>() as BoundsControl;

            boundsControl.ScaleHandlesConfig = new ScaleHandlesConfiguration();
            boundsControl.ScaleHandlesConfig.HandleSize = (float)0.008;
            boundsControl.ScaleHandlesConfig.ColliderPadding = new Vector3((float)0.008, (float)0.008, (float)0.008);

            boundsControl.RotationHandlesConfig = new RotationHandlesConfiguration();
            boundsControl.RotationHandlesConfig.ShowHandleForX = false;
            boundsControl.RotationHandlesConfig.ShowHandleForY = false;
            boundsControl.RotationHandlesConfig.ShowHandleForZ = false;

            boundsControl.enabled = false;
        }

        if (screw.GetComponentInChildren<ConstraintManager>(true) == null)
        {
            ConstraintManager constraintManager = screw.AddComponent<ConstraintManager>();
        }

        if (screw.GetComponentInChildren<ObjectManipulator>(true) == null)
        { 
            ObjectManipulator objectManipulator = screw.AddComponent<ObjectManipulator>();
            objectManipulator.ManipulationType = ManipulationHandFlags.OneHanded;
            objectManipulator.OneHandRotationModeFar = RotateInOneHandType.RotateAboutObjectCenter;
            objectManipulator.OneHandRotationModeNear = RotateInOneHandType.RotateAboutObjectCenter;

            objectManipulator.enabled = false;
        }

        if (screw.GetComponentInChildren<ScaleConstraint>(true) == null)
        { 
            ScaleConstraint scaleConstraint = screw.AddComponent<ScaleConstraint>();
            scaleConstraint.enabled = false;
        }

        if (screw.GetComponentInChildren<PositionConstraint>(true) == null)
        {
            PositionConstraint positionConstraint = screw.AddComponent<PositionConstraint>();
            positionConstraint.enabled = false;
        }

        if (screw.GetComponentInChildren<WholeScaleConstraint>(true) == null)
        {
            WholeScaleConstraint wholeScaleConstraint = screw.AddComponent<WholeScaleConstraint>();
            wholeScaleConstraint.enabled = false;
        }

        if (screw.GetComponentInChildren<NearInteractionGrabbable>(true) == null)
        {
            NearInteractionGrabbable nearInteractionGrabbable = screw.AddComponent<NearInteractionGrabbable>();
            nearInteractionGrabbable.enabled = false;
        }
    }
    */

    /*
    private List<Tuple<Vector3, Vector3>> ProcessScrewPosition(bool latScrew)
    {
        String textAsset = referenceMedPositions.text.Replace("\"", "");
        if(latScrew)
        {
            textAsset = referenceLatPositions.text.Replace("\"", "");
        }
        String[] lines = Regex.Split(textAsset, "\n|\r|\r\n");
        List<Tuple<Vector3, Vector3>> points = new List<Tuple<Vector3, Vector3>>();
        Vector3 dummyVector = new Vector3(0, 0, 0);
        Vector3 item1 = dummyVector;

        foreach(String line in lines)
        {
            if(line.Contains(",")) 
            {
                String[] vector = Regex.Split(line, ",");
                Vector3 newPoint = new Vector3(float.Parse(vector[0]), float.Parse(vector[1]), float.Parse(vector[2]));


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
    */

    /*
    private Matrix<float> FindTransformationMatrix(Vector3[] thisSysCenters, Tuple<Vector3, Vector3>[] textScrewPoints)
    {
        Vector3[] otherSysPoints = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            Tuple<Vector3, Vector3> otherSysTuple = textScrewPoints[i];
            otherSysPoints[i] = new Vector3(
            (otherSysTuple.Item1.x + otherSysTuple.Item2.x) / 2,
            (otherSysTuple.Item1.y + otherSysTuple.Item2.y) / 2,
            (otherSysTuple.Item1.z + otherSysTuple.Item2.z) / 2);
        }

        float[][] thisSysColumnArrays = {
            new float[]{ thisSysCenters[0].x, thisSysCenters[0].y, thisSysCenters[0].z },
            new float[]{ thisSysCenters[1].x, thisSysCenters[1].y, thisSysCenters[1].z },
            new float[]{ thisSysCenters[2].x, thisSysCenters[2].y, thisSysCenters[2].z } };
        Matrix<float> thisSystemMat = Matrix<float>.Build.DenseOfColumnArrays(thisSysColumnArrays);

        float[][] otherSysColumnArrays = {
            new float[]{ otherSysPoints[0].x, otherSysPoints[0].y, otherSysPoints[0].z },
            new float[]{ otherSysPoints[1].x, otherSysPoints[1].y, otherSysPoints[1].z },
            new float[]{ otherSysPoints[2].x, otherSysPoints[2].y, otherSysPoints[2].z } };
        Matrix<float> otherSystemMat = Matrix<float>.Build.DenseOfColumnArrays(otherSysColumnArrays);

        // thisCoordSys = basis (matrix.mul) otherCoordSys -->
        //     basis = thisCoordSys (matrix.mul) otherCoordSys^(-1)

        Matrix<float> basis = thisSystemMat.Multiply((otherSystemMat.Inverse()));

        Debug.Log(basis);

        return basis;
    }
    */

    private GameObject GenerateScrewFromObj(GameObject screw)
    {
        scene.SetActive(true);
        MeshCollider collider = screw.AddComponent<MeshCollider>();
        collider.convex = true;
        Vector3 minn = screw.transform.GetComponent<Renderer>().bounds.min;
        Vector3 maxx = screw.transform.GetComponent<Renderer>().bounds.max;

        Vector3[] boundPoints = new Vector3[]
        {
                minn,
                new Vector3(minn.x, minn.y, maxx.z),
                new Vector3(minn.x, maxx.y, minn.z),
                new Vector3(minn.x, maxx.y, maxx.z),
                new Vector3(maxx.x, minn.y, minn.z),
                new Vector3(maxx.x, minn.y, maxx.z),
                new Vector3(maxx.x, maxx.y, minn.z),
                maxx
        };

        Vector3[] closestPoints = new Vector3[boundPoints.Length];
        Vector3 firstStartPt = new Vector3(),
            secondStartPt = new Vector3(),
            firstEndPt = new Vector3(),
            secondEndPt = new Vector3();
        float startPtDist = float.MaxValue, endPtDist = float.MaxValue;
        for (int i = 0; i < boundPoints.Length; i++)
        {
            closestPoints[i] = collider.ClosestPoint(boundPoints[i]);
            float ptDist = Vector3.Distance(closestPoints[i], boundPoints[i]);
            if (startPtDist < endPtDist && ptDist < endPtDist)
            {
                endPtDist = ptDist;
                firstEndPt = closestPoints[i];
            }
            else if (ptDist < startPtDist)
            {
                startPtDist = ptDist;
                firstStartPt = closestPoints[i];
            }
        }

        startPtDist = float.MaxValue;
        endPtDist = float.MaxValue;
        foreach (Vector3 pt in closestPoints)
        {
            if (pt != firstStartPt)
            {
                float tempStartPtDist = Vector3.Distance(pt, firstStartPt);
                if (tempStartPtDist < startPtDist)
                {
                    startPtDist = tempStartPtDist;
                    secondStartPt = pt;
                }
            }

            if (pt != firstEndPt)
            {
                float tempEndPtDist = Vector3.Distance(pt, firstEndPt);
                if (tempEndPtDist < endPtDist)
                {
                    endPtDist = tempEndPtDist;
                    secondEndPt = pt;
                }
            }
        }

        Vector3 startPoint = new Vector3(
            (firstStartPt.x + secondStartPt.x) / 2,
            (firstStartPt.y + secondStartPt.y) / 2,
            (firstStartPt.z + secondStartPt.z) / 2
            );

        Vector3 endPoint = new Vector3(
            (firstEndPt.x + secondEndPt.x) / 2,
            (firstEndPt.y + secondEndPt.y) / 2,
            (firstEndPt.z + secondEndPt.z) / 2
            );

        Destroy(screw.GetComponent<MeshCollider>());
        scene.SetActive(false);

        GameObject cylinderScrew = CreateCylinderBetweenPoints(startPoint, endPoint);
        cylinderScrew.transform.parent = screw.transform.parent;
        cylinderScrew.tag = screw.tag;
        cylinderScrew.name = screw.name;
        cylinderScrew.GetComponent<MeshRenderer>().material = screw.GetComponent<MeshRenderer>().material;
        Destroy(screw);

        cylinderScrew.GetComponent<BoundsControl>().enabled = false;
        cylinderScrew.GetComponent<ObjectManipulator>().enabled = false;
        cylinderScrew.GetComponent<ScaleConstraint>().enabled = false;
        cylinderScrew.GetComponent<WholeScaleConstraint>().enabled = false;
        cylinderScrew.GetComponent<PositionConstraint>().enabled = false;
        cylinderScrew.GetComponent<NearInteractionGrabbable>().enabled = false;
        return cylinderScrew;
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

        foreach (Transform screw in screwGroup.transform)
        {
            Transform real_screw = screw.transform.GetChild(0);

            GameObject generatedScrew = GenerateScrewFromObj(real_screw.gameObject);
            screws.Add(generatedScrew);
            originalScrewPositions.Add(screw.gameObject.name, generatedScrew.transform.position);
            originalScrewScales.Add(screw.gameObject.name, generatedScrew.transform.localScale);
            originalScrewRotations.Add(screw.gameObject.name, generatedScrew.transform.rotation);
        }

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
                if(screw.transform.parent.name.Contains(ScrewConstants.LAT_SCREW_TAG, StringComparison.OrdinalIgnoreCase))
                {
                    screw.tag = ScrewConstants.LAT_SCREW_TAG;
                } else
                {
                    screw.tag = ScrewConstants.MED_SCREW_TAG;
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
                
                screw.transform.position = originalScrewPositions[screw.transform.parent.name];
                screw.transform.localScale = originalScrewScales[screw.transform.parent.name];
                screw.transform.rotation = originalScrewRotations[screw.transform.parent.name];
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

static class ScrewConstants
{
    public const String LAT_SCREW_TAG = "Lat";
    public const String MED_SCREW_TAG = "Med";
    public const String NEW_SCREW_TAG = "New";
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

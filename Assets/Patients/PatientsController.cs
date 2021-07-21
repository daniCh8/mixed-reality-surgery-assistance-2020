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

    // Screw Scene References
    public GameObject referencePatientScrew, newPatientScrew;

    // Dummy Object at (0,0,0)
    public GameObject dummyObject;

    public GlobalController globalController;
    public ScrewSceneController screwController;
    public TextAsset newLatScrew, newDistScrew, newMedScrew;
    private string newLatScrewS, newDistScrewS, newMedScrewS;
    private char sep = Path.DirectorySeparatorChar;

    public void Init()
    {
        return;

        newScansB = newScans.bytes;
        referenceScansB = referenceScans.bytes;
        onScreenScansB = onScreenScans.bytes;

        newLatScrewS = newLatScrew.text;
        newMedScrewS = newMedScrew.text;
        newDistScrewS = newDistScrew.text;

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

        foreach (var item in new GameObject[] { pinchSliderHor, pinchSliderVer })
        {
            foreach (SliderSlice sliderSlice in item.GetComponentsInChildren<SliderSlice>())
            {
                if (sliderSlice.tex == null)
                {
                    sliderSlice.Init();
                }
                sliderSlice.UpdateHelper();
            }
        }
        */

        screwController.ResetState();
        AlignScrewScene();

        string boxD = screwController.screwDistPositionsS;
        string boxL = screwController.screwLatPositionsS;
        string boxM = screwController.screwMedPositionsS;

        screwController.screwDistPositionsS = newDistScrewS;
        screwController.screwLatPositionsS = newLatScrewS;
        screwController.screwMedPositionsS = newMedScrewS;
        
        newDistScrewS = boxD;
        newLatScrewS = boxL;
        newMedScrewS = boxM;

        screwController.screwGroup = findChildrenWithName(referencePatientScrew.transform, GlobalConstants.SCREW_GROUP);
        screwController.plateGroup = findChildrenWithName(referencePatientScrew.transform, GlobalConstants.PLATE_GROUP);
        screwController.boneGroup = findChildrenWithName(referencePatientScrew.transform, GlobalConstants.BONE_GROUP);
        screwController.ReInit();
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

    private void AlignScrewScene()
    {
        CenterToRef(newPatientScrew,
                RetrieveCombinedBounds(referencePatientScrew).center);
        GameObject boxPatient = referencePatientScrew;
        referencePatientScrew = newPatientScrew;
        newPatientScrew = boxPatient;
        newPatientScrew.SetActive(false);
        referencePatientScrew.SetActive(true);
    }

    public void CenterToRef(GameObject obj, Vector3 referencePosition)
    {
        obj.transform.position = obj.transform.position -
            RetrieveCombinedBounds(obj).center +
            referencePosition;
    }

    private Bounds RetrieveCombinedBounds(GameObject parent)
    {
        Renderer renderer = parent.GetComponentInChildren<Renderer>();
        Bounds combinedBounds = renderer.bounds;
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
        foreach (Renderer render in renderers)
        {
            if (render != renderer) combinedBounds.Encapsulate(render.bounds);
        }
        return combinedBounds;
    }

    public void PickPatientInFolderOne()
    {
        PickNewPatient(true);
    }

    public void PickPatientInFolderTwo()
    {
        PickNewPatient(false);
    }

    public void PickNewPatient(bool first)
    {
        // instantiate new bones and create new patient with it
        // adjust controllers parameters to have reference patient and new patient
        // call switch patients
        // destroy old patient instead of keeping it

        globalController.GoToScrewScene();
        // LoadNewCT(first);
        LoadNewScrews(first);
        // LoadNewPatientManip(first);
        LoadNewPatientScrew(first);
        SwitchPatient();
    }

    private void LoadObjsFrom(string[] paths, Transform parent)
    {
        foreach (var fracturedP in paths)
        {
            if (!File.Exists(fracturedP))
            {
                Debug.LogError(fracturedP + " is not a valid path.");
                return;
            }

            //load
            var loadedObj = new OBJLoader().Load(fracturedP);
            loadedObj.transform.parent = parent;
            loadedObj.transform.localScale = new Vector3(-1f, 1f, 1f);
            loadedObj.transform.localPosition = new Vector3();
            loadedObj.transform.localEulerAngles = new Vector3();
        }
    }

    private void DestroyAllChildren(Transform t)
    {
        foreach (Transform child in t)
        {
            Destroy(child.gameObject);
        }
    }

    private string ReadStringFromPath(string path)
    {
        return File.ReadAllText(path);
    }

    private byte[] ReadBytesFromPath(string path)
    {
        return File.ReadAllBytes(path);
    }

    public void LoadNewScrews(bool flag)
    {
#if WINDOWS_UWP
        Task loadNewScrewsTask = new Task(
            async () =>
            {
                try
                {
                    StorageFolder cs = await KnownFolders.DocumentsLibrary.GetFolderAsync(FolderCostants.FOLDER_CUSTOM_SURG);
                    StorageFolder p = await cs.GetFolderAsync(flag ? FolderCostants.FOLDER_CUSTOM_PATIENT_1 : FolderCostants.FOLDER_CUSTOM_PATIENT_2);
                    StorageFolder screwsFolder = await p.GetFolderAsync(FolderCostants.SCREWS);
                    var screwPosFiles = await screwsFolder.GetFilesAsync();
                    foreach (StorageFile screwPosFile in screwPosFiles)
                    {
                        if (screwPosFile.Name.IndexOf(ScrewConstants.LAT_SCREW_TAG, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            newLatScrewS = await FileIO.ReadTextAsync(screwPosFile);
                        }
                        else if (screwPosFile.Name.IndexOf(ScrewConstants.MED_SCREW_TAG, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            newMedScrewS = await FileIO.ReadTextAsync(screwPosFile);
                        }
                        else if (screwPosFile.Name.IndexOf(ScrewConstants.DIST_SCREW_TAG, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            newDistScrewS = await FileIO.ReadTextAsync(screwPosFile);
                        }
                    }
                }
                catch (Exception)
                {
                    Debug.Log("Failed to locate documents folder!");
                }
            });

        loadNewScrewsTask.Start();
#endif

#if UNITY_EDITOR
		LoadNewScrewsUnity();
#endif
    }

    public void LoadNewCT(bool flag)
    {
#if WINDOWS_UWP
        Task loadNewCTTask = new Task(
            async () =>
            {
                try
                {
                    StorageFolder cs = await KnownFolders.DocumentsLibrary.GetFolderAsync(FolderCostants.FOLDER_CUSTOM_SURG);
                    StorageFolder p = await cs.GetFolderAsync(flag ? FolderCostants.FOLDER_CUSTOM_PATIENT_1 : FolderCostants.FOLDER_CUSTOM_PATIENT_2);
                    StorageFolder ctFolder = await p.GetFolderAsync(FolderCostants.CT);
                    var ctFiles = await ctFolder.GetFilesAsync();
                    foreach (StorageFile ctFile in ctFiles)
                    {
                        if (ctFile.Name.IndexOf(FolderCostants.OBJ_EXTENSION, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            IBuffer buffer = await FileIO.ReadBufferAsync(ctFile);
                            DataReader dataReader = DataReader.FromBuffer(buffer);
                            byte[] ctBytes = new byte[buffer.Length];
                            dataReader.ReadBytes(ctBytes);
                            newScansB = ctBytes;
                        }
                    }
                }
                catch (Exception)
                {
                    Debug.Log("Failed to locate documents folder!");
                }
            });

        loadNewCTTask.Start();
#endif

#if UNITY_EDITOR
		LoadNewCTUnity();
#endif
    }

    public void LoadNewPatientManip(bool flag)
    {
#if WINDOWS_UWP
        Task loadNewPatientManipTask = new Task(
            async () =>
            {
                try
                {
                    Transform newPatHandles = referencePatientManip == onScreenPatientManip ? newPatientManip.transform : onScreenPatientManip.transform;
                    DestroyAllChildren(newPatHandles);
                    
                    StorageFolder cs = await KnownFolders.DocumentsLibrary.GetFolderAsync(FolderCostants.FOLDER_CUSTOM_SURG);
                    StorageFolder p = await cs.GetFolderAsync(flag ? FolderCostants.FOLDER_CUSTOM_PATIENT_1 : FolderCostants.FOLDER_CUSTOM_PATIENT_2);
                    StorageFolder boneFolder = await p.GetFolderAsync(FolderCostants.FRACTURED_BONES);
                    var boneFiles = await boneFolder.GetFilesAsync();
                    List<string> bonePaths = new List<string>();
                    foreach (StorageFile boneFile in boneFiles)
                    {
                        if (boneFile.Name.IndexOf(FolderCostants.OBJ_EXTENSION, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            bonePaths.Add(boneFile.Path);
                        }
                    }
                    LoadObjsFrom(bonePaths.ToArray(), newPatHandles);
                }
                catch (Exception)
                {
                    Debug.Log("Failed to locate documents folder!");
                }
            });

        loadNewPatientManipTask.Start();
#endif

#if UNITY_EDITOR
		LoadNewPatientManipUnity();
#endif
    }

    public void LoadNewPatientScrew(bool flag)
    {
#if WINDOWS_UWP
        Task loadNewPatientScrewTask = new Task(
            async () =>
            {
                try
                {
                    Transform newPatScrew = newPatientScrew.transform;
                    Transform screwsPat = newPatScrew.Find("Screws"),
                        platesPat = newPatScrew.Find("Plates"),
                        bonesPat = newPatScrew.Find("Bone");

                    DestroyAllChildren(screwsPat);
                    DestroyAllChildren(platesPat);
                    DestroyAllChildren(bonesPat);

                    StorageFolder cs = await KnownFolders.DocumentsLibrary.GetFolderAsync(FolderCostants.FOLDER_CUSTOM_SURG);
                    StorageFolder p = await cs.GetFolderAsync(flag ? FolderCostants.FOLDER_CUSTOM_PATIENT_1 : FolderCostants.FOLDER_CUSTOM_PATIENT_2);

                    StorageFolder boneFolder = await p.GetFolderAsync(FolderCostants.REALIGNED_BONES);
                    var boneFiles = await boneFolder.GetFilesAsync();
                    List<string> bonePaths = new List<string>();
                    foreach (StorageFile boneFile in boneFiles)
                    {
                        if (boneFile.Name.IndexOf(FolderCostants.OBJ_EXTENSION, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            bonePaths.Add(boneFile.Path);
                        }
                    }
                    LoadObjsFrom(bonePaths.ToArray(), bonesPat);

                    StorageFolder plateFolder = await p.GetFolderAsync(FolderCostants.PLATES);
                    var plateFiles = await plateFolder.GetFilesAsync();
                    List<string> platePaths = new List<string>();
                    foreach (StorageFile plateFile in plateFiles)
                    {
                        if (plateFile.Name.IndexOf(FolderCostants.OBJ_EXTENSION, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            platePaths.Add(plateFile.Path);
                        }
                    }
                    LoadObjsFrom(platePaths.ToArray(), platesPat);
                }
                catch (Exception)
                {
                    Debug.Log("Failed to locate documents folder!");
                }
            });

        loadNewPatientScrewTask.Start();
#endif

#if UNITY_EDITOR
		LoadNewPatientScrewUnity();
#endif
    }

    private void LoadNewPatientScrewUnity()
    {
        Transform newPatScrew = newPatientScrew.transform;
        Transform screwsPat = newPatScrew.Find("Screws"),
            platesPat = newPatScrew.Find("Plates"),
            bonesPat = newPatScrew.Find("Bone");

        DestroyAllChildren(screwsPat);
        DestroyAllChildren(platesPat);
        DestroyAllChildren(bonesPat);

        string[] alignedPaths = Directory.GetFiles($"Assets{sep}Patients{sep}TestPatient{sep}Aligned{sep}", "*.obj", SearchOption.TopDirectoryOnly);
        LoadObjsFrom(alignedPaths, bonesPat);

        string[] platesPaths = Directory.GetFiles($"Assets{sep}Patients{sep}TestPatient{sep}Plates{sep}", "*.obj", SearchOption.TopDirectoryOnly);
        LoadObjsFrom(platesPaths, platesPat);
    }

    private void LoadNewPatientManipUnity()
    {
        Transform newPatHandles = referencePatientManip == onScreenPatientManip ? newPatientManip.transform : onScreenPatientManip.transform;
        DestroyAllChildren(newPatHandles);

        // fractured
        string[] fracturedPaths = Directory.GetFiles($"Assets{sep}Patients{sep}TestPatient{sep}Fractured{sep}", "*.obj", SearchOption.TopDirectoryOnly);
        LoadObjsFrom(fracturedPaths, newPatHandles);
    }

    private void LoadNewScrewsUnity()
    {
        string[] screwPosPaths = Directory.GetFiles($"Assets{sep}Patients{sep}TestPatient{sep}Screws{sep}", "*.txt", SearchOption.TopDirectoryOnly);
        foreach (var screwPosPath in screwPosPaths)
        {
            if (screwPosPath.IndexOf(ScrewConstants.LAT_SCREW_TAG, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                newLatScrewS = ReadStringFromPath(screwPosPath);
            }
            else if (screwPosPath.IndexOf(ScrewConstants.MED_SCREW_TAG, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                newMedScrewS = ReadStringFromPath(screwPosPath);
            }
            else if (screwPosPath.IndexOf(ScrewConstants.DIST_SCREW_TAG, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                newDistScrewS = ReadStringFromPath(screwPosPath);
            }
        }
    }

    private void LoadNewCTUnity()
    {
        string ctPath = $"Assets{sep}Patients{sep}TestPatient{sep}CT{sep}ct.bytes";
        newScansB = ReadBytesFromPath(ctPath);
    }

}
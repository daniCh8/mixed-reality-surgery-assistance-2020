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
    public GameObject patientManip;

    public GameObject pinchSliderHor, pinchSliderVer;

    public CTReader cTReader;
    public TextAsset scans;
    private byte[] scansB;

    // Dummy Object at (0,0,0)
    public GameObject dummyObject;

    public GlobalController globalController;
    private char sep = Path.DirectorySeparatorChar;

    public LogScript logger;

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

    private void TutuneTranslation()
    {
        Vector3 refCenter = CTConstants.REFERENCE_CENTER;
        Vector3 newCenter = cTReader.GetCenterOfCt();
        float xTranslation = refCenter.x - newCenter.x,
            yTranslation = refCenter.y - newCenter.y,
            zTranslation = refCenter.z - newCenter.z;
        patientManip.transform.localPosition = new Vector3(xTranslation, yTranslation, zTranslation);

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

    public void PickNewPatient(bool first)
    {
        // instantiate new bones and create new patient with it
        // adjust controllers parameters to have reference patient and new patient
        // call switch patients
        // destroy old patient instead of keeping it

        LoadNewPatientManip(first);
        FilePicker.Log("Loaded Bones");
        LoadNewCT(first);
        FilePicker.Log("\nLoaded CT");
        cTReader.ct_bytes = scansB;
        cTReader.Init(); //ArgumentNullException: Buffer cannot be null, parameter name: buffer
        FilePicker.Log("CT Reader Init");
        TutunePinchSliders();
        FilePicker.Log("Pinch Sliders Tuned");
        TutuneTranslation();
        FilePicker.Log("Translations Tuned");
        cTReader.ComputeOffsets();
        FilePicker.Log("Offsets Computed");
        globalController.Init();
        FilePicker.Log("Global Controller Inited");
        globalController.GoToManipScene();
        globalController.LogBones();
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

            FilePicker.Log("loading file: " + fracturedP);

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

    /*
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
                            // newLatScrewS = await FileIO.ReadTextAsync(screwPosFile);
                        }
                        else if (screwPosFile.Name.IndexOf(ScrewConstants.MED_SCREW_TAG, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            // newMedScrewS = await FileIO.ReadTextAsync(screwPosFile);
                        }
                        else if (screwPosFile.Name.IndexOf(ScrewConstants.DIST_SCREW_TAG, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            // newDistScrewS = await FileIO.ReadTextAsync(screwPosFile);
                        }
                    }
                }
                catch (Exception)
                {
                    FilePicker.Log(e.Message);
                    FilePicker.Log(e.StackTrace);
                }
            });

        loadNewScrewsTask.Start();
#endif

#if UNITY_EDITOR
		LoadNewScrewsUnity();
#endif
    }
    */

    public void LoadNewCT(bool flag)
    {
#if WINDOWS_UWP
        Task loadNewCTTask = new Task(
            async () =>
            {
                try
                {
                    int counter = 1;
                    FilePicker.Log("ct task " + counter++);
                    StorageFolder cs = await KnownFolders.DocumentsLibrary.GetFolderAsync(FolderCostants.FOLDER_CUSTOM_SURG);
                    FilePicker.Log("ct task " + counter++);
                    StorageFolder p = await cs.GetFolderAsync(flag ? FolderCostants.FOLDER_CUSTOM_PATIENT_1 : FolderCostants.FOLDER_CUSTOM_PATIENT_2);
                    FilePicker.Log("ct task " + counter++);
                    StorageFolder ctFolder = await p.GetFolderAsync(FolderCostants.CT);
                    FilePicker.Log("ct task " + counter++);
                    var ctFiles = await ctFolder.GetFilesAsync();
                    FilePicker.Log("ct task " + counter++ + "; files: " + ctFiles.ToString());
                    foreach (StorageFile ctFile in ctFiles)
                    {
                        FilePicker.Log("ct task " + counter++ + "; file name: " + ctFile.Name);
                        if (ctFile.Name.IndexOf(FolderCostants.BYTES_EXTENSION, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            FilePicker.Log("ct task " + counter++ + "; we're inside");
                            IBuffer buffer = await FileIO.ReadBufferAsync(ctFile);
                            FilePicker.Log("ct task " + counter++ + "; read ct file buffer");
                            DataReader dataReader = DataReader.FromBuffer(buffer);
                            FilePicker.Log("ct task " + counter++ + "; created datareader");
                            byte[] ctBytes = new byte[buffer.Length];
                            FilePicker.Log("ct task " + counter++ + "; created byte array");
                            dataReader.ReadBytes(ctBytes);
                            FilePicker.Log("ct task " + counter++ + "; copied bytes");
                            scansB = ctBytes;
                            FilePicker.Log("ct task " + counter++ + "; saved bytes");
                        }
                    }
                }
                catch (Exception e)
                {
                    FilePicker.Log(e.Message);
                    FilePicker.Log(e.StackTrace);
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
                    int counter = 1;
                    FilePicker.Log("new patient manip " + counter++);
                    Transform newPatHandles = patientManip.transform;
                    FilePicker.Log("new patient manip " + counter++);
                    DestroyAllChildren(newPatHandles);
                    FilePicker.Log("new patient manip " + counter++);

                    StorageFolder cs = await KnownFolders.DocumentsLibrary.GetFolderAsync(FolderCostants.FOLDER_CUSTOM_SURG);
                    FilePicker.Log("new patient manip " + counter++);
                    StorageFolder p = await cs.GetFolderAsync(flag ? FolderCostants.FOLDER_CUSTOM_PATIENT_1 : FolderCostants.FOLDER_CUSTOM_PATIENT_2);
                    FilePicker.Log("new patient manip " + counter++);
                    StorageFolder boneFolder = await p.GetFolderAsync(FolderCostants.FRACTURED_BONES);
                    FilePicker.Log("new patient manip " + counter++);
                    var boneFiles = await boneFolder.GetFilesAsync();
                    FilePicker.Log("new patient manip " + counter++ + "; boneFiles: " + boneFiles.ToString());
                    List<string> bonePaths = new List<string>();
                    foreach (StorageFile boneFile in boneFiles)
                    {
                        FilePicker.Log("new patient manip " + counter++ + "; boneFile name: " + boneFile.Name);
                        if (boneFile.Name.IndexOf(FolderCostants.OBJ_EXTENSION, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            FilePicker.Log("new patient manip " + counter++ + "; " + boneFile.Name + " added!");
                            bonePaths.Add(boneFile.Path);
                        }
                    }
                    LoadObjsFrom(bonePaths.ToArray(), newPatHandles);
                    FilePicker.Log("new patient manip " + counter++ + "; obj loaded.");
                }
                catch (Exception e)
                {
                    FilePicker.Log(e.Message);
                    FilePicker.Log(e.StackTrace);
                }
            });

        loadNewPatientManipTask.Start();
#endif

#if UNITY_EDITOR
		LoadNewPatientManipUnity();
#endif
    }

    /*
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
                    FilePicker.Log(e.Message);
                    FilePicker.Log(e.StackTrace);
                }
            });

        loadNewPatientScrewTask.Start();
#endif

#if UNITY_EDITOR
		LoadNewPatientScrewUnity();
#endif
    }
    */

    /*
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
    */

    private void LoadNewPatientManipUnity()
    {
        Transform newPatHandles = patientManip.transform;
        DestroyAllChildren(newPatHandles);

        // fractured
        string[] fracturedPaths = Directory.GetFiles($"Assets{sep}Patients{sep}TestPatient{sep}Fractured{sep}", "*.obj", SearchOption.TopDirectoryOnly);
        LoadObjsFrom(fracturedPaths, newPatHandles);
    }

    /*
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
    */

    private void LoadNewCTUnity()
    {
        string ctPath = $"Assets{sep}Patients{sep}TestPatient{sep}CT{sep}ct.bytes";
        scansB = ReadBytesFromPath(ctPath);
    }

}

static class CTConstants
{
    public static readonly Vector3 REFERENCE_CENTER = new Vector3(-3.244141f, -226.2559f, -248.5f);
}
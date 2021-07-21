using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using System;
using System.Threading.Tasks;
using Windows.Storage;
#endif

public class FilePicker : MonoBehaviour
{

    public static void CreateFolders()
    {
#if WINDOWS_UWP
        Task createFolderTask = new Task(
            async () =>
            {
                try
                {
                    var customSurg = await KnownFolders.DocumentsLibrary.CreateFolderAsync(FolderCostants.FOLDER_CUSTOM_SURG);
                    var newFolderDescription = await customSurg.CreateFileAsync(FolderCostants.OVERALL_FILE, CreationCollisionOption.ReplaceExisting);
                    await FileIO.AppendTextAsync(newFolderDescription, FolderCostants.OVERALL_FILE + Environment.NewLine);

                    var p1 = await customSurg.CreateFolderAsync(FolderCostants.FOLDER_CUSTOM_PATIENT_1, CreationCollisionOption.OpenIfExists);
                    var p2 = await customSurg.CreateFolderAsync(FolderCostants.FOLDER_CUSTOM_PATIENT_2, CreationCollisionOption.OpenIfExists);
                    

                    for (int i = 0; i < FolderCostants.FOLDER_CUSTOM_SUBFOLDERS.Length; i++)
                    {
                        var newFolderP1 = await p1.CreateFolderAsync(FolderCostants.FOLDER_CUSTOM_SUBFOLDERS[i], CreationCollisionOption.OpenIfExists);
                        var newFolderDescriptionP1 = await newFolderP1.CreateFileAsync(FolderCostants.FOLDER_CUSTOM_SUBFOLDERS_INFO[i], CreationCollisionOption.ReplaceExisting);
                        await FileIO.AppendTextAsync(newFolderDescriptionP1, FolderCostants.FOLDER_CUSTOM_TEXTS[i] + Environment.NewLine);
                        var newFolderP2 = await p2.CreateFolderAsync(FolderCostants.FOLDER_CUSTOM_SUBFOLDERS[i], CreationCollisionOption.OpenIfExists);
                        var newFolderDescriptionP2 = await newFolderP2.CreateFileAsync(FolderCostants.FOLDER_CUSTOM_SUBFOLDERS_INFO[i], CreationCollisionOption.ReplaceExisting);
                        await FileIO.AppendTextAsync(newFolderDescriptionP2, FolderCostants.FOLDER_CUSTOM_TEXTS[i] + Environment.NewLine);
                    }
                }
                catch (Exception)
                {
                    Debug.Log("Failed to locate documents folder!");
                }
            });

        createFolderTask.Start();
#endif

#if UNITY_EDITOR
		Debug.Log("We are not on the HoloLens Device, but still in Unity.");
#endif
    }
}

public static class FolderCostants
{
    public readonly static string FOLDER_CUSTOM_SURG = "CustomSurg";
    public readonly static string FOLDER_CUSTOM_PATIENT_1 = "Patient1";
    public readonly static string FOLDER_CUSTOM_PATIENT_2 = "Patient2";
    public readonly static string CT = "CT";
    public readonly static string FRACTURED_BONES = "FracturedBones";
    public readonly static string REALIGNED_BONES = "RealignedBones";
    public readonly static string SCREWS = "Screws";
    public readonly static string PLATES = "Plates";
    public readonly static string[] FOLDER_CUSTOM_SUBFOLDERS = new string[] { CT, FRACTURED_BONES, REALIGNED_BONES, SCREWS, PLATES };
    public readonly static string CT_INFO = "CT_Info.txt";
    public readonly static string FRACTURED_BONES_INFO = "FracturedBones_Info.txt";
    public readonly static string REALIGNED_BONES_INFO = "RealignedBones_Info.txt";
    public readonly static string SCREWS_INFO = "Screws_Info.txt";
    public readonly static string PLATES_INFO = "Plates_Info.txt";
    public readonly static string[] FOLDER_CUSTOM_SUBFOLDERS_INFO = new string[] { CT_INFO, FRACTURED_BONES_INFO, REALIGNED_BONES_INFO, SCREWS_INFO, PLATES_INFO };
    public readonly static string CT_EXPLANATION = "Here you should put the ct.nrrd file, containing the ct scans of the patient.";
    public readonly static string FRACTURED_BONES_EXPLANATION = "Here you should put the fractured bone models.They should have the.obj extension.";
    public readonly static string REALIGNED_BONES_EXPLANATION = "Here you should put the realigned bone models. They should have the .obj extension.";
    public readonly static string SCREWS_EXPLANATION = "Here you should put the .txt files indicating the positions of the screws.";
    public readonly static string PLATES_EXPLANATION = "Here you should put the plate models. They should have the .obj extension.";
    public readonly static string[] FOLDER_CUSTOM_TEXTS = new string[] { CT_EXPLANATION, FRACTURED_BONES_EXPLANATION, REALIGNED_BONES_EXPLANATION, SCREWS_EXPLANATION, PLATES_EXPLANATION };
    public readonly static string OVERALL_FILE = "Info.txt";
    public readonly static string OVERALL_EXPLANATION = "These are the patients folders. You can use them to change the patient. Once you completed uploading all the files for one patient, you can go back to the application and choose that patient. Go inside a patient's subfolder to find more information about the files you should upload.";
    public readonly static string OBJ_EXTENSION = ".obj";
}

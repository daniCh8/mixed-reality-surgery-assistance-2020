using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
#endif

public class FilePicker : MonoBehaviour
{
	public TextMesh textMesh;

	public void UpdateText()
    {
#if !UNITY_EDITOR && UNITY_WSA_10_0
		Debug.Log("***********************************");
		Debug.Log("File Picker start.");
		Debug.Log("***********************************");

		UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
		{
			var filepicker = new FileOpenPicker();
			// filepicker.FileTypeFilter.Add("*");
			filepicker.FileTypeFilter.Add(".txt");

			var file = await filepicker.PickSingleFileAsync();
			UnityEngine.WSA.Application.InvokeOnAppThread(() => 
			{
				Debug.Log("***********************************");
				string name = (file != null) ? file.Name : "No data";
				Debug.Log("Name: " + name);
				Debug.Log("***********************************");
				string path = (file != null) ? file.Path : "No data";
				Debug.Log("Path: " + path);
				Debug.Log("***********************************");

				ReadTextFile(path);
				// StartCoroutine(ReadTextFileCoroutine(path));

			}, false);
		}, false);

		
		Debug.Log("***********************************");
		Debug.Log("File Picker end.");
		Debug.Log("***********************************");
#endif

#if UNITY_EDITOR
		textMesh.text = "We are not on the HoloLens Device, but still in Unity.";
#endif
	}

	void ReadTextFile(string path)
	{
		StreamReader sr = new StreamReader(new FileStream(path, FileMode.OpenOrCreate), System.Text.Encoding.UTF8);
		string fileText = sr.ReadToEnd();
		sr.Dispose();
		Debug.Log("***********************************");
		Debug.Log(" Text: " + fileText);
		Debug.Log("***********************************");

		if (textMesh != null)
		{
			textMesh.text = fileText;
		}
	}

	IEnumerator ReadTextFileCoroutine(string path)
	{
		Debug.Log("***********************************");
		Debug.Log(" Coroutine start: " + path);
		Debug.Log("***********************************");
		var www = new WWW("file://" + path);
		yield return www;

		string fileText = www.text;
		Debug.Log("***********************************");
		Debug.Log(" Text: " + fileText);
		Debug.Log("***********************************");

		if (textMesh != null)
		{
			textMesh.text = fileText;
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogScript : MonoBehaviour
{
    TextMesh textMesh;

    // Use this for initialization
    void Start()
    {
        textMesh = GameObject.Find("LogWindow").GetComponent<TextMesh>();
        //textMesh = gameObject.GetComponentInChildren<TextMesh>();
    }

    void OnEnable()
    {
        Application.logMessageReceived += LogMessage;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogMessage;
    }

    public void LogMessage(string message, string stackTrace, LogType type)
    {
        if (textMesh.text.Length > 200)
        {
            textMesh.text = message + "\n";
        }
        else
        {
            textMesh.text += message + "\n";
        }
    }

    public void saySomething(string something)
    {
        if (textMesh.text.Length > 200)
        {
            textMesh.text = something + "\n";
        }
        else
        {
            textMesh.text += something + "\n";
        }
    }
}
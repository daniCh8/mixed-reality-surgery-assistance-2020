using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustBoneColor : MonoBehaviour
{
    // Start is called before the first frame update

    private static string _ColorID = "_Color";
    private static string _EmissionEnableID = "_EnableEmission";
    private static string _EmissionColorID = "_EmissiveColor";

    [Header("Default Color Settings")]
    public Color defaultColor = new Color(1,1,1,1);
    public Color onHoldColor = new Color(0.2f, 0.5f, 1f, 0.75f);
    public Color onHoldEmissionColor = new Color(1f, 1f, 1f, 1f);
    public float minOpacity = 0.3f;
    private float maxOpacity = 1.0f;

    private Color _currentColor = new Color(1,1,1,1);
    private Color _lastColor;


    [Header("Color Shifting Speed")]
    [Range(1f, 3f)]
    public float periodOfOpacityChange = 2.0f;
    private float magnitude;

    private bool onHold = false;
    private float holdBeginTime;
    private float holdBeginAlpha;
    private float holdBeginPhase;

    private Material _material;
    private Material _adjustedMaterial;

    private ColorState mode;

    void Start()
    {
        _material = gameObject.GetComponentInChildren<Renderer>().material;
        
        _material.SetColor(_ColorID, defaultColor);

        magnitude = (maxOpacity - minOpacity) * 0.5f;

        string name = gameObject.name;

        _adjustedMaterial = GameObject.Find(name + "_aligned").GetComponentInChildren<Renderer>().material;

        _adjustedMaterial.SetColor(_ColorID, defaultColor);
    }

    void Update()
    {
        mode = GlobalController.gColorState;
        magnitude = (maxOpacity - minOpacity) * 0.5f;

        if (mode == ColorState.Edit)
        {
            // In edit-color mode, change opacity
            if (onHold)
            {
                float time = Time.time;
                float alpha = magnitude * Mathf.Cos(2 * Mathf.PI / periodOfOpacityChange * (time - holdBeginTime) + holdBeginPhase) + magnitude + minOpacity;
                //Debug.Log("Current Alpha: " + alpha);
                _currentColor.a = alpha;
                _material.SetColor(_ColorID, _currentColor);
                _adjustedMaterial.SetColor(_ColorID, _currentColor);
            }
        }
        else
        {
            // do nothing
        }
    }

    public void ResetColor()
    {
        _currentColor = defaultColor;
        _material.SetColor(_ColorID, defaultColor);
        _adjustedMaterial.SetColor(_ColorID, defaultColor);
    }

    public void OnSelect()
    {
        onHold = true;
        if (mode == ColorState.Edit)
        {
            holdBeginTime = Time.time;
            holdBeginAlpha = _material.GetColor(_ColorID).a;
            holdBeginPhase = Mathf.Acos(((holdBeginAlpha - minOpacity) - magnitude) / magnitude);

            Debug.Log("Begin time:" + holdBeginTime);
            Debug.Log("Begin alpha:" + holdBeginAlpha);
        } else
        {
            SelectModeColorChangeStart();
        }
    }

    public void ExitSelect()
    {
        onHold = false;
        if (mode == ColorState.Edit)
        {

        }
        else
        {
            SelectModeColorChangeEnd();
        }
    }

    private void SelectModeColorChangeStart()
    {
        _lastColor = _currentColor;
        _currentColor = onHoldColor;

        _material.SetColor(_ColorID, _currentColor);
        _material.SetFloat(_EmissionEnableID, 1.0f);
        _material.SetColor(_EmissionColorID, onHoldEmissionColor);

        _adjustedMaterial.SetColor(_ColorID, _currentColor);
        _adjustedMaterial.SetFloat(_EmissionEnableID, 1.0f);
        _adjustedMaterial.SetColor(_EmissionColorID, onHoldEmissionColor);

        Debug.Log("OnSelect:" + _currentColor);
    }

    private void SelectModeColorChangeEnd()
    {
        _currentColor = _lastColor;
        _material.SetColor(_ColorID, _currentColor);
        _material.SetFloat(_EmissionEnableID, 0.0f);

        _adjustedMaterial.SetColor(_ColorID, _currentColor);
        _adjustedMaterial.SetFloat(_EmissionEnableID, 0.0f);
        Debug.Log("ExitSelect" + _currentColor);
    }

}

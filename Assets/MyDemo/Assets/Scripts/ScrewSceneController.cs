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

    // New Screw Ends
    public GameObject screwFrontEnd, screwBackEnd;

    // Menus
    public GameObject nearMenu, handMenu;

    // Groups
    public GameObject screwGroup, plateGroup, boneGroup, allGroup;

    // Screws Materials
    public Material newScrewMaterial, medScrewMaterial, latScrewMaterial, selectedScrewMaterial;

    // Screw Button Handler
    public GameObject screwButton;

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

    void Start()
    {
        screws = new List<GameObject>();
        bones = new List<GameObject>();

        // Initialize Screw List
        foreach (Transform screw in screwGroup.transform)
        {
            foreach(Transform real_screw in screw.gameObject.transform)
            {
                screws.Add(real_screw.gameObject);
            }
        }

        // Initialize Plate List
        foreach (Transform plate in plateGroup.transform)
        {
            if (plate.gameObject.name.StartsWith(Constants.LAT))
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

    private bool ScrewIsNotFlag(GameObject screw, String flag)
    {
        return !ScrewIsFlag(screw, flag);
    }

    private bool ScrewIsFlag(GameObject screw, String flag)
    {
        return screw.name.Contains(flag);
    }

    private void SetLatScrewsActive(bool active)
    {
        foreach (GameObject screw in screws)
        {
            if (ScrewIsFlag(screw, Constants.LAT))
            {
                screw.SetActive(active);
            }
        }
    }

    private void SetMedScrewsActive(bool active)
    {
        foreach (GameObject screw in screws)
        {
            if (ScrewIsFlag(screw, Constants.MED))
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
            return;
        }

        String flag = gPlatesState == PlatesState.Lat ? Constants.LAT : Constants.MED;
        NextIndex();
        while (ScrewIsNotFlag(screws[screwIndex], flag))
        {
            NextIndex();
        }
    }

    private void FindPrevIndex()
    {
        if (gPlatesState == PlatesState.Both || gPlatesState == PlatesState.None)
        {
            PrevIndex();
            return;
        }

        String flag = gPlatesState == PlatesState.Lat ? Constants.LAT : Constants.MED;
        PrevIndex();
        while (ScrewIsNotFlag(screws[screwIndex], flag))
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
            case Constants.LAT:
                screw.GetComponentInChildren<Renderer>().material = latScrewMaterial;
                break;
            case Constants.MED:
                screw.GetComponentInChildren<Renderer>().material = medScrewMaterial;
                break;
            case Constants.NEW:
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

        if (boundsControl.enabled)
        {
            boundsControl.enabled = false;
            objectManipulator.enabled = false;
            SetTexts(texts, Constants.ALLOW_MANIPULATION);
        }
        else
        {
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

    public void NewScrew()
    {
        var newScrew = CreateCylinderBetweenPoints(screwFrontEnd.GetComponentInChildren<Renderer>(true).bounds.center,
                                                    screwBackEnd.GetComponentInChildren<Renderer>(true).bounds.center);
        newScrew.tag = Constants.NEW;
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

    private GameObject CreateCylinderBetweenPoints(Vector3 start, Vector3 end)
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
        screw.GetComponentInChildren<WholeScaleConstraint>(true).enabled = activate;
        screw.GetComponentInChildren<ScaleConstraint>(true).enabled = !activate;
        screw.GetComponentInChildren<PositionConstraint>(true).enabled = !activate;
    }
}

static class Constants
{
    public const String LAT = "Lat";
    public const String MED = "Med";
    public const String NEW = "New";
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
}

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

    //Plates Visibility State
    public enum PlatesState { Both, Lat, Med, None }

    // Menus
    public GameObject nearMenu, handMenu;

    // Groups
    public GameObject screwGroup, plateGroup, boneGroup, allGroup;

    // Screws Materials
    public Material medScrewMaterial, latScrewMaterial, selectedScrewMaterial;

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

    void Start()
    {
        screws = new List<GameObject>();
        bones = new List<GameObject>();

        // Initialize Screw List
        foreach (Transform screw in screwGroup.transform)
        {
            screws.Add(screw.gameObject);
        }

        // Initialize Plate List
        foreach (Transform plate in plateGroup.transform)
        {
            if (plate.gameObject.name.StartsWith("Lat"))
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
        Debug.Log("Change Menu Button pressed");

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
        TextMeshPro[] texts = GameObject.Find("ChangeBoneVisibility").GetComponentsInChildren<TextMeshPro>();

        if (boneGroup.activeInHierarchy)
        {
            boneGroup.SetActive(false);
            SetTexts(texts, "Show Bone");
        }
        else
        {
            boneGroup.SetActive(true);
            SetTexts(texts, "Hide Bone");
        }
    }

    public void ChangePlatesVisibility()
    {
        TextMeshPro[] texts = GameObject.Find("ChangePlatesVisibility").GetComponentsInChildren<TextMeshPro>();

        switch (gPlatesState)
        {
            case PlatesState.Both:
                latPlate.SetActive(true);
                medPlate.SetActive(false);
                SetLatScrewsActive(true);
                SetMedScrewsActive(false);
                gPlatesState = PlatesState.Lat;
                SetTexts(texts, "Show Only Med Plate");
                break;
            case PlatesState.Lat:
                latPlate.SetActive(false);
                medPlate.SetActive(true);
                SetLatScrewsActive(false);
                SetMedScrewsActive(true);
                gPlatesState = PlatesState.Med;
                SetTexts(texts, "Hide Plates");
                break;
            case PlatesState.Med:
                latPlate.SetActive(false);
                medPlate.SetActive(false);
                SetLatScrewsActive(true);
                SetMedScrewsActive(true);
                gPlatesState = PlatesState.None;
                SetTexts(texts, "Show Plates");
                break;
            case PlatesState.None:
                latPlate.SetActive(true);
                medPlate.SetActive(true);
                SetLatScrewsActive(true);
                SetMedScrewsActive(true);
                gPlatesState = PlatesState.Both;
                SetTexts(texts, "Show Only Lat Plate");
                break;
            default:
                break;
        }
    }

    private bool ScrewIsLat(GameObject screw)
    {
        return screw.name.Contains("Lat");
    }

    private void SetLatScrewsActive(bool active)
    {
        foreach (GameObject screw in screws)
        {
            if (ScrewIsLat(screw))
            {
                screw.SetActive(active);
            }
        }
    }

    private void SetMedScrewsActive(bool active)
    {
        foreach (GameObject screw in screws)
        {
            if (!ScrewIsLat(screw))
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

        String flag = gPlatesState == PlatesState.Lat ? "Lat" : "Med";
        NextIndex();
        while (!screws[screwIndex].name.Contains(flag))
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

        String flag = gPlatesState == PlatesState.Lat ? "Lat" : "Med";
        PrevIndex();
        while (!screws[screwIndex].name.Contains(flag))
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
        screw.GetComponentInChildren<BoundsControl>(true).enabled = false;
        screw.GetComponentInChildren<ScaleConstraint>(true).enabled = false;
        screw.GetComponentInChildren<PositionConstraint>(true).enabled = false;
        screw.GetComponentInChildren<Renderer>().material = screw.GetComponentInChildren<IsLat>().isLat ? latScrewMaterial : medScrewMaterial;
    }

    private void ActivateScrew(GameObject screw)
    {
        screw.GetComponentInChildren<BoundsControl>(true).enabled = true;
        screw.GetComponentInChildren<ScaleConstraint>(true).enabled = true;
        PositionConstraint posConstr = screw.GetComponentInChildren<PositionConstraint>(true);
        posConstr.enabled = true;
        posConstr.screwPosition = screw.transform.position;
       
        screw.GetComponentInChildren<Renderer>().material = selectedScrewMaterial;
    }

    public void ChangeScrewState()
    {
        TextMeshPro[] texts = GameObject.Find("ChangeScrewState").GetComponentsInChildren<TextMeshPro>();

        if (screwButton.activeInHierarchy)
        {
            screwButton.SetActive(false);
            foreach (GameObject screw in screws)
            {
                DeactivateScrew(screw);
            }
            SetTexts(texts, "Manipulate Screws");
        }
        else
        {
            screwButton.SetActive(true);
            FindNextIndex();
            ActivateScrew(screws[screwIndex]);
            SetTexts(texts, "Stop Manipulationg Screws");
        }
    }

    public void ChangeBoundsControlState()
    {
        TextMeshPro[] texts = GameObject.Find("ChangeBoundsControl").GetComponentsInChildren<TextMeshPro>();
        BoundsControl boundsControl = allGroup.GetComponentInChildren<BoundsControl>(true);

        if (boundsControl.enabled)
        {
            boundsControl.enabled = false;
            SetTexts(texts, "Allow Manipulation");
        }
        else
        {
            boundsControl.enabled = true;
            SetTexts(texts, "Disallow Manipulation");
        }
    }

    private bool isASharedButton(PressableButton button)
    {
        return button.name != "ButtonPin" && button.name != "ChangeMenuType";
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

}

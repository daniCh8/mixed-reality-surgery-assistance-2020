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
    // Menus
    public GameObject nearMenu, handMenu;

    // Groups
    public GameObject screwGroup, plateGroup, boneGroup, allGroup;

    // Screws Materials
    public List<Material> screwMaterials;

    // Screw Button Handler
    public GameObject screwButton;

    // Lists
    public static List<GameObject> screws, bones, plates;

    // Screw List Index
    private static int screwIndex;

    // Menu State
    public enum MenuState { Hand, Near }

    void Start()
    {
        screws = new List<GameObject>();
        bones = new List<GameObject>();
        plates = new List<GameObject>();

        // Initialize Screw List
        foreach (Transform screw in screwGroup.transform)
        {
            screws.Add(screw.gameObject);
        }

        // Initialize Plate List
        foreach (Transform plate in plateGroup.transform)
        {
            plates.Add(plate.gameObject);
        }

        screwIndex = 0;
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
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Show Bone";
            }
        }
        else
        {
            boneGroup.SetActive(true);
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Hide Bone";
            }
        }
    }

    public void NextScrew()
    {
        DeactivateScrew(screws[screwIndex]);

        screwIndex = (screwIndex + 1 == screws.Count) ? 0 : (screwIndex + 1);

        ActivateScrew(screws[screwIndex]);
    }

    public void PrevScrew()
    {
        DeactivateScrew(screws[screwIndex]);

        screwIndex = (screwIndex == 0) ? (screws.Count-1) : (screwIndex-1);

        ActivateScrew(screws[screwIndex]);
    }

    private void DeactivateScrew(GameObject screw)
    {
        screw.GetComponentInChildren<BoundsControl>(true).enabled = false;
        screw.GetComponentInChildren<Renderer>().material = screwMaterials[0];
    }

    private void ActivateScrew(GameObject screw)
    {
        screw.GetComponentInChildren<BoundsControl>(true).enabled = true;
        screw.GetComponentInChildren<Renderer>().material = screwMaterials[1];
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
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Manipulate Screws";
            }
        }
        else
        {
            screwButton.SetActive(true);
            ActivateScrew(screws[screwIndex]);
        }
        foreach (TextMeshPro tmp in texts)
        {
            tmp.text = "Stop Manipulating Screws";
        }
    }

    public void ChangeBoundsControlState()
    {
        TextMeshPro[] texts = GameObject.Find("ChangeBoundsControl").GetComponentsInChildren<TextMeshPro>();
        BoundsControl boundsControl = allGroup.GetComponentInChildren<BoundsControl>(true);

        if (boundsControl.enabled)
        {
            boundsControl.enabled = false;

            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Allow Manipulation";
            }
        }
        else
        {
            boundsControl.enabled = true;   

            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Disallow Manipulation";
            }
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

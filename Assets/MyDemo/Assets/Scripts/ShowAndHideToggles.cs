using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowAndHideToggles : MonoBehaviour
{
    static bool isDisplayed;

    static List<GameObject> toggleSwitches = new List<GameObject>();

    void Start()
    {
        isDisplayed = false;

        toggleSwitches.Clear();
        for (int i = 1; i <= 6; i++)
        {
            toggleSwitches.Add(GameObject.Find("ToggleSwitch_Bone" + i));
        }

        Debug.Log("#switches: " + toggleSwitches.Count);

        for(int i = 0; i< toggleSwitches.Count; i++)
        {
            toggleSwitches[i].SetActive(false);
        }
    }

    public void ShowAndHideToggelSwitches()
    {
        if (isDisplayed)
        {
            for (int i = 0; i < toggleSwitches.Count; i++)
            {
                toggleSwitches[i].SetActive(false);
            }
            isDisplayed = false;
        } else
        {
            for (int i = 0; i < toggleSwitches.Count; i++)
            {
                toggleSwitches[i].SetActive(true);
            }
            isDisplayed = true;
        }
    }
}

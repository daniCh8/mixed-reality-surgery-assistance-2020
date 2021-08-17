using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAlign : MonoBehaviour
{
    // rotations sum up, scaling multiplies, translation's different
    public GameObject boneGroup, allGroup;
    private bool go = false;

    void Update()
    {
        if (!go)
        {
            return;
        }

        transform.localEulerAngles = boneGroup.transform.localEulerAngles +
            allGroup.transform.localEulerAngles;

        transform.localScale = Vector3.Scale(boneGroup.transform.localScale,
            allGroup.transform.localScale);

        transform.localPosition = allGroup.transform.localPosition +
            Vector3.Scale(boneGroup.transform.localPosition, allGroup.transform.localScale);
    }

    public void SetGo(bool flag)
    {
        go = flag;
    }
}

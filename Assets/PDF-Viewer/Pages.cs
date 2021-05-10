using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pages : MonoBehaviour
{
    public List<Material> materials;
    private int curIndex;

    void Start()
    {
        curIndex = 0;
    }

    public Material nextMat()
    {
        curIndex = (curIndex + 1 == materials.Count) ? 0 : curIndex+1;
        return materials[curIndex];
    }

    public Material prevMat()
    {
        curIndex = (curIndex == 0) ? materials.Count-1 : curIndex-1;
        return materials[curIndex];
    }

    public Material getMat()
    {
        return materials[curIndex];
    }
}

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

public class ResetPositions : MonoBehaviour
{
    class TransformInfo
    {
        public Vector3 pos;
        public Quaternion rotate;
        public Vector3 scale;
    }

    private static List<GameObject> bones = new List<GameObject>();

    private static List<TransformInfo> originalTransform = new List<TransformInfo>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 2; i <= GlobalController.numberOfBones; i++)
        {
            GameObject t = GameObject.Find("Bone_" + i);
            bones.Add(t);
            Transform tr = t.GetComponent<Transform>();
            TransformInfo ti = new TransformInfo
            {
                pos = tr.localPosition,
                rotate = tr.localRotation,
                scale = tr.localScale
            };
            //Debug.Log(tr);
            originalTransform.Add(ti);
        }
        Debug.Log("Init: #bones, " + bones.Count);
        Debug.Log("Init: #transforms, " + originalTransform.Count);

    }

    public void resetPositions()
    {
        //    StartMethod example = new StartMethod();
        //    bones = example.getBones();
        //    transformOfBones = example.getTransform();

        Debug.Log("Botton Pressed");

        //Debug.Log("Num of Bones transform, " + transformOfBones.Count);
        Debug.Log("Num of Bonesbones, " + bones.Count);

        //Transform worldTransform = GameObject.Find("BoneCollection").transform;

        for (int i = 0; i < originalTransform.Count; i++)
        {
            bones[i].transform.localPosition = originalTransform[i].pos;
            bones[i].transform.localRotation = originalTransform[i].rotate;
            bones[i].transform.localScale = originalTransform[i].scale;
        }

    }
}
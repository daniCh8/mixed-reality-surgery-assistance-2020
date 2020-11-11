using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

public class HandPlaneMove : MonoBehaviour
{
    public HandSlice hs;
    public Plane oldPlane;

    // Start is called before the first frame update
    void Start()
    {
        GameObject handSlicer = GameObject.Find("CTPlane3");
        HandSlice handSlicerScript = handSlicer.GetComponent<HandSlice>();
        oldPlane = hs.plane;
    }

    // Update is called once per frame
    void Update()
    {
        HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximalJoint, hs.leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po5);
        
        transform.position = po5.Position;
        //Quaternion rotation = Quaternion.FromToRotation(oldPlane.normal, hs.plane.normal); 
        transform.rotation = Quaternion.LookRotation(hs.plane.normal, transform.up);
    }
}

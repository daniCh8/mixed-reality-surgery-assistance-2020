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

    // Start is called before the first frame update
    void Start()
    {
        //GameObject handSlicer = GameObject.Find("CTPlane3");
        //HandSlice handSlicerScript = handSlicer.GetComponent<HandSlice>();
    }

    // Update is called once per frame
    void Update()
    {
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexKnuckle, hs.leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po1) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, hs.leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po2) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyKnuckle, hs.leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po3) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, hs.leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po4) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximalJoint, hs.leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po5)){
            
            // Set plane position to hand
            transform.position = po1.Position;

            // Set rotation of plane
            Vector3 a = po2.Position - po1.Position;
            Vector3 b = po3.Position - po1.Position;
            Vector3 normal = Vector3.Cross(a, b).normalized;
            Debug.Log(normal);
            transform.up = normal;
        }
    }
}

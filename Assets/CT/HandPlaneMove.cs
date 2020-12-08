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
    public Vector3 pos = new Vector3(0, 0, 0);
    public bool locked = false;

    public Material unlockedMaterial;
    public Material lockedMaterial;

    private Vector3 normal = new Vector3(0, 1, 0);
    private Plane plane = new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0));

    // Start is called before the first frame update
    void Start()
    {
        GameObject handSlicer = GameObject.Find("CTPlane3");
        HandSlice hs = handSlicer.GetComponent<HandSlice>();
    }

    // Update is called once per frame
    void Update()
    {
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexKnuckle, hs.leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po1) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, hs.leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po2) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyKnuckle, hs.leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po3) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, hs.leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po4) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximalJoint, hs.leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po5)){
            locked = hs.locked;
            if (!locked) {
                // Set to green material (unlocked)
                GetComponent<MeshRenderer>().material = unlockedMaterial;
                // Set plane position to hand
                pos = po1.Position;
                transform.position = pos;
                // Set rotation of plane
                Vector3 a = po2.Position - po1.Position;
                Vector3 b = po3.Position - po1.Position;
                normal = Vector3.Cross(a, b).normalized;
                transform.up = normal;
                plane = new Plane(normal, pos);
            } else {
                // Set to red material (locked)
                GetComponent<MeshRenderer>().material = lockedMaterial;
                transform.position = pos + plane.GetDistanceToPoint(po1.Position) * normal.normalized;
            }

        }
    }
}

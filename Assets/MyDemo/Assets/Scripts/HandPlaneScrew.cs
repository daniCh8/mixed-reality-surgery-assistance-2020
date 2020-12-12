using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

public class HandPlaneScrew : MonoBehaviour
{
    public Vector3 pos = new Vector3(0, 0, 0);
    public Material Material;

    public Vector3 normal = new Vector3(0, 1, 0);
    private Plane plane = new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0));

    // Start is called before the first frame update

    void Update()
    {
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexKnuckle,Handedness.Right, out MixedRealityPose po1) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip,Handedness.Right, out MixedRealityPose po2) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyKnuckle, Handedness.Right, out MixedRealityPose po3)&&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Right, out MixedRealityPose po4) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximalJoint, Handedness.Right, out MixedRealityPose po5))
        {
            GetComponent<MeshRenderer>().material = Material;
            pos = po1.Position;
            transform.position = ScrewSceneController.AddScrewPoint;
            // Set rotation of plane
            Vector3 a = po2.Position - po1.Position;
            Vector3 b = po3.Position - po1.Position;
            normal = Vector3.Cross(a, b).normalized;
            transform.up = normal;
            // plane = new Plane(normal, ScrewSceneController.AddScrewPoint);

            float angle = Vector3.Angle(po4.Position - po5.Position, po1.Position - po5.Position);
            // FocusHandler.ScrewAngleRegister();
        }
    }
    public Vector3 getNormal()
    {
        return normal;  
    }
}

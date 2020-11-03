using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Net.Mime;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class HandSlice : MonoBehaviour {
    public CTReader ct;
    public int width, height;
    public int interval;

    public bool enableReferencePlane;
    public bool leftHanded;
    public bool active = false;

    Texture2D tex;
    GameObject referencePlane;
    LogScript log;

    int curInterval = 0;

    void Start() {
        tex = new Texture2D(width, height);
        GetComponent<Renderer>().material.mainTexture = tex;
        log = GameObject.Find("LogWindow").GetComponent<LogScript>();
    }

    void Update() {
        if (!active || ++curInterval < interval) return;
        curInterval = 0;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexKnuckle, leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po1) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po2) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyKnuckle, leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po3) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po4) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximalJoint, leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po5)) {

            var p1 = ct.TransformWorldCoords(po1.Position);
            var p2 = ct.TransformWorldCoords(po2.Position);
            var p3 = ct.TransformWorldCoords(po3.Position);
            var p4 = ct.TransformWorldCoords(po4.Position);
            var p5 = ct.TransformWorldCoords(po5.Position);

            /*
            float deltaX = p4.x - p1.x;
            float deltaY = p4.y - p1.y;
            float deltaZ = p4.z - p1.z;
            float distance = (float)System.Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
            log.saySomething("distance: " + distance);
            */

            float angle = Vector3.Angle(p4 - p5, p1 - p5);
            
            if (angle > 70 ||
                p1.x < -0.5 || 0.5 < p1.x ||
                p1.y < -0.5 || 0.5 < p1.y ||
                p1.z < -0.5 || 0.5 < p1.z ||
                p2.x < -0.5 || 0.5 < p2.x ||
                p2.y < -0.5 || 0.5 < p2.y ||
                p2.z < -0.5 || 0.5 < p2.z ||
                p3.x < -0.5 || 0.5 < p3.x ||
                p3.y < -0.5 || 0.5 < p3.y ||
                p3.z < -0.5 || 0.5 < p3.z) return;

            var plane = leftHanded ? new Plane(p1, p3, p2) : new Plane(p1, p2, p3);

            var orig = plane.ClosestPointOnPlane(Vector3.zero);
            var dy = (p2 - p1).normalized;
            var dx = Vector3.Cross(dy, plane.normal);

            if (enableReferencePlane) {
                if (!referencePlane) {
                    referencePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    referencePlane.GetComponent<Transform>().SetParent(ct.gameObject.GetComponent<Transform>());
                    referencePlane.GetComponent<Transform>().localScale = new Vector3(0.02f, 0.02f, 0.02f);
                }
                referencePlane.GetComponent<Transform>().up = plane.normal;
                referencePlane.GetComponent<Transform>().localPosition = p2;
            }

            ct.Slice(orig, dx, dy, tex);
        }
    }
}
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleConstraint : TransformConstraint
{
    public override TransformFlags ConstraintType => TransformFlags.Scale;

    public override void ApplyConstraint(ref MixedRealityTransform transform)
    {
        Vector3 scale = transform.Scale;

        scale.x = worldPoseOnManipulationStart.Scale.x;
        scale.z = worldPoseOnManipulationStart.Scale.z;

        transform.Scale = scale;
    }
}

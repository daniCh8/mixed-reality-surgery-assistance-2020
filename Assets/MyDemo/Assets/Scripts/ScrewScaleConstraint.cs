using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//screw should only be scaled in one direction
public class ScrewScaleConstraint : TransformConstraint
{
    public override TransformFlags ConstraintType => TransformFlags.Scale;
    //public override TransformFlags ConstraintType => TransformFlags.Position;

    public override void ApplyConstraint(ref MixedRealityTransform transform)
    {
        Vector3 scale = transform.Scale;
        Vector3 pos = transform.Position; // add something like this to keep position of the screw during scaling?? doesnt work yet

        scale.x = worldPoseOnManipulationStart.Scale.x;
        scale.z = worldPoseOnManipulationStart.Scale.z;
        pos.x = worldPoseOnManipulationStart.Position.x;
        pos.y = worldPoseOnManipulationStart.Position.y;
        pos.z = worldPoseOnManipulationStart.Position.z;
        transform.Scale = scale;
        transform.Position = pos;
    }
}
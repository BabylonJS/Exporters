using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Utilities;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonAnimationKey : IComparable<BabylonAnimationKey>, ICloneable
    {
        [DataMember]
        public float frame { get; set; }

        [DataMember]
        public float[] values { get; set; }

        public object Clone()
        {
            return new BabylonAnimationKey
            {
                frame = frame,
                values = (float[])values.Clone()
            };
        }

        public int CompareTo(BabylonAnimationKey other)
        {
            if (other == null)
                return 1;
            else
                return this.frame.CompareTo(other.frame);
        }

        public static float[] Interpolate(BabylonAnimation animation, BabylonAnimationKey fromKey, BabylonAnimationKey toKey, float frame)
        {
            switch (animation.property)
            {
                case "_matrix":
                    var fromMatrix = new BabylonMatrix();
                    fromMatrix.m = new List<float>(fromKey.values).ToArray();
                    var toMatrix = new BabylonMatrix();
                    toMatrix.m = new List<float>(toKey.values).ToArray();
                    var fromPosition = new BabylonVector3();
                    var fromRotation = new BabylonQuaternion();
                    var fromScaling = new BabylonVector3();
                    var toPosition = new BabylonVector3();
                    var toRotation = new BabylonQuaternion();
                    var toScaling = new BabylonVector3();

                    fromMatrix.decompose(fromScaling, fromRotation, fromPosition);
                    toMatrix.decompose(toScaling, toRotation, toPosition);

                    var lerpFactor = MathUtilities.GetLerpFactor(fromKey.frame, toKey.frame, frame);

                    var interpolatedKeyPosition = BabylonVector3.FromArray(MathUtilities.Lerp(fromPosition.ToArray(), toPosition.ToArray(), lerpFactor));
                    var interpolatedKeyRotation = BabylonQuaternion.Slerp(fromRotation, toRotation, lerpFactor);
                    var interpolatedKeyScaling = BabylonVector3.FromArray(MathUtilities.Lerp(fromScaling.ToArray(), toScaling.ToArray(), lerpFactor));

                    return BabylonMatrix.Compose(interpolatedKeyScaling, interpolatedKeyRotation, interpolatedKeyPosition).m;
                case "rotationQuaternion":
                    return BabylonQuaternion.Slerp(BabylonQuaternion.FromArray(fromKey.values), BabylonQuaternion.FromArray(toKey.values), MathUtilities.GetLerpFactor(fromKey.frame, toKey.frame, frame)).ToArray();
                case "scaling":
                case "position":
                default:
                    return MathUtilities.Lerp(fromKey.values, toKey.values, MathUtilities.GetLerpFactor(fromKey.frame, toKey.frame, frame));
            }
        }
    }
}

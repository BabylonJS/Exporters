using System;
using System.Collections.Generic;
using System.Text;
using BabylonExport.Entities;

namespace Utilities
{
    static class MathUtilities
    {
        public static float Lerp(float min, float max, float t)
        {
            return min + (max - min) * t;
        }

        public static int RoundToInt(float f)
        {
            return Convert.ToInt32(Math.Round(f, MidpointRounding.AwayFromZero));
        }

        /**
         * Computes a texture transform matrix with a pre-transformation
         */
        public static BabylonMatrix ComputeTextureTransformMatrix(BabylonVector3 pivotCenter, BabylonVector3 offset, BabylonQuaternion rotation, BabylonVector3 scale)
        {
            var dOffset = new BabylonVector3();
            var dRotation = new BabylonQuaternion();
            var dScale = new BabylonVector3();
            offset.X *= scale.X;
            offset.Y *= scale.Y;
            offset.Z *= 0;

            var transformMatrix = BabylonMatrix.Translation(new BabylonVector3(-pivotCenter.X, -pivotCenter.Y, 0)).multiply(BabylonMatrix.Compose(scale, rotation, offset)).multiply(BabylonMatrix.Translation(pivotCenter));
            return transformMatrix;
        }
    }
}

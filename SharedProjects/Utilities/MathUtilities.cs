using System;
using System.Collections.Generic;
using System.Text;
using BabylonExport.Entities;

namespace Utilities
{
    static class MathUtilities
    {
        public static float GetLerpFactor(float from, float to, float value)
        {
            return (value - from) / (to - from);
        }

        public static float Lerp(float min, float max, float t)
        {
            return min + (max - min) * t;
        }

        public static float LerpEulerAngle(float min, float max, float t)
        {
            while(min < 0 || max < 0)
            {
                min += 360.0f;
                max += 360.0f;
            }

            return min + (max - min) * t;
        }

        public static float[] Lerp(float[] minArray, float[] maxArray, float t)
        {
            float[] res = new float[minArray.Length];
            for (int index = 0; index < minArray.Length; index++)
            {
                res[index] = MathUtilities.Lerp(minArray[index], maxArray[index], t);
            }
            return res;
        }

        public static float[] LerpEulerAngle(float[] minArray, float[] maxArray, float t)
        {
            float[] res = new float[minArray.Length];
            for (int index = 0; index < minArray.Length; index++)
            {
                res[index] = MathUtilities.LerpEulerAngle(minArray[index], maxArray[index], t);
            }
            return res;
        }

        public static int RoundToInt(float f)
        {
            return Convert.ToInt32(Math.Round(f, MidpointRounding.AwayFromZero));
        }

        public static bool IsAlmostEqualTo(float first, float second, float epsilon)
        {
            return Math.Abs(first - second) < epsilon;
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

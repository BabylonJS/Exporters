using System;
using BabylonExport.Entities;

namespace Utilities
{
    static class MathUtilities
    {
        public const float Epsilon = 1E-7f;
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

        public static bool IsAlmostEqualTo(float first, float second, float epsilon = Epsilon)
        {
            return Math.Abs(first - second) <= epsilon;
        }

        /// <summary>
        /// This is used to round floating 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static float RoundToIfAlmostEqualTo(float a, float b, float epsilon = Epsilon)
        {
            return Math.Abs(a - b) <= epsilon ? b : a ;
        }

        public static float DotProduct(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            return x1 * x2 + y1 * y2 + z1 * z2;
        }
        public static void CrossProduct(float x1, float y1, float z1, float x2, float y2, float z2, out float x3, out float y3, out float z3)
        {
            x3 = y1 * z2 - z1 * y2;
            y3 = z1 * x2 - x1 * z2;
            z3 = x1 * y2 - y1 * x2;
        }


        /**
         * Computes a texture transform matrix with a pre-transformation 
         */
        public static BabylonMatrix ComputeTextureTransformMatrix(BabylonVector3 pivotCenter, BabylonVector3 offset, BabylonQuaternion rotation, BabylonVector3 scale)
        {
            var transformMatrix = BabylonMatrix.Translation(new BabylonVector3(-pivotCenter.X, -pivotCenter.Y, 0))
                                               .multiply(BabylonMatrix.Compose(scale, rotation, offset))
                                               .multiply(BabylonMatrix.Translation(pivotCenter));
            return transformMatrix;
        }
    }
}

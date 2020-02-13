using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;

namespace Maya2Babylon
{
    static class MTransformationMatrixExtension
    {
        /// <summary>
        /// Default space is transform
        /// </summary>
        /// <param name="mTransformationMatrix"></param>
        /// <returns></returns>
        public static float[] getTranslation(this MTransformationMatrix mTransformationMatrix)
        {
            MVector mVector = mTransformationMatrix.getTranslation(MSpace.Space.kTransform);
            return mVector.toArray();
        }

        /// <summary>
        /// Default space is transform
        /// Default rotation order is YXZ
        /// </summary>
        /// <param name="mTransformationMatrix"></param>
        /// <returns></returns>
        public static float[] getRotation(this MTransformationMatrix mTransformationMatrix)
        {
            double x = 0, y = 0, z = 0, w = 0;
            mTransformationMatrix.getRotationQuaternion(ref x, ref y, ref z, ref w);
            // Maya conversion algorithm is bugged when reaching limits (angle like (-90,89,90))
            // Convert quaternion to vector3 using Babylon conversion algorithm
            BabylonQuaternion babylonQuaternion = new BabylonQuaternion((float)x, (float)y, (float)z, (float)w);
            return babylonQuaternion.toEulerAngles().ToArray();
        }

        /// <summary>
        /// Default space is transform
        /// </summary>
        /// <param name="mTransformationMatrix"></param>
        /// <returns></returns>
        public static float[] getRotationQuaternion(this MTransformationMatrix mTransformationMatrix)
        {
            double x = 0, y = 0, z = 0, w = 0;
            mTransformationMatrix.getRotationQuaternion(ref x, ref y, ref z, ref w);
            return new float[] { (float) x, (float) y, (float) z, (float) w };
        }

        /// <summary>
        /// Default space is transform
        /// </summary>
        /// <param name="mTransformationMatrix"></param>
        /// <returns></returns>
        public static float[] getScale(this MTransformationMatrix mTransformationMatrix)
        {
            double[] scale = new double[3];
            mTransformationMatrix.getScale(scale, MSpace.Space.kTransform);
            return Array.ConvertAll(scale, item => (float)item);
        }
    }
}

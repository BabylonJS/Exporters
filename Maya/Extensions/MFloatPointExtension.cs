using Autodesk.Maya.OpenMaya;

namespace Maya2Babylon
{
    static class MFloatPointExtension
    {
        /// <summary>
        /// [x,y,z]
        /// </summary>
        /// <param name="mFloatPoint"></param>
        /// <returns></returns>
        public static float[] toArray(this MFloatPoint mFloatPoint)
        {
            float[] array = new float[3];
            for (uint index = 0; index < 3; index++)
            {
                array[index] = mFloatPoint[index];
            }
            return array;
        }

        public static string toString(this MFloatPoint mFloatPoint)
        {
            return mFloatPoint == null ? "" : mFloatPoint.toArray().toString();
        }
    }
}

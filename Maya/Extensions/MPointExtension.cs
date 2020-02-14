using Autodesk.Maya.OpenMaya;

namespace Maya2Babylon
{
    static class MPointExtension
    {
        /// <summary>
        /// [x,y,z]
        /// </summary>
        /// <param name="mFloatPoint"></param>
        /// <returns></returns>
        public static float[] toArray(this MPoint mPoint)
        {
            float[] array = new float[3];
            for (uint index = 0; index < 3; index++)
            {
                array[index] = (float) mPoint[index];
            }
            return array;
        }

        public static string toString(this MPoint mPoint)
        {
            return mPoint == null ? "" : mPoint.toArray().toString();
        }
    }
}

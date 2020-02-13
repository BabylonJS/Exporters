using Autodesk.Maya.OpenMaya;

namespace Maya2Babylon
{
    static class MVectorExtension
    {
        public static float[] toArray(this MVector mVector)
        {
            float[] array = new float[3];
            for (uint index = 0; index < 3; index++)
            {
                array[index] = (float) mVector[index];
            }
            return array;
        }

        public static string toString(this MVector mVector)
        {
            return mVector == null ? "" : mVector.toArray().toString();
        }
    }
}

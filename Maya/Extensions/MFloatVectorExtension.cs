using Autodesk.Maya.OpenMaya;

namespace Maya2Babylon
{
    static class MFloatVectorExtension
    {
        public static float[] toArray(this MFloatVector mFloatVector)
        {
            float[] array = new float[3];
            for (uint index = 0; index < 3; index++)
            {
                array[index] = mFloatVector[index];
            }
            return array;
        }

        public static string toString(this MFloatVector mFloatVector)
        {
            return mFloatVector == null ? "" : mFloatVector.toArray().toString();
        }
    }
}

using Autodesk.Maya.OpenMaya;

namespace Maya2Babylon
{
    static class MPlugExtension
    {
        public static float[] asFloatArray(this MPlug mPlug)
        {
            float[] array = new float[mPlug.numChildren];
            for (uint index = 0; index < mPlug.numChildren; index++)
            {
                array[index] = mPlug.child(index).asFloat();
            }
            return array;
        }
    }
}

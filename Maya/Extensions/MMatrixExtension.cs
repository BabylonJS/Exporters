using Autodesk.Maya.OpenMaya;

namespace Maya2Babylon
{
    static class MMatrixExtension
    {
        public static float[] toArray(this MMatrix mMatrix, uint nbRow = 4, uint nbCol = 4)
        {
            float[] array = new float[nbRow * nbCol];
            for (uint row = 0; row < nbRow; row++)
            {
                for (uint col = 0; col < nbCol; col++)
                {
                    array[row * nbCol + col] = (float) mMatrix[row, col];
                }
            }
            return array;
        }

        public static string toString(this MMatrix mMatrix)
        {
            return mMatrix == null ? "" : mMatrix.toArray().toString();
        }
    }
}

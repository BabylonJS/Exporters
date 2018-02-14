using Autodesk.Maya.OpenMaya;

namespace Maya2Babylon
{
    static class MColorExtension
    {
        /// <summary>
        /// [r,g,b,a]
        /// </summary>
        /// <param name="mColor"></param>
        /// <returns></returns>
        public static float[] toArray(this MColor mColor)
        {
            return new float[] { mColor.r, mColor.g, mColor.b, mColor.a };
        }

        /// <summary>
        /// [r,g,b]
        /// </summary>
        /// <param name="mColor"></param>
        /// <returns></returns>
        public static float[] toArrayRGB(this MColor mColor)
        {
            return new float[] { mColor.r, mColor.g, mColor.b };
        }

        public static string toString(this MColor mColor)
        {
            return mColor == null ? "" : mColor.toArray().toString();
        }
    }
}

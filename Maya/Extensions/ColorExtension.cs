using System.Drawing;

namespace Maya2Babylon
{
    static class ColorExtension
    {
        /// <summary>
        /// [r,g,b,a]
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static float[] toArray(this Color color)
        {
            return new float[] { color.R, color.G, color.B, color.A };
        }

        /// <summary>
        /// [r,g,b]
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static float[] toArrayRGB(this Color color)
        {
            return new float[] { color.R, color.G, color.B };
        }

        public static string toString(this Color color)
        {
            return color == null ? "" : color.toArray().toString();
        }
    }
}

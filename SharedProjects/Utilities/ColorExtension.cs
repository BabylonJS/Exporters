using System.Drawing;

namespace Utilities
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

        public static Color multiply(this Color color, float factor)
        {
            return Color.FromArgb(
            (int)(color.A * factor),
            (int)(color.R * factor),
            (int)(color.G * factor),
            (int)(color.B * factor));
        }

        public static Color multiply(this Color color, Color otherColor)
        {
            return Color.FromArgb(
            (int)(color.A * otherColor.A / 255.0f),
            (int)(color.R * otherColor.R / 255.0f),
            (int)(color.G * otherColor.G / 255.0f),
            (int)(color.B * otherColor.B / 255.0f));
        }

        public static string toString(this Color color)
        {
            return color == null ? "" : color.toArray().ToString();
        }
    }
}

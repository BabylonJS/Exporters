using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Max2Babylon
{
    internal static class ResourceHelper
    {
        /// <summary>
        /// Load an embedded image from the assembly.
        /// </summary>
        public static Image LoadImage(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using (Stream s = asm.GetManifestResourceStream(resourceName))
            {
                if (s == null)
                    throw new InvalidOperationException($"Resource '{resourceName}' not found.");
                return new Bitmap(s);
            }
        }
    }
}

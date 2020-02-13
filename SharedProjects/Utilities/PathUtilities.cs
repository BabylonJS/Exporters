using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Utilities
{
    static class PathUtilities
    {

        /// <summary>
        /// Creates a relative path from one file or folder to another. Input paths that are directories should have a trailing slash.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path. Directories should have a trailing slash.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path. Directories should have a trailing slash.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="toPath"/> is <c>null</c>.</exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
                return toPath;
            if (string.IsNullOrEmpty(toPath))
                throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
            Uri toUri = new Uri(AppendDirectorySeparatorChar(toPath));

            if (fromUri.Scheme != toUri.Scheme)
                return toPath;

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            
            return relativePath;
        }

        public static bool IsBelowPath(string childPath, string parentPath)
        {
            string modelFolderPath = Path.GetDirectoryName(parentPath);
            if (childPath.StartsWith(modelFolderPath))
            {
                return true;
            }

            return false;
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!Path.HasExtension(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }
    }
}

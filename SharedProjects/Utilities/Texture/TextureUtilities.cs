using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Reflection;
using BabylonExport.Entities;

namespace Utilities
{
    static class TextureUtilities
    {
        public static List<string> validGltfFormats = new List<string>(new string[] { "png", "jpg", "jpeg" });
        public static List<string> invalidGltfFormats = new List<string>(new string[] { "dds", "tga", "tif", "tiff", "bmp", "gif" });
        public static readonly IEnumerable<TextureOperation> NoTransforms = Enumerable.Empty<TextureOperation>();


        public static string EncodeName(this IEnumerable<TextureOperation> operations)
        {
            // use System.Text namespace to avoid conflict with System.Drawing.Imaging namespace
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var o in operations) sb.Append(o.Name);
            return sb.ToString();
        }

        public static void TransformTexture(string sourcePath, IEnumerable<TextureOperation> transforms, string destPath, long imageQuality, ILoggingProvider logger)
        {
            _copyTexture(sourcePath, transforms, destPath, imageQuality, validGltfFormats, invalidGltfFormats, logger);
        }

        public static Bitmap TransformTextureInPlace(this Bitmap source, IEnumerable<TextureOperation> transforms)
        {
            if (transforms.Count() != 0)
            {
                // Lock the bitmap's bits.  
                Rectangle rect = new Rectangle(0, 0, source.Width, source.Height);
                BitmapData data = source.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, source.PixelFormat);
                // Get the address of the first line.
                IntPtr ptr = data.Scan0;
                // Declare an array to hold the bytes of the bitmap.
                int bytes = Math.Abs(data.Stride) * data.Height;
                byte[] values = new byte[bytes];
                // Copy the values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);

                foreach( var op in transforms)
                {
                    op.Apply(values, data);
                }
                // Copy the values back to the bitmap
                System.Runtime.InteropServices.Marshal.Copy(values, 0, ptr, bytes);

                // Unlock the bits.
                source.UnlockBits(data);
            }
            return source;
        }

        public static bool GetMinimalBitmapDimensions(out int width, out int height, params Bitmap[] bitmaps)
        {
            var haveSameDimensions = true;

            var bitmapsNoNull = ((new List<Bitmap>(bitmaps)).FindAll(bitmap => bitmap != null)).ToArray();
            if (bitmapsNoNull.Length > 0)
            {
                // Init with first element
                width = bitmapsNoNull[0].Width;
                height = bitmapsNoNull[0].Height;

                // Update with others
                for (int i = 1; i < bitmapsNoNull.Length; i++)
                {
                    var bitmap = bitmapsNoNull[i];
                    if (width != bitmap.Width || height != bitmap.Height)
                    {
                        haveSameDimensions = false;
                    }
                    width = Math.Min(width, bitmap.Width);
                    height = Math.Min(height, bitmap.Height);
                }
            }
            else
            {
                width = 0;
                height = 0;
            }

            return haveSameDimensions;
        }

        public static Bitmap LoadTexture(string absolutePath, ILoggingProvider logger)
        {
            if (File.Exists(absolutePath))
            {
                try
                {
                    switch (Path.GetExtension(absolutePath).ToLower())
                    {
#if !DONT_USE_GDIMAGE_LIBRARY
                        case ".dds":
                                  // External library GDImageLibrary.dll + TQ.Texture.dll
                                  return GDImageLibrary._DDS.LoadImage(absolutePath);
#endif
#if !DONT_USE_PALOMA_TARGAIMAGE
                        case ".tga":
                            // External library TargaImage.dll
                            return Paloma.TargaImage.LoadTargaImage(absolutePath);
#endif
                        case ".bmp":
                        case ".gif":
                        case ".jpg":
                        case ".jpeg":
                        case ".png":
                        case ".tif":
                        case ".tiff":
                            return new Bitmap(absolutePath);
                        default:
                            logger.RaiseError(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(absolutePath)), 3);
                            return null;
                    }
                }
                catch (Exception e)
                {
                    logger.RaiseError(string.Format("Failed to load texture {0}: {1}", Path.GetFileName(absolutePath), e.Message), 3);
                    return null;
                }
            }
            else
            {
                logger.RaiseError(string.Format("Texture {0} not found.", absolutePath), 3);
                return null;
            }
        }

        // https://dejanstojanovic.net/aspnet/2014/june/getting-systemdrawingimagingimageformat-from-a-string/
        public static ImageFormat GetImageFormat(string extension)
        {
            ImageFormat result = null;
            if (extension == null || extension == "")
            {
                return result;
            }

            if (extension[0] == '.' || extension[0] == ',')
            {
                extension = extension.Substring(1);
            }

            if (extension == "jpg")
            {
                extension = "jpeg";
            }

            PropertyInfo prop = typeof(ImageFormat).GetProperties().Where(p => p.Name.Equals(extension, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (prop != null)
            {
                result = prop.GetValue(prop) as ImageFormat;
            }

            return result;
        }

        public static void CopyTexture(string sourcePath, string destPath, long imageQuality, ILoggingProvider logger)
        {
            _copyTexture(sourcePath, NoTransforms, destPath, imageQuality, validGltfFormats, invalidGltfFormats, logger);
        }

        public static void CopyTexture(string sourcePath, IEnumerable<TextureOperation> transforms, string destPath, long imageQuality, ILoggingProvider logger)
        {
            _copyTexture(sourcePath, transforms, destPath, imageQuality, validGltfFormats, invalidGltfFormats, logger);
        }

        public static Bitmap GetBitmap(string sourcePath, IEnumerable<TextureOperation> transforms, ILoggingProvider logger)
        {
            string imageFormat = Path.GetExtension(sourcePath).Substring(1).ToLower(); // remove the dot
            return _convertToBitmap(sourcePath, transforms, imageFormat, logger);
        }


        public static string GetValidImageFormat(string extension)
        {
            return _getValidImageFormat(extension, validGltfFormats, invalidGltfFormats);
        }

        public static bool ExtensionIsValidGLTFTexture(string _extension)
        {
            if (_extension.StartsWith("."))
            {
                _extension = _extension.Replace(".", "");
            }

            if (validGltfFormats.Contains(_extension))
            {
                return true;
            }

            return false;
        }

        private static string _getValidImageFormat(string extension, List<string> validFormats, List<string> invalidFormats)
        {
            var imageFormat = extension.Substring(1).ToLower(); // remove the dot

            if (validFormats.Contains(imageFormat))
            {
                return imageFormat;
            }
            else if (invalidFormats.Contains(imageFormat))
            {
                switch (imageFormat)
                {
                    case "dds":
                    case "tga":
                    case "tif":
                    case "tiff":
                    case "gif":
                    case "png":
                        return "png";
                    case "bmp":
                    case "jpg":
                    case "jpeg":
                        return "jpg";
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }


        public static string GetPreferredFormat(string path, bool hasAlpha, TextureFormatExportPolicy policy = TextureFormatExportPolicy.CONSERVATIV)
        {
            if (hasAlpha) return "png";

            switch (policy)
            {
                case TextureFormatExportPolicy.CONSERVATIV:
                    {
                        if (!string.IsNullOrEmpty(path))
                        {
                            return GetValidImageFormat(path);
                        }
                        return "png";
                    }
                case TextureFormatExportPolicy.SIZE:
                    {
                        return "jpg";
                    }
                case TextureFormatExportPolicy.QUALITY:
                default:
                    {
                        return "png";
                    }
            }
        }
        public static string GetPreferredFormat(IEnumerable<string> paths, bool hasAlpha, TextureFormatExportPolicy policy = TextureFormatExportPolicy.QUALITY)
        {
            if (hasAlpha) return "png";

            switch (policy)
            {
                case TextureFormatExportPolicy.CONSERVATIV:
                    {
                        if (paths != null)
                        {
                            var exts = paths.Where(p => !string.IsNullOrEmpty(p)).Select(p => Path.GetExtension(p)).Select(e=> GetValidImageFormat(e));
                            return exts.Any(e => e.Equals("jpg")) ? "jpg" : "png";
                        }
                        return "png";
                    }
                case TextureFormatExportPolicy.SIZE:
                    {
                        return "jpg";
                    }
                case TextureFormatExportPolicy.QUALITY:
                default:
                    {
                        return "png";
                    }
            }
        }

        /// <summary>
        /// Copy image from source to dest.
        /// The copy process may include a conversion to another image format:
        /// - a source with a valid format is copied directly
        /// - a source with an invalid format is converted to png or jpg before being copied
        /// - a source with neither a valid nor an invalid format raises a warning and is not copied
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="validFormats"></param>
        /// <param name="invalidFormats"></param>
        private static void _copyTexture(string sourcePath, IEnumerable<TextureOperation> transforms, string destPath, long imageQuality, List<string> validFormats, List<string> invalidFormats, ILoggingProvider logger)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    string imageFormat = Path.GetExtension(sourcePath).Substring(1).ToLower(); // remove the dot

                    if (validFormats.Contains(imageFormat))
                    {
                        if (transforms.Count() == 0)
                        {
                            if (sourcePath != destPath)
                            {
                                File.Copy(sourcePath, destPath, true);
                            }
                        }
                        else
                        {
                            _convertToBitmapAndSave(sourcePath, transforms, destPath, imageFormat, imageQuality, logger);
                        }
                    }
                    else if (invalidFormats.Contains(imageFormat))
                    {
                        _convertToBitmapAndSave(sourcePath, transforms, destPath, imageFormat, imageQuality, logger);
                    }
                    else
                    {
                        logger.RaiseError(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), 3);
                    }
                }
                else logger.RaiseError(string.Format("Texture not found: {0}", sourcePath), 3);
            }
            catch (Exception c)
            {
                logger.RaiseError(string.Format("Exporting texture {0} failed: {1}", sourcePath, c.ToString()), 3);
            }
        }

        /// <summary>
        /// Load image from source to a bitmap and save it to dest as png or jpg.
        /// Loading process to a bitmap depends on extension.
        /// Saved image format depends on alpha presence.
        /// png and jpg are copied directly.
        /// Unsupported format raise a warning and are not copied.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="imageFormat"></param>
        private static void _convertToBitmapAndSave(string sourcePath, IEnumerable<TextureOperation> transforms, string destPath, string imageFormat, long imageQuality, ILoggingProvider logger)
        {
            Bitmap bitmap;
            switch (imageFormat)
            {
#if !DONT_USE_GDIMAGE_LIBRARY
            case "dds":
                    // External libraries GDImageLibrary.dll + TQ.Texture.dll
                    try
                    {
                        bitmap = GDImageLibrary._DDS.LoadImage(sourcePath);

                        SaveBitmap(bitmap, destPath, ImageFormat.Png, imageQuality, logger);
                    }
                    catch (Exception e)
                    {
                        logger.RaiseError(string.Format("Failed to convert texture {0} to png: {1}", Path.GetFileName(sourcePath), e.Message), 3);
                    }
                    break;
#endif
#if !DONT_USE_PALOMA_TARGAIMAGE
                case "tga":
                    {
                        // External library TargaImage.dll
                        try
                        {
                            bitmap = Paloma.TargaImage.LoadTargaImage(sourcePath);
                            if (transforms.Count() != 0) bitmap.TransformTextureInPlace(transforms);
                            SaveBitmap(bitmap, destPath, ImageFormat.Png, imageQuality, logger);
                        }
                        catch (Exception e)
                        {
                            logger.RaiseError(string.Format("Failed to convert texture {0} to png: {1}", Path.GetFileName(sourcePath), e.Message), 3);
                        }
                        break;
                    }
#endif
                case "bmp":
                    {
                        bitmap = new Bitmap(sourcePath);
                        if (transforms.Count() != 0) bitmap.TransformTextureInPlace(transforms);
                        SaveBitmap(bitmap, destPath, ImageFormat.Jpeg, imageQuality, logger); // no alpha
                        break;
                    }
                case "jpeg":
                    {
                        if (transforms.Count() == 0)
                        {
                            File.Copy(sourcePath, destPath, true);
                            break;
                        }
                        bitmap = new Bitmap(sourcePath);
                        if (transforms.Count() != 0) bitmap.TransformTextureInPlace(transforms);
                        SaveBitmap(bitmap, destPath, ImageFormat.Jpeg, imageQuality, logger);
                        break;
                    }
                case "png":
                    {
                        if (transforms.Count() == 0)
                        {
                            File.Copy(sourcePath, destPath, true);
                            break;
                        }
                        bitmap = new Bitmap(sourcePath);
                        if (transforms.Count() != 0) bitmap.TransformTextureInPlace(transforms);
                        SaveBitmap(bitmap, destPath, ImageFormat.Png, imageQuality, logger);
                        break;
                    }
                case "tif":
                case "tiff":
                case "gif":
                    {
                        bitmap = new Bitmap(sourcePath);
                        if (transforms.Count() != 0) bitmap.TransformTextureInPlace(transforms);
                        SaveBitmap(bitmap, destPath, ImageFormat.Png, imageQuality, logger);
                        break;
                    }
                default:
                    logger.RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), 3);
                    break;
            }
        }

        public static Bitmap _convertToBitmap(string sourcePath, IEnumerable<TextureOperation> transforms, string imageFormat, ILoggingProvider logger)
        {
            switch (imageFormat)
            {
#if !DONT_USE_GDIMAGE_LIBRARY
                case "dds":
                    // External libraries GDImageLibrary.dll + TQ.Texture.dll
                    try
                    {
                        return GDImageLibrary._DDS.LoadImage(sourcePath);
                    }
                    catch (Exception e)
                    {
                        logger.RaiseError(string.Format("Failed to convert texture {0} to png: {1}", Path.GetFileName(sourcePath), e.Message), 3);
                    }
                    break;
#endif
#if !DONT_USE_PALOMA_TARGAIMAGE
                case "tga":
                    {
                        // External library TargaImage.dll
                        try
                        {
                            return Paloma.TargaImage.LoadTargaImage(sourcePath).TransformTextureInPlace(transforms);
                        }
                        catch (Exception e)
                        {
                            logger.RaiseError(string.Format("Failed to convert texture {0} to png: {1}", Path.GetFileName(sourcePath), e.Message), 3);
                        }
                        break;
                    }
#endif
                case "bmp":
                case "jpeg":
                case "png":
                case "tif":
                case "tiff":
                case "gif":
                    {
                        return new Bitmap(sourcePath).TransformTextureInPlace(transforms);
                    }
                default:
                    logger.RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), 3);
                    break;
            }
            return null;
       }

        public static void SaveBitmap(Bitmap bitmap, string path, ImageFormat imageFormat, long imageQuality, ILoggingProvider logger)
        {
            SaveBitmap(bitmap, Path.GetDirectoryName(path), Path.GetFileName(path), imageFormat, imageQuality, logger);
        }

        public static void SaveBitmap(Bitmap bitmap, string directoryName, string fileName, ImageFormat imageFormat, long imageQuality, ILoggingProvider logger)
        {
            List<char> invalidCharsInString = GetInvalidChars(directoryName, Path.GetInvalidPathChars());
            if (invalidCharsInString.Count > 0)
            {
                logger.RaiseError($"Failed to save bitmap: directory name '{directoryName}' contains invalid character{(invalidCharsInString.Count > 1 ? "s" : "")} {invalidCharsInString.ToArray().ToString(false)}", 3);
                return;
            }
            invalidCharsInString = GetInvalidChars(fileName, Path.GetInvalidFileNameChars());
            if (invalidCharsInString.Count > 0)
            {
                logger.RaiseError($"Failed to save bitmap: file name '{fileName}' contains invalid character{(invalidCharsInString.Count > 1 ? "s" : "")} {invalidCharsInString.ToArray().ToString(false)}", 3);
                return;
            }

            string path = Path.Combine(directoryName, fileName);
            using (FileStream fs = File.Open(path, FileMode.Create))
            {

                SaveBitmap(fs, bitmap, imageFormat, imageQuality);
            }
        }
        
        public static void SaveBitmap(Stream output, Bitmap bitmap, ImageFormat imageFormat, long imageQuality)
        {
            ImageCodecInfo encoder = GetEncoder(imageFormat);

            if (encoder != null)
            {
                // Create an Encoder object based on the GUID for the Quality parameter category
                EncoderParameters encoderParameters = new EncoderParameters(1);
                EncoderParameter encoderQualityParameter = new EncoderParameter(Encoder.Quality, imageQuality);
                encoderParameters.Param[0] = encoderQualityParameter;

                bitmap.Save(output, encoder, encoderParameters);
            }
            else
            {
                bitmap.Save(output, imageFormat);
            }
        }


        private static List<char> GetInvalidChars(string s, char[] invalidChars)
        {
            List<char> invalidCharsInString = new List<char>();
            foreach (char ch in invalidChars)
            {
                int indexInvalidChar = s.IndexOf(ch);
                if (indexInvalidChar != -1)
                {
                    invalidCharsInString.Add(s[indexInvalidChar]);
                }
            }
            return invalidCharsInString;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public static string ColorToStringName(Color color)
        {
            return "" + color.R + color.G + color.B + color.A;
        }

        public static string ColorToStringName(float[] color)
        {
            return "" + (int)(color[0] * 255) + (int)(color[1] * 255) + (int)(color[2] * 255);
        }
    }
}

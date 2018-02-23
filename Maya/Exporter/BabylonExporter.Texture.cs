using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Maya2Babylon
{
    partial class BabylonExporter
    {
        private static List<string> validFormats = new List<string>(new string[] { "png", "jpg", "jpeg", "tga", "bmp", "gif" });
        private static List<string> invalidFormats = new List<string>(new string[] { "dds", "tif", "tiff" });

        private int logRankTexture = 2;

        public BabylonTexture ExportTexture(MFnDependencyNode materialDependencyNode, string plugName, BabylonScene babylonScene, bool allowCube = false, bool forceAlpha = false, bool forceSpherical = false, float amount = 1.0f)
        {
            logRankTexture = 2;
            return _ExportTexture(materialDependencyNode, plugName, babylonScene, allowCube, forceAlpha, forceSpherical, amount);
        }

        private BabylonTexture _ExportTexture(MFnDependencyNode materialDependencyNode, string plugName, BabylonScene babylonScene, bool allowCube = false, bool forceAlpha = false, bool forceSpherical = false, float amount = 1.0f)
        {
            if (!materialDependencyNode.hasAttribute(plugName))
            {
                RaiseError("Unknown attribute " + materialDependencyNode.name + "." + plugName, logRankTexture);
                return null;
            }

            MFnDependencyNode textureDependencyNode = getTextureDependencyNode(materialDependencyNode, plugName);

            if (textureDependencyNode == null)
            {
                return null;
            }

            Print(textureDependencyNode, logRankTexture, "Print _ExportTexture textureDependencyNode");

            // Retreive texture file path
            string sourcePath = getSourcePathFromFileTexture(textureDependencyNode);
            if (sourcePath == null)
            {
                RaiseError("Texture path is not a valid string.", logRankTexture + 1);
                return null;
            }
            if (sourcePath == "")
            {
                RaiseError("Texture path is missing.", logRankTexture + 1);
                return null;
            }

            // Check format
            var validImageFormat = GetValidImageFormat(Path.GetExtension(sourcePath));
            if (validImageFormat == null)
            {
                // Image format is not supported by the exporter
                RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), logRankTexture + 1);
                return null;
            }
            RaiseVerbose("validImageFormat="+ validImageFormat, logRankTexture + 1);

            var babylonTexture = new BabylonTexture
            {
                name = Path.GetFileNameWithoutExtension(sourcePath) + "." + validImageFormat
            };

            // Level
            babylonTexture.level = amount;

            // Alpha
            // TODO - Get alpha from both RGB and A
            // or assume texture is premultiplied and get alpha from RGB or A only
            babylonTexture.hasAlpha = forceAlpha;
            babylonTexture.getAlphaFromRGB = false;

            // UVs
            _exportUV(textureDependencyNode, babylonTexture, forceSpherical);

            // TODO - Animations

            // Copy texture to output
            if (isBabylonExported)
            {
                var destPath = Path.Combine(babylonScene.OutputPath, babylonTexture.name);
                CopyTexture(sourcePath, destPath);

                // Is cube
                _exportIsCube(Path.Combine(babylonScene.OutputPath, babylonTexture.name), babylonTexture, allowCube);
            }
            else
            {
                babylonTexture.originalPath = sourcePath;
                babylonTexture.isCube = false;
            }

            return babylonTexture;
        }

        private BabylonTexture ExportBaseColorAlphaTexture(MFnDependencyNode materialDependencyNode, bool useColorMap, bool useOpacityMap, float[] baseColor, float alpha, BabylonScene babylonScene)
        {
            MFnDependencyNode textureDependencyNode = getTextureDependencyNode(materialDependencyNode, "TEX_color_map");

            if (textureDependencyNode == null)
            {
                return null;
            }

            // Prints
            Print(textureDependencyNode, logRankTexture, "Print ExportBaseColorAlphaTexture textureDependencyNode");

            // Retreive texture file path
            string sourcePath = getSourcePathFromFileTexture(textureDependencyNode);
            if (sourcePath == null)
            {
                return null;
            }
            if (sourcePath == "")
            {
                RaiseError("Texture path is missing.", logRankTexture + 1);
                return null;
            }

            // Check format
            string extension = Path.GetExtension(sourcePath);
            var validImageFormat = GetValidImageFormat(Path.GetExtension(sourcePath));
            if (validImageFormat == null)
            {
                // Image format is not supported by the exporter
                RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), logRankTexture + 1);
                return null;
            }
            RaiseVerbose("validImageFormat=" + validImageFormat, logRankTexture + 1);

            extension = extension.Substring(1).ToLower(); // remove the dot
            if (useOpacityMap)
            {
                List<string> alphaFormats = new List<string>(new string[] { "png", "tga", "gif" });
                if (!alphaFormats.Contains(extension))
                {
                    validImageFormat = "png";
                }
            }
            else
            {
                List<string> nonAlphaFormats = new List<string>(new string[] { "jpg", "jpeg", "bmp" });
                if (!nonAlphaFormats.Contains(extension))
                {
                    validImageFormat = "jpg";
                }
            }

            var babylonTexture = new BabylonTexture
            {
                name = Path.GetFileNameWithoutExtension(sourcePath) + "." + validImageFormat
            };

            // Level
            babylonTexture.level = 1.0f;

            // UVs
            _exportUV(textureDependencyNode, babylonTexture);

            // Is cube
            _exportIsCube(sourcePath, babylonTexture, false);


            // --- Merge baseColor and alpha maps ---

            if (!File.Exists(sourcePath))
            {
                return null;
            }

            // Alpha
            babylonTexture.hasAlpha = useOpacityMap;
            babylonTexture.getAlphaFromRGB = false;

            if (CopyTexturesToOutput)
            {
                // Load bitmaps
                var baseColorBitmap = LoadTexture(sourcePath);

                // Retreive dimensions
                int width = baseColorBitmap.Width;
                int height = baseColorBitmap.Height;

                // Create baseColor+alpha map
                var _baseColor = Color.FromArgb(
                    (int)(baseColor[0] * 255),
                    (int)(baseColor[1] * 255),
                    (int)(baseColor[2] * 255));
                var _alpha = (int)(alpha * 255);
                Bitmap baseColorAlphaBitmap = new Bitmap(width, height);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        var baseColorAtPixel = baseColorBitmap.GetPixel(x, y);

                        var __baseColor = useColorMap ? baseColorAtPixel : _baseColor;
                        var __alpha = useOpacityMap ? baseColorAtPixel.A : _alpha;
                        Color baseColorAlpha = Color.FromArgb(__alpha, __baseColor);
                        baseColorAlphaBitmap.SetPixel(x, y, baseColorAlpha);
                    }
                }

                // Write bitmap
                if (isBabylonExported)
                {
                    var absolutePath = Path.Combine(babylonScene.OutputPath, babylonTexture.name);
                    RaiseMessage($"Texture | write image '{babylonTexture.name}'", 2);
                    var imageFormat = useOpacityMap ? System.Drawing.Imaging.ImageFormat.Png : System.Drawing.Imaging.ImageFormat.Jpeg;
                    baseColorAlphaBitmap.Save(absolutePath, imageFormat);
                }
                else
                {
                    // Store created bitmap for further use in gltf export
                    babylonTexture.bitmap = baseColorAlphaBitmap;
                }
            }

            return babylonTexture;
        }

        private BabylonTexture ExportMetallicRoughnessTexture(MFnDependencyNode materialDependencyNode, bool useMetallicMap, bool useRoughnessMap, BabylonScene babylonScene, string materialName)
        {
            MFnDependencyNode metallicTextureDependencyNode = useMetallicMap ? getTextureDependencyNode(materialDependencyNode, "TEX_metallic_map") : null;
            MFnDependencyNode roughnessTextureDependencyNode = useRoughnessMap ? getTextureDependencyNode(materialDependencyNode, "TEX_roughness_map") : null;

            // Prints
            if (metallicTextureDependencyNode != null)
            {
                Print(metallicTextureDependencyNode, logRankTexture, "Print ExportMetallicRoughnessTexture metallicTextureDependencyNode");
            }
            if (roughnessTextureDependencyNode != null)
            {
                Print(roughnessTextureDependencyNode, logRankTexture, "Print ExportMetallicRoughnessTexture roughnessTextureDependencyNode");
            }

            // Use one as a reference for UVs parameters
            var textureDependencyNode = metallicTextureDependencyNode != null ? metallicTextureDependencyNode : roughnessTextureDependencyNode;
            if (textureDependencyNode == null)
            {
                return null;
            }

            var babylonTexture = new BabylonTexture
            {
                name = materialName + "_metallicRoughness" + ".jpg" // TODO - unsafe name, may conflict with another texture name
            };

            // Level
            babylonTexture.level = 1.0f;

            // No alpha
            babylonTexture.hasAlpha = false;
            babylonTexture.getAlphaFromRGB = false;

            // UVs
            _exportUV(textureDependencyNode, babylonTexture);

            // Is cube
            string sourcePath = getSourcePathFromFileTexture(textureDependencyNode);
            if (sourcePath == null)
            {
                return null;
            }
            if (sourcePath == "")
            {
                RaiseError("Texture path is missing.", logRankTexture + 1);
                return null;
            }
            _exportIsCube(sourcePath, babylonTexture, false);


            // --- Merge metallic and roughness maps ---

            if (CopyTexturesToOutput)
            {
                // Load bitmaps
                var metallicBitmap = LoadTexture(metallicTextureDependencyNode);
                var roughnessBitmap = LoadTexture(roughnessTextureDependencyNode);

                // Retreive dimensions
                int width = 0;
                int height = 0;
                var haveSameDimensions = _getMinimalBitmapDimensions(out width, out height, metallicBitmap, roughnessBitmap);
                if (!haveSameDimensions)
                {
                    RaiseError("Metallic and roughness maps should have same dimensions", 2);
                }

                // Create metallic+roughness map
                Bitmap metallicRoughnessBitmap = new Bitmap(width, height);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        var _metallic = metallicBitmap != null ? metallicBitmap.GetPixel(x, y).B : 255.0f;
                        var _roughness = roughnessBitmap != null ? roughnessBitmap.GetPixel(x, y).G : 255.0f;

                        // The metalness values are sampled from the B channel.
                        // The roughness values are sampled from the G channel.
                        // These values are linear. If other channels are present (R or A), they are ignored for metallic-roughness calculations.
                        Color colorMetallicRoughness = Color.FromArgb(
                            0,
                            (int)_roughness,
                            (int)_metallic
                        );
                        metallicRoughnessBitmap.SetPixel(x, y, colorMetallicRoughness);
                    }
                }

                // Write bitmap
                if (isBabylonExported)
                {
                    var absolutePath = Path.Combine(babylonScene.OutputPath, babylonTexture.name);
                    RaiseMessage($"Texture | write image '{babylonTexture.name}'", 2);
                    metallicRoughnessBitmap.Save(absolutePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                else
                {
                    // Store created bitmap for further use in gltf export
                    babylonTexture.bitmap = metallicRoughnessBitmap;
                }
            }

            return babylonTexture;
        }

        private void _exportUV(MFnDependencyNode textureDependencyNode, BabylonTexture babylonTexture, bool forceSpherical = false)
        {
            // TODO - coordinatesMode
            babylonTexture.coordinatesMode = forceSpherical ? BabylonTexture.CoordinatesMode.SPHERICAL_MODE : BabylonTexture.CoordinatesMode.EXPLICIT_MODE;

            // TODO - get UV set from uvChooser
            //babylonTexture.coordinatesIndex = uvGen.MapChannel - 1;
            //if (uvGen.MapChannel > 2)
            //{
            //    RaiseWarning(string.Format("Unsupported map channel, Only channel 1 and 2 are supported."), logRank + 1);
            //}

            // For more information about UV
            // see http://help.autodesk.com/view/MAYAUL/2018/ENU/?guid=GUID-94070C7E-C550-42FD-AFC9-FBE82B173B1D
            babylonTexture.uOffset = textureDependencyNode.findPlug("offsetU").asFloatProperty;
            babylonTexture.vOffset = textureDependencyNode.findPlug("offsetV").asFloatProperty;
            
            babylonTexture.uScale = textureDependencyNode.findPlug("repeatU").asFloatProperty;
            babylonTexture.vScale = textureDependencyNode.findPlug("repeatV").asFloatProperty;
            
            if (Path.GetExtension(babylonTexture.name).ToLower() == ".dds")
            {
                babylonTexture.vScale *= -1; // Need to invert Y-axis for DDS texture
            }

            // Maya only has a W rotation
            babylonTexture.uAng = 0;
            babylonTexture.vAng = 0;
            babylonTexture.wAng = textureDependencyNode.findPlug("rotateFrame").asFloatProperty;

            // Adress mode U
            // TODO - What is adress mode when both wrap and mirror?
            if (textureDependencyNode.findPlug("mirrorU").asBoolProperty)
            {
                babylonTexture.wrapU = BabylonTexture.AddressMode.MIRROR_ADDRESSMODE;
            }
            else if (textureDependencyNode.findPlug("wrapU").asBoolProperty)
            {
                babylonTexture.wrapU = BabylonTexture.AddressMode.WRAP_ADDRESSMODE;
            }
            else
            {
                // TODO - What is adress mode when not wrap nor mirror?
                babylonTexture.wrapU = BabylonTexture.AddressMode.CLAMP_ADDRESSMODE;
            }

            // Adress mode V
            // TODO - What is adress mode when both wrap and mirror?
            if (textureDependencyNode.findPlug("mirrorV").asBoolProperty)
            {
                babylonTexture.wrapV = BabylonTexture.AddressMode.MIRROR_ADDRESSMODE;
            }
            else if (textureDependencyNode.findPlug("wrapV").asBoolProperty)
            {
                babylonTexture.wrapV = BabylonTexture.AddressMode.WRAP_ADDRESSMODE;
            }
            else
            {
                // TODO - What is adress mode when not wrap nor mirror?
                babylonTexture.wrapV = BabylonTexture.AddressMode.CLAMP_ADDRESSMODE;
            }
        }

        private void _exportIsCube(string absolutePath, BabylonTexture babylonTexture, bool allowCube)
        {
            if (Path.GetExtension(absolutePath).ToLower() != ".dds")
            {
                babylonTexture.isCube = false;
            }
            else
            {
                try
                {
                    if (File.Exists(absolutePath))
                    {
                        babylonTexture.isCube = _isTextureCube(absolutePath);
                    }
                    else
                    {
                        RaiseWarning(string.Format("Texture {0} not found.", absolutePath), logRankTexture + 1);
                    }

                }
                catch
                {
                    // silently fails
                }

                if (babylonTexture.isCube && !allowCube)
                {
                    RaiseWarning(string.Format("Cube texture are only supported for reflection channel"), logRankTexture + 1);
                }
            }
        }

        private bool _isTextureCube(string filepath)
        {
            try
            {
                var data = File.ReadAllBytes(filepath);
                var intArray = new int[data.Length / 4];

                Buffer.BlockCopy(data, 0, intArray, 0, intArray.Length * 4);


                int width = intArray[4];
                int height = intArray[3];
                int mipmapsCount = intArray[7];

                if ((width >> (mipmapsCount - 1)) > 1)
                {
                    var expected = 1;
                    var currentSize = Math.Max(width, height);

                    while (currentSize > 1)
                    {
                        currentSize = currentSize >> 1;
                        expected++;
                    }

                    RaiseWarning(string.Format("Mipmaps chain is not complete: {0} maps instead of {1} (based on texture max size: {2})", mipmapsCount, expected, width), logRankTexture + 1);
                    RaiseWarning(string.Format("You must generate a complete mipmaps chain for .dds)"), logRankTexture + 1);
                    RaiseWarning(string.Format("Mipmaps will be disabled for this texture. If you want automatic texture generation you cannot use a .dds)"), logRankTexture + 1);
                }

                bool isCube = (intArray[28] & 0x200) == 0x200;

                return isCube;
            }
            catch
            {
                return false;
            }
        }

        // -------------------------
        // --------- Utils ---------
        // -------------------------

        private string getSourcePathFromFileTexture(MFnDependencyNode textureDependencyNode)
        {
            MObject sourceObject = textureDependencyNode.objectProperty;

            // Retreive texture file path
            if (!sourceObject.hasFn(MFn.Type.kFileTexture))
            {
                RaiseError("Only file texture is supported.", logRankTexture + 1);
                return null;
            }
            MPlug fileTextureNamePlug = textureDependencyNode.findPlug("fileTextureName");
            if (fileTextureNamePlug == null || fileTextureNamePlug.isNull)
            {
                RaiseError("Texture path is missing.", logRankTexture + 1);
                return null;
            }
            string sourcePath = fileTextureNamePlug.asStringProperty;
            return sourcePath;
        }

        private MFnDependencyNode getTextureDependencyNode(MFnDependencyNode materialDependencyNode, string plugName)
        {
            MPlug mPlug = materialDependencyNode.findPlug(plugName);

            if (mPlug == null || mPlug.isNull || !mPlug.isConnected)
            {
                return null;
            }

            MObject sourceObject = mPlug.source.node;
            MFnDependencyNode textureDependencyNode = new MFnDependencyNode(sourceObject);

            RaiseMessage(materialDependencyNode.name + "." + plugName, logRankTexture);

            // Bump texture uses an intermediate node
            if (sourceObject.hasFn(MFn.Type.kBump))
            {
                Print(textureDependencyNode, logRankTexture, "Print bump node");
                logRankTexture++;
                return getTextureDependencyNode(textureDependencyNode, "bumpValue");
            }

            // If a reverse node is used as an intermediate node
            if (sourceObject.hasFn(MFn.Type.kReverse))
            {
                // TODO - reverse?
                logRankTexture++;
                return getTextureDependencyNode(textureDependencyNode, "input");
            }

            return textureDependencyNode;
        }

        private bool _getMinimalBitmapDimensions(out int width, out int height, params Bitmap[] bitmaps)
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

        private Bitmap LoadTexture(MFnDependencyNode textureDependencyNode)
        {
            string sourcePath = getSourcePathFromFileTexture(textureDependencyNode);
            if (sourcePath == null)
            {
                return null;
            }
            if (sourcePath == "")
            {
                RaiseError("Texture path is missing for node " + textureDependencyNode.name + ".", logRankTexture + 1);
                return null;
            }
            return LoadTexture(sourcePath);
        }

        private Bitmap LoadTexture(string absolutePath)
        {
            if (File.Exists(absolutePath))
            {
                switch (Path.GetExtension(absolutePath))
                {
                    case ".dds":
                        // External library GDImageLibrary.dll + TQ.Texture.dll
                        return GDImageLibrary._DDS.LoadImage(absolutePath);
                    case ".tga":
                        // External library TargaImage.dll
                        return Paloma.TargaImage.LoadTargaImage(absolutePath);
                    case ".bmp":
                    case ".gif":
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".tif":
                    case ".tiff":
                        return new Bitmap(absolutePath);
                    default:
                        RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(absolutePath)), 2);
                        return null;
                }
            }
            else
            {
                RaiseWarning(string.Format("Texture {0} not found.", Path.GetFileName(absolutePath)), 2);
                return null;
            }
        }

        private void CopyTexture(string sourcePath, string destPath)
        {
            _copyTexture(sourcePath, destPath, validFormats, invalidFormats);
        }

        private string GetValidImageFormat(string extension)
        {
            return _getValidImageFormat(extension, validFormats, invalidFormats);
        }

        private string _getValidImageFormat(string extension, List<string> validFormats, List<string> invalidFormats)
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
        private void _copyTexture(string sourcePath, string destPath, List<string> validFormats, List<string> invalidFormats)
        {
            if (CopyTexturesToOutput)
            {
                try
                {
                    if (File.Exists(sourcePath))
                    {
                        string imageFormat = Path.GetExtension(sourcePath).Substring(1).ToLower(); // remove the dot

                        if (validFormats.Contains(imageFormat))
                        {
                            File.Copy(sourcePath, destPath, true);
                        }
                        else if (invalidFormats.Contains(imageFormat))
                        {
                            _convertToBitmapAndSave(sourcePath, destPath, imageFormat);
                        }
                        else
                        {
                            RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), logRankTexture + 1);
                        }
                    }
                }
                catch
                {
                    // silently fails
                }
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
        private void _convertToBitmapAndSave(string sourcePath, string destPath, string imageFormat)
        {
            Bitmap bitmap;
            switch (imageFormat)
            {
                case "dds":
                    // External libraries GDImageLibrary.dll + TQ.Texture.dll
                    bitmap = GDImageLibrary._DDS.LoadImage(sourcePath);
                    bitmap.Save(destPath, System.Drawing.Imaging.ImageFormat.Png);
                    break;
                case "tga":
                    // External library TargaImage.dll
                    bitmap = Paloma.TargaImage.LoadTargaImage(sourcePath);
                    bitmap.Save(destPath, System.Drawing.Imaging.ImageFormat.Png);
                    break;
                case "bmp":
                    bitmap = new Bitmap(sourcePath);
                    bitmap.Save(destPath, System.Drawing.Imaging.ImageFormat.Jpeg); // no alpha
                    break;
                case "tif":
                case "tiff":
                case "gif":
                    bitmap = new Bitmap(sourcePath);
                    bitmap.Save(destPath, System.Drawing.Imaging.ImageFormat.Png);
                    break;
                case "jpeg":
                case "png":
                    File.Copy(sourcePath, destPath, true);
                    break;
                default:
                    RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), logRankTexture + 1);
                    break;
            }
        }
    }
}

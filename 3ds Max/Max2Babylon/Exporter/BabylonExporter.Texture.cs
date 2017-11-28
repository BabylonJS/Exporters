using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Max;
using BabylonExport.Entities;
using System.Drawing;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        private static List<string> validFormats = new List<string>(new string[] { "png", "jpg", "jpeg", "tga", "bmp", "gif" });
        private static List<string> invalidFormats = new List<string>(new string[] { "dds", "tif", "tiff" });

        // -------------------------------
        // --- "public" export methods ---
        // -------------------------------

        private BabylonTexture ExportTexture(IStdMat2 stdMat, int index, out BabylonFresnelParameters fresnelParameters, BabylonScene babylonScene, bool allowCube = false, bool forceAlpha = false)
        {
            fresnelParameters = null;

            if (!stdMat.MapEnabled(index))
            {
                return null;
            }

            var texMap = stdMat.GetSubTexmap(index);

            if (texMap == null)
            {
                RaiseWarning("Texture channel " + index + " activated but no texture found.", 2);
                return null;
            }

            texMap = _exportFresnelParameters(texMap, out fresnelParameters);

            var amount = stdMat.GetTexmapAmt(index, 0);

            return ExportTexture(texMap, amount, babylonScene, allowCube, forceAlpha);
        }

        private BabylonTexture ExportPBRTexture(IIGameMaterial materialNode, int index, BabylonScene babylonScene, float amount = 1.0f, bool allowCube = false)
        {
            var texMap = _getTexMap(materialNode, index);
            if (texMap != null)
            {
                return ExportTexture(texMap, amount, babylonScene, allowCube);
            }
            return null;
        }

        private BabylonTexture ExportBaseColorAlphaTexture(IIGameMaterial materialNode, float[] baseColor, float alpha, BabylonScene babylonScene, string materialName)
        {
            ITexmap baseColorTexMap = _getTexMap(materialNode, 1);
            ITexmap alphaTexMap = _getTexMap(materialNode, 9); // Transparency weight map

            // --- Babylon texture ---

            var baseColorTexture = _getBitmapTex(baseColorTexMap);
            var alphaTexture = _getBitmapTex(alphaTexMap);

            // Use one as a reference for UVs parameters
            var texture = baseColorTexture != null ? baseColorTexture : alphaTexture;
            if (texture == null)
            {
                return null;
            }

            var babylonTexture = new BabylonTexture
            {
                name = materialName + "_baseColor.png" // TODO - unsafe name, may conflict with another texture name
            };

            // Level
            babylonTexture.level = 1.0f;

            // UVs
            var uvGen = _exportUV(texture.UVGen, babylonTexture);

            // Is cube
            _exportIsCube(texture.Map.FullFilePath, babylonTexture, false);


            // --- Merge baseColor and alpha maps ---

            var hasBaseColor = isTextureOk(baseColorTexMap);
            var hasAlpha = isTextureOk(alphaTexMap);

            // Alpha
            babylonTexture.hasAlpha = isTextureOk(alphaTexMap) || (isTextureOk(baseColorTexMap) && baseColorTexture.AlphaSource == 0);
            babylonTexture.getAlphaFromRGB = false;

            if (!hasBaseColor && !hasAlpha)
            {
                return null;
            }

            if (CopyTexturesToOutput)
            {
                // Load bitmaps
                var baseColorBitmap = _loadTexture(baseColorTexMap);
                var alphaBitmap = _loadTexture(alphaTexMap);

                // Retreive dimensions
                int width = 0;
                int height = 0;
                var haveSameDimensions = _getMinimalBitmapDimensions(out width, out height, baseColorBitmap, alphaBitmap);
                if (!haveSameDimensions)
                {
                    RaiseWarning("Base color and transparency color maps should have same dimensions", 2);
                }

                var getAlphaFromRGB = false;
                if (alphaTexture != null)
                {
                    getAlphaFromRGB = (alphaTexture.AlphaSource == 2) || (alphaTexture.AlphaSource == 3); // 'RGB intensity' or 'None (Opaque)'
                }

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
                        var baseColorAtPixel = baseColorBitmap != null ? baseColorBitmap.GetPixel(x, y) : _baseColor;

                        Color baseColorAlpha;
                        if (alphaBitmap != null)
                        {
                            // Retreive alpha from alpha texture
                            var alphaColor = alphaBitmap.GetPixel(x, y);
                            var alphaAtPixel = 255 - (getAlphaFromRGB ? alphaColor.R : alphaColor.A);
                            baseColorAlpha = Color.FromArgb(alphaAtPixel, baseColorAtPixel);
                        }
                        else if (baseColorTexture != null && baseColorTexture.AlphaSource == 0) // Alpha source is 'Image Alpha'
                        {
                            // Use all channels from base color
                            baseColorAlpha = baseColorAtPixel;
                        }
                        else
                        {
                            // Use RGB channels from base color and default alpha
                            baseColorAlpha = Color.FromArgb(_alpha, baseColorAtPixel.R, baseColorAtPixel.G, baseColorAtPixel.B);
                        }
                        baseColorAlphaBitmap.SetPixel(x, y, baseColorAlpha);
                    }
                }

                // Write bitmap
                if (isBabylonExported)
                {
                    var absolutePath = Path.Combine(babylonScene.OutputPath, babylonTexture.name);
                    RaiseMessage($"Texture | write image '{babylonTexture.name}'", 2);
                    baseColorAlphaBitmap.Save(absolutePath, System.Drawing.Imaging.ImageFormat.Png); // Explicit image format even though png is default
                }
                else
                {
                    // Store created bitmap for further use in gltf export
                    babylonTexture.bitmap = baseColorAlphaBitmap;
                }
            }

            return babylonTexture;
        }

        private BabylonTexture ExportMetallicRoughnessTexture(IIGameMaterial materialNode, float metallic, float roughness, BabylonScene babylonScene, string materialName, bool invertRoughness)
        {
            ITexmap metallicTexMap = _getTexMap(materialNode, 5);
            ITexmap roughnessTexMap = _getTexMap(materialNode, 4);

            // --- Babylon texture ---
            
            var metallicTexture = _getBitmapTex(metallicTexMap);
            var roughnessTexture = _getBitmapTex(roughnessTexMap);

            // Use one as a reference for UVs parameters
            var texture = metallicTexture != null ? metallicTexture : roughnessTexture;
            if (texture == null)
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
            var uvGen = _exportUV(texture.UVGen, babylonTexture);

            // Is cube
            _exportIsCube(texture.Map.FullFilePath, babylonTexture, false);


            // --- Merge metallic and roughness maps ---

            if (!isTextureOk(metallicTexMap) && !isTextureOk(roughnessTexMap))
            {
                return null;
            }

            if (CopyTexturesToOutput)
            {
                // Load bitmaps
                var metallicBitmap = _loadTexture(metallicTexMap);
                var roughnessBitmap = _loadTexture(roughnessTexMap);

                // Retreive dimensions
                int width = 0;
                int height = 0;
                var haveSameDimensions = _getMinimalBitmapDimensions(out width, out height, metallicBitmap, roughnessBitmap);
                if (!haveSameDimensions)
                {
                    RaiseWarning("Metallic and roughness maps should have same dimensions", 2);
                }

                // Create metallic+roughness map
                Bitmap metallicRoughnessBitmap = new Bitmap(width, height);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        var _metallic = metallicBitmap != null ? metallicBitmap.GetPixel(x, y).B :
                                    metallic * 255.0f;
                        var _roughness = roughnessBitmap != null ? (invertRoughness ? 255 - roughnessBitmap.GetPixel(x, y).G : roughnessBitmap.GetPixel(x, y).G) :
                                     roughness * 255.0f;

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

        private BabylonTexture ExportEnvironmnentTexture(ITexmap texMap, BabylonScene babylonScene)
        {
            if (texMap.GetParamBlock(0) == null || texMap.GetParamBlock(0).Owner == null)
            {
                return null;
            }

            var texture = texMap.GetParamBlock(0).Owner as IBitmapTex;

            if (texture == null)
            {
                return null;
            }

            var sourcePath = texture.Map.FullFilePath;

            var babylonTexture = new BabylonTexture
            {
                name = Path.GetFileName(sourcePath)
            };

            // Copy texture to output
            if (isBabylonExported)
            {
                var destPath = Path.Combine(babylonScene.OutputPath, babylonTexture.name);

                if (CopyTexturesToOutput)
                {
                    try
                    {
                        if (File.Exists(sourcePath))
                        {
                            File.Copy(sourcePath, destPath, true);
                        }
                    }
                    catch
                    {
                        // silently fails
                    }
                }
            }

            return babylonTexture;
        }

        // -------------------------
        // -- Export sub methods ---
        // -------------------------

        private BabylonTexture ExportTexture(ITexmap texMap, float amount, BabylonScene babylonScene, bool allowCube = false, bool forceAlpha = false)
        {
            if (texMap.GetParamBlock(0) == null || texMap.GetParamBlock(0).Owner == null)
            {
                return null;
            }

            var texture = texMap.GetParamBlock(0).Owner as IBitmapTex;

            if (texture == null)
            {
                return null;
            }

            var sourcePath = texture.Map.FullFilePath;

            if (sourcePath == null || sourcePath == "")
            {
                RaiseWarning("Texture path is missing.", 2);
                return null;
            }

            var validImageFormat = GetValidImageFormat(Path.GetExtension(sourcePath));
            if (validImageFormat == null)
            {
                // Image format is not supported by the exporter
                RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), 2);
                return null;
            }

            var babylonTexture = new BabylonTexture
            {
                name = Path.GetFileNameWithoutExtension(texture.MapName) + "." + validImageFormat
            };

            // Level
            babylonTexture.level = amount;

            // Alpha
            if (forceAlpha)
            {
                babylonTexture.hasAlpha = true;
                babylonTexture.getAlphaFromRGB = (texture.AlphaSource == 2) || (texture.AlphaSource == 3); // 'RGB intensity' or 'None (Opaque)'
            }
            else
            {
                babylonTexture.hasAlpha = (texture.AlphaSource != 3); // Not 'None (Opaque)'
                babylonTexture.getAlphaFromRGB = (texture.AlphaSource == 2); // 'RGB intensity'
            }

            // UVs
            var uvGen = _exportUV(texture.UVGen, babylonTexture);

            // Animations
            var animations = new List<BabylonAnimation>();
            ExportFloatAnimation("uOffset", animations, key => new[] { uvGen.GetUOffs(key) });
            ExportFloatAnimation("vOffset", animations, key => new[] { -uvGen.GetVOffs(key) });
            ExportFloatAnimation("uScale", animations, key => new[] { uvGen.GetUScl(key) });
            ExportFloatAnimation("vScale", animations, key => new[] { uvGen.GetVScl(key) });
            ExportFloatAnimation("uAng", animations, key => new[] { uvGen.GetUAng(key) });
            ExportFloatAnimation("vAng", animations, key => new[] { uvGen.GetVAng(key) });
            ExportFloatAnimation("wAng", animations, key => new[] { uvGen.GetWAng(key) });
            babylonTexture.animations = animations.ToArray();
            
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

        private ITexmap _exportFresnelParameters(ITexmap texMap, out BabylonFresnelParameters fresnelParameters)
        {
            fresnelParameters = null;

            // Fallout
            if (texMap.ClassName == "Falloff") // This is the only way I found to detect it. This is crappy but it works
            {
                RaiseMessage("fresnelParameters", 2);
                fresnelParameters = new BabylonFresnelParameters();

                var paramBlock = texMap.GetParamBlock(0);
                var color1 = paramBlock.GetColor(0, 0, 0);
                var color2 = paramBlock.GetColor(4, 0, 0);

                fresnelParameters.isEnabled = true;
                fresnelParameters.leftColor = color2.ToArray();
                fresnelParameters.rightColor = color1.ToArray();

                if (paramBlock.GetInt(8, 0, 0) == 2)
                {
                    fresnelParameters.power = paramBlock.GetFloat(12, 0, 0);
                }
                else
                {
                    fresnelParameters.power = 1;
                }
                var texMap1 = paramBlock.GetTexmap(2, 0, 0);
                var texMap1On = paramBlock.GetInt(3, 0, 0);

                var texMap2 = paramBlock.GetTexmap(6, 0, 0);
                var texMap2On = paramBlock.GetInt(7, 0, 0);

                if (texMap1 != null && texMap1On != 0)
                {
                    texMap = texMap1;
                    fresnelParameters.rightColor = new float[] { 1, 1, 1 };

                    if (texMap2 != null && texMap2On != 0)
                    {
                        RaiseWarning(string.Format("You cannot specify two textures for falloff. Only one is supported"), 2);
                    }
                }
                else if (texMap2 != null && texMap2On != 0)
                {
                    fresnelParameters.leftColor = new float[] { 1, 1, 1 };
                    texMap = texMap2;
                }
                else
                {
                    return null;
                }
            }

            return texMap;
        }

        private IStdUVGen _exportUV(IStdUVGen uvGen, BabylonTexture babylonTexture)
        {
            switch (uvGen.GetCoordMapping(0))
            {
                case 1: //MAP_SPHERICAL
                    babylonTexture.coordinatesMode = 1;
                    break;
                case 2: //MAP_PLANAR
                    babylonTexture.coordinatesMode = 2;
                    break;
                default:
                    babylonTexture.coordinatesMode = 0;
                    break;
            }

            babylonTexture.coordinatesIndex = uvGen.MapChannel - 1;
            if (uvGen.MapChannel > 2)
            {
                RaiseWarning(string.Format("Unsupported map channel, Only channel 1 and 2 are supported."), 2);
            }

            babylonTexture.uOffset = uvGen.GetUOffs(0);
            babylonTexture.vOffset = uvGen.GetVOffs(0);

            babylonTexture.uScale = uvGen.GetUScl(0);
            babylonTexture.vScale = uvGen.GetVScl(0);

            if (Path.GetExtension(babylonTexture.name).ToLower() == ".dds")
            {
                babylonTexture.vScale *= -1; // Need to invert Y-axis for DDS texture
            }

            babylonTexture.uAng = uvGen.GetUAng(0);
            babylonTexture.vAng = uvGen.GetVAng(0);
            babylonTexture.wAng = uvGen.GetWAng(0);

            babylonTexture.wrapU = BabylonTexture.AddressMode.CLAMP_ADDRESSMODE; // CLAMP
            if ((uvGen.TextureTiling & 1) != 0) // WRAP
            {
                babylonTexture.wrapU = BabylonTexture.AddressMode.WRAP_ADDRESSMODE;
            }
            else if ((uvGen.TextureTiling & 4) != 0) // MIRROR
            {
                babylonTexture.wrapU = BabylonTexture.AddressMode.MIRROR_ADDRESSMODE;
            }

            babylonTexture.wrapV = BabylonTexture.AddressMode.CLAMP_ADDRESSMODE; // CLAMP
            if ((uvGen.TextureTiling & 2) != 0) // WRAP
            {
                babylonTexture.wrapV = BabylonTexture.AddressMode.WRAP_ADDRESSMODE;
            }
            else if ((uvGen.TextureTiling & 8) != 0) // MIRROR
            {
                babylonTexture.wrapV = BabylonTexture.AddressMode.MIRROR_ADDRESSMODE;
            }

            return uvGen;
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
                        RaiseWarning(string.Format("Texture {0} not found.", absolutePath), 2);
                    }

                }
                catch
                {
                    // silently fails
                }

                if (babylonTexture.isCube && !allowCube)
                {
                    RaiseWarning(string.Format("Cube texture are only supported for reflection channel"), 2);
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

                    RaiseWarning(string.Format("Mipmaps chain is not complete: {0} maps instead of {1} (based on texture max size: {2})", mipmapsCount, expected, width), 2);
                    RaiseWarning(string.Format("You must generate a complete mipmaps chain for .dds)"), 2);
                    RaiseWarning(string.Format("Mipmaps will be disabled for this texture. If you want automatic texture generation you cannot use a .dds)"), 2);
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

        private IBitmapTex _getBitmapTex(ITexmap texMap)
        {
            if (texMap == null || texMap.GetParamBlock(0) == null || texMap.GetParamBlock(0).Owner == null)
            {
                return null;
            }

            return texMap.GetParamBlock(0).Owner as IBitmapTex;
        }

        private ITexmap _getTexMap(IIGameMaterial materialNode, int index)
        {
            ITexmap texMap = null;
            if (materialNode.MaxMaterial.SubTexmapOn(index) == 1)
            {
                texMap = materialNode.MaxMaterial.GetSubTexmap(index);

                // No warning displayed because by default, physical material in 3ds Max have all maps on
                // Would be tedious for the user to uncheck all unused maps

                //if (texMap == null)
                //{
                //    RaiseWarning("Texture channel " + index + " activated but no texture found.", 2);
                //}
            }
            return texMap;
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

        private bool isTextureOk(ITexmap texMap)
        {
            if (texMap == null || texMap.GetParamBlock(0) == null || texMap.GetParamBlock(0).Owner == null)
            {
                return false;
            }

            var texture = texMap.GetParamBlock(0).Owner as IBitmapTex;

            if (texture == null)
            {
                return false;
            }

            if (!File.Exists(texture.Map.FullFilePath))
            {
                return false;
            }

            return true;
        }

        private Bitmap _loadTexture(ITexmap texMap)
        {
            if (texMap == null || texMap.GetParamBlock(0) == null || texMap.GetParamBlock(0).Owner == null)
            {
                return null;
            }

            var texture = texMap.GetParamBlock(0).Owner as IBitmapTex;

            if (texture == null)
            {
                return null;
            }

            return LoadTexture(texture.Map.FullFilePath);
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
                            RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), 2);
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
                    RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), 2);
                    break;
            }
        }
    }
}

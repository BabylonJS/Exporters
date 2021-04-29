using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Max;
using BabylonExport.Entities;
using Utilities;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

namespace Max2Babylon
{
    static partial class MaxConstants
    {
        public const int IMAGE_ALPHA_FILE = 0;
        public const int IMAGE_ALPHA_RGB = 2;
        public const int IMAGE_ALPHA_NONE = 3;
    }

    partial class BabylonExporter
    {
        private static List<string> validFormats = new List<string>(new string[] { "png", "jpg", "jpeg", "tga", "bmp", "gif" });
        private static List<string> invalidFormats = new List<string>(new string[] { "dds", "tif", "tiff" });
        private Dictionary<string, BabylonTexture> textureMap = new Dictionary<string, BabylonTexture>();


        public ITexmap GetSubTexmap(IStdMat2 stdMat, int index)
        {
            if (!stdMat.MapEnabled(index))
            {
                return null;
            }

            return stdMat.GetSubTexmap(index);
        }

        // -------------------------------
        // --- "public" export methods ---
        // -------------------------------

        private ITexmap GetTexmap(IStdMat2 stdMat, int index, out BabylonFresnelParameters fresnelParameters, out float amount)
        {
            fresnelParameters = null;
            amount = 0;

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

            amount = stdMat.GetTexmapAmt(index, 0);

            return texMap;
        }

        private BabylonTexture ExportTexture(IStdMat2 stdMat, int index, out BabylonFresnelParameters fresnelParameters, BabylonScene babylonScene, bool allowCube = false, bool forceAlpha = false)
        {
            float amount = 1.0f;
            ITexmap texMap = GetTexmap(stdMat, index, out fresnelParameters, out amount);

            if (texMap == null)
            {
                return null;
            }

            return ExportTexture(texMap, babylonScene, amount, allowCube, forceAlpha);
        }

        private BabylonTexture ExportDiffuseTexture(IStdMat2 stdMat, int index, out BabylonFresnelParameters fresnelParameters, BabylonScene babylonScene, out float[] multiplyColor, bool allowCube = false, bool forceAlpha = false)
        {
            multiplyColor = null;

            float amount = 1.0f;
            ITexmap texMap = GetTexmap(stdMat, index, out fresnelParameters, out amount);

            if (texMap == null)
            {
                return null;
            }

            return ExportDiffuseTexture(texMap, babylonScene, out multiplyColor, amount, allowCube, forceAlpha);
        }

        private BabylonTexture ExportSpecularTexture(IIGameMaterial materialNode, float[] specularColor, BabylonScene babylonScene)
        {
            ITexmap specularColorTexMap = _getTexMap(materialNode, 2);
            ITexmap specularLevelTexMap = _getTexMap(materialNode, 3);

            // --- Babylon texture ---

            var specularColorTexture = _getBitmapTex(specularColorTexMap);
            var specularLevelTexture = _getBitmapTex(specularLevelTexMap);

            if (specularLevelTexture == null)
            {
                // Copy specular color image
                // Assume specular color texture is already pre-multiplied by a global specular level value
                // So do not use global specular level
                return ExportTexture(specularColorTexture, babylonScene);
            }

            // Use one as a reference for UVs parameters
            var texture = specularColorTexture != null ? specularColorTexture : specularLevelTexture;
            if (texture == null)
            {
                return null;
            }

            RaiseMessage("Multiply specular color and level textures", 2);

            string nameText = null;

            nameText = (specularColorTexture != null ? Path.GetFileNameWithoutExtension(specularColorTexture.Map.FullFilePath) : TextureUtilities.ColorToStringName(specularColor)) +
                        Path.GetFileNameWithoutExtension(specularLevelTexture.Map.FullFilePath) + "_specularColor";

            var textureID = texture.GetGuid().ToString();
            if (textureMap.ContainsKey(textureID))
            {
                return textureMap[textureID];
            }
            else
            { 
                var babylonTexture = new BabylonTexture(textureID)
                {
                    name = nameText + ".jpg" // TODO - unsafe name, may conflict with another texture name
                };

                // Level
                babylonTexture.level = 1.0f;

                // UVs
                var uvGen = _exportUV(texture.UVGen, babylonTexture);

                // Is cube
                _exportIsCube(texture.Map.FullFilePath, babylonTexture, false);


                // --- Multiply specular color and level maps ---

                // Alpha
                babylonTexture.hasAlpha = false;
                babylonTexture.getAlphaFromRGB = false;

                if (exportParameters.writeTextures)
                {
                    // Load bitmaps
                    var specularColorBitmap = _loadTexture(specularColorTexMap);
                    var specularLevelBitmap = _loadTexture(specularLevelTexMap);

                    if (specularLevelBitmap == null)
                    {
                        // Copy specular color image
                        RaiseError("Failed to load specular level texture. Specular color is exported alone.", 3);
                        return ExportTexture(specularColorTexture, babylonScene);
                    }

                    // Retreive dimensions
                    int width = 0;
                    int height = 0;
                    var haveSameDimensions = TextureUtilities.GetMinimalBitmapDimensions(out width, out height, specularColorBitmap, specularLevelBitmap);
                    if (!haveSameDimensions)
                    {
                        RaiseError("Specular color and specular level maps should have same dimensions", 3);
                    }

                    // Create pre-multiplied specular color map
                    var _specularColor = Color.FromArgb(
                        (int)(specularColor[0] * 255),
                        (int)(specularColor[1] * 255),
                        (int)(specularColor[2] * 255));
                    Bitmap specularColorPreMultipliedBitmap = new Bitmap(width, height);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            var specularColorAtPixel = specularColorBitmap != null ? specularColorBitmap.GetPixel(x, y) : _specularColor;
                            var specularLevelAtPixel = specularLevelBitmap.GetPixel(x, y);

                            var specularColorPreMultipliedAtPixel = specularColorAtPixel.multiply(specularLevelAtPixel);

                            specularColorPreMultipliedBitmap.SetPixel(x, y, specularColorPreMultipliedAtPixel);
                        }
                    }

                    // Write bitmap
                    if (isBabylonExported)
                    {
                        RaiseMessage($"Texture | write image '{babylonTexture.name}'", 3);
                        TextureUtilities.SaveBitmap(specularColorPreMultipliedBitmap, babylonScene.OutputPath, babylonTexture.name, ImageFormat.Jpeg, exportParameters.txtQuality, this);
                    }
                    else
                    {
                        // Store created bitmap for further use in gltf export
                        babylonTexture.bitmap = specularColorPreMultipliedBitmap;
                    }
                }
                textureMap.Add(babylonTexture.Id, babylonTexture);
                return babylonTexture;
            }
        }

        private BabylonTexture ExportPBRTexture(IIGameMaterial materialNode, int index, BabylonScene babylonScene, float amount = 1.0f, bool allowCube = false)
        {
            var texMap = _getTexMap(materialNode, index);
            if (texMap != null)
            {
                return ExportTexture(texMap, babylonScene, amount, allowCube);
            }
            return null;
        }

        private BabylonTexture ExportClearCoatTexture(ITexmap intensityTexMap, ITexmap roughnessTexMap, float coatWeight, float coatRoughness, BabylonScene babylonScene, string materialName, bool invertRoughness)
        {
            // --- Babylon texture ---
            var intensityTexture = _getBitmapTex(intensityTexMap);
            var roughnessTexture = _getBitmapTex(roughnessTexMap);

            var texture = intensityTexture != null ? intensityTexture : roughnessTexture;
            if (texture == null)
            {
                return null;
            }

            // Use one as a reference for UVs parameters

            RaiseMessage("Export Clear Coat weight+roughness texture", 2);

            string nameText = Path.GetFileNameWithoutExtension(texture.Map.FullFilePath);

            var textureID = texture.GetGuid().ToString();
            if (textureMap.ContainsKey(textureID))
            {
                return textureMap[textureID];
            }
            else
            {
                var babylonTexture = new BabylonTexture(textureID)
                {
                    name = nameText // TODO - unsafe name, may conflict with another texture name
                };

                // Level
                babylonTexture.level = 1.0f;

                // UVs
                var uvGen = _exportUV(texture.UVGen, babylonTexture);

                // Is cube
                _exportIsCube(texture.Map.FullFilePath, babylonTexture, false);

                // --- Merge maps ---
                var hasIntensity = isTextureOk(intensityTexture);
                var hasRoughness = isTextureOk(roughnessTexture);
                if (!hasIntensity && !hasRoughness)
                {
                    return null;
                }

                // Set image format
                ImageFormat imageFormat = ImageFormat.Jpeg;
                babylonTexture.name += ".jpg";

                if (exportParameters.writeTextures)
                {
                    // Load bitmaps
                    var intensityBitmap = _loadTexture(intensityTexture);
                    var roughnessBitmap = _loadTexture(roughnessTexture);

                    // Retreive dimensions
                    int width = 0;
                    int height = 0;
                    var haveSameDimensions = TextureUtilities.GetMinimalBitmapDimensions(out width, out height, intensityBitmap, roughnessBitmap);
                    if (!haveSameDimensions)
                    {
                        RaiseError("Base color and transparency color maps should have same dimensions", 3);
                    }

                    // Create map
                    var _intensity = (int)(coatWeight * 255);
                    var _roughness = (int)(coatRoughness * 255);
                    Bitmap intensityRoughnessBitmap = new Bitmap(width, height);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            var intensityAtPixel = (intensityBitmap == null) ? _intensity : intensityBitmap.GetPixel(x, y).R;

                            Color intensityRoughness;
                            if (roughnessBitmap == null)
                            {
                                intensityRoughness = Color.FromArgb(intensityAtPixel, _roughness, 0);
                            }
                            else
                            {
                                var roughnessAtPixel = (roughnessBitmap == null) ?
                                    _roughness :
                                    invertRoughness ? 255 - roughnessBitmap.GetPixel(x, y).G : roughnessBitmap.GetPixel(x, y).G;

                                intensityRoughness = Color.FromArgb(intensityAtPixel, roughnessAtPixel, 0);
                            }
                            intensityRoughnessBitmap.SetPixel(x, y, intensityRoughness);
                        }
                    }

                    // Write bitmap
                    if (isBabylonExported)
                    {
                        RaiseMessage($"Texture | write image '{babylonTexture.name}'", 3);
                        TextureUtilities.SaveBitmap(intensityRoughnessBitmap, babylonScene.OutputPath, babylonTexture.name, imageFormat, exportParameters.txtQuality, this);
                    }
                    else
                    {
                        // Store created bitmap for further use in gltf export
                        babylonTexture.bitmap = intensityRoughnessBitmap;
                    }
                }

                return babylonTexture;
            }
        }

        /// <returns></returns>
        private BabylonTexture ExportBaseColorAlphaTexture(ITexmap baseColorTexMap, ITexmap alphaTexMap, float[] baseColor, float alpha, BabylonScene babylonScene, out float[] multiplyColor, bool isOpacity = false)
        {
            // --- Babylon texture ---
            var baseColorTexture = _getBitmapTex(baseColorTexMap, out multiplyColor);
            var alphaTexture = _getBitmapTex(alphaTexMap);

            // If we don't retrieve any textures from Max, return null.
            if (baseColorTexture == null && alphaTexture == null)
            {
                return null;
            }

            string baseColorTextureMapExtension = null;
            var baseColorSourcePath = baseColorTexture?.Map.FullFilePath;
            var alphaSourcePath = alphaTexture?.Map.FullFilePath;

            // If we only have a base color texture, and we are using an opaque texture, export the base color image only.
            if (baseColorTexture != null && alphaTexture == null)
            {
                baseColorTextureMapExtension = Path.GetExtension(baseColorTexture.Map.FullFilePath).ToLower();
                if (alpha == 1)
                {
                    if (baseColorTexture.AlphaSource == MaxConstants.IMAGE_ALPHA_FILE &&
                        (baseColorTextureMapExtension == ".tif" || baseColorTextureMapExtension == ".tiff"))
                    {
                        RaiseWarning($"Diffuse texture named {baseColorTexture.Map.FullFilePath} is a .tif file and its Alpha Source is 'Image Alpha' by default.", 3);
                        RaiseWarning($"If you don't want material to be in BLEND mode, set diffuse texture Alpha Source to 'None (Opaque)'", 3);
                    }

                    // Copy base color image
                    var outTexture = ExportTexture(baseColorTexture, babylonScene);
                    if (outTexture != null)
                    {
                        // used to keep trace of the base + apha merge sources
                        outTexture.baseColorPath = baseColorSourcePath;
                        outTexture.alphaPath = null;

                        textureMap[outTexture.Id] = outTexture;
                    }
                    return outTexture;
                }

            }

            // Otherwise combine base color and alpha textures to a single output texture
            RaiseMessage("Export baseColor+Alpha texture", 2);

            var hasBaseColor = baseColorTexture != null && File.Exists(baseColorTexture.Map.FullFilePath);
            var hasAlpha = isTextureOk(alphaTexMap);
            var texture = hasBaseColor ? baseColorTexture : alphaTexture;

            ImageFormat imageFormat = null;
            if (hasBaseColor)
            {
                imageFormat = TextureUtilities.GetImageFormat(Path.GetExtension(baseColorTexture.Map.FullFilePath));
            }

            if (hasAlpha || imageFormat == null)
            {
                baseColorTextureMapExtension = ".png"; // since we are adding an alpha channel, export as png. This will convert any other input base texture format to PNG.
                imageFormat = ImageFormat.Png;
            }

            // since we are creating a new texture, give it a unique ID based on the base color and alpha maps.
            var nameText = (hasBaseColor ? Path.GetFileNameWithoutExtension(baseColorTexture.Map.FullFilePath) + (hasAlpha ?  "_" + Path.GetFileNameWithoutExtension(alphaTexture.Map.FullFilePath) : "") : TextureUtilities.ColorToStringName(baseColor));
            var textureID = hasBaseColor ? texture.GetGuid().ToString() + (hasAlpha ? "_" + alphaTexture.GetGuid().ToString() : "") : string.Format("{0}_{1}", texture.GetGuid().ToString(), nameText);

            if (textureMap.ContainsKey(textureID))
            {
                return textureMap[textureID];
            }

            BabylonTexture babylonTexture = null;
            babylonTexture = new BabylonTexture(textureID)
            {
                name = nameText,
                // used to keep trace of the base + apha merge sources
                baseColorPath = baseColorSourcePath,
                alphaPath = alphaSourcePath
            };

            // Alpha
            babylonTexture.hasAlpha = hasAlpha || (hasBaseColor && (baseColorTexture.AlphaSource == MaxConstants.IMAGE_ALPHA_FILE || baseColorTexture.AlphaSource == MaxConstants.IMAGE_ALPHA_RGB)) || alpha < 1.0f;
            babylonTexture.getAlphaFromRGB = false;

            if (!hasBaseColor && !hasAlpha)
            {
                return null;
            }

            if ((!isTextureOk(alphaTexMap) && alpha == 1.0f && (hasBaseColor && baseColorTexture.AlphaSource == MaxConstants.IMAGE_ALPHA_FILE)) &&
                (baseColorTextureMapExtension == ".tif" || baseColorTextureMapExtension == ".tiff"))
            {
                RaiseWarning($"Diffuse texture named {baseColorTexture.Map.FullFilePath} is a .tif file and its Alpha Source is 'Image Alpha' by default.", 3);
                RaiseWarning($"If you don't want material to be in BLEND mode, set diffuse texture Alpha Source to 'None (Opaque)'", 3);
            }

            // Set image format
            
            if (hasAlpha)
            {
                babylonTexture.name += "_alpha_" + alphaTexture.Name;
            }

            if (imageFormat == ImageFormat.Jpeg)
            {
                babylonTexture.name += ".jpg";
            }
            else
            {
                babylonTexture.name += "." + imageFormat.ToString();
            }

            // Level
            babylonTexture.level = 1.0f;

            // UVs
            var uvGen = _exportUV(texture.UVGen, babylonTexture);

            // Is cube
            _exportIsCube(texture.Map.FullFilePath, babylonTexture, false);

            // --- Merge baseColor and alpha maps ---

            if (exportParameters.writeTextures && baseColorTexture != alphaTexture && alphaTexture != null)
            {
                // Load bitmaps
                var baseColorBitmap = baseColorTexture != null ? TextureUtilities.LoadTexture(baseColorTexture.Map.FullFilePath, this) : null;
                var alphaBitmap = _loadTexture(alphaTexMap);

                // Retreive dimensions
                int width = 0;
                int height = 0;
                var haveSameDimensions = TextureUtilities.GetMinimalBitmapDimensions(out width, out height, baseColorBitmap, alphaBitmap);
                if (!haveSameDimensions)
                {
                    RaiseError("Base color and transparency color maps should have same dimensions", 3);
                }

                var getAlphaFromRGB = alphaTexture != null && ((alphaTexture.AlphaSource == MaxConstants.IMAGE_ALPHA_RGB) || (alphaTexture.AlphaSource == MaxConstants.IMAGE_ALPHA_NONE)); // 'RGB intensity' or 'None (Opaque)'

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
                            Color alphaColor = alphaBitmap.GetPixel(x, y);
                            int alphaAtPixel = getAlphaFromRGB ? alphaColor.R : alphaColor.A;
                            if (isOpacity == false)
                            {
                                // Convert transparency to opacity
                                alphaAtPixel = 255 - alphaAtPixel;
                            }
                            baseColorAlpha = Color.FromArgb(alphaAtPixel, baseColorAtPixel);
                        }
                        else if (baseColorTexture != null && baseColorTexture.AlphaSource == MaxConstants.IMAGE_ALPHA_FILE) // Alpha source is 'Image Alpha'
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

                // If there is a "#" in the generated texture filename it causes problems with Exporter webserver
                // when viewing the final exported project. This converts any troublesome characters to underscores
                babylonTexture.name = PathUtilities.VerifyLegalFileName(babylonTexture.name);

                // Write bitmap
                if (isBabylonExported)
                {
                    RaiseMessage($"Texture | write image '{babylonTexture.name}'", 3);
                    TextureUtilities.SaveBitmap(baseColorAlphaBitmap, babylonScene.OutputPath, babylonTexture.name, imageFormat, exportParameters.txtQuality, this);
                }
                else
                {
                    // Store created bitmap for further use in gltf export
                    babylonTexture.bitmap = baseColorAlphaBitmap;
                }
            }

            return babylonTexture;
        }

        /// <summary>
        /// This texture Specifies both the specular color RGB and the glossiness A of the material per pixels.
        /// </summary>
        /// <param name="ambientOcclusionTexMap"></param>
        /// <param name="glossinessTexMap"></param>
        /// <param name="specularTexMap"></param>
        /// <param name="glossiness"></param>
        /// <param name="specularColor"></param>
        /// <param name="babylonScene"></param>
        /// <returns></returns>
        private BabylonTexture ExportSpecularGlossinessTexture(float[] specularColor, ITexmap specularTexMap, float glossiness, ITexmap glossinessTexMap, BabylonScene babylonScene)
        {
            var specularTexture = _getBitmapTex(specularTexMap);
            var glossinessTexture = _getBitmapTex(glossinessTexMap);
             
            // we are trying to get the best output format, function of source and policy.
            var paths = new IBitmapTex[] { specularTexture, glossinessTexture}.Where(t => t != null).Select(t => t.Map.FileName);
            var policy = exportParameters.textureFormatExportPolicy;
            var preferredExtension = TextureUtilities.GetPreferredFormat(paths, false, policy);

            // Use metallic or roughness texture as a reference for UVs parameters
            var refTexture = specularTexture?? glossinessTexture;
            if (refTexture == null)
            {
                return null;
            }

            RaiseMessage("Export Specular Glossiness texture", 2);

            var textureID = refTexture.GetGuid().ToString();
            if (textureMap.ContainsKey(textureID))
            {
                return textureMap[textureID];
            }
            var babylonTexture = new BabylonTexture(textureID)
            {
                name = BuildSpecularGlossinessTextureName(specularColor, specularTexture, glossiness, glossinessTexture) + "." + preferredExtension // TODO - unsafe name, may conflict with another texture name
            };

            // UVs
            var uvGen = _exportUV(refTexture.UVGen, babylonTexture);
            
            // Is cube
            _exportIsCube(refTexture.Map.FullFilePath, babylonTexture, false);

            // --- Merge specular and gloossiness maps ---
            if (!isTextureOk(specularTexMap) && !isTextureOk(glossinessTexMap))
            {
                return null;
            }
            if (exportParameters.writeTextures)
            {
                // Load bitmaps
                var specularBitmap = _loadTexture(specularTexMap);
                var glossinessBitmap = _loadTexture(glossinessTexMap);

                // Retreive dimensions
                int width = 0;
                int height = 0;
                var haveSameDimensions = TextureUtilities.GetMinimalBitmapDimensions(out width, out height, specularBitmap, glossinessBitmap);
                if (!haveSameDimensions)
                {
                    RaiseError("Specular and glossiness maps should have same dimensions", 3);
                }

                // Create specular glossiness map
                Bitmap newBitmap = new Bitmap(width, height);
                int[] rgbColor = specularColor.Select(v => (int)(v * 255)).ToArray();
                int glossinessFactor = (int)(glossiness * 255);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int alpha = glossinessBitmap?.GetPixel(x, y).R ?? glossinessFactor;
                        float [] rgb = specularBitmap?.GetPixel(x, y).toArrayRGB()??specularColor;
                        Color color = Color.FromArgb(alpha, (int)rgb[0], (int)rgb[1], (int)rgb[2]);
                        newBitmap.SetPixel(x, y, color);
                    }
                }

                // Write bitmap
                if (isBabylonExported)
                {
                    RaiseMessage($"Texture | write image '{babylonTexture.name}'", 3);
                    var imageFormat = TextureUtilities.GetImageFormat(Path.GetExtension(babylonTexture.name));
                    TextureUtilities.SaveBitmap(newBitmap, babylonScene.OutputPath, babylonTexture.name, imageFormat, exportParameters.txtQuality, this);
                }
                else
                {
                    // Store created bitmap for further use in gltf export
                    babylonTexture.bitmap = newBitmap;
                }
            }
            textureMap[babylonTexture.Id] = babylonTexture;
            return babylonTexture;
        }

        private static string BuildSpecularGlossinessTextureName(float[] specularColor, IBitmapTex specularMap, float glossiness, IBitmapTex glossinessMap)
        {
            var specPart = $"{GetFileNameWithoutExtension(specularMap)}_{GetColorCode(specularColor)}";
            var glossPart = $"{GetFileNameWithoutExtension(glossinessMap)}_{glossiness*255}";
            return $"{specPart}_{glossPart}";
        }

        private static string GetFileNameWithoutExtension(IBitmapTex text)
        {
            var str = text.Map?.FileName;
            return str != null ? Path.GetFileNameWithoutExtension(str) : string.Empty;
        }
        private static string GetColorCode(float[] c)
        {
            if( c == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            for(int i=0; i < c.Length && i < 3; i++)
            {
                sb.Append(((int)(c[i]*255)).ToString("X2"));
            }
            return sb.ToString();
        }

        private BabylonTexture ExportORMTexture(ITexmap ambientOcclusionTexMap, ITexmap roughnessTexMap, ITexmap metallicTexMap, float metallic, float roughness, BabylonScene babylonScene, bool invertRoughness)
        {
            // --- Babylon texture ---

            var metallicTexture = _getBitmapTex(metallicTexMap);
            var roughnessTexture = _getBitmapTex(roughnessTexMap);
            var ambientOcclusionTexture = _getBitmapTex(ambientOcclusionTexMap);

            // we are trying to get the best output format, function of source and policy.
            var paths = new IBitmapTex[] { metallicTexture, roughnessTexture, ambientOcclusionTexture }.Where(t => t != null).Select(t => t.Map.FileName);
            var policy = exportParameters.textureFormatExportPolicy;
            var preferredExtension = TextureUtilities.GetPreferredFormat(paths, false, policy);

            // Use metallic or roughness texture as a reference for UVs parameters
            var refTexture = metallicTexture != null ? metallicTexture : roughnessTexture;
            if (refTexture == null)
            {
                return null;
            }

            RaiseMessage("Export ORM texture", 2);

            var textureID = refTexture.GetGuid().ToString();
            if (textureMap.ContainsKey(textureID))
            {
                return textureMap[textureID];
            }
            else {
                var babylonTexture = new BabylonTexture(textureID)
                {
                    name = (ambientOcclusionTexMap != null ? Path.GetFileNameWithoutExtension(ambientOcclusionTexture.Map.FileName) : "") +
                           (roughnessTexMap != null ? Path.GetFileNameWithoutExtension(roughnessTexture.Map.FileName) : ("" + (int)(roughness * 255))) +
                           (metallicTexMap != null ? Path.GetFileNameWithoutExtension(metallicTexture.Map.FileName) : ("" + (int)(metallic * 255))) + "." + preferredExtension // TODO - unsafe name, may conflict with another texture name
                };

                // UVs
                var uvGen = _exportUV(refTexture.UVGen, babylonTexture);

                // Is cube
                _exportIsCube(refTexture.Map.FullFilePath, babylonTexture, false);

                // --- Merge metallic and roughness maps ---

                if (!isTextureOk(metallicTexMap) && !isTextureOk(roughnessTexMap))
                {
                    return null;
                }

                if (exportParameters.writeTextures)
                {
                    // Load bitmaps
                    var metallicBitmap = _loadTexture(metallicTexMap);
                    var roughnessBitmap = _loadTexture(roughnessTexMap);
                    var ambientOcclusionBitmap = _loadTexture(ambientOcclusionTexMap);

                    // Retreive dimensions
                    int width = 0;
                    int height = 0;
                    var haveSameDimensions = TextureUtilities.GetMinimalBitmapDimensions(out width, out height, metallicBitmap, roughnessBitmap, ambientOcclusionBitmap);
                    if (!haveSameDimensions)
                    {
                        RaiseError((ambientOcclusionBitmap != null ? "Occlusion, roughness and metallic " : "Metallic and roughness") + " maps should have same dimensions", 3);
                    }

                    // Create ORM map
                    Bitmap ormBitmap = new Bitmap(width, height);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            int _occlusion = ambientOcclusionBitmap != null ? ambientOcclusionBitmap.GetPixel(x, y).R : 0;
                            int _roughness = roughnessBitmap != null ? (invertRoughness ? 255 - roughnessBitmap.GetPixel(x, y).G : roughnessBitmap.GetPixel(x, y).G) : (int)(roughness * 255.0f);
                            int _metallic = metallicBitmap != null ? metallicBitmap.GetPixel(x, y).B : (int)(metallic * 255.0f);

                            // The occlusion values are sampled from the R channel.
                            // The roughness values are sampled from the G channel.
                            // The metalness values are sampled from the B channel.
                            Color colorMetallicRoughness = Color.FromArgb(_occlusion, _roughness, _metallic);
                            ormBitmap.SetPixel(x, y, colorMetallicRoughness);
                        }
                    }

                    // Write bitmap
                    if (isBabylonExported)
                    {
                        RaiseMessage($"Texture | write image '{babylonTexture.name}'", 3);
                        var imageFormat = TextureUtilities.GetImageFormat(Path.GetExtension(babylonTexture.name));
                        TextureUtilities.SaveBitmap(ormBitmap, babylonScene.OutputPath, babylonTexture.name, imageFormat, exportParameters.txtQuality, this);
                    }
                    else
                    {
                        // Store created bitmap for further use in gltf export
                        babylonTexture.bitmap = ormBitmap;
                    }
                }
                textureMap[babylonTexture.Id] = babylonTexture;
                return babylonTexture;
            }
        }

        private BabylonTexture ExportEnvironmnentTexture(ITexmap texMap, BabylonScene babylonScene)
        {
            if (texMap.GetParamBlock(0) == null || texMap.GetParamBlock(0).Owner == null)
            {
                RaiseWarning("Failed to export environment texture. Uncheck \"Use Map\" option to fix this warning.");
                return null;
            }

            var texture = texMap.GetParamBlock(0).Owner as IBitmapTex;

            if (texture == null)
            {
                RaiseWarning("Failed to export environment texture. Uncheck \"Use Map\" option to fix this warning.");
                return null;
            }

            var sourcePath = texture.Map.FullFilePath;
            var fileName = Path.GetFileName(sourcePath);

            // Allow only dds file format
            if (!fileName.EndsWith(".dds"))
            {
                RaiseWarning("Failed to export environment texture: only .dds format is supported. Uncheck \"Use map\" to fix this warning.");
                return null;
            }

            var textureID = texture.GetGuid().ToString();
            if (textureMap.ContainsKey(textureID))
            {
                return textureMap[textureID];
            }
            else
            { 
                var babylonTexture = new BabylonTexture(textureID)
                {
                    name = fileName
                };

                // Copy texture to output
                if (isBabylonExported)
                {
                    var destPath = Path.Combine(babylonScene.OutputPath, babylonTexture.name);

                    if (exportParameters.writeTextures)
                    {
                        try
                        {
                            if (File.Exists(sourcePath) && sourcePath != destPath)
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
        }

        // -------------------------
        // -- Export sub methods ---
        // -------------------------

        private ITexmap _getSpecialTexmap(ITexmap texMap, out float amount)
        {
            if (texMap == null)
            {
                amount = 0.0f;
                return null;
            }

            if (texMap.ClassName == "Normal Bump")
            {
                var block = texMap.GetParamBlockByID(0);        // General Block
                if (block != null)
                {
                    amount = block.GetFloat(0, 0, 0);           // Normal texture Mult Spin
                    var map = block.GetTexmap(2, 0, 0);         // Normal texture
                    var mapEnabled = block.GetInt(4, 0, 0);     // Normal texture Enable
                    if (map == null || mapEnabled == 0)
                    {
                        RaiseError($"Only Normal Bump Texture with Normal enabled are supported.", 2);
                        return null;
                    }

                    var method = block.GetInt(6, 0, 0);         // Normal texture mode (Tangent, screen...)
                    if (method != 0)
                    {
                        RaiseError($"Only Normal Bump Texture in tangent space are supported.", 2);
                        return null;
                    }
                    var flipR = block.GetInt(7, 0, 0);          // Normal texture Red chanel Flip
                    if (flipR != 0)
                    {
                        RaiseError($"Only Normal Bump Texture without R flip are supported.", 2);
                        return null;
                    }
                    var flipG = block.GetInt(8, 0, 0);          // Normal texture Green chanel Flip
                    if (flipG != 0)
                    {
                        RaiseError($"Only Normal Bump Texture without G flip are supported.", 2);
                        return null;
                    }
                    var swapRG = block.GetInt(9, 0, 0);         // Normal texture swap R and G channels
                    if (swapRG != 0)
                    {
                        RaiseError($"Only Normal Bump Texture without R and G swap are supported.", 2);
                        return null;
                    }

                    var bumpAmount = block.GetFloat(1, 0, 0);   // Bump texture Mult Spin
                    var bumpMap = block.GetTexmap(3, 0, 0);     // Bump texture
                    var bumpMapEnable = block.GetInt(5, 0, 0);  // Bump texture Enable
                    if (bumpMap != null && bumpMapEnable == 1)
                    {
                        RaiseError($"Only Normal Bump Texture without Bump are supported.", 2);
                        return null;
                    }

                    return map;
                }
            }

            amount = 0.0f;
            RaiseError($"Texture type is not supported. Use a Bitmap or Normal Bump map instead.", 2);
            return null;
        }

        private BabylonTexture ExportTexture(ITexmap texMap, BabylonScene babylonScene, float amount = 1.0f, bool allowCube = false, bool forceAlpha = false)
        {
            if(texMap == null)
            {
                return null;
            }
            IBitmapTex texture = _getBitmapTex(texMap, false);
            if (texture == null)
            {
                float specialAmount;
                var specialTexMap = _getSpecialTexmap(texMap, out specialAmount);
                texture = _getBitmapTex(specialTexMap, false);
                amount *= specialAmount;
            }

            return ExportBitmapTexture(texture, babylonScene, amount, allowCube, forceAlpha);
        }

        private BabylonTexture ExportDiffuseTexture(ITexmap texMap, BabylonScene babylonScene, out float[] multiplyColor, float amount = 1.0f, bool allowCube = false, bool forceAlpha = false)
        {
            IBitmapTex texture = _getBitmapTex(texMap, out multiplyColor);
            return ExportBitmapTexture(texture, babylonScene, amount, allowCube, forceAlpha);
        }

        private BabylonTexture ExportBitmapTexture(IBitmapTex texture, BabylonScene babylonScene, float amount = 1.0f, bool allowCube = false, bool forceAlpha = false)
        {
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

            RaiseMessage("Export texture named: " + Path.GetFileName(sourcePath), 2);

            var validImageFormat = TextureUtilities.GetValidImageFormat(Path.GetExtension(sourcePath));
            if (validImageFormat == null)
            {
                // Image format is not supported by the exporter
                RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), 3);
                return null;
            }
            var textureID = texture.GetGuid().ToString();
            if (textureMap.ContainsKey(textureID))
            {
                return textureMap[textureID];
            }
            else
            { 
                var babylonTexture = new BabylonTexture(textureID)
                {
                    name = Path.GetFileNameWithoutExtension(texture.MapName) + "." + validImageFormat
                };
                RaiseMessage($"texture id = {babylonTexture.Id}", 2);

                // Level
                babylonTexture.level = amount;

                // Alpha
                if (forceAlpha)
                {
                    babylonTexture.hasAlpha = true;
                    babylonTexture.getAlphaFromRGB = (texture.AlphaSource == MaxConstants.IMAGE_ALPHA_RGB) || (texture.AlphaSource == MaxConstants.IMAGE_ALPHA_NONE); // 'RGB intensity' or 'None (Opaque)'
                }
                else
                {
                    babylonTexture.hasAlpha = (texture.AlphaSource != MaxConstants.IMAGE_ALPHA_NONE); // Not 'None (Opaque)'
                    babylonTexture.getAlphaFromRGB = (texture.AlphaSource == MaxConstants.IMAGE_ALPHA_RGB); // 'RGB intensity'
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
                    TextureUtilities.CopyTexture(sourcePath, destPath, exportParameters.txtQuality, this);

                    // Is cube
                    _exportIsCube(Path.Combine(babylonScene.OutputPath, babylonTexture.name), babylonTexture, allowCube);
                }
                else
                {
                    babylonTexture.isCube = false;
                }
                babylonTexture.originalPath = sourcePath;

                return babylonTexture;
            }
        }

        private ITexmap _exportFresnelParameters(ITexmap texMap, out BabylonFresnelParameters fresnelParameters)
        {
            fresnelParameters = null;

            // Fallout
            if (texMap.ClassName == "Falloff") // This is the only way I found to detect it. This is crappy but it works
            {
                RaiseMessage("fresnelParameters", 3);
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
                        RaiseWarning(string.Format("You cannot specify two textures for falloff. Only one is supported"), 3);
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
                    babylonTexture.coordinatesMode = BabylonTexture.CoordinatesMode.SPHERICAL_MODE;
                    break;
                case 2: //MAP_PLANAR
                    babylonTexture.coordinatesMode = BabylonTexture.CoordinatesMode.PLANAR_MODE;
                    break;
                default:
                    babylonTexture.coordinatesMode = BabylonTexture.CoordinatesMode.EXPLICIT_MODE;
                    break;
            }

            babylonTexture.coordinatesIndex = uvGen.MapChannel - 1;
            if (uvGen.MapChannel > 2)
            {
                RaiseWarning(string.Format("Unsupported map channel, Only channel 1 and 2 are supported."), 3);
            }

            babylonTexture.uOffset = uvGen.GetUOffs(0);
            babylonTexture.vOffset = -uvGen.GetVOffs(0);

            babylonTexture.uScale = uvGen.GetUScl(0);
            babylonTexture.vScale = uvGen.GetVScl(0);

            var offset = new BabylonVector3(babylonTexture.uOffset, -babylonTexture.vOffset, 0);
            var scale = new BabylonVector3(babylonTexture.uScale, babylonTexture.vScale, 1);
            var rotationEuler = new BabylonVector3(uvGen.GetUAng(0), uvGen.GetVAng(0), uvGen.GetWAng(0));
            var rotation = BabylonQuaternion.FromEulerAngles(rotationEuler.X, rotationEuler.Y, rotationEuler.Z);
            var pivotCenter = new BabylonVector3(-0.5f, -0.5f, 0);
            var transformMatrix = MathUtilities.ComputeTextureTransformMatrix(pivotCenter, offset, rotation, scale);

            transformMatrix.decompose(scale, rotation, offset);
            var texTransformRotationEuler = rotation.toEulerAngles();

            babylonTexture.uOffset = -offset.X;
            babylonTexture.vOffset = -offset.Y;
            babylonTexture.uScale = scale.X;
            babylonTexture.vScale = -scale.Y;
            babylonTexture.uRotationCenter = 0.0f;
            babylonTexture.vRotationCenter = 0.0f;
            babylonTexture.invertY = false;
            babylonTexture.uAng = texTransformRotationEuler.X;
            babylonTexture.vAng = texTransformRotationEuler.Y;
            babylonTexture.wAng = texTransformRotationEuler.Z;

            if (Path.GetExtension(babylonTexture.name).ToLower() == ".dds")
            {
                babylonTexture.vScale *= -1; // Need to invert Y-axis for DDS texture
            }

            if (babylonTexture.wAng != 0f 
                && (babylonTexture.uScale != 1f || babylonTexture.vScale != 1f) 
                && (Math.Abs(babylonTexture.uScale) - Math.Abs(babylonTexture.vScale)) > float.Epsilon)
            {
                RaiseWarning("Rotation and non-uniform tiling (scale) on a texture is not supported as it will cause texture shearing. You can use the map UV of the mesh for those transformations.", 3);
            }


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
                        RaiseWarning(string.Format("Texture {0} not found.", absolutePath), 3);
                    }

                }
                catch
                {
                    // silently fails
                }

                if (babylonTexture.isCube && !allowCube)
                {
                    RaiseWarning(string.Format("Cube texture are only supported for reflection channel"), 3);
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

                    RaiseWarning(string.Format("Mipmaps chain is not complete: {0} maps instead of {1} (based on texture max size: {2})", mipmapsCount, expected, width), 3);
                    RaiseWarning(string.Format("You must generate a complete mipmaps chain for .dds)"), 3);
                    RaiseWarning(string.Format("Mipmaps will be disabled for this texture. If you want automatic texture generation you cannot use a .dds)"), 3);
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

        private IBitmapTex _getBitmapTex(ITexmap texMap, bool raiseError = true)
        {
            if (texMap == null || texMap.GetParamBlock(0) == null || texMap.GetParamBlock(0).Owner == null)
            {
                return null;
            }

            var texture = texMap.GetParamBlock(0).Owner as IBitmapTex;
            if (texture == null && raiseError)
            {
                RaiseError($"Texture type is not supported. Use a Bitmap instead.", 2);
            }

            return texture;
        }

        private IBitmapTex _getBitmapTex(ITexmap texMap, out float[] multiplyColor, bool raiseError = true)
        {
            multiplyColor = null;

            if (texMap == null)
            {
                return null;
            }

            var texture = _getBitmapTex(texMap, false);

            if (texture == null)
            {
                texture = _getMultiplyTexmap(texMap, out multiplyColor);

                if (texture == null && raiseError)
                {
                    RaiseError($"Texture type is not supported. Use a Bitmap or a RGB Multiply instead.", 2);
                }
            }

            return texture;
        }

        private IBitmapTex _getMultiplyTexmap(ITexmap texMap, out float[] multiplyColor)
        {
            multiplyColor = null;

            if (texMap == null)
            {
                return null;
            }

            if (texMap.ClassName == "RGB Multiply")
            {
                RaiseVerbose($"RGB Multiply found", 2);

                var block = texMap.GetParamBlockByID(0);
                if (block != null)
                {
                    Print(block, 2);

                    // Color #1
                    IBitmapTex bitmap1 = null;
                    bool map1Enable = block.GetInt(4, 0, 0) == 1;
                    if (map1Enable)
                    {
                        ITexmap map1 = block.GetTexmap(2, 0, 0);
                        bitmap1 = _getBitmapTex(map1, false);
                    }

                    // Color #2
                    IBitmapTex bitmap2 = null;
                    bool map2Enable = block.GetInt(5, 0, 0) == 1;
                    if (map2Enable)
                    {
                        ITexmap map2 = block.GetTexmap(3, 0, 0);
                        bitmap2 = _getBitmapTex(map2, false);
                    }

                    // Constraints
                    if (bitmap1 == null && bitmap2 == null)
                    {
                        RaiseError($"Specify one texture in the RGB Multiply map.", 2);
                        return null;
                    }
                    if (bitmap1 != null && bitmap2 != null)
                    {
                        RaiseError($"Specify only one texture in the RGB Multiply map.", 2);
                        return null;
                    }

                    // Return bitmap texture and multiplicative color
                    if (bitmap1 != null)
                    {
                        multiplyColor = block.GetColor(1, 0, 0).ToArray(); // Color #2
                        return bitmap1;
                    }
                    else
                    {
                        multiplyColor = block.GetColor(0, 0, 0).ToArray(); // Color #1
                        return bitmap2;
                    }
                }

                return null;
            }

            RaiseError($"Texture type is not supported. Use a Bitmap or RGB Multiply map instead.", 2);
            return null;
        }

        private string getSourcePath(ITexmap texMap)
        {
            IBitmapTex bitmapTex = _getBitmapTex(texMap);
            if (bitmapTex != null)
            {
                return bitmapTex.Map.FullFilePath;
            }
            else
            {
                return null;
            }
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

        private ITexmap _getTexMap(IIGameMaterial materialNode, string name)
        {
            for (int i = 0; i < materialNode.MaxMaterial.NumSubTexmaps; i++)
            {
                if (materialNode.MaxMaterial.GetSubTexmapSlotName(i) == name)
                {
                    return _getTexMap(materialNode, i);
                }
            }
            return null;
        }

        private bool isTextureOk(ITexmap texMap)
        {
            var texture = _getBitmapTex(texMap);
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
            IBitmapTex texture = _getBitmapTex(texMap);
            if (texture == null)
            {
                return null;
            }

            return TextureUtilities.LoadTexture(texture.Map.FullFilePath, this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Max;
using BabylonExport.Entities;
using System.Drawing;
using System.Drawing.Imaging;

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

            return ExportTexture(texMap, babylonScene, amount, allowCube, forceAlpha);
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

            nameText = (specularColorTexture != null ? Path.GetFileNameWithoutExtension(specularColorTexture.Map.FullFilePath) : ColorToStringName(specularColor)) +
                        Path.GetFileNameWithoutExtension(specularLevelTexture.Map.FullFilePath) + "_specularColor";

            var babylonTexture = new BabylonTexture
            {
                name = nameText+".jpg" // TODO - unsafe name, may conflict with another texture name
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
                var haveSameDimensions = _getMinimalBitmapDimensions(out width, out height, specularColorBitmap, specularLevelBitmap);
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
                    SaveBitmap(specularColorPreMultipliedBitmap, babylonScene.OutputPath, babylonTexture.name, ImageFormat.Jpeg);
                }
                else
                {
                    // Store created bitmap for further use in gltf export
                    babylonTexture.bitmap = specularColorPreMultipliedBitmap;
                }
            }

            return babylonTexture;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseColorTexMap"></param>
        /// <param name="alphaTexMap">Transparency weight map</param>
        /// <param name="baseColor"></param>
        /// <param name="alpha"></param>
        /// <param name="babylonScene"></param>
        /// <param name="materialName"></param>
        /// <returns></returns>
        private BabylonTexture ExportBaseColorAlphaTexture(ITexmap baseColorTexMap, ITexmap alphaTexMap, float[] baseColor, float alpha, BabylonScene babylonScene, string materialName)
        {
            // --- Babylon texture ---

            var baseColorTexture = _getBitmapTex(baseColorTexMap);
            var alphaTexture = _getBitmapTex(alphaTexMap);

            if (alphaTexture == null && baseColorTexture != null && alpha == 1)
            {
                if (baseColorTexture.AlphaSource == 0 &&
                    (baseColorTexture.Map.FullFilePath.EndsWith(".tif") || baseColorTexture.Map.FullFilePath.EndsWith(".tiff")))
                {
                    RaiseWarning($"Diffuse texture named {baseColorTexture.Map.FullFilePath} is a .tif file and its Alpha Source is 'Image Alpha' by default.", 3);
                    RaiseWarning($"If you don't want material to be in BLEND mode, set diffuse texture Alpha Source to 'None (Opaque)'", 3);
                }

                var extension = Path.GetExtension(baseColorTexture.Map.FullFilePath).ToLower();
                if (baseColorTexture.AlphaSource == 3 && // 'None (Opaque)'
                    extension == ".jpg" || extension == ".jpeg" || extension == ".bmp")
                {
                    // Copy base color image
                    return ExportTexture(baseColorTexture, babylonScene);
                }
            }

            // Use one as a reference for UVs parameters
            var texture = baseColorTexture != null ? baseColorTexture : alphaTexture;
            if (texture == null)
            {
                return null;
            }

            RaiseMessage("Export baseColor+Alpha texture", 2);

            string nameText = null;

            nameText = (baseColorTexture != null ? Path.GetFileNameWithoutExtension(baseColorTexture.Map.FullFilePath) : ColorToStringName(baseColor)) +
                        (alphaTexture != null ? Path.GetFileNameWithoutExtension(alphaTexture.Map.FullFilePath) : (""+ (int) (alpha * 255))) +
                        (alphaTexture == null && baseColorTexture == null ? materialName : "") + "_baseColor";

            var babylonTexture = new BabylonTexture
            {
                name = nameText // TODO - unsafe name, may conflict with another texture name
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
            babylonTexture.hasAlpha = isTextureOk(alphaTexMap) || (isTextureOk(baseColorTexMap) && baseColorTexture.AlphaSource == 0) || alpha < 1.0f;
            babylonTexture.getAlphaFromRGB = false;
            if ((!isTextureOk(alphaTexMap) && alpha == 1.0f && (isTextureOk(baseColorTexMap) && baseColorTexture.AlphaSource == 0)) &&
                (baseColorTexture.Map.FullFilePath.EndsWith(".tif") || baseColorTexture.Map.FullFilePath.EndsWith(".tiff")))
            {
                RaiseWarning($"Diffuse texture named {baseColorTexture.Map.FullFilePath} is a .tif file and its Alpha Source is 'Image Alpha' by default.", 3);
                RaiseWarning($"If you don't want material to be in BLEND mode, set diffuse texture Alpha Source to 'None (Opaque)'", 3);
            }

            if (!hasBaseColor && !hasAlpha)
            {
                return null;
            }

            // Set image format
            ImageFormat imageFormat = babylonTexture.hasAlpha ? ImageFormat.Png : ImageFormat.Jpeg;
            babylonTexture.name += imageFormat == ImageFormat.Png ? ".png" : ".jpg";

            // --- Merge baseColor and alpha maps ---

            if (exportParameters.writeTextures)
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
                    RaiseError("Base color and transparency color maps should have same dimensions", 3);
                }

                var getAlphaFromRGB = alphaTexture != null && ((alphaTexture.AlphaSource == 2) || (alphaTexture.AlphaSource == 3)); // 'RGB intensity' or 'None (Opaque)'

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
                    RaiseMessage($"Texture | write image '{babylonTexture.name}'", 3);
                    SaveBitmap(baseColorAlphaBitmap, babylonScene.OutputPath, babylonTexture.name, imageFormat);
                }
                else
                {
                    // Store created bitmap for further use in gltf export
                    babylonTexture.bitmap = baseColorAlphaBitmap;
                }
            }

            return babylonTexture;
        }

        private BabylonTexture ExportORMTexture(ITexmap ambientOcclusionTexMap, ITexmap roughnessTexMap, ITexmap metallicTexMap,  float metallic, float roughness, BabylonScene babylonScene, bool invertRoughness)
        {
            // --- Babylon texture ---

            var metallicTexture = _getBitmapTex(metallicTexMap);
            var roughnessTexture = _getBitmapTex(roughnessTexMap);
            var ambientOcclusionTexture = _getBitmapTex(ambientOcclusionTexMap);

            // Use metallic or roughness texture as a reference for UVs parameters
            var texture = metallicTexture != null ? metallicTexture : roughnessTexture;
            if (texture == null)
            {
                return null;
            }

            RaiseMessage("Export ORM texture", 2);

            var babylonTexture = new BabylonTexture
            {
                name = (ambientOcclusionTexMap != null ? Path.GetFileNameWithoutExtension(ambientOcclusionTexture.Map.FileName) : "") +
                       (roughnessTexMap != null ? Path.GetFileNameWithoutExtension(roughnessTexture.Map.FileName) : ("" + (int)(roughness * 255))) +
                       (metallicTexMap != null ? Path.GetFileNameWithoutExtension(metallicTexture.Map.FileName) : ("" + (int)(metallic * 255))) + ".jpg" // TODO - unsafe name, may conflict with another texture name
            };

            // UVs
            var uvGen = _exportUV(texture.UVGen, babylonTexture);

            // Is cube
            _exportIsCube(texture.Map.FullFilePath, babylonTexture, false);


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
                var haveSameDimensions = _getMinimalBitmapDimensions(out width, out height, metallicBitmap, roughnessBitmap, ambientOcclusionBitmap);
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
                    SaveBitmap(ormBitmap, babylonScene.OutputPath, babylonTexture.name, ImageFormat.Jpeg);
                }
                else
                {
                    // Store created bitmap for further use in gltf export
                    babylonTexture.bitmap = ormBitmap;
                }
            }

            return babylonTexture;
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

            var babylonTexture = new BabylonTexture
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

        // -------------------------
        // -- Export sub methods ---
        // -------------------------

        private BabylonTexture ExportTexture(ITexmap texMap, BabylonScene babylonScene, float amount = 1.0f, bool allowCube = false, bool forceAlpha = false)
        {
            IBitmapTex texture = _getBitmapTex(texMap);
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

            var validImageFormat = GetValidImageFormat(Path.GetExtension(sourcePath));
            if (validImageFormat == null)
            {
                // Image format is not supported by the exporter
                RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), 3);
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
                babylonTexture.isCube = false;
            }
            babylonTexture.originalPath = sourcePath;

            return babylonTexture;
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

            babylonTexture.uOffset = -uvGen.GetUOffs(0);
            babylonTexture.vOffset = -uvGen.GetVOffs(0);

            babylonTexture.uScale = uvGen.GetUScl(0);
            babylonTexture.vScale = uvGen.GetVScl(0);

            if (Path.GetExtension(babylonTexture.name).ToLower() == ".dds")
            {
                babylonTexture.vScale *= -1; // Need to invert Y-axis for DDS texture
            }

            babylonTexture.uAng = uvGen.GetUAng(0);
            babylonTexture.vAng = uvGen.GetVAng(0);
            babylonTexture.wAng = uvGen.GetWAng(0);


            // Fix offset according to the rotation
            // 3DS Max and babylon don't use the same origin for the rotation
            if(babylonTexture.wAng != 0f)
            {
                var angle = -babylonTexture.wAng;
                var cos = (float)Math.Cos(angle);
                var sin = (float)Math.Sin(angle);
                var u = babylonTexture.uOffset;
                var v = babylonTexture.vOffset;

                // uOffset
                babylonTexture.uOffset = u * cos;
                babylonTexture.vOffset = u * -sin;
                // vOffset
                babylonTexture.uOffset += v * sin;
                babylonTexture.vOffset += v * cos;
                // rotation
                babylonTexture.uOffset -= sin;
                babylonTexture.vOffset -= cos;
            }

            // Fix offset according to the scale
            // 3DS Max keep the tiling symmetrical
            if(babylonTexture.uScale != 0f)
            {
                babylonTexture.uOffset += (1f - babylonTexture.uScale) / 2f;
            }
            if(babylonTexture.vScale != 0f)
            {
                babylonTexture.vOffset += (1f - babylonTexture.vScale) * 1.5f;
            }

            // TODO - rotation and scale
            if (babylonTexture.wAng != 0f && (babylonTexture.uScale != 1f || babylonTexture.vScale != 1f))
            {
                RaiseWarning("Rotation and tiling (scale) on a texture are only supported separatly. You can use the map UV of the mesh for those transformation.", 3);
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

        private IBitmapTex _getBitmapTex(ITexmap texMap)
        {
            if (texMap == null || texMap.GetParamBlock(0) == null || texMap.GetParamBlock(0).Owner == null)
            {
                return null;
            }

            var texture = texMap.GetParamBlock(0).Owner as IBitmapTex;

            if (texture == null)
            {
                RaiseError($"Texture type is not supported. Use a Bitmap instead.", 2);
            }

            return texture;
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
                try
                {
                    switch (Path.GetExtension(absolutePath).ToLower())
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
                            RaiseError(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(absolutePath)), 3);
                            return null;
                    }
                }
                catch (Exception e)
                {
                    RaiseError(string.Format("Failed to load texture {0}: {1}", Path.GetFileName(absolutePath), e.Message), 3);
                    return null;
                }
            }
            else
            {
                RaiseError(string.Format("Texture {0} not found.", absolutePath), 3);
                return null;
            }
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
            if (exportParameters.writeTextures)
            {
                try
                {
                    if (File.Exists(sourcePath))
                    {
                        string imageFormat = Path.GetExtension(sourcePath).Substring(1).ToLower(); // remove the dot

                        if (validFormats.Contains(imageFormat))
                        {
                            if (sourcePath != destPath)
                            {
                                File.Copy(sourcePath, destPath, true);
                            }
                        }
                        else if (invalidFormats.Contains(imageFormat))
                        {
                            _convertToBitmapAndSave(sourcePath, destPath, imageFormat);
                        }
                        else
                        {
                            RaiseError(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), 3);
                        }
                    }
                    else RaiseError(string.Format("Texture not found: {0}", sourcePath), 3);
                }
                catch (Exception c)
                {
                    RaiseError(string.Format("Exporting texture {0} failed: {1}", sourcePath, c.ToString()), 3);
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
                    try
                    {
                        bitmap = GDImageLibrary._DDS.LoadImage(sourcePath);
                        SaveBitmap(bitmap, destPath, ImageFormat.Png);
                    }
                    catch (Exception e)
                    {
                        RaiseError(string.Format("Failed to convert texture {0} to png: {1}", Path.GetFileName(sourcePath), e.Message), 3);
                    }
                    break;
                case "tga":
                    // External library TargaImage.dll
                    try
                    {
                        bitmap = Paloma.TargaImage.LoadTargaImage(sourcePath);
                        SaveBitmap(bitmap, destPath, ImageFormat.Png);
                    }
                    catch (Exception e)
                    {
                        RaiseError(string.Format("Failed to convert texture {0} to png: {1}", Path.GetFileName(sourcePath), e.Message), 3);
                    }
                    break;
                case "bmp":
                    bitmap = new Bitmap(sourcePath);
                    SaveBitmap(bitmap, destPath, ImageFormat.Jpeg); // no alpha
                    break;
                case "tif":
                case "tiff":
                case "gif":
                    bitmap = new Bitmap(sourcePath);
                    SaveBitmap(bitmap, destPath, ImageFormat.Png);
                    break;
                case "jpeg":
                case "png":
                    File.Copy(sourcePath, destPath, true);
                    break;
                default:
                    RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), 3);
                    break;
            }
        }

        private void SaveBitmap(Bitmap bitmap, string path, ImageFormat imageFormat)
        {
            SaveBitmap(bitmap, Path.GetDirectoryName(path), Path.GetFileName(path), imageFormat);
        }

        private void SaveBitmap(Bitmap bitmap, string directoryName, string fileName, ImageFormat imageFormat)
        {
            List<char> invalidCharsInString = GetInvalidChars(directoryName, Path.GetInvalidPathChars());
            if (invalidCharsInString.Count > 0)
            {
                RaiseError($"Failed to save bitmap: directory name '{directoryName}' contains invalid character{(invalidCharsInString.Count > 1 ? "s" : "")} {invalidCharsInString.ToArray().ToString(false)}", 3);
                return;
            }
            invalidCharsInString = GetInvalidChars(fileName, Path.GetInvalidFileNameChars());
            if (invalidCharsInString.Count > 0)
            {
                RaiseError($"Failed to save bitmap: file name '{fileName}' contains invalid character{(invalidCharsInString.Count > 1 ? "s" : "")} {invalidCharsInString.ToArray().ToString(false)}", 3);
                return;
            }

            string path = Path.Combine(directoryName, fileName);
            using (FileStream fs = File.Open(path, FileMode.Create))
            {
                ImageCodecInfo encoder = GetEncoder(imageFormat);

                if (encoder != null)
                {
                    // Create an Encoder object based on the GUID for the Quality parameter category
                    EncoderParameters encoderParameters = new EncoderParameters(1);
                    EncoderParameter encoderQualityParameter = new EncoderParameter(Encoder.Quality, long.Parse(exportParameters.txtQuality));
                    encoderParameters.Param[0] = encoderQualityParameter;

                    bitmap.Save(fs, encoder, encoderParameters);
                }
                else
                {
                    bitmap.Save(fs, imageFormat);
                }
            }
        }

        private List<char> GetInvalidChars(string s, char[] invalidChars)
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

        private ImageCodecInfo GetEncoder(ImageFormat format)
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

        private string ColorToStringName(Color color)
        {
            return "" + color.R + color.G + color.B + color.A;
        }

        private string ColorToStringName(float[] color)
        {
            return "" + (int)(color[0] * 255) + (int)(color[1] * 255) + (int)(color[2] * 255);
        }
    }
}

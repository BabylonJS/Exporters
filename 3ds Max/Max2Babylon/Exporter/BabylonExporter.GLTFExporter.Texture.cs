using BabylonExport.Entities;
using GLTFExport.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        private static List<string> validGltfFormats = new List<string>(new string[] { "png", "jpg", "jpeg" });
        private static List<string> invalidGltfFormats = new List<string>(new string[] { "dds", "tga", "tif", "tiff", "bmp", "gif" });
        public const string KHR_texture_transform = "KHR_texture_transform";  // Name of the extension

        /// <summary>
        /// Export the texture using the parameters of babylonTexture except its name.
        /// Write the bitmap file
        /// </summary>
        /// <param name="babylonTexture"></param>
        /// <param name="bitmap"></param>
        /// <param name="name"></param>
        /// <param name="gltf"></param>
        /// <returns></returns>
        private GLTFTextureInfo ExportBitmapTexture(GLTF gltf, BabylonTexture babylonTexture, Bitmap bitmap = null, string name = null)
        {
            if (babylonTexture != null)
            {
                if (bitmap == null)
                {
                    bitmap = babylonTexture.bitmap;
                }
                if (name == null)
                {
                    name = babylonTexture.name;
                }
            }

            return ExportTexture(babylonTexture, gltf, name, () =>
            {
                var extension = Path.GetExtension(name).ToLower();

                // Write image to output
                if (exportParameters.writeTextures)
                {
                    var absolutePath = Path.Combine(gltf.OutputFolder, name);
                    var imageFormat = extension == ".jpg" ? System.Drawing.Imaging.ImageFormat.Jpeg : System.Drawing.Imaging.ImageFormat.Png;
                    RaiseMessage($"GLTFExporter.Texture | write image '{name}' to '{absolutePath}'", 3);
                    SaveBitmap(bitmap, absolutePath, imageFormat);
                }

                return extension.Substring(1); // remove the dot
            });
        }

        private GLTFTextureInfo ExportTexture(BabylonTexture babylonTexture, GLTF gltf)
        {
            return ExportTexture(babylonTexture, gltf, null, () =>
            {
                var sourcePath = babylonTexture.originalPath;

                if (sourcePath == null || sourcePath == "")
                {
                    RaiseWarning("Texture path is missing.", 3);
                    return null;
                }

                var validImageFormat = GetGltfValidImageFormat(Path.GetExtension(sourcePath));

                if (validImageFormat == null)
                {
                    // Image format is not supported by the exporter
                    RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), 3);
                    return null;
                }

                // Copy texture to output
                var destPath = Path.Combine(gltf.OutputFolder, babylonTexture.name);
                destPath = Path.ChangeExtension(destPath, validImageFormat);
                CopyGltfTexture(sourcePath, destPath);

                return validImageFormat;
            });
        }

        private GLTFTextureInfo ExportTexture(BabylonTexture babylonTexture, GLTF gltf, string name, Func<string> writeImageFunc)
        {
            if (babylonTexture == null)
            {
                return null;
            }

            if (name == null)
            {
                name = babylonTexture.name;
            }

            // Check for texture optimisation
            if (CheckIfImageIsRegistered(name))
            {
                var TextureComponent = GetRegisteredTexture(name);

                return TextureComponent;
            }

            RaiseMessage("GLTFExporter.Texture | Export texture named: " + name, 2);

            string validImageFormat = writeImageFunc.Invoke();
            if (validImageFormat == null)
            {
                return null;
            }

            name = Path.ChangeExtension(name, validImageFormat);

            // --------------------------
            // -------- Sampler ---------
            // --------------------------

            RaiseMessage("GLTFExporter.Texture | create sampler", 3);
            GLTFSampler gltfSampler = new GLTFSampler();
            gltfSampler.index = gltf.SamplersList.Count;
            gltf.SamplersList.Add(gltfSampler);

            // --- Retreive info from babylon texture ---
            // Mag and min filters
            GLTFSampler.TextureMagFilter? magFilter;
            GLTFSampler.TextureMinFilter? minFilter;
            getSamplingParameters(babylonTexture.samplingMode, out magFilter, out minFilter);
            gltfSampler.magFilter = magFilter;
            gltfSampler.minFilter = minFilter;
            // WrapS and wrapT
            gltfSampler.wrapS = getWrapMode(babylonTexture.wrapU);
            gltfSampler.wrapT = getWrapMode(babylonTexture.wrapV);


            // --------------------------
            // --------- Image ----------
            // --------------------------

            RaiseMessage("GLTFExporter.Texture | create image", 3);
            GLTFImage gltfImage = new GLTFImage
            {
                uri = name
            };

            gltfImage.index = gltf.ImagesList.Count;
            gltf.ImagesList.Add(gltfImage);
            switch (validImageFormat)
            {
                case "jpg":
                    gltfImage.FileExtension = "jpeg";
                    break;
                case "png":
                    gltfImage.FileExtension = "png";
                    break;
            }


            // --------------------------
            // -------- Texture ---------
            // --------------------------

            RaiseMessage("GLTFExporter.Texture | create texture", 3);
            var gltfTexture = new GLTFTexture
            {
                name = name,
                sampler = gltfSampler.index,
                source = gltfImage.index
            };
            gltfTexture.index = gltf.TexturesList.Count;
            gltf.TexturesList.Add(gltfTexture);


            // --------------------------
            // ------ TextureInfo -------
            // --------------------------
            var gltfTextureInfo = new GLTFTextureInfo
            {
                index = gltfTexture.index,
                texCoord = babylonTexture.coordinatesIndex
            };

            if (babylonTexture.uOffset != 0f || babylonTexture.vOffset != 0f || babylonTexture.uScale != 1f || babylonTexture.vScale != 1f || babylonTexture.wAng != 0f)
            {
                // Add texture extension if enabled in the export settings
                if (exportParameters.enableKHRTextureTransform)
                {
                    AddTextureTransformExtension(ref gltf, ref gltfTextureInfo, babylonTexture);
                }
                else
                {
                    RaiseWarning("GLTFExporter.Texture | KHR_texture_transform is not enabled, so the texture may look incorrect at runtime!");
                }
            }
            
            // Add the texture in the dictionary
            RegisterTexture(gltfTextureInfo, name);

            return gltfTextureInfo;
        }

        private GLTFTextureInfo ExportEmissiveTexture(BabylonStandardMaterial babylonMaterial, GLTF gltf, float[] defaultEmissive, float[] defaultDiffuse)
        {
            // Use one as a reference for UVs parameters
            var babylonTexture = babylonMaterial.emissiveTexture != null ? babylonMaterial.emissiveTexture : babylonMaterial.diffuseTexture;
            if (babylonTexture == null)
            {
                return null;
            }

            // Anticipate if a black texture is going to be export 
            if (babylonMaterial.emissiveTexture == null && defaultEmissive.IsAlmostEqualTo(new float[] { 0, 0, 0 }, 0))
            {
                return null;
            }

            // Check if the texture has already been exported
            if (GetRegisteredEmissive(babylonMaterial, defaultDiffuse, defaultEmissive) != null)
            {
                return GetRegisteredEmissive(babylonMaterial, defaultDiffuse, defaultEmissive);
            }

            Bitmap emissivePremultipliedBitmap = null;

            if (exportParameters.writeTextures)
            {
                // Emissive
                Bitmap emissiveBitmap = null;
                if (babylonMaterial.emissiveTexture != null)
                {
                    emissiveBitmap = LoadTexture(babylonMaterial.emissiveTexture.originalPath);
                }

                // Diffuse
                Bitmap diffuseBitmap = null;
                if (babylonMaterial.diffuseTexture != null)
                {
                    diffuseBitmap = LoadTexture(babylonMaterial.diffuseTexture.originalPath);
                }

                if (emissiveBitmap != null || diffuseBitmap != null)
                {
                    // Retreive dimensions
                    int width = 0;
                    int height = 0;
                    var haveSameDimensions = _getMinimalBitmapDimensions(out width, out height, emissiveBitmap, diffuseBitmap);
                    if (!haveSameDimensions)
                    {
                        RaiseError("Emissive and diffuse maps should have same dimensions", 2);
                    }

                    // Create pre-multiplied emissive map
                    emissivePremultipliedBitmap = new Bitmap(width, height);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            var _emissive = emissiveBitmap != null ? emissiveBitmap.GetPixel(x, y).toArrayRGB().Multiply(1f / 255.0f) : defaultEmissive;
                            var _diffuse = diffuseBitmap != null ? diffuseBitmap.GetPixel(x, y).toArrayRGB().Multiply(1f / 255.0f) : defaultDiffuse;

                            var emissivePremultiplied = _emissive.Multiply(_diffuse);

                            Color colorEmissivePremultiplied = Color.FromArgb(
                                (int)(emissivePremultiplied[0] * 255),
                                (int)(emissivePremultiplied[1] * 255),
                                (int)(emissivePremultiplied[2] * 255)
                            );
                            emissivePremultipliedBitmap.SetPixel(x, y, colorEmissivePremultiplied);
                        }
                    }
                }
            }

            var name = babylonMaterial.name + "_emissive.jpg";
            var emissiveTextureInfo = ExportBitmapTexture(gltf, babylonTexture, emissivePremultipliedBitmap, name);

            // Register the texture for optimisation
            RegisterEmissive(emissiveTextureInfo, babylonMaterial, defaultDiffuse, defaultEmissive);

            return emissiveTextureInfo;
        }

        private void getSamplingParameters(BabylonTexture.SamplingMode samplingMode, out GLTFSampler.TextureMagFilter? magFilter, out GLTFSampler.TextureMinFilter? minFilter)
        {
            switch (samplingMode)
            {
                case BabylonTexture.SamplingMode.NEAREST_NEAREST_MIPLINEAR:
                    magFilter = GLTFSampler.TextureMagFilter.NEAREST;
                    minFilter = GLTFSampler.TextureMinFilter.NEAREST_MIPMAP_LINEAR;
                    break;
                case BabylonTexture.SamplingMode.LINEAR_LINEAR_MIPNEAREST:
                    magFilter = GLTFSampler.TextureMagFilter.LINEAR;
                    minFilter = GLTFSampler.TextureMinFilter.LINEAR_MIPMAP_NEAREST;
                    break;
                case BabylonTexture.SamplingMode.LINEAR_LINEAR_MIPLINEAR:
                    magFilter = GLTFSampler.TextureMagFilter.LINEAR;
                    minFilter = GLTFSampler.TextureMinFilter.LINEAR_MIPMAP_LINEAR;
                    break;
                case BabylonTexture.SamplingMode.NEAREST_NEAREST_MIPNEAREST:
                    magFilter = GLTFSampler.TextureMagFilter.NEAREST;
                    minFilter = GLTFSampler.TextureMinFilter.NEAREST_MIPMAP_NEAREST;
                    break;
                case BabylonTexture.SamplingMode.NEAREST_LINEAR_MIPNEAREST:
                    magFilter = GLTFSampler.TextureMagFilter.NEAREST;
                    minFilter = GLTFSampler.TextureMinFilter.LINEAR_MIPMAP_NEAREST;
                    break;
                case BabylonTexture.SamplingMode.NEAREST_LINEAR_MIPLINEAR:
                    magFilter = GLTFSampler.TextureMagFilter.NEAREST;
                    minFilter = GLTFSampler.TextureMinFilter.LINEAR_MIPMAP_LINEAR;
                    break;
                case BabylonTexture.SamplingMode.NEAREST_LINEAR:
                    magFilter = GLTFSampler.TextureMagFilter.NEAREST;
                    minFilter = GLTFSampler.TextureMinFilter.LINEAR;
                    break;
                case BabylonTexture.SamplingMode.NEAREST_NEAREST:
                    magFilter = GLTFSampler.TextureMagFilter.NEAREST;
                    minFilter = GLTFSampler.TextureMinFilter.NEAREST;
                    break;
                case BabylonTexture.SamplingMode.LINEAR_NEAREST_MIPNEAREST:
                    magFilter = GLTFSampler.TextureMagFilter.LINEAR;
                    minFilter = GLTFSampler.TextureMinFilter.NEAREST_MIPMAP_NEAREST;
                    break;
                case BabylonTexture.SamplingMode.LINEAR_NEAREST_MIPLINEAR:
                    magFilter = GLTFSampler.TextureMagFilter.LINEAR;
                    minFilter = GLTFSampler.TextureMinFilter.NEAREST_MIPMAP_LINEAR;
                    break;
                case BabylonTexture.SamplingMode.LINEAR_LINEAR:
                    magFilter = GLTFSampler.TextureMagFilter.LINEAR;
                    minFilter = GLTFSampler.TextureMinFilter.LINEAR;
                    break;
                case BabylonTexture.SamplingMode.LINEAR_NEAREST:
                    magFilter = GLTFSampler.TextureMagFilter.LINEAR;
                    minFilter = GLTFSampler.TextureMinFilter.NEAREST;
                    break;
                default:
                    RaiseError("GLTFExporter.Texture | texture sampling mode not found", 3);
                    magFilter = null;
                    minFilter = null;
                    break;
            }
        }

        private GLTFSampler.TextureWrapMode? getWrapMode(BabylonTexture.AddressMode babylonTextureAdresseMode)
        {
            switch (babylonTextureAdresseMode)
            {
                case BabylonTexture.AddressMode.CLAMP_ADDRESSMODE:
                    return GLTFSampler.TextureWrapMode.CLAMP_TO_EDGE;
                case BabylonTexture.AddressMode.WRAP_ADDRESSMODE:
                    return GLTFSampler.TextureWrapMode.REPEAT;
                case BabylonTexture.AddressMode.MIRROR_ADDRESSMODE:
                    return GLTFSampler.TextureWrapMode.MIRRORED_REPEAT;
                default:
                    RaiseError("GLTFExporter.Texture | texture wrap mode not found", 3);
                    return null;
            }
        }

        private string GetGltfValidImageFormat(string extension)
        {
            return _getValidImageFormat(extension, validGltfFormats, invalidGltfFormats);
        }

        private void CopyGltfTexture(string sourcePath, string destPath)
        {
            _copyTexture(sourcePath, destPath, validGltfFormats, invalidGltfFormats);
        }

        /// <summary>
        /// Add the KHR_texture_transform to the glTF file
        /// </summary>
        /// <param name="gltf"></param>
        /// <param name="babylonMaterial"></param>
        private void AddTextureTransformExtension(ref GLTF gltf, ref GLTFTextureInfo gltfTextureInfo, BabylonTexture babylonTexture)
        {
            if (!gltf.extensionsUsed.Contains(KHR_texture_transform))
            {
                gltf.extensionsUsed.Add(KHR_texture_transform);
            }
            if (!gltf.extensionsRequired.Contains(KHR_texture_transform))
            {
                gltf.extensionsRequired.Add(KHR_texture_transform);
            }

            float angle = babylonTexture.wAng;
            float angleDirect = -babylonTexture.wAng;

            KHR_texture_transform textureTransform = new KHR_texture_transform
            {
                offset = new float[] { babylonTexture.uOffset, -babylonTexture.vOffset },
                rotation = angle,
                scale = new float[] { babylonTexture.uScale, babylonTexture.vScale },
                texCoord = babylonTexture.coordinatesIndex
            };

            textureTransform.offset[1] += 1 - babylonTexture.vScale;    // update vOffset according to the vScale
            textureTransform.offset[0] += (float)(0.5 * (1 - (Math.Cos(angleDirect) - Math.Sin(angleDirect)))); // update uOffset according to the rotation
            textureTransform.offset[1] += (float)(0.5 * (1 - (Math.Sin(angleDirect) + Math.Cos(angleDirect)))); // update vOffset according to the rotation

            if (gltfTextureInfo.extensions == null)
            {
                gltfTextureInfo.extensions = new GLTFExtensions();
            }
            gltfTextureInfo.extensions[KHR_texture_transform] = textureTransform;
        }
    }
}

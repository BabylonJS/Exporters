using BabylonExport;
using BabylonExport.Entities;
using GLTFExport.Entities;
using Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Babylon2GLTF
{
    partial class GLTFExporter
    {
        public const string KHR_texture_transform = "KHR_texture_transform";  // Name of the extension
        private Dictionary<string, GLTFTextureInfo> glTFTextureInfoMap = new Dictionary<string, GLTFTextureInfo>();
        private Dictionary<string, GLTFImage> glTFImageMap = new Dictionary<string, GLTFImage>();
        public string relativeTextureFolder = "";
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
                else
                {
                    if(bitmap != babylonTexture.bitmap)
                    {
                        babylonTexture = new BabylonTexture(babylonTexture);
                        babylonTexture.bitmap = bitmap;
                    }
                }
                if (name == null)
                {
                    name = babylonTexture.name;
                }
            }

            return ExportTexture(babylonTexture, gltf, name);
        }

        private GLTFTextureInfo ExportTexture(BabylonTexture babylonTexture, GLTF gltf)
        {
            return ExportTexture(babylonTexture, gltf, null);
        }

        private GLTFTextureInfo ExportTexture(BabylonTexture babylonTexture, GLTF gltf, string name)
        {
            if (babylonTexture == null)
            {
                return null;
            }

            if (name == null)
            {
                name = babylonTexture.name;
            }

            logger.RaiseMessage("GLTFExporter.Texture | Export texture named: " + name, 2);

            if (glTFTextureInfoMap.ContainsKey(babylonTexture.Id))
            {
                return glTFTextureInfoMap[babylonTexture.Id];
            }
            else
            {
                var sourcePath = babylonTexture.originalPath;
                if (babylonTexture.bitmap != null)
                {
                    sourcePath = Path.Combine(gltf.OutputFolder, name);
                }
                
                if (sourcePath == null || sourcePath == "")
                {
                    logger.RaiseWarning("Texture path is missing.", 3);
                    return null;
                }

                var validImageFormat = TextureUtilities.GetValidImageFormat(Path.GetExtension(sourcePath));

                if (validImageFormat == null)
                {
                    // Image format is not supported by the exporter
                    logger.RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), 3);
                    return null;
                }

                var destPath = Path.Combine(gltf.OutputFolder, name);
                destPath = Path.ChangeExtension(destPath, validImageFormat);

                name = Path.ChangeExtension(name, validImageFormat);

                // --------------------------
                // -------- Sampler ---------
                // --------------------------
                logger.RaiseMessage("GLTFExporter.Texture | create sampler", 3);
                GLTFSampler gltfSampler = new GLTFSampler();
                gltfSampler.index = gltf.SamplersList.Count;
                
                // --- Retrieve info from babylon texture ---
                // Mag and min filters
                GLTFSampler.TextureMagFilter? magFilter;
                GLTFSampler.TextureMinFilter? minFilter;
                getSamplingParameters(babylonTexture.samplingMode, out magFilter, out minFilter);
                gltfSampler.magFilter = magFilter;
                gltfSampler.minFilter = minFilter;
                // WrapS and wrapT
                gltfSampler.wrapS = getWrapMode(babylonTexture.wrapU);
                gltfSampler.wrapT = getWrapMode(babylonTexture.wrapV);

                var matchingSampler = gltf.SamplersList.FirstOrDefault(sampler => sampler.wrapS == gltfSampler.wrapS && sampler.wrapT == gltfSampler.wrapT && sampler.magFilter == gltfSampler.magFilter && sampler.minFilter == gltfSampler.minFilter);
                if (matchingSampler != null)
                {
                    gltfSampler = matchingSampler;
                }
                else
                {
                    gltf.SamplersList.Add(gltfSampler);
                }


                // --------------------------
                // --------- Image ----------
                // --------------------------

                logger.RaiseMessage("GLTFExporter.Texture | create image", 3);
                GLTFImage gltfImage = null;
                if (glTFImageMap.ContainsKey(name))
                {
                    gltfImage = glTFImageMap[name];
                }
                else
                {
                    string textureUri = name;
                    if (!string.IsNullOrWhiteSpace(exportParameters.textureFolder))
                    {
                        textureUri = PathUtilities.GetRelativePath( exportParameters.outputPath,exportParameters.textureFolder);
                        textureUri = Path.Combine(textureUri, name);
                    }
                    gltfImage = new GLTFImage
                    {
                        uri = textureUri
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
                    if (exportParameters.outputFormat == "glb")
                    {
                        var imageBufferView = WriteImageToGltfBuffer(gltf, gltfImage, sourcePath, babylonTexture.bitmap);
                        gltfImage.uri = null;
                        gltfImage.bufferView = imageBufferView.index;
                        gltfImage.mimeType = "image/" + gltfImage.FileExtension;
                    }
                    else
                    {
                        if (exportParameters.writeTextures)
                        {
                            if (babylonTexture.bitmap != null)
                            {
                                // We may have modified this texture image, copy the buffer contents to disk
                                var extension = Path.GetExtension(name).ToLower();
                                var imageFormat = extension == ".jpg" ? System.Drawing.Imaging.ImageFormat.Jpeg : System.Drawing.Imaging.ImageFormat.Png;
                                logger.RaiseMessage($"GLTFExporter.Texture | write image '{name}' to '{destPath}'", 3);
                                TextureUtilities.SaveBitmap(babylonTexture.bitmap, destPath, imageFormat, exportParameters.txtQuality, logger);
                            }
                            else
                            {
                                // Copy texture from source to output
                                TextureUtilities.CopyTexture(sourcePath, destPath, exportParameters.txtQuality, logger);
                            }
                        }
                    }
                    glTFImageMap.Add(name, gltfImage);
                }

                // --------------------------
                // -------- Texture ---------
                // --------------------------

                logger.RaiseMessage("GLTFExporter.Texture | create texture", 3);
                var gltfTexture = new GLTFTexture
                {
                    name = name,
                    sampler = gltfSampler.index,
                    source = gltfImage.index
                };
                gltfTexture.index = gltf.TexturesList.Count;

                if (!CheckIfImageIsRegistered(name))
                {
                    gltf.TexturesList.Add(gltfTexture);
                }
                else
                {
                    gltfTexture = gltf.TexturesList[GetRegisteredTexture(gltfTexture.name).index];
                }

                // --------------------------
                // ------ TextureInfo -------
                // --------------------------
                var gltfTextureInfo = new GLTFTextureInfo
                {
                    index = gltfTexture.index,
                    texCoord = babylonTexture.coordinatesIndex
                };

                if (!(babylonTexture.uOffset == 0) || !(babylonTexture.vOffset == 0) || !(babylonTexture.uScale == 1) || !(babylonTexture.vScale == 1) || !(babylonTexture.wAng == 0))
                {
                    // Add texture extension if enabled in the export settings
                    if (exportParameters.enableKHRTextureTransform)
                    {
                        AddTextureTransformExtension(ref gltf, ref gltfTextureInfo, babylonTexture);
                    }
                    else
                    {
                        logger.RaiseWarning("GLTFExporter.Texture | KHR_texture_transform is not enabled, so the texture may look incorrect at runtime!", 3);
                        logger.RaiseWarning("GLTFExporter.Texture | KHR_texture_transform is not enabled, so the texture may look incorrect at runtime!", 3);
                    }
                }
                var textureID = name + TextureTransformID(gltfTextureInfo);
                // Check for texture optimization.  This is done here after the texture transform has been potentially applied to the texture extension
                if (CheckIfImageIsRegistered(textureID))
                {
                    var textureComponent = GetRegisteredTexture(textureID);

                    return textureComponent;
                }

                // Add the texture in the dictionary
                RegisterTexture(gltfTextureInfo, textureID);
                glTFTextureInfoMap[babylonTexture.Id] = gltfTextureInfo;

                return gltfTextureInfo;
            }
        }

        private string TextureTransformID(GLTFTextureInfo gltfTextureInfo)
        {
            if (gltfTextureInfo.extensions == null || !gltfTextureInfo.extensions.ContainsKey(KHR_texture_transform))
            {
                return "";
            }
            else { 
                // Set an id for the texture transform and append to the name
                KHR_texture_transform textureTransform = gltfTextureInfo.extensions[GLTFExporter.KHR_texture_transform] as KHR_texture_transform;
                var offsetID = textureTransform.offset[0] + "_" + textureTransform.offset[1];
                var rotationID = textureTransform.rotation.ToString();
                var scaleID = textureTransform.scale[0] + "_" + textureTransform.scale[1];
                var textureTransformID = offsetID + "_" + rotationID + "_" + scaleID;

                return textureTransformID;
            }
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
                    emissiveBitmap = TextureUtilities.LoadTexture(babylonMaterial.emissiveTexture.originalPath, logger);
                }

                // Diffuse
                Bitmap diffuseBitmap = null;
                if (babylonMaterial.diffuseTexture != null)
                {
                    diffuseBitmap = TextureUtilities.LoadTexture(babylonMaterial.diffuseTexture.originalPath, logger);
                }

                if (emissiveBitmap != null || diffuseBitmap != null)
                {
                    // Retreive dimensions
                    int width = 0;
                    int height = 0;
                    var haveSameDimensions = TextureUtilities.GetMinimalBitmapDimensions(out width, out height, emissiveBitmap, diffuseBitmap);
                    if (!haveSameDimensions)
                    {
                        logger.RaiseError("Emissive and diffuse maps should have same dimensions", 2);
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

            var emissiveTextureInfo = ExportBitmapTexture(gltf, babylonTexture, emissivePremultipliedBitmap);

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
                    logger.RaiseError("GLTFExporter.Texture | texture sampling mode not found", 3);
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
                    logger.RaiseError("GLTFExporter.Texture | texture wrap mode not found", 3);
                    return null;
            }
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
                scale = new float[] { babylonTexture.uScale, -babylonTexture.vScale },
                texCoord = babylonTexture.coordinatesIndex
            };


            if (gltfTextureInfo.extensions == null)
            {
                gltfTextureInfo.extensions = new GLTFExtensions();
            }
            gltfTextureInfo.extensions[KHR_texture_transform] = textureTransform;
        }

        private GLTFBufferView WriteImageToGltfBuffer(GLTF gltf, GLTFImage gltfImage, string imageSourcePath = null, Bitmap imageBitmap = null)
        {
            byte[] imageBytes = null;
            if (imageBitmap != null)
            {
                ImageConverter converter = new ImageConverter();
                imageBytes = (byte[])converter.ConvertTo(imageBitmap, typeof(byte[]));
            }
            else
            {
                imageBytes = File.ReadAllBytes(imageSourcePath);
            }

            // Chunk must be padded with trailing zeros (0x00) to satisfy alignment requirements
            imageBytes = padChunk(imageBytes, 4, 0x00);

            // BufferView - Image
            var buffer = gltf.buffer;
            var imageBufferView = new GLTFBufferView
            {
                name = "bufferViewImage",
                buffer = buffer.index,
                Buffer = buffer,
                byteOffset = buffer.byteLength
            };
            imageBufferView.index = gltf.BufferViewsList.Count;
            gltf.BufferViewsList.Add(imageBufferView);

            imageBufferView.bytesList.AddRange(imageBytes);
            imageBufferView.byteLength += imageBytes.Length;
            imageBufferView.Buffer.byteLength += imageBytes.Length;
            imageBufferView.Buffer.bytesList.AddRange(imageBufferView.bytesList);

            return imageBufferView;
        }
    }
}

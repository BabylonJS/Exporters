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
                var extension = Path.GetExtension(name);

                // Write image to output
                if (CopyTexturesToOutput)
                {
                    var absolutePath = Path.Combine(gltf.OutputFolder, name);
                    var imageFormat = extension == ".jpg" ? System.Drawing.Imaging.ImageFormat.Jpeg : System.Drawing.Imaging.ImageFormat.Png;
                    RaiseMessage($"GLTFExporter.Texture | write image '{name}' to '{absolutePath}'", 2);
                    bitmap.Save(absolutePath, imageFormat);
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
                    RaiseWarning("Texture path is missing.", 2);
                    return null;
                }

                var validImageFormat = GetGltfValidImageFormat(Path.GetExtension(sourcePath));

                if (validImageFormat == null)
                {
                    // Image format is not supported by the exporter
                    RaiseWarning(string.Format("Format of texture {0} is not supported by the exporter. Consider using a standard image format like jpg or png.", Path.GetFileName(sourcePath)), 2);
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

            RaiseMessage("GLTFExporter.Texture | Export texture named: " + name, 1);

            string validImageFormat = writeImageFunc.Invoke();
            if (validImageFormat == null)
            {
                return null;
            }

            name = Path.ChangeExtension(name, validImageFormat);

            // --------------------------
            // -------- Sampler ---------
            // --------------------------

            RaiseMessage("GLTFExporter.Texture | create sampler", 2);
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

            RaiseMessage("GLTFExporter.Texture | create image", 2);
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

            RaiseMessage("GLTFExporter.Texture | create texture", 2);
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
                index = gltfTexture.index
            };

            return gltfTextureInfo;
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
                    RaiseError("GLTFExporter.Texture | texture sampling mode not found");
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
                    RaiseError("GLTFExporter.Texture | texture wrap mode not found");
                    return null;
            }
        }

        private string GetGltfValidImageFormat(string extension)
        {
            return _getValidImageFormat(extension, validGltfFormats, invalidGltfFormats);
        }

        private void CopyGltfTexture(string sourcePath, string destPath)
        {
            _copyTexture(sourcePath, destPath, validFormats, invalidFormats);
        }
    }
}

using BabylonExport.Entities;
using Utilities;
using GLTFExport.Entities;
using System;
using System.Drawing;
using System.IO;

namespace Babylon2GLTF
{
    partial class GLTFExporter
    {
        private void ExportMaterial(BabylonMaterial babylonMaterial, GLTF gltf)
        {
            var name = babylonMaterial.name;
            var id = babylonMaterial.id;
            logger.RaiseMessage("GLTFExporter.Material | Export material named: " + name, 1);

            GLTFMaterial gltfMaterial = null;
            IGLTFMaterialExporter customMaterialExporter = exportParameters.customGLTFMaterialExporter;
            if (customMaterialExporter != null && customMaterialExporter.GetGltfMaterial(babylonMaterial, gltf, logger, out gltfMaterial))
            {
                gltfMaterial.index = gltf.MaterialsList.Count;
                gltf.MaterialsList.Add(gltfMaterial);
            }
            else if (babylonMaterial is BabylonStandardMaterial babylonStandardMaterial)
            {
                // --- prints ---
                #region prints

                logger.RaiseVerbose("GLTFExporter.Material | babylonMaterial data", 2);
                logger.RaiseVerbose("GLTFExporter.Material | babylonMaterial.alpha=" + babylonMaterial.alpha, 3);
                logger.RaiseVerbose("GLTFExporter.Material | babylonMaterial.alphaMode=" + babylonMaterial.alphaMode, 3);
                logger.RaiseVerbose("GLTFExporter.Material | babylonMaterial.backFaceCulling=" + babylonMaterial.backFaceCulling, 3);
                logger.RaiseVerbose("GLTFExporter.Material | babylonMaterial.wireframe=" + babylonMaterial.wireframe, 3);

                // Ambient
                for (int i = 0; i < babylonStandardMaterial.ambient.Length; i++)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.ambient[" + i + "]=" + babylonStandardMaterial.ambient[i], 3);
                }

                // Diffuse
                logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.diffuse.Length=" + babylonStandardMaterial.diffuse.Length, 3);
                for (int i = 0; i < babylonStandardMaterial.diffuse.Length; i++)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.diffuse[" + i + "]=" + babylonStandardMaterial.diffuse[i], 3);
                }
                if (babylonStandardMaterial.diffuseTexture == null)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.diffuseTexture=null", 3);
                }
                else
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.diffuseTexture.name=" + babylonStandardMaterial.diffuseTexture.name, 3);
                }

                // Normal / bump
                if (babylonStandardMaterial.bumpTexture == null)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.bumpTexture=null", 3);
                }
                else
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.bumpTexture.name=" + babylonStandardMaterial.bumpTexture.name, 3);
                }

                // Opacity
                if (babylonStandardMaterial.opacityTexture == null)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.opacityTexture=null", 3);
                }
                else
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.opacityTexture.name=" + babylonStandardMaterial.opacityTexture.name, 3);
                }

                // Specular
                for (int i = 0; i < babylonStandardMaterial.specular.Length; i++)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.specular[" + i + "]=" + babylonStandardMaterial.specular[i], 3);
                }
                logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.specularPower=" + babylonStandardMaterial.specularPower, 3);
                if (babylonStandardMaterial.specularTexture == null)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.specularTexture=null", 3);
                }
                else
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.specularTexture.name=" + babylonStandardMaterial.specularTexture.name, 3);
                }

                // Occlusion
                if (babylonStandardMaterial.ambientTexture == null)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.ambientTexture=null", 3);
                }
                else
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.ambientTexture.name=" + babylonStandardMaterial.ambientTexture.name, 3);
                }

                // Emissive
                for (int i = 0; i < babylonStandardMaterial.emissive.Length; i++)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.emissive[" + i + "]=" + babylonStandardMaterial.emissive[i], 3);
                }
                if (babylonStandardMaterial.emissiveTexture == null)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.emissiveTexture=null", 3);
                }
                else
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.emissiveTexture.name=" + babylonStandardMaterial.emissiveTexture.name, 3);
                }
                #endregion

                // --------------------------------
                // --------- gltfMaterial ---------
                // --------------------------------
                logger.RaiseMessage("GLTFExporter.Material | create gltfMaterial", 2);
                gltfMaterial = new GLTFMaterial
                {
                    name = name
                };
                gltfMaterial.id = babylonMaterial.id;
                gltfMaterial.index = gltf.MaterialsList.Count;
                gltf.MaterialsList.Add(gltfMaterial);

                //Custom user properties
                if (babylonStandardMaterial.metadata != null && babylonStandardMaterial.metadata.Count != 0)
                {
                    gltfMaterial.extras = babylonStandardMaterial.metadata;
                }

                // Alpha
                GLTFMaterial.AlphaMode alphaMode;
                float? alphaCutoff;
                getAlphaMode(babylonStandardMaterial, out alphaMode, out alphaCutoff);
                gltfMaterial.alphaMode = alphaMode;
                if (alphaCutoff.HasValue && alphaCutoff.Value != 0.5f) // do not export glTF default value
                {
                    gltfMaterial.alphaCutoff = alphaCutoff;
                }

                // DoubleSided
                gltfMaterial.doubleSided = !babylonMaterial.backFaceCulling;

                // Normal
                gltfMaterial.normalTexture = ExportTexture(babylonStandardMaterial.bumpTexture, gltf);

                // Occlusion
                gltfMaterial.occlusionTexture = ExportTexture(babylonStandardMaterial.ambientTexture, gltf);

                // Emissive
                gltfMaterial.emissiveFactor = babylonStandardMaterial.emissive;
                // linkEmissiveWithDiffuse attribute doesn't have an equivalent in gltf format
                // When true, the emissive texture needs to be manually multiplied with diffuse texture
                // Otherwise, the emissive texture is assumed to be already pre-multiplied
                if (babylonStandardMaterial.linkEmissiveWithDiffuse)
                {
                    // Even when no emissive texture is provided, the self illumination value needs to be multiplied to the diffuse texture in order to get the pre-multiplied emissive (texture)
                    if (babylonStandardMaterial.emissiveTexture != null || babylonStandardMaterial.selfIllum > 0)
                    {
                        // Default emissive is the raw value of the self illumination
                        // It is not the babylon emissive value which is already pre-multiplied with diffuse color
                        float[] defaultEmissive = new float[] { 1, 1, 1 }.Multiply(babylonStandardMaterial.selfIllum);
                        gltfMaterial.emissiveTexture = ExportEmissiveTexture(babylonStandardMaterial, gltf, defaultEmissive, babylonStandardMaterial.diffuse);
                    }
                }
                else
                {
                    gltfMaterial.emissiveTexture = ExportTexture(babylonStandardMaterial.emissiveTexture, gltf);
                }

                // Constraints
                if (gltfMaterial.emissiveTexture != null)
                {
                    gltfMaterial.emissiveFactor = new[] { 1.0f, 1.0f, 1.0f };
                }

                // --------------------------------
                // --- gltfPbrMetallicRoughness ---
                // --------------------------------

                logger.RaiseMessage("GLTFExporter.Material | create gltfPbrMetallicRoughness", 2);
                var gltfPbrMetallicRoughness = new GLTFPBRMetallicRoughness();
                gltfMaterial.pbrMetallicRoughness = gltfPbrMetallicRoughness;

                // --- Global ---

                SpecularGlossiness _specularGlossiness = new SpecularGlossiness
                {
                    diffuse = new BabylonColor3(babylonStandardMaterial.diffuse),
                    opacity = babylonMaterial.alpha,
                    specular = new BabylonColor3(babylonStandardMaterial.specular),
                    glossiness = babylonStandardMaterial.specularPower / 256
                };

                MetallicRoughness _metallicRoughness = ConvertToMetallicRoughness(_specularGlossiness, true);

                // Base color
                gltfPbrMetallicRoughness.baseColorFactor = new float[4]
                {
                    _metallicRoughness.baseColor.r,
                    _metallicRoughness.baseColor.g,
                    _metallicRoughness.baseColor.b,
                    _metallicRoughness.opacity
                };

                // Metallic roughness
                gltfPbrMetallicRoughness.metallicFactor = _metallicRoughness.metallic;
                gltfPbrMetallicRoughness.roughnessFactor = _metallicRoughness.roughness;


                // --- Textures ---
                var babylonTexture = babylonStandardMaterial.diffuseTexture != null ? babylonStandardMaterial.diffuseTexture :
                                     babylonStandardMaterial.specularTexture != null ? babylonStandardMaterial.specularTexture :
                                     babylonStandardMaterial.opacityTexture != null ? babylonStandardMaterial.opacityTexture :
                                     null;

                if (babylonTexture != null)
                {
                    //Check if the texture already exist
                    var _key = SetStandText(babylonStandardMaterial);

                    if (GetStandTextInfo(_key) != null)
                    {
                        var _pairBCMR = GetStandTextInfo(_key);
                        gltfPbrMetallicRoughness.baseColorTexture = _pairBCMR.baseColor;
                        gltfPbrMetallicRoughness.metallicRoughnessTexture = _pairBCMR.metallicRoughness;
                    }
                    else
                    {

                        bool isAlphaInTexture = (isTextureOk(babylonStandardMaterial.diffuseTexture) && babylonStandardMaterial.diffuseTexture.hasAlpha) ||
                                                  isTextureOk(babylonStandardMaterial.opacityTexture);

                        Bitmap baseColorBitmap = null;
                        Bitmap metallicRoughnessBitmap = null;
                        string baseColorBitmapName = null;
                        string metallicRoughnessBitmapName = null;

                        GLTFTextureInfo textureInfoBC = null;
                        GLTFTextureInfo textureInfoMR = null;

                        if (exportParameters.writeTextures)
                        {
                            // Diffuse
                            Bitmap diffuseBitmap = null;
                            if (babylonStandardMaterial.diffuseTexture != null)
                            {
                                if (babylonStandardMaterial.diffuseTexture.bitmap != null)
                                {
                                    // Diffuse color map has been computed by the exporter
                                    diffuseBitmap = babylonStandardMaterial.diffuseTexture.bitmap;
                                }
                                else
                                {
                                    // Diffuse color map is straight input
                                    diffuseBitmap = TextureUtilities.LoadTexture(babylonStandardMaterial.diffuseTexture.originalPath, logger);
                                    babylonStandardMaterial.diffuseTexture.bitmap = diffuseBitmap;
                                }
                            }

                            // Specular
                            Bitmap specularBitmap = null;
                            if (babylonStandardMaterial.specularTexture != null)
                            {
                                if (babylonStandardMaterial.specularTexture.bitmap != null)
                                {
                                    // Specular color map has been computed by the exporter
                                    specularBitmap = babylonStandardMaterial.specularTexture.bitmap;
                                }
                                else
                                {
                                    // Specular color map is straight input
                                    specularBitmap = TextureUtilities.LoadTexture(babylonStandardMaterial.specularTexture.originalPath, logger);
                                }
                            }

                            // Opacity / Alpha / Transparency
                            Bitmap opacityBitmap = null;
                            if ((babylonStandardMaterial.diffuseTexture == null || babylonStandardMaterial.diffuseTexture.hasAlpha == false) && babylonStandardMaterial.opacityTexture != null)
                            {
                                opacityBitmap = TextureUtilities.LoadTexture(babylonStandardMaterial.opacityTexture.originalPath, logger);
                            }

                            if ((diffuseBitmap != null && (specularBitmap != null || opacityBitmap != null)) || specularBitmap != null || opacityBitmap != null)
                            {
                                // Retreive dimensions
                                int width = 0;
                                int height = 0;
                                var haveSameDimensions = TextureUtilities.GetMinimalBitmapDimensions(out width, out height, diffuseBitmap, specularBitmap, opacityBitmap);
                                if (!haveSameDimensions)
                                {
                                    logger.RaiseError("Diffuse, specular and opacity maps should have same dimensions", 2);
                                }
                                baseColorBitmapName = (babylonStandardMaterial.diffuseTexture != null ? Path.GetFileNameWithoutExtension(babylonStandardMaterial.diffuseTexture.name) : TextureUtilities.ColorToStringName(Color.FromArgb(255, (int)(babylonStandardMaterial.diffuse[0]), (int)(babylonStandardMaterial.diffuse[1]), (int)(babylonStandardMaterial.diffuse[2]))))
                                                    + (babylonStandardMaterial.opacityTexture != null ? "_alpha_" + Path.GetFileNameWithoutExtension(babylonStandardMaterial.opacityTexture.name) : "")
                                                    + ".png";

                                metallicRoughnessBitmapName = "MR_" + baseColorBitmapName
                                                            + "_spec_" + (babylonStandardMaterial.specularTexture != null ? Path.GetFileNameWithoutExtension(babylonStandardMaterial.specularTexture.name) : TextureUtilities.ColorToStringName(Color.FromArgb(255, (int)(babylonStandardMaterial.specular[0]), (int)(babylonStandardMaterial.specular[1]), (int)(babylonStandardMaterial.specular[2]))))
                                                            + "_gloss_" + babylonStandardMaterial.specularPower
                                                            +".png";
                                // Create baseColor+alpha and metallic+roughness maps
                                baseColorBitmap = new Bitmap(width, height);
                                metallicRoughnessBitmap = new Bitmap(width, height);
                                for (int x = 0; x < width; x++)
                                {
                                    for (int y = 0; y < height; y++)
                                    {
                                        SpecularGlossiness specularGlossinessTexture = new SpecularGlossiness
                                        {
                                            diffuse = diffuseBitmap != null ? new BabylonColor3(diffuseBitmap.GetPixel(x, y)) :
                                                        _specularGlossiness.diffuse,
                                            opacity = diffuseBitmap != null && babylonStandardMaterial.diffuseTexture.hasAlpha ? diffuseBitmap.GetPixel(x, y).A / 255.0f :
                                                        opacityBitmap != null && babylonStandardMaterial.opacityTexture.getAlphaFromRGB ? opacityBitmap.GetPixel(x, y).R / 255.0f :
                                                        opacityBitmap != null && babylonStandardMaterial.opacityTexture.getAlphaFromRGB == false ? opacityBitmap.GetPixel(x, y).A / 255.0f :
                                                        _specularGlossiness.opacity,
                                            specular = specularBitmap != null ? new BabylonColor3(specularBitmap.GetPixel(x, y)) :
                                                        _specularGlossiness.specular,
                                            glossiness = babylonStandardMaterial.useGlossinessFromSpecularMapAlpha && specularBitmap != null ? specularBitmap.GetPixel(x, y).A / 255.0f :
                                                            _specularGlossiness.glossiness
                                        };

                                        var displayPrints = x == width / 2 && y == height / 2;
                                        MetallicRoughness metallicRoughnessTexture = ConvertToMetallicRoughness(specularGlossinessTexture, displayPrints);

                                        Color colorBase = Color.FromArgb(
                                            (int)(metallicRoughnessTexture.opacity * 255),
                                            (int)(metallicRoughnessTexture.baseColor.r * 255),
                                            (int)(metallicRoughnessTexture.baseColor.g * 255),
                                            (int)(metallicRoughnessTexture.baseColor.b * 255)
                                        );
                                        baseColorBitmap.SetPixel(x, y, colorBase);

                                        // The metalness values are sampled from the B channel.
                                        // The roughness values are sampled from the G channel.
                                        // These values are linear. If other channels are present (R or A), they are ignored for metallic-roughness calculations.
                                        Color colorMetallicRoughness = Color.FromArgb(
                                            0,
                                            (int)(metallicRoughnessTexture.roughness * 255),
                                            (int)(metallicRoughnessTexture.metallic * 255)
                                        );
                                        metallicRoughnessBitmap.SetPixel(x, y, colorMetallicRoughness);
                                    }
                                }
                            }
                        }

                        //export textures
                        if (baseColorBitmap != null || babylonTexture.bitmap != null)
                        {
                            textureInfoBC = ExportBitmapTexture(gltf, babylonTexture, baseColorBitmap, baseColorBitmapName);
                            gltfPbrMetallicRoughness.baseColorTexture = textureInfoBC;
                        }

                        if (isTextureOk(babylonStandardMaterial.specularTexture))
                        {
                            textureInfoMR = ExportBitmapTexture(gltf, babylonTexture, metallicRoughnessBitmap, metallicRoughnessBitmapName);
                            gltfPbrMetallicRoughness.metallicRoughnessTexture = textureInfoMR;
                        }

                        //register the texture
                        AddStandText(_key, textureInfoBC, textureInfoMR);
                    }

                    // Constraints
                    if (gltfPbrMetallicRoughness.baseColorTexture != null)
                    {
                        gltfPbrMetallicRoughness.baseColorFactor[3] = 1.0f;
                    }

                    if (gltfPbrMetallicRoughness.metallicRoughnessTexture != null)
                    {
                        gltfPbrMetallicRoughness.metallicFactor = 1.0f;
                        gltfPbrMetallicRoughness.roughnessFactor = 1.0f;
                    }
                }
            }
            else if (typeof(BabylonPBRBaseSimpleMaterial).IsAssignableFrom(babylonMaterial.GetType()))
            {
                var babylonPBRBaseSimpleMaterial = babylonMaterial as BabylonPBRBaseSimpleMaterial;

                // --- prints ---
                #region prints

                logger.RaiseVerbose("GLTFExporter.Material | babylonMaterial data", 2);
                logger.RaiseVerbose("GLTFExporter.Material | babylonMaterial.alpha=" + babylonMaterial.alpha, 3);
                logger.RaiseVerbose("GLTFExporter.Material | babylonMaterial.alphaMode=" + babylonMaterial.alphaMode, 3);
                logger.RaiseVerbose("GLTFExporter.Material | babylonMaterial.backFaceCulling=" + babylonMaterial.backFaceCulling, 3);
                logger.RaiseVerbose("GLTFExporter.Material | babylonMaterial.wireframe=" + babylonMaterial.wireframe, 3);

                // Global
                logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.maxSimultaneousLights=" + babylonPBRBaseSimpleMaterial.maxSimultaneousLights, 3);
                logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.disableLighting=" + babylonPBRBaseSimpleMaterial.disableLighting, 3);
                logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.alphaCutOff=" + babylonPBRBaseSimpleMaterial.alphaCutOff, 3);
                logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.transparencyMode=" + babylonPBRBaseSimpleMaterial.transparencyMode, 3);
                logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.doubleSided=" + babylonPBRBaseSimpleMaterial.doubleSided, 3);

                // Base color
                logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.baseColor.Length=" + babylonPBRBaseSimpleMaterial.baseColor.Length, 3);
                for (int i = 0; i < babylonPBRBaseSimpleMaterial.baseColor.Length; i++)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.baseColor[" + i + "]=" + babylonPBRBaseSimpleMaterial.baseColor[i], 3);
                }
                if (babylonPBRBaseSimpleMaterial.baseTexture == null)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.baseTexture=null", 3);
                }

 
                // Normal / bump
                if (babylonPBRBaseSimpleMaterial.normalTexture == null)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.normalTexture=null", 3);
                }
                logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.invertNormalMapX=" + babylonPBRBaseSimpleMaterial.invertNormalMapX, 3);
                logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.invertNormalMapY=" + babylonPBRBaseSimpleMaterial.invertNormalMapY, 3);

                // Emissive
                for (int i = 0; i < babylonPBRBaseSimpleMaterial.emissive.Length; i++)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.emissiveColor[" + i + "]=" + babylonPBRBaseSimpleMaterial.emissive[i], 3);
                }
                if (babylonPBRBaseSimpleMaterial.emissiveTexture == null)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.emissiveTexture=null", 3);
                }

                // Ambient occlusion
                logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.occlusionStrength=" + babylonPBRBaseSimpleMaterial.occlusionStrength, 3);
                if (babylonPBRBaseSimpleMaterial.occlusionTexture == null)
                {
                    logger.RaiseVerbose("GLTFExporter.Material | babylonPBRBaseSimpleMaterial.occlusionTexture=null", 3);
                }

                if (babylonMaterial is BabylonPBRMetallicRoughnessMaterial pbrMRMatPrint )
                {
                    // Metallic+roughness
                    logger.RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.metallic=" + pbrMRMatPrint.metallic, 3);
                    logger.RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.roughness=" + pbrMRMatPrint.roughness, 3);
                    if (pbrMRMatPrint.metallicRoughnessTexture == null)
                    {
                        logger.RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.metallicRoughnessTexture=null", 3);
                    }
                } 
                else if (babylonMaterial is BabylonPBRSpecularGlossinessMaterial pbrSGMatPrint)
                {
                    // Metallic+roughness
                    logger.RaiseVerbose("GLTFExporter.Material | babylonPBRSpecularGlossinessMaterial.specular=" + pbrSGMatPrint.specularColor, 3);
                    logger.RaiseVerbose("GLTFExporter.Material | babylonPBRSpecularGlossinessMaterial.glossiness=" + pbrSGMatPrint.glossiness, 3);
                    if (pbrSGMatPrint.specularGlossinessTexture == null)
                    {
                        logger.RaiseVerbose("GLTFExporter.Material | babylonPBRSpecularGlossinessMaterial.specularGlossinessTexture=null", 3);
                    }
                }

                #endregion

                // --------------------------------
                // --------- gltfMaterial ---------
                // --------------------------------

                logger.RaiseMessage("GLTFExporter.Material | create gltfMaterial", 2);
                gltfMaterial = new GLTFMaterial
                {
                    name = name
                };
                gltfMaterial.id = babylonMaterial.id;
                gltfMaterial.index = gltf.MaterialsList.Count;
                gltf.MaterialsList.Add(gltfMaterial);

                //Custom user properties
                if (babylonMaterial.metadata != null && babylonMaterial.metadata.Count != 0)
                {
                    gltfMaterial.extras = babylonMaterial.metadata;
                }

                // Alpha
                GLTFMaterial.AlphaMode alphaMode;
                float? alphaCutoff;
                getAlphaMode(babylonPBRBaseSimpleMaterial, out alphaMode, out alphaCutoff);
                gltfMaterial.alphaMode = alphaMode;
                if (alphaCutoff.HasValue && alphaCutoff.Value != 0.5f) // do not export glTF default value
                {
                    gltfMaterial.alphaCutoff = alphaCutoff;
                }

                // DoubleSided
                gltfMaterial.doubleSided = babylonPBRBaseSimpleMaterial.doubleSided;

                // Normal
                gltfMaterial.normalTexture = ExportTexture(babylonPBRBaseSimpleMaterial.normalTexture, gltf);

                // Occlusion
                if (babylonPBRBaseSimpleMaterial.occlusionTexture != null)
                {
                    if (babylonPBRBaseSimpleMaterial.occlusionTexture.bitmap != null)
                    {
                        // ORM texture has been merged manually by the exporter
                        // Occlusion is defined as well as metallic and/or roughness
                        logger.RaiseVerbose("Occlusion is defined as well as metallic and/or roughness", 2);
                        gltfMaterial.occlusionTexture = ExportBitmapTexture(gltf, babylonPBRBaseSimpleMaterial.occlusionTexture);
                    }
                    else
                    {
                        // ORM texture was already merged or only occlusion is defined
                        logger.RaiseVerbose("ORM texture was already merged or only occlusion is defined", 2);
                        gltfMaterial.occlusionTexture = ExportTexture(babylonPBRBaseSimpleMaterial.occlusionTexture, gltf);
                    }
                }

                // Emissive
                gltfMaterial.emissiveFactor = babylonPBRBaseSimpleMaterial.emissive;
                gltfMaterial.emissiveTexture = ExportTexture(babylonPBRBaseSimpleMaterial.emissiveTexture, gltf);

                // --------------------------------
                // --- gltfPbrMetallicRoughness ---
                // --------------------------------
                if (babylonMaterial is BabylonPBRMetallicRoughnessMaterial pbrMRMat)
                {
                    // Metallic+roughness
                    logger.RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.metallic=" + pbrMRMat.metallic, 3);
                    logger.RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.roughness=" + pbrMRMat.roughness, 3);
                    if (pbrMRMat.metallicRoughnessTexture == null)
                    {
                        logger.RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.metallicRoughnessTexture=null", 3);
                    }

                    logger.RaiseMessage("GLTFExporter.Material | create gltfPbrMetallicRoughness", 2);
                    var gltfPbrMetallicRoughness = new GLTFPBRMetallicRoughness();
                    gltfMaterial.pbrMetallicRoughness = gltfPbrMetallicRoughness;

                    // --- Global ---

                    // Base color
                    gltfPbrMetallicRoughness.baseColorFactor = new float[4]
                    {
                        pbrMRMat.baseColor[0],
                        pbrMRMat.baseColor[1],
                        pbrMRMat.baseColor[2],
                        pbrMRMat.alpha
                    };
                    if (pbrMRMat.baseTexture != null)
                    {
                        gltfPbrMetallicRoughness.baseColorTexture = ExportBaseColorTexture(gltf, pbrMRMat.baseTexture);
                    }

                    // Metallic roughness
                    gltfPbrMetallicRoughness.metallicFactor = pbrMRMat.metallic;
                    gltfPbrMetallicRoughness.roughnessFactor = pbrMRMat.roughness;
                    if (pbrMRMat.metallicRoughnessTexture != null)
                    {
                        if (pbrMRMat.metallicRoughnessTexture == pbrMRMat.occlusionTexture)
                        {
                            // Occlusion is defined as well as metallic and/or roughness
                            // Use same texture
                            logger.RaiseVerbose("Occlusion is defined as well as metallic and/or roughness", 2);
                            gltfPbrMetallicRoughness.metallicRoughnessTexture = gltfMaterial.occlusionTexture;
                        }
                        else
                        {
                            // Occlusion is not defined, only metallic and/or roughness
                            logger.RaiseVerbose("Occlusion is not defined, only metallic and/or roughness", 2);

                            if (pbrMRMat.metallicRoughnessTexture.bitmap != null)
                            {
                                // Metallic & roughness texture has been merged manually by the exporter
                                // Write bitmap file
                                logger.RaiseVerbose("Metallic & roughness texture has been merged manually by the exporter", 2);
                                gltfPbrMetallicRoughness.metallicRoughnessTexture = ExportBitmapTexture(gltf, pbrMRMat.metallicRoughnessTexture);
                            }
                            else
                            {

                                // Metallic & roughness texture was already merged
                                // Copy file
                                logger.RaiseVerbose("Metallic & roughness texture was already merged", 2);
                                gltfPbrMetallicRoughness.metallicRoughnessTexture = ExportTexture(pbrMRMat.metallicRoughnessTexture, gltf);
                            }
                        }
                    }
                }
                else if (babylonMaterial is BabylonPBRSpecularGlossinessMaterial pbrSGMat)
                {
                    var ext = new KHR_materials_pbrSpecularGlossiness()
                    {
                        specularFactor = pbrSGMat.specularColor,
                        glossinessFactor = pbrSGMat.glossiness,
                        diffuseTexture = ExportTexture(pbrSGMat.diffuseTexture, gltf),
                        specularGlossinessTexture = ExportTexture(pbrSGMat.specularGlossinessTexture, gltf)
                    };

                    gltfMaterial.extensions = gltfMaterial.extensions ?? new GLTFExtensions(); // ensure extensions exist
                    gltfMaterial.extensions.AddExtension(gltf,"KHR_materials_pbrSpecularGlossiness",ext);
                }
            }
            else if (babylonMaterial.GetType() == typeof(BabylonFurMaterial))
            {
                // TODO - Implement proper handling of BabylonFurMaterial once gLTF spec has support
                var babylonPBRFurMaterial = babylonMaterial as BabylonFurMaterial;

                gltfMaterial = new GLTFMaterial
                {
                    name = name
                };
                gltfMaterial.id = babylonMaterial.id;
                gltfMaterial.index = gltf.MaterialsList.Count;
                gltf.MaterialsList.Add(gltfMaterial);

                //Custom user properties
                if (babylonPBRFurMaterial.metadata != null && babylonPBRFurMaterial.metadata.Count != 0)
                {
                    gltfMaterial.extras = babylonPBRFurMaterial.metadata;
                }
            }
            else
            {
                logger.RaiseWarning("GLTFExporter.Material | Unsupported material type: " + babylonMaterial.GetType(), 2);
            }

            if (gltfMaterial != null && babylonMaterial.isUnlit)
            {
                // Add Unlit extension
                if (!exportParameters.enableKHRMaterialsUnlit)
                {
                    logger.RaiseWarning("GLTFExporter.Material | KHR_materials_unlit has not been enabled for export!", 2);
                }
                else
                {
                    if (gltfMaterial.extensions == null)
                    {
                        gltfMaterial.extensions = new GLTFExtensions();
                    }
                    if (gltf.extensionsUsed == null)
                    {
                        gltf.extensionsUsed = new System.Collections.Generic.List<string>();
                    }
                    if (!gltf.extensionsUsed.Contains("KHR_materials_unlit"))
                    {
                        gltf.extensionsUsed.Add("KHR_materials_unlit");
                    }
                    gltfMaterial.extensions["KHR_materials_unlit"] = new object();
                }
            }
        }

        private void getAlphaMode(BabylonStandardMaterial babylonMaterial, out GLTFMaterial.AlphaMode alphaMode, out float? alphaCutoff)
        {
            // TODO: maybe we want to be able to handle both BabylonStandardMaterial and BabylonPBRMetallicRoughnessMaterial via the relevant fields being dropped to BabylonMaterial?
            // Serialization is going to be tricky, as we dont want BabylonStandardMaterial.alphaMode and BabylonStandardMaterial.alphaCutoff to be serialized (till we support it officially in-engine)
            alphaMode = GLTFMaterial.AlphaMode.OPAQUE;
            alphaCutoff = 0.5f;
            switch (babylonMaterial.transparencyMode)
            {
                case (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.OPAQUE: // reuse the BabylonPBRMaterialMetallicRoughness enum.
                    alphaMode = GLTFMaterial.AlphaMode.OPAQUE;
                    break;
                case (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHABLEND:
                    alphaMode = GLTFMaterial.AlphaMode.BLEND;
                    break;
                case (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHATEST:
                    alphaCutoff = babylonMaterial.alphaCutOff;
                    alphaMode = GLTFMaterial.AlphaMode.MASK;
                    break;
                case (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHATESTANDBLEND:
                    logger.RaiseWarning("GLTFExporter.Material | Alpha test and blend mode is not supported in glTF. Alpha blend is used instead.", 3);
                    alphaMode = GLTFMaterial.AlphaMode.BLEND;
                    break;
                default:
                    logger.RaiseWarning("GLTFExporter.Material | Unsupported transparency mode: " + babylonMaterial.transparencyMode, 3);
                    break;
            }
        }

        private void getAlphaMode(BabylonPBRBaseSimpleMaterial babylonMaterial, out GLTFMaterial.AlphaMode alphaMode, out float? alphaCutoff)
        {
            alphaMode = GLTFMaterial.AlphaMode.OPAQUE;
            alphaCutoff = 0.5f;
            switch (babylonMaterial.transparencyMode)
            {
                case (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.OPAQUE:
                    alphaMode = GLTFMaterial.AlphaMode.OPAQUE;
                    break;
                case (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHABLEND:
                    alphaMode = GLTFMaterial.AlphaMode.BLEND;
                    break;
                case (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHATEST:
                    alphaCutoff = babylonMaterial.alphaCutOff;
                    alphaMode = GLTFMaterial.AlphaMode.MASK;
                    break;
                case (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHATESTANDBLEND:
                    logger.RaiseWarning("GLTFExporter.Material | Alpha test and blend mode is not supported in glTF. Alpha blend is used instead.", 3);
                    alphaMode = GLTFMaterial.AlphaMode.BLEND;
                    break;
                default:
                    logger.RaiseWarning("GLTFExporter.Material | Unsupported transparency mode: " + babylonMaterial.transparencyMode, 3);
                    break;
            }
        }

        private bool isTextureOk(BabylonTexture texture)
        {
            return texture != null && File.Exists(texture.originalPath);
        }

        BabylonColor3 dielectricSpecular = new BabylonColor3(0.04f, 0.04f, 0.04f);
        const float epsilon = 1e-6f;

        /**
          * Helper function that defines the bezier curve as well.Given the control points, solve for x based on a given t for a cubic bezier curve
          * @param t a value between 0 and 1
          * @param p0 first control point
          * @param p1 second control point
          * @param p2 third control point
          * @param p3 fourth control point
          * @returns number result of cubic bezier curve at the specified t
          */
        private static float _cubicBezierCurve(float t, float p0, float p1, float p2, float p3)
        {
            return
            (
                (1 - t) * (1 - t) * (1 - t) * p0 +
                3 * (1 - t) * (1 - t) * t * p1 +
                3 * (1 - t) * t * t * p2 +
                t * t * t * p3
            );
        }

        /*
          * Helper function that calculates a roughness coefficient given a blinn-phong specular power coefficient
          * @param specularPower the blinn-phong specular power coefficient
          * @returns number result of specularPower -> roughness conversion curve.
          */
        private static float _solveForRoughness(float specularPower, BabylonVector2 P0, BabylonVector2 P1, BabylonVector2 P2, BabylonVector2 P3)
        {
            var t = Math.Pow(specularPower / P3.X, 0.333333);
            return _cubicBezierCurve((float)t, P0.Y, P1.Y, P2.Y, P3.Y);
        }

        private MetallicRoughness ConvertToMetallicRoughness(SpecularGlossiness specularGlossiness, bool displayPrints = false)
        {
            // Hard coded points used to define the specular power to roughness curve.
            var P0 = new BabylonVector2(0f, 1f);
            var P1 = new BabylonVector2(0f, 0.1f);
            var P2 = new BabylonVector2(0f, 0.1f);
            var P3 = new BabylonVector2(1300f, 0.1f);

            var diffuse = specularGlossiness.diffuse;
            var opacity = specularGlossiness.opacity;
            var glossiness = specularGlossiness.glossiness;
            var metallic = 0;
            var roughness = _solveForRoughness(glossiness * 256, P0, P1, P2, P3); // Glossiness = specularPower / 256

            if (displayPrints)
            {
                logger.RaiseVerbose("-----------------------", 3);
                logger.RaiseVerbose("diffuse=" + diffuse, 3);
                logger.RaiseVerbose("opacity=" + opacity, 3);
                logger.RaiseVerbose("glossiness=" + glossiness, 3);
                logger.RaiseVerbose("roughness=" + roughness, 3);
                logger.RaiseVerbose("metallic=" + metallic, 3);
                logger.RaiseVerbose("-----------------------", 3);
            }

            return new MetallicRoughness
            {
                baseColor = diffuse,
                opacity = opacity,
                metallic = metallic,
                roughness = roughness
            };
        }

        private float solveMetallic(float diffuse, float specular, float oneMinusSpecularStrength)
        {
            if (specular < dielectricSpecular.r)
            {
                return 0;
            }

            var a = dielectricSpecular.r;
            var b = diffuse * oneMinusSpecularStrength / (1 - dielectricSpecular.r) + specular - 2 * dielectricSpecular.r;
            var c = dielectricSpecular.r - specular;
            var D = b * b - 4 * a * c;
            return ClampScalar((float)(-b + Math.Sqrt(D)) / (2 * a), 0, 1);
        }

        /**
         * Returns the value itself if it's between min and max.  
         * Returns min if the value is lower than min.
         * Returns max if the value is greater than max.  
         */
        private static float ClampScalar(float value, float min = 0, float max = 1)
        {
            return Math.Min(max, Math.Max(min, value));
        }

        private class SpecularGlossiness
        {
            public BabylonColor3 diffuse;
            public float opacity;
            public BabylonColor3 specular;
            public float glossiness;
        }

        private class MetallicRoughness
        {
            public BabylonColor3 baseColor;
            public float opacity;
            public float metallic;
            public float roughness;
        }
    }
}

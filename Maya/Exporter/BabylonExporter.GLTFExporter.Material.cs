using BabylonExport.Entities;
using GLTFExport.Entities;
using System;
using System.Drawing;
using System.IO;

namespace Maya2Babylon
{
    partial class BabylonExporter
    {
        private void ExportMaterial(BabylonMaterial babylonMaterial, GLTF gltf)
        {
            var name = babylonMaterial.name;
            var id = babylonMaterial.id;

            RaiseMessage("GLTFExporter.Material | Export material named: " + name, 1);

            if (babylonMaterial.GetType() == typeof(BabylonStandardMaterial))
            {
                var babylonStandardMaterial = babylonMaterial as BabylonStandardMaterial;


                // --- prints ---
                #region prints

                RaiseVerbose("GLTFExporter.Material | babylonMaterial data", 2);
                RaiseVerbose("GLTFExporter.Material | babylonMaterial.alpha=" + babylonMaterial.alpha, 3);
                RaiseVerbose("GLTFExporter.Material | babylonMaterial.alphaMode=" + babylonMaterial.alphaMode, 3);
                RaiseVerbose("GLTFExporter.Material | babylonMaterial.backFaceCulling=" + babylonMaterial.backFaceCulling, 3);
                RaiseVerbose("GLTFExporter.Material | babylonMaterial.wireframe=" + babylonMaterial.wireframe, 3);

                // Ambient
                for (int i = 0; i < babylonStandardMaterial.ambient.Length; i++)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.ambient[" + i + "]=" + babylonStandardMaterial.ambient[i], 3);
                }

                // Diffuse
                RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.diffuse.Length=" + babylonStandardMaterial.diffuse.Length, 3);
                for (int i = 0; i < babylonStandardMaterial.diffuse.Length; i++)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.diffuse[" + i + "]=" + babylonStandardMaterial.diffuse[i], 3);
                }
                if (babylonStandardMaterial.diffuseTexture == null)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.diffuseTexture=null", 3);
                }
                else
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.diffuseTexture.name=" + babylonStandardMaterial.diffuseTexture.name, 3);
                }

                // Normal / bump
                if (babylonStandardMaterial.bumpTexture == null)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.bumpTexture=null", 3);
                }
                else
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.bumpTexture.name=" + babylonStandardMaterial.bumpTexture.name, 3);
                }

                // Opacity
                if (babylonStandardMaterial.opacityTexture == null)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.opacityTexture=null", 3);
                }
                else
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.opacityTexture.name=" + babylonStandardMaterial.opacityTexture.name, 3);
                }

                // Specular
                for (int i = 0; i < babylonStandardMaterial.specular.Length; i++)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.specular[" + i + "]=" + babylonStandardMaterial.specular[i], 3);
                }
                RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.specularPower=" + babylonStandardMaterial.specularPower, 3);
                if (babylonStandardMaterial.specularTexture == null)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.specularTexture=null", 3);
                }
                else
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.specularTexture.name=" + babylonStandardMaterial.specularTexture.name, 3);
                }

                // Occlusion
                if (babylonStandardMaterial.ambientTexture == null)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.ambientTexture=null", 3);
                }
                else
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.ambientTexture.name=" + babylonStandardMaterial.ambientTexture.name, 3);
                }

                // Emissive
                for (int i = 0; i < babylonStandardMaterial.emissive.Length; i++)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.emissive[" + i + "]=" + babylonStandardMaterial.emissive[i], 3);
                }
                if (babylonStandardMaterial.emissiveTexture == null)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.emissiveTexture=null", 3);
                }
                else
                {
                    RaiseVerbose("GLTFExporter.Material | babylonStandardMaterial.emissiveTexture.name=" + babylonStandardMaterial.emissiveTexture.name, 3);
                }
                #endregion


                // --------------------------------
                // --------- gltfMaterial ---------
                // --------------------------------

                RaiseMessage("GLTFExporter.Material | create gltfMaterial", 2);
                var gltfMaterial = new GLTFMaterial
                {
                    name = name
                };
                gltfMaterial.id = babylonMaterial.id;
                gltfMaterial.index = gltf.MaterialsList.Count;
                gltf.MaterialsList.Add(gltfMaterial);

                // Alpha
                string alphaMode;
                float? alphaCutoff;
                getAlphaMode(babylonStandardMaterial, out alphaMode, out alphaCutoff);
                gltfMaterial.alphaMode = alphaMode;
                gltfMaterial.alphaCutoff = alphaCutoff;

                // DoubleSided
                gltfMaterial.doubleSided = !babylonMaterial.backFaceCulling;

                // Normal
                gltfMaterial.normalTexture = ExportTexture(babylonStandardMaterial.bumpTexture, gltf);

                // Occulison
                gltfMaterial.occlusionTexture = ExportTexture(babylonStandardMaterial.ambientTexture, gltf);

                // Emissive
                gltfMaterial.emissiveFactor = babylonStandardMaterial.emissive.Multiply(babylonStandardMaterial.diffuse);
                gltfMaterial.emissiveTexture = ExportEmissiveTexture(babylonStandardMaterial, gltf, babylonStandardMaterial.emissive, babylonStandardMaterial.diffuse);

                // Constraints
                if (gltfMaterial.emissiveTexture != null)
                {
                    gltfMaterial.emissiveFactor = new[] { 1.0f, 1.0f, 1.0f };
                }

                // --------------------------------
                // --- gltfPbrMetallicRoughness ---
                // --------------------------------

                RaiseMessage("GLTFExporter.Material | create gltfPbrMetallicRoughness", 2);
                var gltfPbrMetallicRoughness = new GLTFPBRMetallicRoughness();
                gltfMaterial.pbrMetallicRoughness = gltfPbrMetallicRoughness;

                // --- Global ---

                // Eye Ball correction to limit overall brightness from std to PBR.
                // This only impacts the factors.
                var correctedDiffuse = new BabylonColor3(babylonStandardMaterial.diffuse).scale(0.5f);

                SpecularGlossiness _specularGlossiness = new SpecularGlossiness
                {
                    diffuse = correctedDiffuse,
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

                        GLTFTextureInfo textureInfoBC = new GLTFTextureInfo();
                        GLTFTextureInfo textureInfoMR = new GLTFTextureInfo();

                        if (CopyTexturesToOutput)
                        {
                            // Diffuse
                            Bitmap diffuseBitmap = null;
                            if (babylonStandardMaterial.diffuseTexture != null)
                            {
                                diffuseBitmap = LoadTexture(babylonStandardMaterial.diffuseTexture.originalPath);
                            }

                            // Specular
                            Bitmap specularBitmap = null;
                            if (babylonStandardMaterial.specularTexture != null)
                            {
                                specularBitmap = LoadTexture(babylonStandardMaterial.specularTexture.originalPath);
                            }

                            // Opacity / Alpha / Transparency
                            Bitmap opacityBitmap = null;
                            if ((babylonStandardMaterial.diffuseTexture == null || babylonStandardMaterial.diffuseTexture.hasAlpha == false) && babylonStandardMaterial.opacityTexture != null)
                            {
                                opacityBitmap = LoadTexture(babylonStandardMaterial.opacityTexture.originalPath);
                            }
                            if (diffuseBitmap != null || specularBitmap != null || opacityBitmap != null)
                            {
                                // Retrieve dimensions
                                int width = 0;
                                int height = 0;
                                var haveSameDimensions = _getMinimalBitmapDimensions(out width, out height, diffuseBitmap, specularBitmap, opacityBitmap);
                                if (!haveSameDimensions)
                                {
                                    RaiseError("Diffuse, specular and opacity maps should have same dimensions", 2);
                                }

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

                        if (baseColorBitmap != null || babylonTexture.bitmap != null)
                        {
                            var baseColorFileName = babylonMaterial.name + "_baseColor" + (isAlphaInTexture ? ".png" : ".jpg");
                            baseColorFileName = baseColorFileName.Replace(":", "_");
                            textureInfoBC = ExportBitmapTexture(gltf, babylonTexture, baseColorBitmap, baseColorFileName);
                            gltfPbrMetallicRoughness.baseColorTexture = textureInfoBC;
                        }

                        if (isTextureOk(babylonStandardMaterial.specularTexture))
                        {
                            textureInfoMR = ExportBitmapTexture(gltf, babylonTexture, metallicRoughnessBitmap, babylonMaterial.name + "_metallicRoughness" + ".jpg");
                            gltfPbrMetallicRoughness.metallicRoughnessTexture = textureInfoMR;
                        }

                        //register the texture
                        AddStandText(_key, textureInfoBC, textureInfoMR);
                    }

                    // Constraints
                    if (gltfPbrMetallicRoughness.baseColorTexture != null)
                    {
                        gltfPbrMetallicRoughness.baseColorFactor = new[] { 1.0f, 1.0f, 1.0f, 1.0f };
                    }

                    if (gltfPbrMetallicRoughness.metallicRoughnessTexture != null)
                    {
                        gltfPbrMetallicRoughness.metallicFactor = 1.0f;
                        gltfPbrMetallicRoughness.roughnessFactor = 1.0f;
                    }

                }
            }
            else if (babylonMaterial.GetType() == typeof(BabylonPBRMetallicRoughnessMaterial))
            {

                var babylonPBRMetallicRoughnessMaterial = babylonMaterial as BabylonPBRMetallicRoughnessMaterial;

                // --- prints ---
                #region prints

                RaiseVerbose("GLTFExporter.Material | babylonMaterial data", 2);
                RaiseVerbose("GLTFExporter.Material | babylonMaterial.alpha=" + babylonMaterial.alpha, 3);
                RaiseVerbose("GLTFExporter.Material | babylonMaterial.alphaMode=" + babylonMaterial.alphaMode, 3);
                RaiseVerbose("GLTFExporter.Material | babylonMaterial.backFaceCulling=" + babylonMaterial.backFaceCulling, 3);
                RaiseVerbose("GLTFExporter.Material | babylonMaterial.wireframe=" + babylonMaterial.wireframe, 3);

                // Global
                RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.maxSimultaneousLights=" + babylonPBRMetallicRoughnessMaterial.maxSimultaneousLights, 3);
                RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.disableLighting=" + babylonPBRMetallicRoughnessMaterial.disableLighting, 3);
                RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.alphaCutOff=" + babylonPBRMetallicRoughnessMaterial.alphaCutOff, 3);
                RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.transparencyMode=" + babylonPBRMetallicRoughnessMaterial.transparencyMode, 3);
                RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.doubleSided=" + babylonPBRMetallicRoughnessMaterial.doubleSided, 3);

                // Base color
                RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.baseColor.Length=" + babylonPBRMetallicRoughnessMaterial.baseColor.Length, 3);
                for (int i = 0; i < babylonPBRMetallicRoughnessMaterial.baseColor.Length; i++)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.baseColor[" + i + "]=" + babylonPBRMetallicRoughnessMaterial.baseColor[i], 3);
                }
                if (babylonPBRMetallicRoughnessMaterial.baseTexture == null)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.baseTexture=null", 3);
                }

                // Metallic+roughness
                RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.metallic=" + babylonPBRMetallicRoughnessMaterial.metallic, 3);
                RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.roughness=" + babylonPBRMetallicRoughnessMaterial.roughness, 3);
                if (babylonPBRMetallicRoughnessMaterial.metallicRoughnessTexture == null)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.metallicRoughnessTexture=null", 3);
                }

                // Normal / bump
                if (babylonPBRMetallicRoughnessMaterial.normalTexture == null)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.normalTexture=null", 3);
                }
                RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.invertNormalMapX=" + babylonPBRMetallicRoughnessMaterial.invertNormalMapX, 3);
                RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.invertNormalMapY=" + babylonPBRMetallicRoughnessMaterial.invertNormalMapY, 3);

                // Emissive
                for (int i = 0; i < babylonPBRMetallicRoughnessMaterial.emissive.Length; i++)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.emissiveColor[" + i + "]=" + babylonPBRMetallicRoughnessMaterial.emissive[i], 3);
                }
                if (babylonPBRMetallicRoughnessMaterial.emissiveTexture == null)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.emissiveTexture=null", 3);
                }

                // Ambient occlusion
                RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.occlusionStrength=" + babylonPBRMetallicRoughnessMaterial.occlusionStrength, 3);
                if (babylonPBRMetallicRoughnessMaterial.occlusionTexture == null)
                {
                    RaiseVerbose("GLTFExporter.Material | babylonPBRMetallicRoughnessMaterial.occlusionTexture=null", 3);
                }
                #endregion

                // --------------------------------
                // --------- gltfMaterial ---------
                // --------------------------------

                RaiseMessage("GLTFExporter.Material | create gltfMaterial", 2);
                var gltfMaterial = new GLTFMaterial
                {
                    name = name
                };
                gltfMaterial.id = babylonMaterial.id;
                gltfMaterial.index = gltf.MaterialsList.Count;
                gltf.MaterialsList.Add(gltfMaterial);

                // Alpha
                string alphaMode;
                float? alphaCutoff;
                getAlphaMode(babylonPBRMetallicRoughnessMaterial, out alphaMode, out alphaCutoff);
                gltfMaterial.alphaMode = alphaMode;
                gltfMaterial.alphaCutoff = alphaCutoff;

                // DoubleSided
                gltfMaterial.doubleSided = babylonPBRMetallicRoughnessMaterial.doubleSided;

                // Normal
                gltfMaterial.normalTexture = ExportTexture(babylonPBRMetallicRoughnessMaterial.normalTexture, gltf);

                // Occlusion
                if (babylonPBRMetallicRoughnessMaterial.occlusionTexture != null)
                {
                    if (babylonPBRMetallicRoughnessMaterial.occlusionTexture.bitmap != null)
                    {
                        // ORM texture has been merged manually by the exporter
                        // Occlusion is defined as well as metallic and/or roughness
                        RaiseVerbose("Occlusion is defined as well as metallic and/or roughness", 2);
                        gltfMaterial.occlusionTexture = ExportBitmapTexture(gltf, babylonPBRMetallicRoughnessMaterial.occlusionTexture);
                    }
                    else
                    {
                        // ORM texture was already merged or only occlusion is defined
                        RaiseVerbose("ORM texture was already merged or only occlusion is defined", 2);
                        gltfMaterial.occlusionTexture = ExportTexture(babylonPBRMetallicRoughnessMaterial.occlusionTexture, gltf);
                    }
                }

				// Emissive
                gltfMaterial.emissiveFactor = babylonPBRMetallicRoughnessMaterial.emissive;
                gltfMaterial.emissiveTexture = ExportTexture(babylonPBRMetallicRoughnessMaterial.emissiveTexture, gltf);


                // --------------------------------
                // --- gltfPbrMetallicRoughness ---
                // --------------------------------

                RaiseMessage("GLTFExporter.Material | create gltfPbrMetallicRoughness", 2);
                var gltfPbrMetallicRoughness = new GLTFPBRMetallicRoughness();
                gltfMaterial.pbrMetallicRoughness = gltfPbrMetallicRoughness;

                // --- Global ---

                // Base color
                gltfPbrMetallicRoughness.baseColorFactor = new float[4]
                {
                    babylonPBRMetallicRoughnessMaterial.baseColor[0],
                    babylonPBRMetallicRoughnessMaterial.baseColor[1],
                    babylonPBRMetallicRoughnessMaterial.baseColor[2],
                    babylonPBRMetallicRoughnessMaterial.alpha
                };
                if (babylonPBRMetallicRoughnessMaterial.baseTexture != null)
                {
                    if (babylonPBRMetallicRoughnessMaterial.baseTexture.bitmap != null)
                    {
                        // Base color & Alpha texture has been merged manually by the exporter
                        // Write bitmap file
                        gltfPbrMetallicRoughness.baseColorTexture = ExportBitmapTexture(gltf, babylonPBRMetallicRoughnessMaterial.baseTexture);
                    }
                    else
                    {
                        // Base color & Alpha texture was already merged
                        // Copy file
                        gltfPbrMetallicRoughness.baseColorTexture = ExportTexture(babylonPBRMetallicRoughnessMaterial.baseTexture, gltf);
                    }
                }

                // Metallic roughness
                gltfPbrMetallicRoughness.metallicFactor = babylonPBRMetallicRoughnessMaterial.metallic;
                gltfPbrMetallicRoughness.roughnessFactor = babylonPBRMetallicRoughnessMaterial.roughness;
                if (babylonPBRMetallicRoughnessMaterial.metallicRoughnessTexture != null)
                {
                    if (babylonPBRMetallicRoughnessMaterial.metallicRoughnessTexture == babylonPBRMetallicRoughnessMaterial.occlusionTexture)
                    {
                        // Occlusion is defined as well as metallic and/or roughness
                        // Use same texture
                        RaiseVerbose("Occlusion is defined as well as metallic and/or roughness", 2);
                        gltfPbrMetallicRoughness.metallicRoughnessTexture = gltfMaterial.occlusionTexture;
                    }
                    else
                    {
                        // Occlusion is not defined, only metallic and/or roughness
                        RaiseVerbose("Occlusion is not defined, only metallic and/or roughness", 2);

                        if (babylonPBRMetallicRoughnessMaterial.metallicRoughnessTexture.bitmap != null)
                        {
                            // Metallic & roughness texture has been merged manually by the exporter
                            // Write bitmap file
                            RaiseVerbose("Metallic & roughness texture has been merged manually by the exporter", 2);
                            gltfPbrMetallicRoughness.metallicRoughnessTexture = ExportBitmapTexture(gltf, babylonPBRMetallicRoughnessMaterial.metallicRoughnessTexture);
                        }
                        else
                        {

                            // Metallic & roughness texture was already merged
                            // Copy file
                            RaiseVerbose("Metallic & roughness texture was already merged", 2);
                            gltfPbrMetallicRoughness.metallicRoughnessTexture = ExportTexture(babylonPBRMetallicRoughnessMaterial.metallicRoughnessTexture, gltf);
                        }
                    }
                }
            }
            else
            {
                RaiseWarning("GLTFExporter.Material | Unsupported material type: " + babylonMaterial.GetType(), 2);
            }
        }

        private void getAlphaMode(BabylonStandardMaterial babylonMaterial, out string alphaMode, out float? alphaCutoff)
        {
            if (babylonMaterial.alpha != 1.0f ||
                (babylonMaterial.diffuseTexture != null && babylonMaterial.diffuseTexture.hasAlpha) ||
                 babylonMaterial.opacityTexture != null)
            {
                // Babylon standard material is assumed to useAlphaFromDiffuseTexture. If not, the alpha mode is a mask.
                alphaMode = GLTFMaterial.AlphaMode.BLEND.ToString();
            }
            else
            {
                alphaMode = GLTFMaterial.AlphaMode.OPAQUE.ToString();
            }
            alphaCutoff = null;
        }

        private void getAlphaMode(BabylonPBRMetallicRoughnessMaterial babylonMaterial, out string alphaMode, out float? alphaCutoff)
        {
            alphaMode = null;
            alphaCutoff = null;
            switch (babylonMaterial.transparencyMode)
            {
                case (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.OPAQUE:
                    alphaMode = GLTFMaterial.AlphaMode.OPAQUE.ToString();
                    break;
                case (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHABLEND:
                    alphaMode = GLTFMaterial.AlphaMode.BLEND.ToString();
                    break;
                case (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHATEST:
                    alphaCutoff = babylonMaterial.alphaCutOff;
                    alphaMode = GLTFMaterial.AlphaMode.MASK.ToString();
                    break;
                case (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHATESTANDBLEND:
                    RaiseWarning("GLTFExporter.Material | Alpha test and blend mode is not supported in glTF. Alpha blend is used instead.", 3);
                    alphaMode = GLTFMaterial.AlphaMode.BLEND.ToString();
                    break;
                default:
                    RaiseWarning("GLTFExporter.Material | Unsupported transparency mode: " + babylonMaterial.transparencyMode, 3);
                    break;
            }
        }

        private bool isTextureOk(BabylonTexture texture)
        {
            return texture != null && File.Exists(texture.originalPath);
        }

        BabylonColor3 dielectricSpecular = new BabylonColor3(0.04f, 0.04f, 0.04f);
        const float epsilon = 1e-6f;

        private MetallicRoughness ConvertToMetallicRoughness(SpecularGlossiness specularGlossiness, bool displayPrints = false)
        {
            // Hard coded points used to define the specular power to roughness curve.
            var P0 = new BabylonVector2(0f, 1f);
            var P1 = new BabylonVector2(0f, 0.1f);
            var P2 = new BabylonVector2(0f, 0.1f);
            var P3 = new BabylonVector2(1300f, 0.1f);

            /**
             * Helper function that defines the bezier curve as well. Given the control points, solve for x based on a given t for a cubic bezier curve
             * @param t a value between 0 and 1
             * @param p0 first control point
             * @param p1 second control point
             * @param p2 third control point
             * @param p3 fourth control point
             * @returns number result of cubic bezier curve at the specified t
             */
            float _cubicBezierCurve(float t, float p0, float p1, float p2, float p3)
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
            float _solveForRoughness(float specularPower)
            {
                var t = Math.Pow(specularPower / P3.X, 0.333333);
                return _cubicBezierCurve((float)t, P0.Y, P1.Y, P2.Y, P3.Y);
            }

            var diffuse = specularGlossiness.diffuse;
            var opacity = specularGlossiness.opacity;
            var glossiness = specularGlossiness.glossiness;
            var metallic = 0;
            var roughness = _solveForRoughness(glossiness * 256); // Glossiness = specularPower / 256

            if (displayPrints)
            {
                RaiseVerbose("-----------------------", 3);
                RaiseVerbose("diffuse=" + diffuse, 3);
                RaiseVerbose("opacity=" + opacity, 3);
                RaiseVerbose("glossiness=" + glossiness, 3);
                RaiseVerbose("roughness=" + roughness, 3);
                RaiseVerbose("metallic=" + metallic, 3);
                RaiseVerbose("-----------------------", 3);
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

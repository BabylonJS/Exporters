using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using BabylonExport.Entities;
using FreeImageAPI;

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity3D2Babylon
{
    partial class SceneBuilder
    {
        public static string DefaultPredefinedPropertyNames = "_Mode, _Cutoff, _Color, _MainTex, _Shininess, _SpecColor, _SpecGlossMap, _GlossMapScale, _AmbientColor, _Emission, _EmissionMap, _Illum, _LightMap, _Cube, _BumpMap, _BumpScale, _GlossyReflections, _SmoothnessTextureChannel, _SpecularHighlights, _UVSec, _SrcBlend, _DstBlend, _ZWrite";
        public static string SystemPredefinedPropertyNames = "_Mode, _Cutoff, _Color, _MainTex, _Glossiness, _SpecColor, _SpecGlossMap, _GlossMapScale, _Metallic, _MetallicGlossMap, _EmissionColor, _EmissionMap, _OcclusionMap, _OcclusionStrength, _BumpMap, _BumpScale, _GlossyReflections, _SmoothnessTextureChannel, _SpecularHighlights, _Parallax, _ParallaxMap, _DetailMask, _DetailAlbedoMap, _DetailNormalMapScale, _DetailNormalMap, _UVSec, _SrcBlend, _DstBlend, _ZWrite";
        	        
        private void CopyAssestFile(string outputName, DefaultAsset assestFile, BabylonTexture babylonTexture)
        {
            var srcAssetPath = AssetDatabase.GetAssetPath(assestFile);
            var srcAssetFile = Path.GetFileName(srcAssetPath);
            ExporterWindow.ReportProgress(1, "Copying assest file: " + srcAssetFile);
            var outAssetPath = Path.Combine(babylonScene.OutputPath, Path.GetFileName(outputName));
            File.Copy(srcAssetPath, outAssetPath, true);
            babylonTexture.name = outputName;
        }

        private void CopyCubemapTexture(string texturePath, Cubemap cubemap, BabylonTexture babylonTexture)
        {
            if (!babylonScene.AddTextureCube(texturePath)) return;
            ExporterWindow.ReportProgress(1, "Copying texture cubemap file: " + texturePath);
            Tools.SetTextureWrapMode(babylonTexture, cubemap);
            var srcTexturePath = AssetDatabase.GetAssetPath(cubemap);
            var srcTextureExt = Path.GetExtension(srcTexturePath);
            if (srcTextureExt.Equals(".exr", StringComparison.OrdinalIgnoreCase)) {
                var ddsTexturePath = Path.Combine(babylonScene.OutputPath, Path.GetFileName(texturePath));
                FileStream sourceStream = new FileStream(srcTexturePath, FileMode.Open, FileAccess.Read);
                try
                {
                    bool readResult = false;
                    int readWidth = 0;
                    int readHeight = 0;
                    int readBitsPerPixel = 0;
                    Color[] pixels = Tools.ReadFreeImage(sourceStream, ref readResult, ref readWidth, ref readHeight, ref readBitsPerPixel, Tools.ColorCorrection.NoCorrection);
                    if (readResult == true && pixels != null) {
                        var tempTexture = new Texture2D(readWidth, readHeight, TextureFormat.RGBAFloat, false);
                        tempTexture.SetPixels(pixels);
                        tempTexture.Apply();
                        tempTexture.WriteImageHDR(ddsTexturePath);
                    } else {
                        UnityEngine.Debug.LogError("Failed to convert exr/hdr file");
                    }
                } catch (Exception ex) {
                    UnityEngine.Debug.LogException(ex);
                } finally {
                    sourceStream.Close();
                }
            } else {
                var ddsTexturePath = Path.Combine(babylonScene.OutputPath, Path.GetFileName(texturePath));
                File.Copy(srcTexturePath, ddsTexturePath, true);
                var textureName = Path.GetFileName(texturePath);
                babylonTexture.name = textureName;
            }
        }

        private void CopyTexture(string texturePath, Texture2D texture2D, BabylonTexture babylonTexture, bool isLightmap = false, bool isTerrain = false, bool asJpeg = false, Texture2D shadowMask = null)
        {
            bool needToDelete = false;
            // Convert required file extensions
            string convertList = ".exr, .psd, .tif";
            string textureExt = Path.GetExtension(texturePath);
            bool hasAlpha = texture2D.alphaIsTransparency;
            bool unityexr = textureExt.Equals(".exr", StringComparison.OrdinalIgnoreCase);
            bool nativepng = textureExt.Equals(".png", StringComparison.OrdinalIgnoreCase);
            bool nativetga = textureExt.Equals(".tga", StringComparison.OrdinalIgnoreCase);
            bool nativejpeg = textureExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || textureExt.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
            bool okextension = (nativepng || nativejpeg || nativetga);
            bool enforceimage = (!okextension && exportationOptions.EnforceImageEncoding);
            //..
            bool processImage = true;
            string sourcefile = Path.GetFileName(texturePath);
            if (isLightmap == true && SceneBuilder.LightmapNames.ContainsKey(sourcefile)) {
                texturePath = SceneBuilder.LightmapNames[sourcefile];
                processImage = false;
            }
            if (processImage == true) {
                ExporterWindow.ReportProgress(1, "Copying texture image file: " + sourcefile);
                Tools.SetTextureWrapMode(babylonTexture, texture2D);
                if (isLightmap || isTerrain || enforceimage || convertList.IndexOf(textureExt, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string srcTexturePath = AssetDatabase.GetAssetPath(texture2D);
                    var importTool = new BabylonTextureImporter(srcTexturePath);
                    var previousConvertToNormalmap = importTool.textureImporter.convertToNormalmap;
                    var previousAlphaSource = importTool.textureImporter.alphaSource;
                    var previousTextureType = importTool.textureImporter.textureType;
                    importTool.SetReadable();
                    importTool.textureImporter.textureType = (isLightmap) ? TextureImporterType.Lightmap : TextureImporterType.Default;
                    importTool.textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
                    importTool.textureImporter.convertToNormalmap = false;
                    AssetDatabase.ImportAsset(texturePath);
                    // Validate texture alpha
                    if (isLightmap == false) {                
                        hasAlpha = texture2D.HasAlpha();
                    } else {
                        hasAlpha = true;
                    }
                    // Validate texture format
                    string extension = ".jpg";
                    BabylonImageFormat textureFormat = BabylonImageFormat.JPEG;
                    bool makeJpeg = (unityexr == false && hasAlpha == false && (asJpeg || nativejpeg || (enforceimage && exportationOptions.ImageEncodingOptions == (int)BabylonImageFormat.JPEG)));
                    if (makeJpeg == false) {
                        if (exportationOptions.ImageEncodingOptions == (int)BabylonImageFormat.PNG) {
                            extension = ".png";
                            textureFormat = BabylonImageFormat.PNG;
                        }
                    }
                    // Encode texture image file
                    string rename = (isLightmap) ? SceneName + "_" + Path.GetFileName(texturePath) : null;
                    string outputfile = (!String.IsNullOrEmpty(rename)) ? rename : texturePath;
                    texturePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(outputfile));
                    string isterrain = (isTerrain) ? "_Terrain" : "";
                    string filename = Path.GetFileNameWithoutExtension(Path.GetFileName(texturePath)) + isterrain + extension;
                    texturePath = Path.Combine(Path.GetDirectoryName(texturePath), filename);
                    try
                    {
                        if (unityexr == true) {
                            bool readResult = false;
                            int readWidth = 0;
                            int readHeight = 0;
                            int readBitsPerPixel = 0;
                            string exrFilename = Tools.GetNativePath(srcTexturePath);
                            Color[] pixels = Tools.ReadFreeImage(new FileStream(exrFilename, FileMode.Open, FileAccess.Read), ref readResult, ref readWidth, ref readHeight, ref readBitsPerPixel, Tools.ColorCorrection.LinearToGamma);
                            if (readResult == true && pixels != null && pixels.Length > 0) {
                                if (isLightmap == true && shadowMask != null) {
                                    if (shadowMask.width == texture2D.width && shadowMask.height == texture2D.height) {
                                        Color[] shadows = shadowMask.GetSafePixels();
                                        if (shadows != null && shadows.Length > 0) {
                                            int totalPixels = shadows.Length;
                                            int redPixels = 0;
                                            for (int i = 0; i < pixels.Length; i++) {
                                                // ..
                                                // Shadow Mask Under Construction
                                                // ..
                                                bool red = (shadows[i].r >= 1.0);
                                                if (red == true) {
                                                    redPixels++;
                                                    //pixels[i].r *= 0.75f;
                                                    //pixels[i].g *= 0.75f;
                                                    //pixels[i].b *= 0.75f;
                                                    //pixels[i].a = 1.0f;
                                                } else {
                                                    //pixels[i].r *= pixels[i].a;
                                                    //pixels[i].g *= pixels[i].a;
                                                    //pixels[i].b *= pixels[i].a;
                                                    //pixels[i].a = 1.0f;
                                                }
                                            }
                                            UnityEngine.Debug.Log("Total Pixels: " + totalPixels.ToString() + " -> Red Pixels: " + redPixels.ToString());
                                        } else {
                                            UnityEngine.Debug.LogError("No shadow mask pixels for exr file: " + texturePath);
                                        }
                                    } else{
                                        UnityEngine.Debug.LogError("Shadow mask demensions do not match exr file: " + texturePath);
                                    }
                                }
                                var tempTexture = new Texture2D(readWidth, readHeight, TextureFormat.RGBAFloat, false);
                                tempTexture.SetPixels(pixels);
                                tempTexture.Apply();
                                tempTexture.WriteImage(texturePath, textureFormat);
                            } else {
                                UnityEngine.Debug.LogError("Failed to convert exr file: " + texturePath);
                            }
                        } else {
                            var tempTexture = new Texture2D(texture2D.width, texture2D.height, (hasAlpha) ? TextureFormat.RGBA32 : TextureFormat.RGB24, false);
                            tempTexture.SetPixels(texture2D.GetPixels());
                            tempTexture.Apply();
                            tempTexture.WriteImage(texturePath, textureFormat);
                        }
                        needToDelete = true;
                        if (isLightmap) SceneBuilder.LightmapNames.Add(sourcefile, texturePath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                    finally
                    {
                        importTool.textureImporter.textureType = previousTextureType;
                        importTool.textureImporter.alphaSource = previousAlphaSource;
                        importTool.textureImporter.convertToNormalmap = previousConvertToNormalmap;
                        importTool.ForceUpdate();
                        babylonTexture.hasAlpha = hasAlpha;
                    }
                }
                else if (texture2D.alphaIsTransparency || texturePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    babylonTexture.hasAlpha = true;
                }
                else
                {
                    babylonTexture.hasAlpha = false;
                }
            }
            var textureName = Path.GetFileName(texturePath);
            babylonTexture.name = textureName;
            babylonScene.AddTexture(texturePath);
            if (needToDelete) File.Delete(texturePath);
        }

        private void CopyTextureFace(string texturePath, string textureName, Texture2D textureFace)
        {
            if (!babylonScene.AddTextureCube(textureName)) return;
            ExporterWindow.ReportProgress(1, "Copying texture cube face: " + textureName);
            var srcTexturePath = AssetDatabase.GetAssetPath(textureFace);
            File.Copy(srcTexturePath, texturePath, true);
        }

        private void DumpTextureFace(string texturePath, string textureName, Texture2D textureFace, BabylonImageFormat imageFormat)
        {
            if (!babylonScene.AddTextureCube(textureName)) return;
            ExporterWindow.ReportProgress(1, "Dumping texture cube face: " + textureName);
            textureFace.WriteImage(texturePath, imageFormat);
        }

        private static Color[] GetPixels(Texture2D texture)
        {
            Color[] pixels = null;
            string srcTexturePath = AssetDatabase.GetAssetPath(texture);
            var importTool = new BabylonTextureImporter(srcTexturePath);
            bool isReadable = importTool.IsReadable();
            if (!isReadable) importTool.SetReadable();
            try
            {
                pixels = texture.GetPixels();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                if (!isReadable) importTool.ForceUpdate();
            }
            return pixels;
        }

        private MaterialExportType CheckMaterial(Material material)
        {
            MaterialExportType result = MaterialExportType.Standard;
            if (material.HasProperty("_Metallic") && (material.HasProperty("_Glossiness") || material.HasProperty("_Gloss"))) {
                result = MaterialExportType.Metallic;
            } else if (material.HasProperty("_Roughness") && (material.HasProperty("_Glossiness") || material.HasProperty("_Gloss"))) {
                result = MaterialExportType.Roughness;
            } else if (material.HasProperty("_SpecColor") && (material.HasProperty("_Glossiness") || material.HasProperty("_Gloss"))) {
                result = MaterialExportType.Specular;
            }
            return result;
        }

        private BabylonMaterial DumpMaterial(Material material, bool receiveShadows, int lightmapIndex = -1, Vector4 lightmapScaleOffset = default(Vector4), int lightmapCoordIndex = -1)
        {
            if (material.name == "Default-Material" || material.shader.name == "Babylon/Standard Material")
            {
                return DumpStandardMaterial(material, receiveShadows, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex);
            }
            else if (material.shader.name == "Standard" || material.shader.name == "Standard (Roughness setup)" || material.shader.name == "Standard Plus/Standard Plus")
            {
                return DumpPBRMaterial(material, receiveShadows, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex, true);
            }
            else if (material.shader.name == "Standard (Specular setup)")
            {
                return DumpPBRMaterial(material, receiveShadows, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex, false);
            }
            else if (material.shader.name.StartsWith("Babylon/", StringComparison.OrdinalIgnoreCase))
            {
                return DumpShaderMaterial(material, receiveShadows, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex);
            }
            else
            {
                // Double Check Common Properties
                var check = CheckMaterial(material);
                if (check == MaterialExportType.Metallic) {
                    return DumpPBRMaterial(material, receiveShadows, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex, true);
                } else if (check == MaterialExportType.Roughness) {
                    return DumpPBRMaterial(material, receiveShadows, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex, true);
                } else if (check == MaterialExportType.Specular) {
                    return DumpPBRMaterial(material, receiveShadows, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex, false);
                } else {
                    return DumpStandardMaterial(material, receiveShadows, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex);
                }
            }
        }

        private BabylonMaterial DumpStandardMaterial(Material material, bool receiveShadows, int lightmapIndex = -1, Vector4 lightmapScaleOffset = default(Vector4), int lightmapCoordIndex = -1, BabylonDefaultMaterial defaultMaterial = null)
        {
            bool hasLightmap = (exportationOptions.ExportLightmaps && lightmapIndex >= 0 && lightmapIndex != 65535 && LightmapSettings.lightmaps.Length > lightmapIndex);
            var materialNotSupported = false;
            var materialName = material.name;
            var materialId = Guid.NewGuid().ToString();
            if (hasLightmap && exportationOptions.CreateMaterialInstance) materialName = materialName + ".Instance." + materialId;
            if (!materialsDictionary.ContainsKey(materialName)) {
                var bMat = (defaultMaterial != null) ? defaultMaterial : new BabylonDefaultMaterial {
                    name = materialName,
                    id = materialId,
                    ambient = Color.black.ToFloat(),
                    diffuse = Color.white.ToFloat(),
                    specular = Color.black.ToFloat(),
                    emissive = Color.black.ToFloat(),
                    specularPower = 64,
                    disableLighting = false,
                    useSpecularOverAlpha = false,
                    maxSimultaneousLights = 4,
                    useEmissiveAsIllumination = false
                };
                ExporterWindow.ReportProgress(1, "Exporting standard material: " + material.name);
                if (material.mainTexture && material.mainTexture.GetType().FullName == "UnityEngine.ProceduralTexture")
                {
                    materialNotSupported = true;
                    UnityEngine.Debug.LogWarning("ProceduralTexture: " + material.mainTexture.name + " not supported by Babylon.js");
                }

                if (material.HasProperty("_Shininess"))
                {
                    var specShininess = material.GetFloat("_Shininess");
                    bMat.specularPower = specShininess * 128;
                }
                if (material.HasProperty("_Color"))
                {
                    bMat.diffuse = material.color.ToFloat();
                }
                if (material.HasProperty("_AmbientColor"))
                {
                    var ambientColor = material.GetColor("_AmbientColor");
                    bMat.ambient = ambientColor.ToFloat();
                }
                if (material.HasProperty("_SpecColor"))
                {
                    var specColor = material.GetColor("_SpecColor");
                    bMat.specular = specColor.ToFloat();
                }
                if (material.HasProperty("_Wireframe"))
                {
                    bMat.wireframe = (material.GetInt("_Wireframe") != 0);
                }

                if (material.HasProperty("_DisableLighting"))
                {
                    bMat.disableLighting = (material.GetInt("_DisableLighting") != 0);
                }
                
                if (material.HasProperty("_UseEmissiveAsIllumination"))
                {
                    bMat.useEmissiveAsIllumination = (material.GetInt("_UseEmissiveAsIllumination") != 0);
                }

			    //bool backfaceCulling = material.HasProperty("_Cull") && material.GetInt("_Cull") == (float)UnityEngine.Rendering.CullMode.Back;
                if (material.HasProperty("_BackFaceCulling"))
                {
                    bMat.backFaceCulling = (material.GetInt("_BackFaceCulling") != 0);
                }

                if (material.HasProperty("_TwoSidedLighting"))
                {
                    bMat.twoSidedLighting = (material.GetInt("_TwoSidedLighting") != 0);
                }

                if (material.HasProperty("_MaxSimultaneousLights"))
                {
                    bMat.maxSimultaneousLights = material.GetInt("_MaxSimultaneousLights");
                }

                Texture2D mainTexture2D = material.mainTexture as Texture2D;
                if (material.mainTexture && !materialNotSupported) {
                    var mainTexturePath = AssetDatabase.GetAssetPath(mainTexture2D);
                    bMat.diffuseTexture = new BabylonTexture {
                        uScale = Tools.GetTextureScale(material.mainTextureScale.x),
                        vScale = Tools.GetTextureScale(material.mainTextureScale.y),
                        uOffset = material.mainTextureOffset.x,
                        vOffset = material.mainTextureOffset.y
                    };
                    CopyTexture(mainTexturePath, mainTexture2D, bMat.diffuseTexture);
                }

                // Opacity map
                bMat.opacityTexture = DumpTextureFromMaterial(material, "_OpacityMap");
                DumpTransparency(mainTexture2D, material, bMat);

                // Normal map
                bMat.bumpTexture = DumpTextureFromMaterial(material, "_BumpMap");
                if (bMat.bumpTexture != null && material.HasProperty("_BumpScale"))
                {
                    bMat.bumpTexture.level = material.GetFloat("_BumpScale");
                }
                if (bMat.diffuseTexture != null && bMat.bumpTexture != null) {
                    bMat.bumpTexture.uScale = bMat.diffuseTexture.uScale;
                    bMat.bumpTexture.vScale = bMat.diffuseTexture.vScale;
                    bMat.bumpTexture.uOffset = bMat.diffuseTexture.uOffset;
                    bMat.bumpTexture.vOffset = bMat.diffuseTexture.vOffset;
                }

                // Emission Map
                if (material.HasProperty("_Emission"))
                {
                    if (material.GetColorNames().IndexOf("_Emission") >= 0)
                    {
                        var emissiveColor = material.GetColor("_Emission");
                        bMat.emissive = emissiveColor.ToFloat();
                    }
                    else if (material.GetFloatNames().IndexOf("_Emission") >= 0)
                    {
                        // TODO: Convert Lightmapper Emission Color - ???
                        UnityEngine.Debug.LogWarning("Material Emission Is Float Not Color: " + material.name);
                    }
                }
                bMat.emissiveTexture = DumpTextureFromMaterial(material, "_EmissionMap");
                if (bMat.emissiveTexture == null) bMat.emissiveTexture = DumpTextureFromMaterial(material, "_Illum");
                bMat.reflectionTexture = DumpTextureFromMaterial(material, "_Cube");

                // Use Lightmap As Shadowmap
                bMat.ambientTexture = DumpTextureFromMaterial(material, "_LightMap");
                if (bMat.ambientTexture == null && hasLightmap) {
                    var lightmap = LightmapSettings.lightmaps[lightmapIndex].lightmapColor;
                    var shadowmap = LightmapSettings.lightmaps[lightmapIndex].shadowMask;
                    var texturePath = AssetDatabase.GetAssetPath(lightmap);
                    if (!String.IsNullOrEmpty(texturePath))
                    {
                        ExporterWindow.ReportProgress(1, "Dumping standard material shadow mask: " + lightmap.name);
                        bMat.lightmapTexture = DumpTexture(lightmap, isLightmap: true, shadowMask: shadowmap);
                        bMat.lightmapTexture.coordinatesIndex = (lightmapCoordIndex >= 0) ? lightmapCoordIndex : exportationOptions.DefaultCoordinatesIndex;
                        bMat.useLightmapAsShadowmap = true;

                        bMat.lightmapTexture.uScale = Tools.GetTextureScale(lightmapScaleOffset.x);
                        bMat.lightmapTexture.vScale = Tools.GetTextureScale(lightmapScaleOffset.y);

                        bMat.lightmapTexture.uOffset = lightmapScaleOffset.z;
                        bMat.lightmapTexture.vOffset = lightmapScaleOffset.w;
                    }
                }
                materialsDictionary.Add(bMat.name, bMat);
                return bMat;
            }
            return materialsDictionary[material.name];
        }

        private BabylonMaterial DumpPBRMaterial(Material material, bool receiveShadows, int lightmapIndex = -1, Vector4 lightmapScaleOffset = default(Vector4), int lightmapCoordIndex = -1, bool metallic = true)
        {
            bool hasLightmap = (exportationOptions.ExportLightmaps && lightmapIndex >= 0 && lightmapIndex != 65535 && LightmapSettings.lightmaps.Length > lightmapIndex);
            var materialNotSupported = false;
            var materialName = material.name;
            var materialId = Guid.NewGuid().ToString();
            if (hasLightmap && exportationOptions.CreateMaterialInstance) materialName = materialName + ".Instance." + materialId;
            if (materialsDictionary.ContainsKey(materialName)) {
                return materialsDictionary[materialName];
            }
            var babylonPbrMaterial = new BabylonSystemMaterial {
                name = materialName,
                id = materialId,
                albedo = Color.white.ToFloat(),
                ambient = Color.black.ToFloat(),
                emissive = Color.black.ToFloat(),
                metallic = null,
                roughness = null,
                microSurface = 1.0f,
                cameraContrast = 1.0f,
                cameraExposure = 1.0f,
                reflectivity = Color.white.ToFloat(),
                reflection = Color.white.ToFloat(),
                sideOrientation = 1,
                directIntensity = 1.0f,
                emissiveIntensity = 0.5f,
                specularIntensity = 0.5f,
                environmentIntensity = 1.0f,
                maxSimultaneousLights = 4,
                useSpecularOverAlpha = false,
                useRadianceOverAlpha = false,
                usePhysicalLightFalloff = false,
                useAlphaFromAlbedoTexture = false,
                useEmissiveAsIllumination = false
            };
            float defaultCameraContrast = (SceneController != null) ? SceneController.lightingOptions.defaultShaderOptions.cameraContrast : 1.0f;
            float defaultCameraExposure = (SceneController != null) ? SceneController.lightingOptions.defaultShaderOptions.cameraExposure : 1.0f;
            float defaultDirectIntensity = (SceneController != null) ? SceneController.lightingOptions.defaultShaderOptions.directIntensity : 1.0f;
            float defaultEmissiveIntensity = (SceneController != null) ? SceneController.lightingOptions.defaultShaderOptions.emissiveIntensity : 0.5f;
            float defaultSpecularIntensity = (SceneController != null) ? SceneController.lightingOptions.defaultShaderOptions.specularIntensity : 0.5f;
            float defaultEnvironmentIntensity = (SceneController != null) ? SceneController.lightingOptions.defaultShaderOptions.environmentIntensity : 1.0f;
            float defaultMicroSurfaceScaling = (SceneController != null) ? SceneController.lightingOptions.defaultShaderOptions.microSurfaceScaling : 1.0f;
            ExporterWindow.ReportProgress(1, "Exporting physical material: " + material.name);

            if (material.mainTexture && material.mainTexture.GetType().FullName == "UnityEngine.ProceduralTexture")
            {
                materialNotSupported = true;
                UnityEngine.Debug.LogWarning("ProceduralTexture: " + material.mainTexture.name + " not supported by Babylon.js");
            }

            if (material.HasProperty("_Wireframe"))
            {
                babylonPbrMaterial.wireframe = (material.GetInt("_Wireframe") != 0);
            }

    	    //bool backfaceCulling = material.HasProperty("_Cull") && material.GetInt("_Cull") == (float)UnityEngine.Rendering.CullMode.Back;
            if (material.HasProperty("_BackFaceCulling"))
            {
                babylonPbrMaterial.backFaceCulling = (material.GetInt("_BackFaceCulling") != 0);
            }

            if (material.HasProperty("_TwoSidedLighting"))
            {
                babylonPbrMaterial.twoSidedLighting = (material.GetInt("_TwoSidedLighting") != 0);
            }

            if (material.HasProperty("_MaxSimultaneousLights"))
            {
                babylonPbrMaterial.maxSimultaneousLights = material.GetInt("_MaxSimultaneousLights");
            }

            if (material.HasProperty("_DisableLighting"))
            {
                babylonPbrMaterial.disableLighting = (material.GetInt("_DisableLighting") != 0);
            }
            
            if (material.HasProperty("_UseEmissiveAsIllumination"))
            {
                babylonPbrMaterial.useEmissiveAsIllumination = (material.GetInt("_UseEmissiveAsIllumination") != 0);
            }
            
            if (material.HasProperty("_IndexOfRefraction"))
            {
                babylonPbrMaterial.indexOfRefraction = material.GetFloat("_IndexOfRefraction");
            }
            
            if (material.HasProperty("_LinkRefractionWithTransparency"))
            {
                babylonPbrMaterial.linkRefractionWithTransparency = (material.GetInt("_LinkRefractionWithTransparency") != 0);
            }
            
            if (material.HasProperty("_UseSpecularOverAlpha"))
            {
                babylonPbrMaterial.useSpecularOverAlpha = (material.GetInt("_UseSpecularOverAlpha") != 0);
            }
            
            if (material.HasProperty("_UseRadianceOverAlpha"))
            {
                babylonPbrMaterial.useRadianceOverAlpha = (material.GetInt("_UseRadianceOverAlpha") != 0);
            }

            if (material.HasProperty("_UsePhysicalLightFalloff"))
            {
                babylonPbrMaterial.usePhysicalLightFalloff = (material.GetInt("_UsePhysicalLightFalloff") != 0);
            }

            // Rendering
            babylonPbrMaterial.cameraContrast = defaultCameraContrast;
            if (material.HasProperty("_CameraContrast")) {
                babylonPbrMaterial.cameraContrast = material.GetFloat("_CameraContrast");
            }
            babylonPbrMaterial.cameraExposure = defaultCameraExposure;
            if (material.HasProperty("_CameraExposure")) {
                babylonPbrMaterial.cameraExposure = material.GetFloat("_CameraExposure");
            }
            babylonPbrMaterial.directIntensity = defaultDirectIntensity;
            if (material.HasProperty("_DirectIntensity")) {
                babylonPbrMaterial.directIntensity = material.GetFloat("_DirectIntensity");
            }
            babylonPbrMaterial.emissiveIntensity = defaultEmissiveIntensity;
            if (material.HasProperty("_EmissiveIntensity")) {
                babylonPbrMaterial.emissiveIntensity = material.GetFloat("_EmissiveIntensity");
            }
            babylonPbrMaterial.specularIntensity = defaultSpecularIntensity;
            if (material.HasProperty("_SpecularIntensity")) {
                babylonPbrMaterial.specularIntensity = material.GetFloat("_SpecularIntensity");
            }
            babylonPbrMaterial.environmentIntensity = defaultEnvironmentIntensity;
            if (material.HasProperty("_EnvironmentIntensity")) {
                babylonPbrMaterial.environmentIntensity = material.GetFloat("_EnvironmentIntensity");
            }
            babylonPbrMaterial.environmentIntensity *= RenderSettings.reflectionIntensity;
            if (material.HasProperty("_MicroSurfaceScaling")) {
                defaultMicroSurfaceScaling = material.GetFloat("_MicroSurfaceScaling");
            }

            // Reflection Color
            babylonPbrMaterial.reflection = (SceneController != null) ? SceneController.skyboxOptions.reflectionColor.ToFloat() : Color.white.ToFloat();
            if (material.HasProperty("_ReflectionColor")) {
                babylonPbrMaterial.reflection = material.GetColor("_ReflectionColor").ToFloat();
            }

            // Albedo Coloring
            if (material.HasProperty("_Color")) {
                babylonPbrMaterial.albedo = material.color.ToFloat();
            }
            var mainTexture2D = material.mainTexture as Texture2D;
            babylonPbrMaterial.albedoTexture = DumpTextureFromMaterial(material, "_MainTex");
            if (material.mainTexture != null && !materialNotSupported) {
                var textureScale = material.mainTextureScale;
                babylonPbrMaterial.albedoTexture.uScale = Tools.GetTextureScale(textureScale.x);
                babylonPbrMaterial.albedoTexture.vScale = Tools.GetTextureScale(textureScale.y);
                var textureOffset = material.mainTextureOffset;
                babylonPbrMaterial.albedoTexture.uOffset = textureOffset.x;
                babylonPbrMaterial.albedoTexture.vOffset = textureOffset.y;
            }

            // Transparency Maps
            DumpTransparency(mainTexture2D, material, babylonPbrMaterial);

            // Glossiess/Reflectivity
            DumpGlossinessReflectivity(material, metallic, defaultMicroSurfaceScaling, babylonPbrMaterial);

            // Emissive Coloring
            if (material.HasProperty("_EmissionColor")) {
                babylonPbrMaterial.emissive = material.GetColor("_EmissionColor").ToFloat();
            }
            babylonPbrMaterial.emissiveTexture = DumpTextureFromMaterial(material, "_EmissionMap");

            // Occlusion Mapping
            babylonPbrMaterial.ambientTexture = DumpTextureFromMaterial(material, "_OcclusionMap");
            if (babylonPbrMaterial.ambientTexture != null && material.HasProperty("_OcclusionStrength")) {
                babylonPbrMaterial.ambientTexture.level = material.GetFloat("_OcclusionStrength");
            }

            // Normal Mapping
            babylonPbrMaterial.bumpTexture = DumpTextureFromMaterial(material, "_BumpMap");
            if (babylonPbrMaterial.bumpTexture != null && material.HasProperty("_BumpScale")) {
                babylonPbrMaterial.bumpTexture.level = material.GetFloat("_BumpScale");
            }
            if (babylonPbrMaterial.albedoTexture != null && babylonPbrMaterial.bumpTexture != null) {
                babylonPbrMaterial.bumpTexture.uScale = babylonPbrMaterial.albedoTexture.uScale;
                babylonPbrMaterial.bumpTexture.vScale = babylonPbrMaterial.albedoTexture.vScale;
                babylonPbrMaterial.bumpTexture.uOffset = babylonPbrMaterial.albedoTexture.uOffset;
                babylonPbrMaterial.bumpTexture.vOffset = babylonPbrMaterial.albedoTexture.vOffset;
            }
            // Use Lightmap As Shadowmap
            if (hasLightmap) {
                var lightmap = LightmapSettings.lightmaps[lightmapIndex].lightmapColor;
                var shadowmap = LightmapSettings.lightmaps[lightmapIndex].shadowMask;
                var texturePath = AssetDatabase.GetAssetPath(lightmap);
                if (!String.IsNullOrEmpty(texturePath)) {
                    ExporterWindow.ReportProgress(1, "Dumping physical material shadow mask: " + lightmap.name);
                    babylonPbrMaterial.lightmapTexture = DumpTexture(lightmap, isLightmap: true, shadowMask: shadowmap);
                    babylonPbrMaterial.lightmapTexture.coordinatesIndex = (lightmapCoordIndex >= 0) ? lightmapCoordIndex : exportationOptions.DefaultCoordinatesIndex;
                    babylonPbrMaterial.useLightmapAsShadowmap = true;

                    babylonPbrMaterial.lightmapTexture.uScale = Tools.GetTextureScale(lightmapScaleOffset.x);
                    babylonPbrMaterial.lightmapTexture.vScale = Tools.GetTextureScale(lightmapScaleOffset.y);

                    babylonPbrMaterial.lightmapTexture.uOffset = lightmapScaleOffset.z;
                    babylonPbrMaterial.lightmapTexture.vOffset = lightmapScaleOffset.w;
                }
            }
            materialsDictionary.Add(babylonPbrMaterial.name, babylonPbrMaterial);
            return babylonPbrMaterial;
        }

        private BabylonMaterial DumpShaderMaterial(Material material, bool receiveShadows, int lightmapIndex = -1, Vector4 lightmapScaleOffset = default(Vector4), int lightmapCoordIndex = -1)
        {
            if (materialsDictionary.ContainsKey(material.name))
            {
                return materialsDictionary[material.name];
            }
            var babylonShaderMaterial = new BabylonUniversalMaterial
            {
                name = material.name,
                id = Guid.NewGuid().ToString(),
                ambient = Color.black.ToFloat(),
                diffuse = Color.white.ToFloat(),
                specular = Color.black.ToFloat(),
                emissive = Color.black.ToFloat(),
                specularPower = 64,
                disableLighting = false,
                maxSimultaneousLights = 4,
                useSpecularOverAlpha = false,
                useEmissiveAsIllumination = false
            };

            List<string> tnames = material.GetTextureNames();
            foreach (string tname in tnames)
            {
                BabylonTexture tdata = DumpTextureFromMaterial(material, tname);
                if (tdata != null)
                {
                    if (SceneBuilder.DefaultPredefinedPropertyNames.IndexOf(tname, StringComparison.OrdinalIgnoreCase) == -1) {
                        babylonShaderMaterial.textures.Add(tname, tdata);
                    }
                }
            }

            List<string> fnames = material.GetFloatNames();
            foreach (string fname in fnames)
            {
                if (SceneBuilder.DefaultPredefinedPropertyNames.IndexOf(fname, StringComparison.OrdinalIgnoreCase) == -1) {
                    float fdata = material.GetFloat(fname);
                    babylonShaderMaterial.floats.Add(fname, fdata);
                }
            }

            List<string> rnames = material.GetRangeNames();
            foreach (string rname in rnames)
            {
                if (SceneBuilder.DefaultPredefinedPropertyNames.IndexOf(rname, StringComparison.OrdinalIgnoreCase) == -1) {
                    float rdata = material.GetFloat(rname);
                    babylonShaderMaterial.floats.Add(rname, rdata);
                }
            }

            List<string> cnames = material.GetColorNames();
            foreach (string cname in cnames)
            {
                if (SceneBuilder.DefaultPredefinedPropertyNames.IndexOf(cname, StringComparison.OrdinalIgnoreCase) == -1) {
                    Color cdata = material.GetColor(cname);
                    babylonShaderMaterial.vectors4.Add(cname, cdata.ToFloat());
                }
            }

            List<string> vnames = material.GetVectorNames();
            foreach (string vname in vnames)
            {
                if (SceneBuilder.DefaultPredefinedPropertyNames.IndexOf(vname, StringComparison.OrdinalIgnoreCase) == -1) {
                    Vector4 vdata = material.GetVector(vname);
                    babylonShaderMaterial.vectors4.Add(vname, vdata.ToFloat());
                }
            }

            Shader shader = material.shader;
            string filename = AssetDatabase.GetAssetPath(shader);
            string program = Tools.LoadTextAsset(filename);
            string basename = shader.name.Replace("Babylon/", "").Replace("/", "_").Replace(" ", "");
            string babylonOptions = Tools.GetShaderProgramSection(basename, program);
            string[] babylonLines = babylonOptions.Split('\n');

            foreach (string babylonLine in babylonLines)
            {
                if (babylonLine.IndexOf("controller:", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string[] controllers = babylonLine.Split(':');
                    if (controllers != null && controllers.Length > 1)
                    {
                        string cbuffer = controllers[1].Replace("[", "").Replace("]", "").Replace("\"", "").Trim(); // Note: Trim Direct Buffer
                        if (!String.IsNullOrEmpty(cbuffer))
                        {
                            babylonShaderMaterial.SetCustomType(cbuffer);
                        }
                    }
                }
            }

            return DumpStandardMaterial(material, receiveShadows, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex, babylonShaderMaterial);
        }
        
        private void DumpGlossinessReflectivity(Material material, bool metallic, float microSurfaceScale, BabylonSystemMaterial babylonPbrMaterial)
        {
            babylonPbrMaterial.metallic = null;
            babylonPbrMaterial.roughness = null;
            babylonPbrMaterial.microSurface = 1.0f;
            babylonPbrMaterial.metallicTexture = null;
            babylonPbrMaterial.useRoughnessFromMetallicTextureAlpha = false;
            babylonPbrMaterial.useRoughnessFromMetallicTextureGreen = true;
            babylonPbrMaterial.useMetallnessFromMetallicTextureBlue = true;
            babylonPbrMaterial.useAmbientOcclusionFromMetallicTextureRed = false;
            babylonPbrMaterial.useMicroSurfaceFromReflectivityMapAlpha = false;
            // ..
            // Format Metallic And Specular Glossiness
            // ..
            float metalness = 0.0f, glossiness = 0.5f;
            if (material.HasProperty("_Metallic")) {
                metalness = Tools.GammaToLinearSpace(material.GetFloat("_Metallic"));
            }
            if (material.HasProperty("_Roughness")) {
                glossiness = (1.0f - material.GetFloat("_Roughness"));
            } else if (material.HasProperty("_Glossiness")) {
                glossiness = material.GetFloat("_Glossiness");
            } else if (material.HasProperty("_Gloss")) {
                glossiness = material.GetFloat("_Gloss");
            }
            float glossinessScale = material.HasProperty("_GlossMapScale") ? material.GetFloat("_GlossMapScale") : 0.5f;
            bool glossyRelfections = (material.HasProperty("_GlossyReflections") && material.GetFloat("_GlossyReflections") != 0.0f);
            if (glossyRelfections == false) {
                glossiness = 0.0f;
                glossinessScale = 0.0f;
            }
            glossiness = Tools.GetGlossinessScale(glossiness);
            glossinessScale = Tools.GetGlossinessScale(glossinessScale);
            // ..
            // Metallic Roughness And Specular Glossiness Workflows
            // ..
            if (metallic == true) {
                Texture2D metallicTexture = null;
                if (material.HasProperty("_MetallicRoughnessMap")) {
                    metallicTexture = material.GetTexture("_MetallicRoughnessMap") as Texture2D;
                } else if (material.HasProperty("_MetallicGlossMap")) {
                    metallicTexture = material.GetTexture("_MetallicGlossMap") as Texture2D;
                }
                if (metallicTexture != null) {
                    bool srgb = metallicTexture.IsSRGB();
                    string metallicTextureFile = AssetDatabase.GetAssetPath(metallicTexture);
                    bool metallicTextureAtlas = (!String.IsNullOrEmpty(metallicTextureFile) && Path.GetFileNameWithoutExtension(metallicTextureFile).EndsWith("_Atlas", StringComparison.OrdinalIgnoreCase));
                    // ..
                    // Export Metallic Glossmap Texture
                    // ..
                    babylonPbrMaterial.metallic = 1.0f;
                    babylonPbrMaterial.roughness = 1.0f;
                    babylonPbrMaterial.environmentIntensity *= glossinessScale;
                    metallicTexture = metallicTexture.Copy(TextureFormat.RGBA32, CopyFilterMode.Source, true);
                    string textureName = material.name;
                    if (metallicTextureAtlas == false) {
                        textureName += "_MetallicGlossMap.png";
                        metallicTexture = Tools.CreateMetallicTextureMap(metallicTexture, glossinessScale, srgb);
                    }
                    var babylonTexture = new BabylonTexture { name = textureName };
                    var textureScale = material.GetTextureScale("_MainTex");
                    babylonTexture.uScale = textureScale.x;
                    babylonTexture.vScale = textureScale.y;
                    var textureOffset = material.GetTextureOffset("_MainTex");
                    babylonTexture.uOffset = textureOffset.x;
                    babylonTexture.vOffset = textureOffset.y;
                    var metallicTexturePath = Path.Combine(Path.GetTempPath(), textureName);
                    metallicTexture.WriteImage(metallicTexturePath, BabylonImageFormat.PNG);
                    babylonScene.AddTexture(metallicTexturePath);
                    if (File.Exists(metallicTexturePath)) {
                        File.Delete(metallicTexturePath);
                    }
                    babylonPbrMaterial.metallicTexture = babylonTexture;
                } else {
                    // Setup Metallic-Roughness Workflow
                    babylonPbrMaterial.metallic = metalness;
                    babylonPbrMaterial.roughness = (1.0f - glossiness);
                    babylonPbrMaterial.environmentIntensity *= glossiness;
                }
            }
            else
            {
                // Setup Specular-Glossiness Workflow
                babylonPbrMaterial.microSurface = (glossiness * microSurfaceScale);
                babylonPbrMaterial.environmentIntensity *= glossiness;
                if (material.HasProperty("_SpecColor")) {
                    babylonPbrMaterial.reflectivity = material.GetColor("_SpecColor").ToFloat();
                } else {
                    babylonPbrMaterial.reflectivity = new float[] {0.2f, 0.2f, 0.2f, 1.0f };
                }
                if (material.HasProperty("_SpecGlossMap")) {
                    var specularTexture = material.GetTexture("_SpecGlossMap") as Texture2D;
                    if (specularTexture != null) {
                        string specularTextureFile = AssetDatabase.GetAssetPath(specularTexture);
                        bool specularTextureAtlas = (!String.IsNullOrEmpty(specularTextureFile) && Path.GetFileNameWithoutExtension(specularTextureFile).EndsWith("_Atlas", StringComparison.OrdinalIgnoreCase));
                        // ..
                        // Encode Specular Glossmap Texture
                        // ..
                        babylonPbrMaterial.microSurface = 1.0f;
                        specularTexture = specularTexture.Copy(TextureFormat.RGBA32, CopyFilterMode.Source, true);
                        string textureName = material.name;
                        if (specularTextureAtlas == false) {
                            textureName += "_SpecularGlossMap.png";
                            specularTexture = Tools.CreateSpecularTextureMap(specularTexture, glossiness);
                        }
                        var babylonTexture = new BabylonTexture { name = textureName };
                        var textureScale = material.GetTextureScale("_MainTex");
                        babylonTexture.uScale = textureScale.x;
                        babylonTexture.vScale = textureScale.y;
                        var textureOffset = material.GetTextureOffset("_MainTex");
                        babylonTexture.uOffset = textureOffset.x;
                        babylonTexture.vOffset = textureOffset.y;
                        var specularTexturePath = Path.Combine(Path.GetTempPath(), textureName);
                        specularTexture.WriteImage(specularTexturePath, BabylonImageFormat.PNG);
                        babylonScene.AddTexture(specularTexturePath);
                        if (File.Exists(specularTexturePath)) {
                            File.Delete(specularTexturePath);
                        }
                        babylonPbrMaterial.reflectivityTexture = babylonTexture;
                        babylonPbrMaterial.useMicroSurfaceFromReflectivityMapAlpha = true;
                    }
                }
            }
        }

        private BabylonTexture DumpLightingReflectionTexture()
        {
            if (Tools.GetReflectionsEnabled(SceneController) == false) return null;
            if (sceneReflectionTexture != null)
            {
                return sceneReflectionTexture; // Note: Already Has Reflection Texture
            }
            if (RenderSettings.defaultReflectionMode == UnityEngine.Rendering.DefaultReflectionMode.Skybox)
            {
                var skybox = RenderSettings.skybox;
                if (skybox != null) {
                    ExporterWindow.ReportProgress(1, "Generating skybox reflection probe... This may take a while.");
                    // ..
                    // Convert Unity ReflectionProbe-0.exr Skybox Reflection (Horizontal Strip To LatLong Panorama Texture)
                    // ..
                    string reflectionProbePath = Tools.GetSceneReflectionProbePath();
                    if (!String.IsNullOrEmpty(reflectionProbePath) && File.Exists(reflectionProbePath)) {
                        var faceTextureExt = ".hdr";
                        var faceTextureName = String.Format("{0}_Reflection", SceneName);
                        var hdrFilename = Path.Combine(babylonScene.OutputPath, (faceTextureName + faceTextureExt));
                        int reflectionSize = RenderSettings.defaultReflectionResolution;
                        var tempTexture = Tools.ExportReflections(reflectionProbePath, hdrFilename, reflectionSize, Tools.ColorCorrection.NoCorrection);
                        if (tempTexture != null) {
                            var hdr = new BabylonHDRCubeTexture();
                            hdr.size = reflectionSize;
                            hdr.isBABYLONPreprocessed = false;
                            hdr.boundingBoxSize = Tools.GetLocalCubemapBoxSize(SceneController);
                            if (hdr.boundingBoxSize != null) hdr.boundingBoxPosition = Tools.GetLocalCubemapBoxPosition(SceneController);
                            sceneReflectionTexture = hdr;
                            sceneReflectionTexture.coordinatesMode = 3;
                            sceneReflectionTexture.name = (faceTextureName + faceTextureExt);
                            Tools.SetTextureWrapMode(sceneReflectionTexture, tempTexture);
                            if (hdr.size > 1024) UnityEngine.Debug.LogWarning("HDR: The generated reflection probe exceeds the maximum recommend size of 1024");
                        } else {
                            UnityEngine.Debug.LogWarning("HDR: Failed to export skybox reflection texture pixel buffer.");
                        }
                    } else {
                        UnityEngine.Debug.LogWarning("HDR: " + Path.GetFileName(reflectionProbePath) + " not been generated. You must enable generate lighting.");
                    }
                }
            }
            else if (RenderSettings.defaultReflectionMode == UnityEngine.Rendering.DefaultReflectionMode.Custom)
            {
                var cubeMap = RenderSettings.customReflection;
                if (cubeMap != null)
                {
                    ExporterWindow.ReportProgress(1, "Exporting skybox reflection probe... This may take a while.");
                    var srcTexturePath = AssetDatabase.GetAssetPath(cubeMap);
                    var srcTextureExt = Path.GetExtension(srcTexturePath);
                    if (srcTextureExt.Equals(".hdr", StringComparison.OrdinalIgnoreCase)) {
                        var hdr = new BabylonHDRCubeTexture();
                        hdr.size = cubeMap.height;
                        hdr.isBABYLONPreprocessed = false;
                        hdr.boundingBoxSize = Tools.GetLocalCubemapBoxSize(SceneController);
                        if (hdr.boundingBoxSize != null) hdr.boundingBoxPosition = Tools.GetLocalCubemapBoxPosition(SceneController);
                        sceneReflectionTexture = hdr;
                        sceneReflectionTexture.extensions = null;
                        sceneReflectionTexture.coordinatesMode = 3;
                        sceneReflectionTexture.name = String.Format("{0}_Reflection{1}", SceneName, srcTextureExt);
                        CopyCubemapTexture(sceneReflectionTexture.name, cubeMap, sceneReflectionTexture);
                        if (hdr.size > 1024) UnityEngine.Debug.LogWarning("HDR: The generated reflection probe exceeds the maximum recommend size of 1024");
                    } else if (srcTextureExt.Equals(".exr", StringComparison.OrdinalIgnoreCase)) {
                        var hdr = new BabylonHDRCubeTexture();
                        hdr.size = cubeMap.height;
                        hdr.isBABYLONPreprocessed = false;
                        hdr.boundingBoxSize = Tools.GetLocalCubemapBoxSize(SceneController);
                        if (hdr.boundingBoxSize != null) hdr.boundingBoxPosition = Tools.GetLocalCubemapBoxPosition(SceneController);
                        sceneReflectionTexture = hdr;
                        sceneReflectionTexture.extensions = null;
                        sceneReflectionTexture.coordinatesMode = 3;
                        sceneReflectionTexture.name = String.Format("{0}_Reflection{1}", SceneName, ".hdr");
                        CopyCubemapTexture(sceneReflectionTexture.name, cubeMap, sceneReflectionTexture);
                        if (hdr.size > 1024) UnityEngine.Debug.LogWarning("HDR: The generated reflection probe exceeds the maximum recommend size of 1024");
                    } else if (srcTextureExt.Equals(".dds", StringComparison.OrdinalIgnoreCase)) {
                        var dds = new BabylonCubeTexture();
                        dds.prefiltered = true;
                        dds.boundingBoxSize = Tools.GetLocalCubemapBoxSize(SceneController);
                        if (dds.boundingBoxSize != null) dds.boundingBoxPosition = Tools.GetLocalCubemapBoxPosition(SceneController);
                        sceneReflectionTexture = dds;
                        sceneReflectionTexture.extensions = null;
                        sceneReflectionTexture.coordinatesMode = 3;
                        sceneReflectionTexture.name = String.Format("{0}_Reflection{1}", SceneName, srcTextureExt);
                        CopyCubemapTexture(sceneReflectionTexture.name, cubeMap, sceneReflectionTexture);
                    } else {
                        UnityEngine.Debug.LogWarning("HDR: Unsupported custom reflection cubemap file type " + srcTextureExt + " for " + cubeMap.name);
                    }
                }
            }
            return sceneReflectionTexture;
        }

        private BabylonTexture DumpTextureFromMaterial(Material material, string name)
        {
            if (!material.HasProperty(name))
            {
                return null;
            }
            var texture = material.GetTexture(name);
            return DumpTexture(texture, material, name);
        }

        private BabylonTexture DumpTexture(Texture texture, Material material = null, string name = "", bool isLightmap = false, Texture2D shadowMask = null)
        {
            if (texture == null)
            {
                return null;
            }

            var texturePath = AssetDatabase.GetAssetPath(texture);
            var textureName = Path.GetFileName(texturePath);
            var textureExt = Path.GetExtension(texturePath);
            var babylonTexture = new BabylonTexture { name = textureName };

            if (material != null)
            {
                var textureScale = material.GetTextureScale(name);
                babylonTexture.uScale = Tools.GetTextureScale(textureScale.x);
                babylonTexture.vScale = Tools.GetTextureScale(textureScale.y);

                var textureOffset = material.GetTextureOffset(name);
                babylonTexture.uOffset = textureOffset.x;
                babylonTexture.vOffset = textureOffset.y;
            }

            var texture2D = texture as Texture2D;
            if (texture2D)
            {
                CopyTexture(texturePath, texture2D, babylonTexture, isLightmap, false, false, shadowMask);
            }
            else
            {
                var cubemap = texture as Cubemap;
                if (cubemap != null)
                {
                    babylonTexture.isCube = textureExt.Equals(".dds", StringComparison.OrdinalIgnoreCase);
                    CopyCubemapTexture(texturePath, cubemap, babylonTexture);
                }
            }
            return babylonTexture;
        }

        private static void DumpTransparency(Texture2D texture, Material material, BabylonStandardMaterial babylonStdMaterial)
        {
            // Legacy Alpha Transparency Mode
            babylonStdMaterial.alpha = 1.0f;
            babylonStdMaterial.alphaMode = 2;
            babylonStdMaterial.alphaCutoff = 0.4f;
            babylonStdMaterial.useAlphaFromDiffuseTexture = false;
            if (babylonStdMaterial.diffuse != null && babylonStdMaterial.diffuse.Length >= 4) {
                babylonStdMaterial.alpha = babylonStdMaterial.diffuse[3];
            }
            bool hasCutoff = material.HasProperty("_Cutoff");
            babylonStdMaterial.alphaCutoff =  hasCutoff ? material.GetFloat("_Cutoff") : 0.4f;
            if (hasCutoff == true && babylonStdMaterial.diffuseTexture != null) {
                babylonStdMaterial.alphaMode = 2; // Note: Alpha Not Used For Blending
                babylonStdMaterial.diffuseTexture.hasAlpha = true;
            }
            if (texture != null && texture.alphaIsTransparency == true) {
                babylonStdMaterial.alphaMode = 7; // Note: Alpha Pre Multiply Blending Mode
                babylonStdMaterial.backFaceCulling = false;
                if (babylonStdMaterial.diffuseTexture != null) {
                    babylonStdMaterial.useAlphaFromDiffuseTexture = true;
                    if (babylonStdMaterial.opacityTexture == null) {
                        babylonStdMaterial.opacityTexture = babylonStdMaterial.diffuseTexture;
                    }
                }
            }
        }
        private static void DumpTransparency(Texture2D texture, Material material, BabylonSystemMaterial babylonPbrMaterial)
        {
            // Standard Alpha Transparency Mode
            bool alphaIsTransparency = false;
            babylonPbrMaterial.alpha = 1.0f;
            babylonPbrMaterial.alphaMode = 2;
            babylonPbrMaterial.useAlphaFromAlbedoTexture = false;
            babylonPbrMaterial.alphaCutoff = material.HasProperty("_Cutoff") ? material.GetFloat("_Cutoff") : 0.4f;
            if (babylonPbrMaterial.albedo != null && babylonPbrMaterial.albedo.Length >= 4) {
                babylonPbrMaterial.alpha = babylonPbrMaterial.albedo[3];
            }
            if (texture != null && texture.alphaIsTransparency == true) {
                alphaIsTransparency = true;
                babylonPbrMaterial.backFaceCulling = false;
                if (babylonPbrMaterial.albedoTexture != null) {
                    babylonPbrMaterial.useAlphaFromAlbedoTexture = true;
                    babylonPbrMaterial.albedoTexture.hasAlpha = true;
                }
            } else {
                if (babylonPbrMaterial.albedoTexture != null) {
                    babylonPbrMaterial.albedoTexture.hasAlpha = false;
                }
            }
            if (material.HasProperty("_Mode")) {
                var blendMode = (BlendMode)material.GetFloat("_Mode");
                if (blendMode == BlendMode.Opaque) {
                    babylonPbrMaterial.alphaMode = 2; // Note: Alpha Not Used For Blending
                    babylonPbrMaterial.transparencyMode = (int)BabylonTransparencyMode.Opaque;
                } else if (blendMode == BlendMode.Cutout) {
                    babylonPbrMaterial.alphaMode = 2; // Note: Alpha Not Used For Blending
                    babylonPbrMaterial.transparencyMode = (int)BabylonTransparencyMode.AlphaTest;
                } else if (blendMode == BlendMode.Fade) {
                    babylonPbrMaterial.alphaMode = 2; // Note: Alpha Combine Blending Mode
                    babylonPbrMaterial.transparencyMode = (alphaIsTransparency) ? (int)BabylonTransparencyMode.AlphaBlend : (int)BabylonTransparencyMode.Opaque;
                } else if (blendMode == BlendMode.Transparent) {
                    babylonPbrMaterial.alphaMode = 7; // Note: Alpha Pre Multiply Blending Mode
                    babylonPbrMaterial.transparencyMode = (alphaIsTransparency) ? (int)BabylonTransparencyMode.AlphaBlend : (int)BabylonTransparencyMode.Opaque;
                }
            }
        }
    }
}

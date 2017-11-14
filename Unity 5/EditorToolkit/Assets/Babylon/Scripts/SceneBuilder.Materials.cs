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
        public static string DefaultPredefinedPropertyNames = "_Cutoff, _Color, _MainTex, _Shininess, _SpecColor, _SpecGlossMap, _GlossMapScale, _AmbientColor, _Emission, _EmissionMap, _Illum, _LightMap, _Cube, _BumpMap, _BumpScale, _GlossyReflections, _ReflectionScale, _LightmapScale, _EnvironmentScale, _Wireframe, _AlphaMode, _DisableLighting, _UseEmissiveAsIllumination, _BackFaceCulling, _TwoSidedLighting, _SmoothnessTextureChannel, _SpecularHighlights, _RefractionTexture, _IndexOfRefraction, _LinkRefractionWithTransparency, _UVSec, _TextureLevel, _MaxSimultaneousLights";
        public static string SystemPredefinedPropertyNames = "_Mode, _Cutoff, _Color, _MainTex, _Glossiness, _SpecColor, _SpecGlossMap, _GlossMapScale, _Metallic, _MetallicGlossMap, _EmissionColor, _EmissionMap, _OcclusionMap, _OcclusionStrength, _BumpMap, _BumpScale, _GlossyReflections, _ReflectionScale, _LightmapScale, _EnvironmentScale, _Wireframe, _AlphaMode, _DisableLighting, _UseEmissiveAsIllumination, _BackFaceCulling, _TwoSidedLighting, _DirectIntensity, _EmissiveIntensity, _SpecularIntensity, _RefractionTexture, _IndexOfRefraction, _LinkRefractionWithTransparency, _CameraContrast, _CameraExposure, _UseSpecularOverAlpha, _UseRadianceOverAlpha, _UsePhysicalLightFalloff, _SmoothnessTextureChannel, _SpecularHighlights, _Parallax, _ParallaxMap, _DetailMask, _DetailAlbedoMap, _DetailNormalMapScale, _DetailNormalMap, _UVSec, _TextureLevel, _MaxSimultaneousLights";
        	        
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
                FileStream destStream = new FileStream(ddsTexturePath, FileMode.Create, FileAccess.Write);
                FileStream sourceStream = new FileStream(srcTexturePath, FileMode.Open, FileAccess.Read);
                try
                {
                    Tools.ConvertFreeImage(sourceStream, FREE_IMAGE_FORMAT.FIF_EXR, destStream, FREE_IMAGE_FORMAT.FIF_HDR);
                } catch (Exception ex) {
                    UnityEngine.Debug.LogException(ex);
                } finally {
                    destStream.Close();
                    sourceStream.Close();
                }
            } else {
                var ddsTexturePath = Path.Combine(babylonScene.OutputPath, Path.GetFileName(texturePath));
                File.Copy(srcTexturePath, ddsTexturePath, true);
                var textureName = Path.GetFileName(texturePath);
                babylonTexture.name = textureName;
            }
        }

        private void CopyTexture(string texturePath, Texture2D texture2D, BabylonTexture babylonTexture, bool isLightmap = false, bool isTerrain = false, bool asJpeg = false)
        {
            bool needToDelete = false;
            ExporterWindow.ReportProgress(1, "Copying texture image file: " + Path.GetFileName(texturePath));
            Tools.SetTextureWrapMode(babylonTexture, texture2D);
            // Convert required file extensions
            string convertList = ".exr, .psd, .tif, .tga";
            string textureExt = Path.GetExtension(texturePath);
            bool hasAlpha = texture2D.alphaIsTransparency;
            bool nativepng = textureExt.Equals(".png", StringComparison.OrdinalIgnoreCase);
            bool nativejpeg = textureExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || textureExt.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
            bool okextension = (nativepng || nativejpeg);
            bool enforceimage = (!okextension && exportationOptions.EnforceImageEncoding);
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
                    Color[] apixels = texture2D.GetPixels(0, 0, texture2D.width, texture2D.height);
                    for (int index = 0; index < apixels.Length; index++) {
                        hasAlpha |= apixels[index].a <= 0.99999f;
                    }
                } else {
                    hasAlpha = true;
                }
                // Validate texture format
                string extension = ".jpg";
                BabylonImageFormat textureFormat = BabylonImageFormat.JPEG;
                bool makeJpeg = (hasAlpha == false && (asJpeg || nativejpeg || (enforceimage && exportationOptions.ImageEncodingOptions == (int)BabylonImageFormat.JPEG)));
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
                    var tempTexture = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBA32, false);
                    if (isLightmap)
                    {
                        float factor = exportationOptions.LightmapMapFactor;
                        Color[] pixels = texture2D.GetPixels(0, 0, texture2D.width, texture2D.height);
                        for (int index = 0; index < pixels.Length; index++)
                        {
                            // Indirect light pixels
                            pixels[index].r = pixels[index].r * pixels[index].a * factor;
                            pixels[index].g = pixels[index].g * pixels[index].a * factor;
                            pixels[index].b = pixels[index].b * pixels[index].a * factor;
                            pixels[index].a = 1.0f;
                        }
                        tempTexture.SetPixels(pixels);
                        tempTexture.Apply();
                    }
                    else
                    {
                        tempTexture.SetPixels32(texture2D.GetPixels32());
                        tempTexture.Apply();
                    }
                    if (isTerrain) tempTexture = Tools.RotateTexture(tempTexture, 0.0f);
                    tempTexture.WriteImage(texturePath, textureFormat);
                    needToDelete = true;
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

        private BabylonMaterial DumpMaterial(Material material, int lightmapIndex = -1, Vector4 lightmapScaleOffset = default(Vector4), int lightmapCoordIndex = -1)
        {
            if (material.shader.name == "Standard" || material.shader.name == "BabylonJS/System/Metallic Setup")
            {
                return DumpPBRMaterial(material, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex, true, false);
            }
            else if (material.shader.name == "Standard (Roughness setup)" || material.shader.name == "BabylonJS/System/Roughness Setup")
            {
                return DumpPBRMaterial(material, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex, true, true);
            }
            else if (material.shader.name == "Standard (Specular setup)" || material.shader.name == "BabylonJS/System/Specular Setup")
            {
                return DumpPBRMaterial(material, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex, false, false);
            }
            else if (material.shader.name == "BabylonJS/System/Standard Material")
            {
                return DumpStandardMaterial(material, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex);
            }
            else if (material.shader.name.StartsWith("BabylonJS/", StringComparison.OrdinalIgnoreCase))
            {
                return DumpShaderMaterial(material, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex);
            }
            return DumpStandardMaterial(material, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex);
        }

        private BabylonMaterial DumpStandardMaterial(Material material, int lightmapIndex = -1, Vector4 lightmapScaleOffset = default(Vector4), int lightmapCoordIndex = -1, BabylonDefaultMaterial defaultMaterial = null)
        {
            bool hasLightmap = (exportationOptions.ExportLightmaps && lightmapIndex >= 0 && lightmapIndex != 65535 && LightmapSettings.lightmaps.Length > lightmapIndex);
            var materialNotSupported = false;
            var materialName = material.name;
            var materialId = Guid.NewGuid().ToString();
            if (hasLightmap && exportationOptions.CreateMaterialInstance) materialName = materialName + ".Instance." + materialId;
            if (!materialsDictionary.ContainsKey(materialName))
            {
                var bMat = (defaultMaterial != null) ? defaultMaterial : new BabylonDefaultMaterial
                {
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

                float diffuseTextureLevel = 1.0f;
                float defaultTextureScale = (SceneController != null) ? SceneController.lightingOptions.textureLevel : 1.0f;
                float defaultLightmapScale = (SceneController != null) ? SceneController.lightingOptions.lightmapScale : 1.0f;
                ExporterWindow.ReportProgress(1, "Exporting standard material: " + material.name);

                if (material.mainTexture && material.mainTexture.GetType().FullName == "UnityEngine.ProceduralTexture")
                {
                    materialNotSupported = true;
                    Debug.LogWarning("ProceduralTexture: " + material.mainTexture.name + " not supported by Babylon.js");
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
                if (material.HasProperty("_Emission"))
                {
                    if (material.GetColorNames().IndexOf("_Emission") >= 0)
                    {
                        var emissiveColor = material.GetColor("_Emission");
                        bMat.emissive = emissiveColor.ToFloat();
                    }
                    else if (material.GetFloatNames().IndexOf("_Emission") >= 0)
                    {
                        // TODO: Convert Lightmapper Emission Color
                        UnityEngine.Debug.LogWarning("Material Emission Is Float Not Color: " + material.name);
                    }
                }
                bMat.emissiveTexture = DumpTextureFromMaterial(material, "_EmissionMap");

                if (material.HasProperty("_Wireframe"))
                {
                    bMat.wireframe = (material.GetInt("_Wireframe") != 0);
                }

                if (material.HasProperty("_AlphaMode"))
                {
                    bMat.alphaMode = material.GetInt("_AlphaMode");
                }

                if (material.HasProperty("_DisableLighting"))
                {
                    bMat.disableLighting = (material.GetInt("_DisableLighting") != 0);
                }
                
                if (material.HasProperty("_UseEmissiveAsIllumination"))
                {
                    bMat.useEmissiveAsIllumination = (material.GetInt("_UseEmissiveAsIllumination") != 0);
                }

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
                
                if (material.HasProperty("_TextureLevel"))
                {
                    diffuseTextureLevel = material.GetFloat("_TextureLevel");
                }

                if (material.mainTexture && !materialNotSupported)
                {
                    var mainTexture2D = material.mainTexture as Texture2D;
                    var mainTexturePath = AssetDatabase.GetAssetPath(mainTexture2D);
                    var alphaCuttOff = 0.0f;
                    if (material.HasProperty("_Cutoff"))
                    {
                        alphaCuttOff = material.GetFloat("_Cutoff");
                    }
                    bMat.diffuseTexture = new BabylonTexture
                    {
                        uScale = Tools.GetTextureScale(material.mainTextureScale.x),
                        vScale = Tools.GetTextureScale(material.mainTextureScale.y),
                        uOffset = material.mainTextureOffset.x,
                        vOffset = material.mainTextureOffset.y
                    };
                    if (bMat.diffuseTexture != null)
                    {
                        bMat.diffuseTexture.level = diffuseTextureLevel * defaultTextureScale;
                    }
                    CopyTexture(mainTexturePath, mainTexture2D, bMat.diffuseTexture);
                    if ((mainTexture2D && mainTexture2D.alphaIsTransparency) || alphaCuttOff > 0.0)
                    {
                        if (bMat.diffuseTexture != null) {
                            bMat.diffuseTexture.hasAlpha = true;
                        }
                        bMat.backFaceCulling = false;
                    }
                }

                // Normal map
                bMat.bumpTexture = DumpTextureFromMaterial(material, "_BumpMap");
                if (bMat.bumpTexture != null && material.HasProperty("_BumpScale"))
                {
                    bMat.bumpTexture.level = material.GetFloat("_BumpScale");
                }

                if (bMat.emissiveTexture == null) bMat.emissiveTexture = DumpTextureFromMaterial(material, "_Illum");
                bMat.reflectionTexture = DumpTextureFromMaterial(material, "_Cube");
                if (material.HasProperty("_ReflectionScale"))
                {
                    float reflectionScale = material.GetFloat("_ReflectionScale");
                    if (bMat.reflectionTexture != null) {
                        bMat.reflectionTexture.level *= reflectionScale;
                    }
                }

                // Lightmap Scaling
                float lightmapScale = defaultLightmapScale;
                if (material.HasProperty("_LightmapScale"))
                {
                    lightmapScale *= material.GetFloat("_LightmapScale");
                }

                // Lightmapping Texture (Support Manual Lightmaps)
                bMat.ambientTexture = DumpTextureFromMaterial(material, "_LightMap");
                if (bMat.ambientTexture == null && hasLightmap)
                {
                    var lightmap = LightmapSettings.lightmaps[lightmapIndex].lightmapColor;
                    var texturePath = AssetDatabase.GetAssetPath(lightmap);
                    if (!String.IsNullOrEmpty(texturePath))
                    {
                        ExporterWindow.ReportProgress(1, "Dumping standard material shadow mask: " + lightmap.name);
                        bMat.lightmapTexture = DumpTexture(lightmap, isLightmap: true);
                        bMat.lightmapTexture.coordinatesIndex = (lightmapCoordIndex >= 0) ? lightmapCoordIndex : exportationOptions.DefaultCoordinatesIndex;
                        bMat.useLightmapAsShadowmap = true;

                        bMat.lightmapTexture.uScale = Tools.GetTextureScale(lightmapScaleOffset.x);
                        bMat.lightmapTexture.vScale = Tools.GetTextureScale(lightmapScaleOffset.y);

                        bMat.lightmapTexture.uOffset = lightmapScaleOffset.z;
                        bMat.lightmapTexture.vOffset = lightmapScaleOffset.w;

                        bMat.lightmapTexture.level *= lightmapScale;
                    }
                }
                materialsDictionary.Add(bMat.name, bMat);
                return bMat;
            }
            return materialsDictionary[material.name];
        }

        private BabylonMaterial DumpPBRMaterial(Material material, int lightmapIndex = -1, Vector4 lightmapScaleOffset = default(Vector4), int lightmapCoordIndex = -1, bool metallic = true, bool roughness = false)
        {
            bool hasLightmap = (exportationOptions.ExportLightmaps && lightmapIndex >= 0 && lightmapIndex != 65535 && LightmapSettings.lightmaps.Length > lightmapIndex);
            var materialNotSupported = false;
            var materialName = material.name;
            var materialId = Guid.NewGuid().ToString();
            if (hasLightmap && exportationOptions.CreateMaterialInstance) materialName = materialName + ".Instance." + materialId;
            if (materialsDictionary.ContainsKey(materialName))
            {
                return materialsDictionary[materialName];
            }
            var babylonPbrMaterial = new BabylonSystemMaterial
            {
                name = materialName,
                id = materialId,
                albedo = Color.white.ToFloat(),
                emissive = Color.black.ToFloat(),
                metallic = null,
                roughness = null,
                microSurface = 0.5f,
                cameraContrast = 1.0f,
                cameraExposure = 1.0f,
                reflection = new[] { 0.5f, 0.5f, 0.5f },
                reflectivity = new[] { 0.2f, 0.2f, 0.2f },
                useSpecularOverAlpha = false,
                useRadianceOverAlpha = false,
                usePhysicalLightFalloff = false,
                useAlphaFromAlbedoTexture = false,
                maxSimultaneousLights = 4,
                useEmissiveAsIllumination = false
            };

            float albedoTextureLevel = 1.0f;
            float defaultTextureScale = (SceneController != null) ? SceneController.lightingOptions.textureLevel : 1.0f;
            float defaultLightmapScale = (SceneController != null) ? SceneController.lightingOptions.lightmapScale : 1.0f;
            ExporterWindow.ReportProgress(1, "Exporting physical material: " + material.name);
            babylonPbrMaterial.environmentIntensity = Tools.GetAmbientIntensity(SceneController);

            if (material.mainTexture && material.mainTexture.GetType().FullName == "UnityEngine.ProceduralTexture")
            {
                materialNotSupported = true;
                Debug.LogWarning("ProceduralTexture: " + material.mainTexture.name + " not supported by Babylon.js");
            }

            if (material.HasProperty("_Wireframe"))
            {
                babylonPbrMaterial.wireframe = (material.GetInt("_Wireframe") != 0);
            }

            if (material.HasProperty("_AlphaMode"))
            {
                babylonPbrMaterial.alphaMode = material.GetInt("_AlphaMode");
            }

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

            if (material.HasProperty("_DirectIntensity"))
            {
                babylonPbrMaterial.directIntensity = material.GetFloat("_DirectIntensity");
            }
            
            if (material.HasProperty("_EmissiveIntensity"))
            {
                babylonPbrMaterial.emissiveIntensity = material.GetFloat("_EmissiveIntensity");
            }
            
            if (material.HasProperty("_SpecularIntensity"))
            {
                babylonPbrMaterial.specularIntensity = material.GetFloat("_SpecularIntensity");
            }
            
            if (material.HasProperty("_RefractionTexture"))
            {
                var refactionTexture = material.GetTexture("_RefractionTexture");
                babylonPbrMaterial.refractionTexture = DumpTexture(refactionTexture, material);
            }
            
            if (material.HasProperty("_IndexOfRefraction"))
            {
                babylonPbrMaterial.indexOfRefraction = material.GetFloat("_IndexOfRefraction");
            }
            
            if (material.HasProperty("_LinkRefractionWithTransparency"))
            {
                babylonPbrMaterial.linkRefractionWithTransparency = (material.GetInt("_LinkRefractionWithTransparency") != 0);
            }
            
            if (material.HasProperty("_CameraContrast"))
            {
                babylonPbrMaterial.cameraContrast = material.GetFloat("_CameraContrast");
            }
            
            if (material.HasProperty("_CameraExposure"))
            {
                babylonPbrMaterial.cameraExposure = material.GetFloat("_CameraExposure");
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

            if (material.HasProperty("_TextureLevel"))
            {
                albedoTextureLevel = material.GetFloat("_TextureLevel");
            }

            // Albedo
            if (material.HasProperty("_Color"))
            {
                babylonPbrMaterial.albedo = material.color.ToFloat();
            }
            var mainTexture2D = material.mainTexture as Texture2D;
            var alphaCuttOff = 0f;
            if (material.HasProperty("_Cutoff"))
            {
                alphaCuttOff = material.GetFloat("_Cutoff");
            }
            babylonPbrMaterial.albedoTexture = DumpTextureFromMaterial(material, "_MainTex");
            if (material.mainTexture != null && !materialNotSupported)
            {
                var textureScale = material.mainTextureScale;
                babylonPbrMaterial.albedoTexture.uScale = Tools.GetTextureScale(textureScale.x);
                babylonPbrMaterial.albedoTexture.vScale = Tools.GetTextureScale(textureScale.y);
                var textureOffset = material.mainTextureOffset;
                babylonPbrMaterial.albedoTexture.uOffset = textureOffset.x;
                babylonPbrMaterial.albedoTexture.vOffset = textureOffset.y;
            }
            if (babylonPbrMaterial.albedoTexture != null)
            {
                babylonPbrMaterial.albedoTexture.level = albedoTextureLevel * defaultTextureScale;
            }
            if ((mainTexture2D && mainTexture2D.alphaIsTransparency) || alphaCuttOff > 0.0)
            {
                if (babylonPbrMaterial.albedoTexture != null) {
                    babylonPbrMaterial.albedoTexture.hasAlpha = true;
                    babylonPbrMaterial.backFaceCulling = false;
                }
            }
            
            // Emissive
            if (material.HasProperty("_EmissionColor"))
            {
                babylonPbrMaterial.emissive = material.GetColor("_EmissionColor").ToFloat();
            }
            babylonPbrMaterial.emissiveTexture = DumpTextureFromMaterial(material, "_EmissionMap");

            // Transparency
            DumpTransparency(material, babylonPbrMaterial);

            // Glossiess/Reflectivity
            DumpGlossinessReflectivity(material, metallic, roughness, babylonPbrMaterial);

            // Occlusion
            babylonPbrMaterial.ambientTexture = DumpTextureFromMaterial(material, "_OcclusionMap");
            if (babylonPbrMaterial.ambientTexture != null && material.HasProperty("_OcclusionStrength"))
            {
                babylonPbrMaterial.ambientTexture.level = material.GetFloat("_OcclusionStrength");
            }

            // Normal
            babylonPbrMaterial.bumpTexture = DumpTextureFromMaterial(material, "_BumpMap");
            if (babylonPbrMaterial.bumpTexture != null && material.HasProperty("_BumpScale"))
            {
                babylonPbrMaterial.bumpTexture.level = material.GetFloat("_BumpScale");
            }

            // Skybox Reflection
            if (SceneController == null || ( SceneController != null && SceneController.lightingOptions.enableReflections)) {
                bool enableSkyboxReflection = true;
                if (material.HasProperty("_GlossyReflections"))
                {
                    enableSkyboxReflection = (material.GetInt("_GlossyReflections") == 1.0f);
                }
                babylonPbrMaterial.reflectionTexture = (enableSkyboxReflection) ? DumpLightingReflectionTexture() : null;
                // Reflection Scaling
                if (material.HasProperty("_ReflectionScale"))
                {
                    float reflectionScale = material.GetFloat("_ReflectionScale");
                    if (babylonPbrMaterial.reflectionTexture != null) {
                        babylonPbrMaterial.reflectionTexture.level *= reflectionScale;
                    }
                }
            }

            // Ambient Scaling
            if (material.HasProperty("_EnvironmentScale"))
            {
                float environmentScale = material.GetFloat("_EnvironmentScale");
                babylonPbrMaterial.environmentIntensity *= environmentScale;
            }

            // Lightmap Scaling
            float lightmapScale = defaultLightmapScale;
            if (material.HasProperty("_LightmapScale"))
            {
                lightmapScale *= material.GetFloat("_LightmapScale");
            }

            // Lightmapping Texture
            if (hasLightmap)
            {
                var lightmap = LightmapSettings.lightmaps[lightmapIndex].lightmapColor;
                var texturePath = AssetDatabase.GetAssetPath(lightmap);
                if (!String.IsNullOrEmpty(texturePath))
                {
                    ExporterWindow.ReportProgress(1, "Dumping physical material shadow mask: " + lightmap.name);
                    babylonPbrMaterial.lightmapTexture = DumpTexture(lightmap, isLightmap: true);
                    babylonPbrMaterial.lightmapTexture.coordinatesIndex = (lightmapCoordIndex >= 0) ? lightmapCoordIndex : exportationOptions.DefaultCoordinatesIndex;
                    babylonPbrMaterial.useLightmapAsShadowmap = true;

                    babylonPbrMaterial.lightmapTexture.uScale = Tools.GetTextureScale(lightmapScaleOffset.x);
                    babylonPbrMaterial.lightmapTexture.vScale = Tools.GetTextureScale(lightmapScaleOffset.y);

                    babylonPbrMaterial.lightmapTexture.uOffset = lightmapScaleOffset.z;
                    babylonPbrMaterial.lightmapTexture.vOffset = lightmapScaleOffset.w;

                    babylonPbrMaterial.lightmapTexture.level *= lightmapScale;
                }
            }
            materialsDictionary.Add(babylonPbrMaterial.name, babylonPbrMaterial);
            return babylonPbrMaterial;
        }

        private BabylonMaterial DumpShaderMaterial(Material material, int lightmapIndex = -1, Vector4 lightmapScaleOffset = default(Vector4), int lightmapCoordIndex = -1)
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
                useSpecularOverAlpha = false,
                maxSimultaneousLights = 4,
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
            string basename = shader.name.Replace("BabylonJS/", "").Replace("/", "_").Replace(" ", "");
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

            return DumpStandardMaterial(material, lightmapIndex, lightmapScaleOffset, lightmapCoordIndex, babylonShaderMaterial);
        }

        private void DumpGlossinessReflectivity(Material material, bool metallic, bool roughness, BabylonSystemMaterial babylonPbrMaterial)
        {
            float glossiness = 0.5f;
            if (material.HasProperty("_Glossiness"))
            {
                glossiness = material.GetFloat("_Glossiness");
            }
            if (metallic)
            {
                // Metallic-Roughness Workflow
                float metalness = 0.0f;
                float roughvalue = (roughness == true) ? glossiness : (1.0f - glossiness);
                if (material.HasProperty("_Metallic"))
                {
                    metalness = material.GetFloat("_Metallic");
                    babylonPbrMaterial.metallic = metalness;
                    babylonPbrMaterial.roughness = roughvalue;
                    babylonPbrMaterial.microSurface = glossiness;
                    babylonPbrMaterial.reflectivity = new float[] { metalness * babylonPbrMaterial.albedo[0], metalness * babylonPbrMaterial.albedo[1], metalness * babylonPbrMaterial.albedo[2] };
                    babylonPbrMaterial.reflectivityTexture = DumpTextureFromMaterial(material, "_MetallicGlossMap");

                    /* Note: Append Metalness To Albedo Texture
                    if (babylonPbrMaterial.albedoTexture != null)
                    {
                        var albedoTexture = material.GetTexture("_MainTex") as Texture2D;
                        var metallicTexture = material.GetTexture("_MetallicGlossMap") as Texture2D;
                        if (albedoTexture != null && metallicTexture != null)
                        {
                            var albedoPixels = GetPixels(albedoTexture);
                            var reflectivityTexture = new Texture2D(albedoTexture.width, albedoTexture.height, TextureFormat.RGBA32, false);
                            reflectivityTexture.alphaIsTransparency = true;
                            babylonPbrMaterial.useMicroSurfaceFromReflectivityMapAlpha = true;
                            var metallicPixels = GetPixels(metallicTexture);
                            for (var i = 0; i < albedoTexture.width; i++)
                            {
                                for (var j = 0; j < albedoTexture.height; j++)
                                {
                                    var metallicPixel = metallicPixels[j * albedoTexture.width + i];
                                    albedoPixels[j * albedoTexture.width + i].r *= metallicPixel.r;
                                    albedoPixels[j * albedoTexture.width + i].g *= metallicPixel.r;
                                    albedoPixels[j * albedoTexture.width + i].b *= metallicPixel.r;
                                    albedoPixels[j * albedoTexture.width + i].a = metallicPixel.a;
                                }
                            }
                            reflectivityTexture.SetPixels(albedoPixels);
                            reflectivityTexture.Apply();

                            var textureName = albedoTexture.name + "_MetallicGlossMap.png";
                            var babylonTexture = new BabylonTexture { name = textureName };
                            var textureScale = material.GetTextureScale("_MainTex");
                            babylonTexture.uScale = Tools.GetTextureScale(textureScale.x);
                            babylonTexture.vScale = Tools.GetTextureScale(textureScale.y);
                            var textureOffset = material.GetTextureOffset("_MainTex");
                            babylonTexture.uOffset = textureOffset.x;
                            babylonTexture.vOffset = textureOffset.y;
                            //Tools.SetTextureWrapMode(babylonTexture, mainTexture2D);

                            var reflectivityTexturePath = Path.Combine(Path.GetTempPath(), textureName);
                            reflectivityTexture.WriteImage(reflectivityTexturePath, BabylonImageFormat.PNG);
                            babylonScene.AddTexture(reflectivityTexturePath);
                            if (File.Exists(reflectivityTexturePath))
                            {
                                File.Delete(reflectivityTexturePath);
                            }
                            babylonPbrMaterial.reflectivityTexture = babylonTexture;
                        }
                    }
                    */
                }
                else
                {
                    babylonPbrMaterial.metallic = metalness;
                    babylonPbrMaterial.roughness = roughvalue;
                    babylonPbrMaterial.microSurface = glossiness;
                    babylonPbrMaterial.reflectivity = new float[] { metalness * babylonPbrMaterial.albedo[0], metalness * babylonPbrMaterial.albedo[1], metalness * babylonPbrMaterial.albedo[2] };
                }
            }
            else
            {
                // Specular-Glossiness Workflow
                babylonPbrMaterial.microSurface = glossiness;
                if (material.HasProperty("_SpecColor"))
                {
                    babylonPbrMaterial.reflectivity = material.GetColor("_SpecColor").ToFloat();
                }
                else
                {
                    babylonPbrMaterial.reflectivity = new float[] {0.2f, 0.2f, 0.2f };
                }
                babylonPbrMaterial.reflectivityTexture = DumpTextureFromMaterial(material, "_SpecGlossMap");
            }
            // Use Micro-Surface From Metallic Map
            if (babylonPbrMaterial.reflectivityTexture != null && babylonPbrMaterial.reflectivityTexture.hasAlpha)
            {
                babylonPbrMaterial.useMicroSurfaceFromReflectivityMapAlpha = true;
            }
        }

        private BabylonTexture DumpLightingReflectionTexture()
        {
            if (SceneController == null || SceneController.lightingOptions.enableReflections == false)
            {
                return null;
            }
            if (sceneReflectionTexture != null)
            {
                return sceneReflectionTexture;
            }
            if (RenderSettings.defaultReflectionMode == UnityEngine.Rendering.DefaultReflectionMode.Skybox)
            {
                var skybox = RenderSettings.skybox;
                if (skybox != null) {
                    int reflectionProbe = 0;
                    string srcTexturePath = null;
                    string srcTextureExt = null;
                    bool bilinearScaling = true;
                    int reflectionResolution = RenderSettings.defaultReflectionResolution;
                    if (skybox.shader.name == "Skybox/Cubemap" && SceneController != null && SceneController.skyboxOptions.highDynamicRange == true)
                    {
                        var cubeMap = skybox.GetTexture("_Tex") as Cubemap;
                        if (cubeMap != null)
                        {
                            srcTexturePath = AssetDatabase.GetAssetPath(cubeMap);
                            srcTextureExt = Path.GetExtension(srcTexturePath);
                            if (srcTextureExt.Equals(".hdr", StringComparison.OrdinalIgnoreCase)) {
                                reflectionProbe = 1;
                            } else if (srcTextureExt.Equals(".exr", StringComparison.OrdinalIgnoreCase)) {
                                reflectionProbe = 2;
                            } else if (srcTextureExt.Equals(".dds", StringComparison.OrdinalIgnoreCase)) {
                                return null; // Note: Custom skybox reflection required for high dynamic range DDS cubemap
                            }
                        }
                    }
                    if (reflectionProbe > 0)
                    {
                        ExporterWindow.ReportProgress(1, "Baking skybox reflection probe... This may take a while.");
                        var hdr = new BabylonHDRCubeTexture();
                        hdr.size = reflectionResolution;
                        hdr.isBABYLONPreprocessed = false;
                        sceneReflectionTexture = hdr;
                        sceneReflectionTexture.isCube = true;
                        sceneReflectionTexture.coordinatesMode = 3;
                        sceneReflectionTexture.name = String.Format("{0}_Reflection{1}", SceneName, ".hdr");
                        try
                        {
                            FREE_IMAGE_FORMAT srcType = FREE_IMAGE_FORMAT.FIF_HDR;
                            if (reflectionProbe == 1) {
                                srcType = FREE_IMAGE_FORMAT.FIF_HDR;
                            } else if (reflectionProbe == 2) {
                                srcType = FREE_IMAGE_FORMAT.FIF_EXR;
                            }
                            FREE_IMAGE_FILTER rescaleFilter = FREE_IMAGE_FILTER.FILTER_LANCZOS3;
                            int rescaleWidth = reflectionResolution * 4;
                            int rescaleHeight = rescaleWidth / 2;
                            var probeFilename = Path.Combine(babylonScene.OutputPath, sceneReflectionTexture.name);
                            FileStream destStream = new FileStream(probeFilename, FileMode.Create, FileAccess.Write);
                            FileStream sourceStream = new FileStream(srcTexturePath, FileMode.Open, FileAccess.Read);
                            try
                            {
                                Tools.ConvertFreeImage(sourceStream, srcType, destStream, FREE_IMAGE_FORMAT.FIF_HDR, FREE_IMAGE_TYPE.FIT_UNKNOWN, true, FREE_IMAGE_COLOR_DEPTH.FICD_AUTO, FREE_IMAGE_LOAD_FLAGS.DEFAULT, FREE_IMAGE_SAVE_FLAGS.DEFAULT, 0.0, false, false, rescaleWidth, rescaleHeight, rescaleFilter);
                            } catch (Exception ex) {
                                UnityEngine.Debug.LogException(ex);
                            } finally {
                                destStream.Close();
                                sourceStream.Close();
                            }
                        }
                        catch (Exception ex) 
                        {
                            UnityEngine.Debug.LogException(ex);
                        }
                        sceneReflectionTexture.level = Tools.GetReflectIntensity(SceneController);
                    }
                    else if (this.skyboxTextures != null && this.skyboxTextures.Length > 0)
                    {
                        ExporterWindow.ReportProgress(1, "Generating skybox reflection textures... This may take a while.");
                        string skyboxExt = Path.GetExtension(this.skyboxTextures[0].filename);
                        var faceTextureExt = ".jpg";
                        var faceTextureFormat = BabylonImageFormat.JPEG;
                        if (skyboxExt.Equals(".png", StringComparison.OrdinalIgnoreCase)) {
                            faceTextureExt = ".png";
                            faceTextureFormat = BabylonImageFormat.PNG;
                        }
                        string frontTextureExt = "_pz" + faceTextureExt;
                        string backTextureExt = "_nz" + faceTextureExt;
                        string leftTextureExt = "_px" + faceTextureExt;
                        string rightTextureExt = "_nx" + faceTextureExt;
                        string upTextureExt = "_py" + faceTextureExt;
                        string downTextureExt = "_ny" + faceTextureExt;
                        sceneReflectionTexture = new BabylonTexture();
                        sceneReflectionTexture.name = String.Format("{0}_Reflection", SceneName);
                        sceneReflectionTexture.isCube = true;
                        sceneReflectionTexture.coordinatesMode = 5;
                        sceneReflectionTexture.extensions = new string[] { leftTextureExt, upTextureExt, frontTextureExt, rightTextureExt, downTextureExt, backTextureExt };
                        Tools.SetTextureWrapMode(sceneReflectionTexture, this.skyboxTextures[0].texture);
                        foreach (var face in this.skyboxTextures) {
                            face.texture.Scale(reflectionResolution, reflectionResolution, bilinearScaling);
                            face.texture.WriteImage(face.filename.Replace("_Skybox", "_Reflection").Replace(skyboxExt, faceTextureExt), faceTextureFormat);
                        }
                        sceneReflectionTexture.level = Tools.GetReflectIntensity(SceneController);
                    }
                }
            }
            else if (RenderSettings.defaultReflectionMode == UnityEngine.Rendering.DefaultReflectionMode.Custom)
            {
                var cubeMap = RenderSettings.customReflection;
                if (cubeMap != null)
                {
                    ExporterWindow.ReportProgress(1, "Exporting custom reflection texture... This may take a while.");
                    var srcTexturePath = AssetDatabase.GetAssetPath(cubeMap);
                    var srcTextureExt = Path.GetExtension(srcTexturePath);
                    if (srcTextureExt.Equals(".hdr", StringComparison.OrdinalIgnoreCase)) {
                        var hdr = new BabylonHDRCubeTexture();
                        hdr.size = cubeMap.height;
                        hdr.isBABYLONPreprocessed = false;
                        sceneReflectionTexture = hdr;
                        sceneReflectionTexture.isCube = true;
                        sceneReflectionTexture.coordinatesMode = 3;
                        sceneReflectionTexture.name = String.Format("{0}_Reflection{1}", SceneName, srcTextureExt);
                        CopyCubemapTexture(sceneReflectionTexture.name, cubeMap, sceneReflectionTexture);
                        sceneReflectionTexture.level = Tools.GetReflectIntensity(SceneController);
                    } else if (srcTextureExt.Equals(".exr", StringComparison.OrdinalIgnoreCase)) {
                        var hdr = new BabylonHDRCubeTexture();
                        hdr.size = cubeMap.height;
                        hdr.isBABYLONPreprocessed = false;
                        sceneReflectionTexture = hdr;
                        sceneReflectionTexture.isCube = true;
                        sceneReflectionTexture.coordinatesMode = 3;
                        sceneReflectionTexture.name = String.Format("{0}_Reflection{1}", SceneName, ".hdr");
                        CopyCubemapTexture(sceneReflectionTexture.name, cubeMap, sceneReflectionTexture);
                        sceneReflectionTexture.level = Tools.GetReflectIntensity(SceneController);
                    } else if (srcTextureExt.Equals(".dds", StringComparison.OrdinalIgnoreCase)) {
                        var dds = new BabylonCubeTexture();
                        sceneReflectionTexture = dds;
                        sceneReflectionTexture.isCube = true;
                        sceneReflectionTexture.coordinatesMode = 5;
                        sceneReflectionTexture.name = String.Format("{0}_Reflection{1}", SceneName, srcTextureExt);
                        CopyCubemapTexture(sceneReflectionTexture.name, cubeMap, sceneReflectionTexture);
                        sceneReflectionTexture.level = Tools.GetReflectIntensity(SceneController);
                    } else {
                        UnityEngine.Debug.LogWarning("HDR: Unsupported custom reflection cubemap file type " + srcTextureExt + " for " + cubeMap.name);
                        return null;
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

        private BabylonTexture DumpTexture(Texture texture, Material material = null, string name = "", bool isLightmap = false)
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
                CopyTexture(texturePath, texture2D, babylonTexture, isLightmap);
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

        private static void DumpTransparency(Material material, BabylonSystemMaterial babylonPbrMaterial)
        {
            if (material.HasProperty("_Mode"))
            {
                var blendMode = (ShaderInterface.BlendMode)material.GetFloat("_Mode");
                if (blendMode == ShaderInterface.BlendMode.Fade || blendMode == ShaderInterface.BlendMode.Transparent)
                {
                    // Transparent Albedo
                    if (babylonPbrMaterial.albedoTexture != null && babylonPbrMaterial.albedoTexture.hasAlpha)
                    {
                        babylonPbrMaterial.useAlphaFromAlbedoTexture = true;
                    }
                    else
                    {
                        // Material Alpha
                        babylonPbrMaterial.alpha = babylonPbrMaterial.albedo[3];
                    }
                }
                else if (blendMode == ShaderInterface.BlendMode.Cutout)
                {
                    // Cutout
                    // Note: Uses the texture hasAlpha property.
                }
                else
                {
                    // Opaque
                    if (babylonPbrMaterial.albedoTexture != null)
                    {
                        babylonPbrMaterial.albedoTexture.hasAlpha = false;
                    }
                    babylonPbrMaterial.alpha = 1.0f;
                }
            }
        }
    }
}

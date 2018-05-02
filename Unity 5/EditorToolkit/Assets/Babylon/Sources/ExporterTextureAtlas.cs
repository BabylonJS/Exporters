using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity3D2Babylon
{
    public class ExporterTextureAtlas : EditorWindow
    {
        SkinnedMeshRenderer skinMeshRenderer = null;
        BabylonImageFormat textureAtlasFormat = BabylonImageFormat.PNG;
        BabylonTextureScale textureImageScaling = BabylonTextureScale.Bilinear;
        Shader textureAtlasShader = null;
        int textureAtlasSize = 4096;
        int maxTextureImageSize = 0;
        bool bakeTextureMaps = true;
        bool linearInterpolation = true;
        bool updateSkinRenderer = true;
        bool bakeAlphaEncoding = false;

        // Texture Atlas Buffers        
        Material atlasMaterial = null;
        List<Texture2D> mainTextures = null;
        List<Texture2D> bumpTextures = null;
        List<Texture2D> ambientTextures = null;
        List<Texture2D> emissionTextures = null;
        List<Texture2D> metallicTextures = null;
        List<Texture2D> specularTextures = null;
        string bumpFilename = String.Empty;
        string ambientFilename = String.Empty;
        string emissionFilename = String.Empty;
        string metallicFilename = String.Empty;
        string specularFilename = String.Empty;

        [MenuItem("Babylon/Texture Atlas Skin", false, 208)]
        public static void InitConverter()
        {
            ExporterTextureAtlas combiner = ScriptableObject.CreateInstance<ExporterTextureAtlas>();
            combiner.OnInitialize();
            combiner.ShowUtility();
        }

        public void OnInitialize()
        {
            maxSize = new Vector2(512, 300);
            minSize = this.maxSize;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Texture Atlas Skin");
            textureAtlasShader = Shader.Find("Standard");
            if (skinMeshRenderer == null) {
                skinMeshRenderer = ((GameObject)Selection.activeObject).GetComponent<SkinnedMeshRenderer>();
            }
        }

        public void OnGUI()
        {
            // Update texture atlas window
            EditorGUILayout.Space();
            skinMeshRenderer = EditorGUILayout.ObjectField("Skin Mesh Renderer:", skinMeshRenderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
            EditorGUILayout.Space();
            bakeTextureMaps = EditorGUILayout.Toggle("Bake Texture Maps:", bakeTextureMaps);
            EditorGUILayout.Space();
            bakeAlphaEncoding = EditorGUILayout.Toggle("Bake Image Alpha:", bakeAlphaEncoding);
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.EndDisabledGroup();
            textureAtlasSize = (int)EditorGUILayout.Slider("Texture Atlas Size:", textureAtlasSize, 128, 8192);
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(true);
                textureAtlasShader = EditorGUILayout.ObjectField("Texture Atlas Shader:", textureAtlasShader, typeof(Shader), true) as Shader;
                EditorGUILayout.Space();
                textureAtlasFormat = (BabylonImageFormat)EditorGUILayout.EnumPopup("Texture Atlas Format:", textureAtlasFormat, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();
            textureImageScaling = (BabylonTextureScale)EditorGUILayout.EnumPopup("Texture Image Scale:", textureImageScaling, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            maxTextureImageSize = (int)EditorGUILayout.Slider("Texture Image Max:", maxTextureImageSize, 0, 4096);
            EditorGUILayout.Space();
            linearInterpolation = EditorGUILayout.Toggle("Use Linear Interpolation:", linearInterpolation);
            EditorGUILayout.Space();
            updateSkinRenderer = EditorGUILayout.Toggle("Update Skin Renderers:", updateSkinRenderer);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("Bake Texture Atlas Skin"))
            {
                if (skinMeshRenderer)
                {
                    Bake();
                }
                if (!skinMeshRenderer)
                {
                    ExporterWindow.ShowMessage("You must select a skin mesh renderer");
                }
            }
        }

        public void Bake()
        {
            // Validate Project Platform
            if (!Unity3D2Babylon.Tools.ValidateProjectPlatform()) return;

            try
            {
                atlasMaterial = null;
                mainTextures = null;
                bumpTextures = null;
                ambientTextures = null;
                emissionTextures = null;
                metallicTextures = null;
                specularTextures = null;
                bumpFilename = String.Empty;
                ambientFilename = String.Empty;
                emissionFilename = String.Empty;
                metallicFilename = String.Empty;
                specularFilename = String.Empty;

                CreateTextureAtlas();
                PackTextureAtlasNormals();
                PackTextureAtlasAmbients();
                PackTextureAtlasEmissions();
                PackTextureAtlasMetallics();
                PackTextureAtlasSpeculars();

                ExporterWindow.ReportProgress(1, "Saving assets to disk...");
                AssetDatabase.SaveAssets();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            finally
            {
                ExporterWindow.ReportProgress(1, "Refresing assets database...");
                AssetDatabase.Refresh();
                ExporterWindow.ReportProgress(1, "Texture atlas baking complete.");
                EditorUtility.ClearProgressBar();
            }
            this.Close();
        }

        public void CreateTextureAtlas()
        {
            if (skinMeshRenderer != null && skinMeshRenderer.sharedMaterials != null && skinMeshRenderer.sharedMaterials.Length > 1)
            {
                string filename = EditorUtility.SaveFilePanelInProject("Texture Atlas Skin", "", "asset", "Bake Skin Mesh Renderer Texture Atlas");
                if (!String.IsNullOrEmpty(filename))
                {
                    ExporterWindow.ReportProgress(1, "Baking texture atlas skin... This may take a while.");
                    string filepath = Path.GetDirectoryName(filename);
                    string filelabel = Path.GetFileNameWithoutExtension(filename);
                    Tools.ValidateAssetFolders(filepath.TrimEnd('/'));
                    
                    // Texture atlas file info
                    string atlasExt = "png";
                    string atlasName = String.Format("{0}_Texture_Atlas", filelabel);
                    string atlasFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), atlasName, atlasExt);
                    string bumpName = String.Format("{0}_Normal_Atlas", filelabel);
                    string bumpFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), bumpName, atlasExt);
                    string ambientName = String.Format("{0}_Ambient_Atlas", filelabel);
                    string ambientFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), ambientName, atlasExt);
                    string emissionName = String.Format("{0}_Emission_Atlas", filelabel);
                    string emissionFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), emissionName, atlasExt);
                    string metallicName = String.Format("{0}_Metallic_Atlas", filelabel);
                    string metallicFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), metallicName, atlasExt);
                    string specularName = String.Format("{0}_Specular_Atlas", filelabel);
                    string specularFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), specularName, atlasExt);
                    string materialName = String.Format("{0}_Material", filelabel);
                    string materialFile = String.Format("{0}/Materials/{1}.asset", filepath.TrimEnd('/'), materialName);
                    
                    // Create atlas textures
                    mainTextures = new List<Texture2D>();
                    bumpTextures = new List<Texture2D>();
                    ambientTextures = new List<Texture2D>();
                    emissionTextures = new List<Texture2D>();
                    metallicTextures = new List<Texture2D>();
                    specularTextures = new List<Texture2D>();
                    foreach (var material in skinMeshRenderer.sharedMaterials) {
                        Texture2D colorTexture = null;
                        Texture2D normalTexture = null;
                        Texture2D ambientTexture = null;
                        Texture2D emissionTexture = null;
                        Texture2D metallicTexture = null;
                        Texture2D specularTexture = null;
                        bool metallicTextureSRGB = true;
                        if (material.mainTexture != null) {
                            Texture2D primaryTexture = material.mainTexture as Texture2D;
                            primaryTexture.ForceReadable();
                            colorTexture = primaryTexture.Copy();
                            if (colorTexture != null && bakeTextureMaps == true) {
                                if (material.HasProperty("_BumpMap")) {
                                    Texture2D bumpTexture = material.GetTexture("_BumpMap") as Texture2D;
                                    if (bumpTexture != null)
                                    {
                                        // Format texture import settings
                                        string bumpTexturePath = AssetDatabase.GetAssetPath(bumpTexture);
                                        var importTool = new BabylonTextureImporter(bumpTexturePath);
                                        var importType = importTool.textureImporter.textureType;
                                        try
                                        {
                                            importTool.textureImporter.isReadable = true;
                                            importTool.textureImporter.textureType = TextureImporterType.Default;
                                            importTool.ForceUpdate();
                                            normalTexture = bumpTexture.Copy();
                                            if (normalTexture.width != colorTexture.width || normalTexture.height != colorTexture.height) {
                                                normalTexture.Scale(colorTexture.width, colorTexture.height);
                                            }
                                        }
                                        catch(Exception ex)
                                        {
                                            UnityEngine.Debug.LogException(ex);
                                        }
                                        finally
                                        {
                                            // Restore texture importer type
                                            importTool.textureImporter.textureType = importType;
                                            importTool.ForceUpdate();
                                        }
                                    }
                                }
                                if (material.HasProperty("_EmissionMap")) {
                                    Texture2D emitTexture = material.GetTexture("_EmissionMap") as Texture2D;
                                    if (emitTexture != null)
                                    {
                                        // Format texture import settings
                                        string emissionTexturePath = AssetDatabase.GetAssetPath(emitTexture);
                                        var importTool = new BabylonTextureImporter(emissionTexturePath);
                                        var importType = importTool.textureImporter.textureType;
                                        try
                                        {
                                            importTool.textureImporter.isReadable = true;
                                            importTool.textureImporter.textureType = TextureImporterType.Default;
                                            importTool.ForceUpdate();
                                            emissionTexture = emitTexture.Copy();
                                            if (emissionTexture.width != colorTexture.width || emissionTexture.height != colorTexture.height) {
                                                emissionTexture.Scale(colorTexture.width, colorTexture.height);
                                            }
                                        }
                                        catch(Exception ex)
                                        {
                                            UnityEngine.Debug.LogException(ex);
                                        }
                                        finally
                                        {
                                            // Restore texture importer type
                                            importTool.textureImporter.textureType = importType;
                                            importTool.ForceUpdate();
                                        }
                                    }
                                }
                                if (material.HasProperty("_OcclusionMap")) {
                                    Texture2D occlusionTexture = material.GetTexture("_OcclusionMap") as Texture2D;
                                    if (occlusionTexture != null)
                                    {
                                        // Format texture import settings
                                        string occlusionTexturePath = AssetDatabase.GetAssetPath(occlusionTexture);
                                        var importTool = new BabylonTextureImporter(occlusionTexturePath);
                                        var importType = importTool.textureImporter.textureType;
                                        try
                                        {
                                            importTool.textureImporter.isReadable = true;
                                            importTool.textureImporter.textureType = TextureImporterType.Default;
                                            importTool.ForceUpdate();
                                            ambientTexture = occlusionTexture.Copy();
                                            if (ambientTexture.width != colorTexture.width || ambientTexture.height != colorTexture.height) {
                                                ambientTexture.Scale(colorTexture.width, colorTexture.height);
                                            }
                                        }
                                        catch(Exception ex)
                                        {
                                            UnityEngine.Debug.LogException(ex);
                                        }
                                        finally
                                        {
                                            // Restore texture importer type
                                            importTool.textureImporter.textureType = importType;
                                            importTool.ForceUpdate();
                                        }
                                    }
                                }
                                if (material.HasProperty("_MetallicGlossMap")) {
                                    Texture2D metalnessTexture = material.GetTexture("_MetallicGlossMap") as Texture2D;
                                    if (metalnessTexture != null)
                                    {
                                        // Format texture import settings
                                        metallicTextureSRGB = metalnessTexture.IsSRGB();
                                        string metalnessTexturePath = AssetDatabase.GetAssetPath(metalnessTexture);
                                        var importTool = new BabylonTextureImporter(metalnessTexturePath);
                                        var importType = importTool.textureImporter.textureType;
                                        try
                                        {
                                            importTool.textureImporter.isReadable = true;
                                            importTool.textureImporter.textureType = TextureImporterType.Default;
                                            importTool.ForceUpdate();
                                            metallicTexture = metalnessTexture.Copy();
                                            if (metallicTexture.width != colorTexture.width || metallicTexture.height != colorTexture.height) {
                                                metallicTexture.Scale(colorTexture.width, colorTexture.height);
                                            }
                                        }
                                        catch(Exception ex)
                                        {
                                            UnityEngine.Debug.LogException(ex);
                                        }
                                        finally
                                        {
                                            // Restore texture importer type
                                            importTool.textureImporter.textureType = importType;
                                            importTool.ForceUpdate();
                                        }
                                    }
                                }
                                if (material.HasProperty("_SpecGlossMap")) {
                                    Texture2D glossTexture = material.GetTexture("_SpecGlossMap") as Texture2D;
                                    if (glossTexture != null)
                                    {
                                        // Format texture import settings
                                        string specularTexturePath = AssetDatabase.GetAssetPath(glossTexture);
                                        var importTool = new BabylonTextureImporter(specularTexturePath);
                                        var importType = importTool.textureImporter.textureType;
                                        try
                                        {
                                            importTool.textureImporter.isReadable = true;
                                            importTool.textureImporter.textureType = TextureImporterType.Default;
                                            importTool.ForceUpdate();
                                            specularTexture = glossTexture.Copy();
                                            if (specularTexture.width != colorTexture.width || specularTexture.height != colorTexture.height) {
                                                specularTexture.Scale(colorTexture.width, colorTexture.height);
                                            }
                                        }
                                        catch(Exception ex)
                                        {
                                            UnityEngine.Debug.LogException(ex);
                                        }
                                        finally
                                        {
                                            // Restore texture importer type
                                            importTool.textureImporter.textureType = importType;
                                            importTool.ForceUpdate();
                                        }
                                    }
                                }
                            }
                        }
                        if (colorTexture == null) {
                            colorTexture = Tools.CreateBlankTextureMap(256, 256, material.color);
                            normalTexture = null;
                            ambientTexture = null;
                            emissionTexture = null;
                            metallicTexture = null;
                            specularTexture = null;
                        }
                        var mode = material.HasProperty("_Mode") ? (BlendMode)material.GetFloat("_Mode") : BlendMode.Opaque;
                        if (colorTexture != null && mode == BlendMode.Opaque) {
                            colorTexture.MakeAlpha(1.0f);
                        }
                        if (normalTexture == null) {
                            normalTexture = Tools.CreateBlankNormalMap(colorTexture.width, colorTexture.height);
                        }
                        if (emissionTexture == null) {
                            Color emissionColor = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : material.HasProperty("_Emission") ? material.GetColor("_Emission") : Color.black;
                            emissionTexture = Tools.CreateBlankTextureMap(colorTexture.width, colorTexture.height, emissionColor);
                        }
                        if (ambientTexture == null) {
                            Color ambientColor = material.HasProperty("_AmbientColor") ? material.GetColor("_AmbientColor") : Color.white;
                            ambientTexture = Tools.CreateBlankTextureMap(colorTexture.width, colorTexture.height, ambientColor);
                        }
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
                        if (metallicTexture == null) {
                            Color metallicColor = Color.white;
                            metallicTexture = Tools.CreateBlankTextureMap(colorTexture.width, colorTexture.height, metallicColor);
                            metallicTexture = Tools.EncodeMetallicTextureMap(metallicTexture, metalness, glossinessScale);
                        } else {
                            metallicTexture = Tools.CreateMetallicTextureMap(metallicTexture, glossinessScale, metallicTextureSRGB);
                        }
                        if (specularTexture == null) {
                            Color specularColor = material.HasProperty("_SpecColor") ? material.GetColor("_SpecColor") : Color.black;
                            specularColor.a *= glossiness;
                            specularTexture = Tools.CreateBlankTextureMap(colorTexture.width, colorTexture.height, specularColor);
                        } else {
                            specularTexture = Tools.CreateSpecularTextureMap(specularTexture, glossiness);
                        }
                        // Buffer baked material info
                        mainTextures.Add(colorTexture);
                        bumpTextures.Add(normalTexture);
                        ambientTextures.Add(ambientTexture);
                        emissionTextures.Add(emissionTexture);
                        metallicTextures.Add(metallicTexture);
                        specularTextures.Add(specularTexture);
                    }

                    // Encode atlas textures
                    bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                    Texture2D skinnedMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                    Rect[] atlasPackingResult = Tools.PackTextureAtlas(skinnedMeshAtlas, mainTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false);
                    Texture2D skinnedMeshBuffer = skinnedMeshAtlas.Copy();
                    skinnedMeshBuffer.alphaIsTransparency = true;
                    skinnedMeshBuffer.WriteImage(atlasFile, textureAtlasFormat);
                    AssetDatabase.ImportAsset(atlasFile, ImportAssetOptions.ForceUpdate);
                    
                    // Create atlas material(s)
                    if (textureAtlasShader == null) textureAtlasShader = Shader.Find("Standard");
                    atlasMaterial = new Material(textureAtlasShader);
                    atlasMaterial.name = materialName;
                    atlasMaterial.mainTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(atlasFile, typeof(Texture2D));
                    AssetDatabase.CreateAsset(atlasMaterial, materialFile);
                    if (bakeTextureMaps) {
                        bumpFilename = bumpFile;
                        ambientFilename = ambientFile;
                        emissionFilename = emissionFile;
                        metallicFilename = metallicFile;
                        specularFilename = specularFile;
                    }

                    // Texture atlas uv coordinates
                    Mesh mesh = skinMeshRenderer.sharedMesh;
                    int numSubs = mesh.subMeshCount;
                    int uvCount = mesh.uv.Length;
                    List<Vector2> uvList = new List<Vector2>();
                    if (atlasPackingResult != null && atlasPackingResult.Length > 0) {
                        for( int ctr = 0; ctr < numSubs; ctr++ ) {
                            Mesh sub = mesh.GetSubmesh(ctr);
                            Vector2[] uvs = Tools.GetTextureAtlasCoordinates(sub.uv, ctr, atlasPackingResult, linearInterpolation);
                            uvList.AddRange(uvs);
                        }
                        if (uvList.Count != uvCount) throw new Exception("Skin vertex count mismatch. Failed to convert uv coordinates.");
                    } else {
                        UnityEngine.Debug.LogError("Null atlas packing result rects");
                    }

                    // Create new mesh asset
                    Mesh newmesh = mesh.Copy();
                    if (uvList.Count > 0) {
                        newmesh.uv = uvList.ToArray();
                    }

                    // Save new mesh asset
                    string label = Tools.FirstUpper(materialName.Replace("_Material", ""));
                    string meshName = String.Format("{0}_{1}_Mesh", label, skinMeshRenderer.name.MakeSafe());
                    string meshFile = String.Format("{0}/Geometry/{1}.asset", filepath.TrimEnd('/'), meshName);
                    AssetDatabase.CreateAsset(newmesh, meshFile);
                    if (updateSkinRenderer) {
                        skinMeshRenderer.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshFile, typeof(Mesh));
                        skinMeshRenderer.sharedMaterials = new Material[] { (Material)AssetDatabase.LoadAssetAtPath(materialFile, typeof(Material)) };
                    }
                }
            }
            else
            {
                ExporterWindow.ShowMessage("At least 2 materials required for texture atlas skin");
            }
        }

        private void PackTextureAtlasNormals()
        {
            if (atlasMaterial != null && !String.IsNullOrEmpty(bumpFilename)) {
                ExporterWindow.ReportProgress(1, "Generating normal bump maps... This may take a while.");
                bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                Texture2D bumpMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                Tools.PackTextureAtlas(bumpMeshAtlas, bumpTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false);
                Texture2D bumpMeshBuffer = bumpMeshAtlas.Copy();
                bumpMeshBuffer.WriteImage(bumpFilename, textureAtlasFormat);
                AssetDatabase.ImportAsset(bumpFilename, ImportAssetOptions.ForceUpdate);
                // Import As Normal Map
                var importTool = new BabylonTextureImporter(bumpFilename);
                importTool.textureImporter.textureType = TextureImporterType.NormalMap;
                importTool.textureImporter.convertToNormalmap = false;
                importTool.ForceUpdate();
                atlasMaterial.SetTexture("_BumpMap", (Texture2D)AssetDatabase.LoadAssetAtPath(bumpFilename, typeof(Texture2D)));
            }
        }

        private void PackTextureAtlasAmbients()
        {
            if (atlasMaterial != null && !String.IsNullOrEmpty(ambientFilename)) {
                ExporterWindow.ReportProgress(1, "Generating ambient occlusion maps... This may take a while.");
                bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                Texture2D ambientMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                Tools.PackTextureAtlas(ambientMeshAtlas, ambientTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false);
                Texture2D ambientMeshBuffer = ambientMeshAtlas.Copy();
                ambientMeshBuffer.WriteImage(ambientFilename, textureAtlasFormat);
                AssetDatabase.ImportAsset(ambientFilename, ImportAssetOptions.ForceUpdate);
                // Import As Ambient Map
                var importTool = new BabylonTextureImporter(ambientFilename);
                importTool.textureImporter.textureType = TextureImporterType.Default;
                importTool.textureImporter.convertToNormalmap = false;
                importTool.ForceUpdate();
                atlasMaterial.SetTexture("_OcclusionMap", (Texture2D)AssetDatabase.LoadAssetAtPath(ambientFilename, typeof(Texture2D)));
            }
        }

        private void PackTextureAtlasEmissions()
        {
            if (atlasMaterial != null && !String.IsNullOrEmpty(emissionFilename)) {
                ExporterWindow.ReportProgress(1, "Generating emissive color maps... This may take a while.");
                bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                Texture2D emissionMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                Tools.PackTextureAtlas(emissionMeshAtlas, emissionTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false);
                Texture2D emissionMeshBuffer = emissionMeshAtlas.Copy();
                emissionMeshBuffer.WriteImage(emissionFilename, textureAtlasFormat);
                AssetDatabase.ImportAsset(emissionFilename, ImportAssetOptions.ForceUpdate);
                // Import As Emission Map
                var importTool = new BabylonTextureImporter(emissionFilename);
                importTool.textureImporter.textureType = TextureImporterType.Default;
                importTool.textureImporter.convertToNormalmap = false;
                importTool.ForceUpdate();
                atlasMaterial.SetTexture("_EmissionMap", (Texture2D)AssetDatabase.LoadAssetAtPath(emissionFilename, typeof(Texture2D)));
            }
        }

        private void PackTextureAtlasMetallics()
        {
            if (atlasMaterial != null && !String.IsNullOrEmpty(metallicFilename)) {
                ExporterWindow.ReportProgress(1, "Generating metallic gloss maps... This may take a while.");
                bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                Texture2D metallicMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                Tools.PackTextureAtlas(metallicMeshAtlas, metallicTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false);
                Texture2D metallicMeshBuffer = metallicMeshAtlas.Copy();
                metallicMeshBuffer.WriteImage(metallicFilename, textureAtlasFormat);
                AssetDatabase.ImportAsset(metallicFilename, ImportAssetOptions.ForceUpdate);
                // Import As Metallic Map
                var importTool = new BabylonTextureImporter(metallicFilename);
                importTool.textureImporter.textureType = TextureImporterType.Default;
                importTool.textureImporter.convertToNormalmap = false;
                importTool.ForceUpdate();
                atlasMaterial.SetFloat("_Gloss", 0.5f); // Default Texture Atlas Scaling
                atlasMaterial.SetFloat("_Glossiness", 0.5f); // Default Texture Atlas Scaling
                atlasMaterial.SetFloat("_GlossMapScale", 0.5f); // Default Texture Atlas Scaling
                atlasMaterial.SetTexture("_MetallicGlossMap", (Texture2D)AssetDatabase.LoadAssetAtPath(metallicFilename, typeof(Texture2D)));
            }
        }

        private void PackTextureAtlasSpeculars()
        {
            if (atlasMaterial != null && !String.IsNullOrEmpty(specularFilename)) {
                ExporterWindow.ReportProgress(1, "Generating specular gloss maps... This may take a while.");
                bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                Texture2D specularMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                Tools.PackTextureAtlas(specularMeshAtlas, specularTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false);
                Texture2D specularMeshBuffer = specularMeshAtlas.Copy();
                specularMeshBuffer.WriteImage(specularFilename, textureAtlasFormat);
                AssetDatabase.ImportAsset(specularFilename, ImportAssetOptions.ForceUpdate);
                // Import As Specular Map
                var importTool = new BabylonTextureImporter(specularFilename);
                importTool.textureImporter.textureType = TextureImporterType.Default;
                importTool.textureImporter.convertToNormalmap = false;
                importTool.ForceUpdate();
                atlasMaterial.SetFloat("_Gloss", 0.5f); // Default Texture Atlas Scaling
                atlasMaterial.SetFloat("_Glossiness", 0.5f); // Default Texture Atlas Scaling
                atlasMaterial.SetFloat("_GlossMapScale", 0.5f); // Default Texture Atlas Scaling
                atlasMaterial.SetTexture("_SpecGlossMap", (Texture2D)AssetDatabase.LoadAssetAtPath(specularFilename, typeof(Texture2D)));
            }
        }

        public void OnInspectorUpdate()
        {
            this.Repaint();
        }
    }
}

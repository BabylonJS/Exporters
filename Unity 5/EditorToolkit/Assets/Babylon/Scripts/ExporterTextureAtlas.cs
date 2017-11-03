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
        bool bakeTextureNormals = true;
        bool linearInterpolation = true;
        bool removeAlphaEncoding = true;
        bool updateSkinRenderer = true;

        // Texture Atlas Buffers        
        Material atlasMaterial = null;
        List<Texture2D> mainTextures = null;
        List<Texture2D> bumpTextures = null;
        string bumpFilename = String.Empty;
        bool hasBumpTexture = false;

        [MenuItem("BabylonJS/Texture Atlas Skin", false, 208)]
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
            textureAtlasShader = Shader.Find("BabylonJS/System/Standard Material");
            textureAtlasFormat = (BabylonImageFormat)ExporterWindow.exportationOptions.ImageEncodingOptions;
            if(skinMeshRenderer == null && Selection.activeObject is SkinnedMeshRenderer) {
                skinMeshRenderer = Selection.activeObject as SkinnedMeshRenderer;                
            }
        }

        public void OnGUI()
        {
            // Update texture atlas window
            EditorGUILayout.Space();
            skinMeshRenderer = EditorGUILayout.ObjectField("Skin Mesh Renderer:", skinMeshRenderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
            EditorGUILayout.Space();
            bakeTextureNormals = EditorGUILayout.Toggle("Bake Normal Maps:", bakeTextureNormals);
            EditorGUILayout.Space();
            removeAlphaEncoding = EditorGUILayout.Toggle("Clear Image Alpha:", removeAlphaEncoding);
            EditorGUILayout.Space();
            textureAtlasSize = (int)EditorGUILayout.Slider("Texture Atlas Size:", textureAtlasSize, 128, 8192);
            EditorGUILayout.Space();
            textureAtlasShader = EditorGUILayout.ObjectField("Texture Atlas Shader:", textureAtlasShader, typeof(Shader), true) as Shader;
            EditorGUILayout.Space();
            textureAtlasFormat = (BabylonImageFormat)EditorGUILayout.EnumPopup("Texture Atlas Format:", textureAtlasFormat, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            textureImageScaling = (BabylonTextureScale)EditorGUILayout.EnumPopup("Texture Image Scaling:", textureImageScaling, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            maxTextureImageSize = (int)EditorGUILayout.Slider("Texture Image Maximum:", maxTextureImageSize, 0, 4096);
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
                bumpFilename = String.Empty;
                hasBumpTexture = false;

                CreateTextureAtlas();
                PackTextureAtlasNormals();

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
                    bool jpeg = (textureAtlasFormat == BabylonImageFormat.JPEG);
                    string atlasExt = (jpeg) ? "jpg" : "png";
                    string atlasName = String.Format("{0}_Atlas", filelabel);
                    string atlasFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), atlasName, atlasExt);
                    string bumpName = String.Format("{0}_Normal", filelabel);
                    string bumpFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), bumpName, atlasExt);
                    string materialName = String.Format("{0}_Material", filelabel);
                    string materialFile = String.Format("{0}/Materials/{1}.asset", filepath.TrimEnd('/'), materialName);
                    
                    // Create atlas textures
                    mainTextures = new List<Texture2D>();
                    bumpTextures = new List<Texture2D>();
                    foreach (var material in skinMeshRenderer.sharedMaterials) {
                        Texture2D colorTexture = null;
                        Texture2D normalTexture = null;
                        if (material.mainTexture != null) {
                            Texture2D primaryTexture = material.mainTexture as Texture2D;
                            primaryTexture.ForceReadable();
                            colorTexture = primaryTexture.Copy();
                            if (bakeTextureNormals) {
                                if (material.HasProperty("_BumpMap")) {
                                    Texture2D bumpTexture = material.GetTexture("_BumpMap") as Texture2D;
                                    if (bumpTexture != null && bumpTexture.width == colorTexture.width && bumpTexture.height == colorTexture.height)
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
                                            hasBumpTexture = true;
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
                            colorTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                            colorTexture.Clear(material.color);
                            normalTexture = null;
                        }
                        if (normalTexture == null) {
                            normalTexture = Tools.CreateBlankNormalMap(colorTexture.width, colorTexture.height);
                        }
                        // Buffer baked material info
                        mainTextures.Add(colorTexture);
                        bumpTextures.Add(normalTexture);
                    }

                    // Encode atlas textures
                    bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                    Texture2D skinnedMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                    Rect[] atlasPackingResult = Tools.PackTextureAtlas(skinnedMeshAtlas, mainTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false, removeAlphaEncoding);
                    Texture2D skinnedMeshBuffer = skinnedMeshAtlas.Copy();
                    skinnedMeshBuffer.WriteImage(atlasFile, textureAtlasFormat);
                    AssetDatabase.ImportAsset(atlasFile, ImportAssetOptions.ForceUpdate);
                    
                    // Create atlas material
                    if (textureAtlasShader == null) textureAtlasShader = Shader.Find("BabylonJS/System/Standard Material");
                    atlasMaterial = new Material(textureAtlasShader);
                    atlasMaterial.name = materialName;
                    atlasMaterial.mainTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(atlasFile, typeof(Texture2D));
                    AssetDatabase.CreateAsset(atlasMaterial, materialFile);
                    if (bakeTextureNormals && hasBumpTexture) {
                        bumpFilename = bumpFile;
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
                    string meshName = String.Format("{0}_{1}_Mesh", label, skinMeshRenderer.name);
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
                ExporterWindow.ReportProgress(1, "Generating normal map atlas... This may take a while.");
                //bool jpeg = (textureAtlasFormat == BabylonImageFormat.JPEG);
                bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                Texture2D bumpMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                Tools.PackTextureAtlas(bumpMeshAtlas, bumpTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false, removeAlphaEncoding);
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

        public void OnInspectorUpdate()
        {
            this.Repaint();
        }
    }
}

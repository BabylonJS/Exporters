using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using FreeImageAPI;

namespace Unity3D2Babylon
{
    public class ExporterCubemaps : EditorWindow
    {
        Cubemap convertCube;
        BabylonImageFormat imageFormat = BabylonImageFormat.PNG;
        BabylonCubemapTool cubemapTool = BabylonCubemapTool.CubemapSplitter;
        BabylonImageLibrary imageLibrary = BabylonImageLibrary.UnityImageLibrary;
        BabylonProbeFormat reflectionType = BabylonProbeFormat.Probe128;
        bool createSkyboxMaterial = true;
        bool keepGeneratorOpen = true;

        string splitLabel = "Bake Cubemap Faces";

        [MenuItem("BabylonJS/Cubemap Baker", false, 204)]
        public static void InitConverter()
        {
            ExporterCubemaps splitter = ScriptableObject.CreateInstance<ExporterCubemaps>();
            splitter.OnInitialize();
            splitter.ShowUtility();
        }

        public void OnInitialize()
        {
            maxSize = new Vector2(500, 248);
            minSize = this.maxSize;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Bake Cubemap Textures");
            imageFormat = (BabylonImageFormat)ExporterWindow.exportationOptions.ImageEncodingOptions;
            if(convertCube == null && Selection.activeObject is Cubemap) {
                convertCube = Selection.activeObject as Cubemap;                
            }
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();
            cubemapTool = (BabylonCubemapTool)EditorGUILayout.EnumPopup("Cubemap Texture Tool:", cubemapTool, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            if (cubemapTool == BabylonCubemapTool.PixelPerfectTools) {
                splitLabel = "Open Pixel Perfect";
                if (this.maxSize.y != 94.0f) {
                    this.maxSize = new Vector2(500.0f, 94.0f);
                    this.minSize = this.maxSize;
                }
            } else if (cubemapTool == BabylonCubemapTool.ReflectionProbes) {
                splitLabel = "Bake Reflection Probe";
                if (this.maxSize.y != 222.0f) {
                    this.maxSize = new Vector2(500.0f, 222.0f);
                    this.minSize = this.maxSize;
                }
            } else {
                splitLabel = "Bake Cubemap Faces";
                if (this.maxSize.y != 248.0f) {
                    this.maxSize = new Vector2(500.0f, 248.0f);
                    this.minSize = this.maxSize;
                }
            }
            if (cubemapTool == BabylonCubemapTool.CubemapSplitter || cubemapTool == BabylonCubemapTool.ReflectionProbes) {
                imageLibrary = (BabylonImageLibrary)EditorGUILayout.EnumPopup("Default Image Library:", imageLibrary, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                convertCube = EditorGUILayout.ObjectField("Source Cubemap Image:", convertCube, typeof(Cubemap), false) as Cubemap;
                EditorGUILayout.Space();
            }
            if (cubemapTool == BabylonCubemapTool.CubemapSplitter) {
                imageFormat = (BabylonImageFormat)EditorGUILayout.EnumPopup("Output Image Format:", imageFormat, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
            }
            if (cubemapTool == BabylonCubemapTool.ReflectionProbes) {
                reflectionType = (BabylonProbeFormat)EditorGUILayout.EnumPopup("Reflection Probe Size:", reflectionType, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
            }
            if (cubemapTool == BabylonCubemapTool.CubemapSplitter) {
                createSkyboxMaterial = EditorGUILayout.Toggle("Create Skybox Material:", createSkyboxMaterial);
                EditorGUILayout.Space();
            }
            keepGeneratorOpen = EditorGUILayout.Toggle("Keep Generator Open:", keepGeneratorOpen);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button(splitLabel)) {
                if (cubemapTool == BabylonCubemapTool.PixelPerfectTools) {
                    bool jpeg = (imageFormat == BabylonImageFormat.JPEG);
                    string splitter = (ExporterWindow.PixelPrefect + "?jpeg=" + jpeg.ToString().ToLower());
                    Application.OpenURL(splitter);
                    this.Close();
                } else {
                    if (convertCube) {
                        Bake();
                    }
                    if (!convertCube) {
                        ExporterWindow.ShowMessage("You must select a cubemap");
                    }
                }
            }
        }

        public void Bake()
        {
            // Validate Project Platform
            if (!Unity3D2Babylon.Tools.ValidateProjectPlatform()) return;

            try
            {
                string inputFile = AssetDatabase.GetAssetPath(convertCube);
                string inputExt = Path.GetExtension(inputFile);
                if (cubemapTool == BabylonCubemapTool.ReflectionProbes) {
                    if (inputExt.Equals(".hdr", StringComparison.OrdinalIgnoreCase) || inputExt.Equals(".exr", StringComparison.OrdinalIgnoreCase)) {
                        ExporterWindow.ReportProgress(1, "Baking cubemap reflection probe... This may take a while.");
                        string outputFile = inputFile.Replace(inputExt, "Probe.hdr");
                        int reflectionResolution = (int)reflectionType;
                        FREE_IMAGE_FORMAT srcType = FREE_IMAGE_FORMAT.FIF_HDR;
                        if (inputExt.Equals(".hdr", StringComparison.OrdinalIgnoreCase)) {
                            srcType = FREE_IMAGE_FORMAT.FIF_HDR;
                        } else if (inputExt.Equals(".exr", StringComparison.OrdinalIgnoreCase)) {
                            srcType = FREE_IMAGE_FORMAT.FIF_EXR;
                        }
                        FREE_IMAGE_FILTER rescaleFilter = FREE_IMAGE_FILTER.FILTER_LANCZOS3;
                        int rescaleWidth = reflectionResolution * 4;
                        int rescaleHeight = rescaleWidth / 2;
                        FileStream destStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
                        FileStream sourceStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
                        try
                        {
                            Tools.ConvertFreeImage(sourceStream, srcType, destStream, FREE_IMAGE_FORMAT.FIF_HDR, FREE_IMAGE_TYPE.FIT_UNKNOWN, true, FREE_IMAGE_COLOR_DEPTH.FICD_AUTO, FREE_IMAGE_LOAD_FLAGS.DEFAULT, FREE_IMAGE_SAVE_FLAGS.DEFAULT, 0.0, false, false, rescaleWidth, rescaleHeight, rescaleFilter);
                        } catch (Exception ex) {
                            UnityEngine.Debug.LogException(ex);
                        } finally {
                            destStream.Close();
                            sourceStream.Close();
                        }
                        if (System.IO.File.Exists(outputFile)) {
                            AssetDatabase.ImportAsset(outputFile, ImportAssetOptions.ForceUpdate);
                            var importTool = new BabylonTextureImporter(outputFile);
                            importTool.textureImporter.textureShape = TextureImporterShape.TextureCube;
                            importTool.textureImporter.isReadable = true;
                            importTool.ForceUpdate();
                        }
                    } else {
                        ExporterWindow.ShowMessage("You must select a high dynamic range cubemap");
                    }
                } else if (cubemapTool == BabylonCubemapTool.CubemapSplitter) {
                    ExporterWindow.ReportProgress(1, "Baking cubemap texture faces... This may take a while.");
                    bool jpeg = (imageFormat == BabylonImageFormat.JPEG);
                    string faceExt = (jpeg) ? ".jpg" : ".png";
                    var splitterOpts = new BabylonSplitterOptions();
                    var outputFile = inputFile.Replace(inputExt, faceExt);
                    Tools.ExportCubemap(convertCube, outputFile, imageFormat, splitterOpts);
                    if (createSkyboxMaterial == true) {
                        ExporterWindow.ReportProgress(1, "Generating skybox material assets... This may take a while.");
                        AssetDatabase.Refresh();
                        Material skyboxMaterial = new Material(Shader.Find("Mobile/Skybox"));
                        if (skyboxMaterial != null) {
                            string frontFilename = outputFile.Replace(faceExt, ("_pz" + faceExt));
                            AssetDatabase.ImportAsset(frontFilename, ImportAssetOptions.ForceUpdate);
                            Texture2D frontTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(frontFilename, typeof(Texture2D));
                            if (frontTexture != null) skyboxMaterial.SetTexture("_FrontTex", frontTexture);

                            string backFilename = outputFile.Replace(faceExt, ("_nz" + faceExt));
                            AssetDatabase.ImportAsset(backFilename, ImportAssetOptions.ForceUpdate);
                            Texture2D backTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(backFilename, typeof(Texture2D));
                            if (backTexture != null) skyboxMaterial.SetTexture("_BackTex", backTexture);

                            string leftFilename = outputFile.Replace(faceExt, ("_px" + faceExt));
                            AssetDatabase.ImportAsset(leftFilename, ImportAssetOptions.ForceUpdate);
                            Texture2D leftTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(leftFilename, typeof(Texture2D));
                            if (leftTexture != null) skyboxMaterial.SetTexture("_LeftTex", leftTexture);

                            string rightFilename = outputFile.Replace(faceExt, ("_nx" + faceExt));
                            AssetDatabase.ImportAsset(rightFilename, ImportAssetOptions.ForceUpdate);
                            Texture2D rightTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(rightFilename, typeof(Texture2D));
                            if (rightTexture != null) skyboxMaterial.SetTexture("_RightTex", rightTexture);

                            string upFilename = outputFile.Replace(faceExt, ("_py" + faceExt));
                            AssetDatabase.ImportAsset(upFilename, ImportAssetOptions.ForceUpdate);
                            Texture2D upTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(upFilename, typeof(Texture2D));
                            if (upTexture != null) skyboxMaterial.SetTexture("_UpTex", upTexture);

                            string downFilename = outputFile.Replace(faceExt, ("_ny" + faceExt));
                            AssetDatabase.ImportAsset(downFilename, ImportAssetOptions.ForceUpdate);
                            Texture2D downTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(downFilename, typeof(Texture2D));
                            if (downTexture != null) skyboxMaterial.SetTexture("_DownTex", downTexture);
                            
                            string outputMaterialName = Path.GetFileNameWithoutExtension(inputFile);
                            string outputMaterialPath = Path.GetDirectoryName(inputFile);
                            string outputMaterialFile =  outputMaterialPath + "/" + outputMaterialName + ".mat";
                            AssetDatabase.CreateAsset(skyboxMaterial, outputMaterialFile);
                        } else {
                            throw new Exception("Failed to create 'Mobile/Skybox' material");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            finally
            {
                ExporterWindow.ReportProgress(1, "Refresing assets database...");
                AssetDatabase.Refresh();
            }
            ExporterWindow.ReportProgress(1, "Cubemap conversion complete.");
            EditorUtility.ClearProgressBar();
            if (this.keepGeneratorOpen) {
                ExporterWindow.ShowMessage("Cubemap optimzation complete.", "Babylon.js");
            } else {
                this.Close();
            }
        }

        public void OnInspectorUpdate()
        {
            this.Repaint();
        }
    }
}
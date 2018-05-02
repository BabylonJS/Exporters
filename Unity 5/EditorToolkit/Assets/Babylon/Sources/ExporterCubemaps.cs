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
        bool generateSkybox = true;
        BabylonSkyboxOption skyboxOption = BabylonSkyboxOption.LowDynamicRange;
        BabylonImageFormat skyboxSplitter = BabylonImageFormat.PNG;
        BabylonFilterFormat skyboxConverter = BabylonFilterFormat.DDS;
        bool generateRadiance = false;
        BabylonReflectionProbe radianceSize = BabylonReflectionProbe._256;
        BabylonCubemapFormat filterOutput = BabylonCubemapFormat.RGBA16F;
        BabylonCubemapLight filterLighting = BabylonCubemapLight.BLINNBRDF;
        int glossScale = 10;
        int gloassBias = 3;
        bool excludeBase = true;
        int numberOfCpus = 4;
        float inputGammaNumerator = 2.2f;
        float inputGammaDenominator = 1.0f;
        float outputGammaNumerator = 1.0f;
        float outputGammaDenominator = 2.2f;
        bool createSkyboxMaterial = true;
        bool keepGeneratorOpen = true;
        string splitLabel = "Bake Cubemap Faces";

        [MenuItem("Babylon/Cubemap Baking", false, 207)]
        public static void InitConverter()
        {
            ExporterCubemaps splitter = ScriptableObject.CreateInstance<ExporterCubemaps>();
            splitter.OnInitialize();
            splitter.ShowUtility();
        }

        public void OnInitialize()
        {
            maxSize = new Vector2(500, 602);
            minSize = this.maxSize;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Cubemap Filter Tool");
            skyboxSplitter = (BabylonImageFormat)ExporterWindow.exportationOptions.ImageEncodingOptions;
            if(convertCube == null && Selection.activeObject is Cubemap) {
                convertCube = Selection.activeObject as Cubemap;                
            }
        }

        public void OnGUI()
        {
            splitLabel = "Bake Cubemap Textures";
            if (this.maxSize.y != 602f) {
                this.maxSize = new Vector2(500.0f, 602.0f);
                this.minSize = this.maxSize;
            }
            EditorGUILayout.Space();
            convertCube = EditorGUILayout.ObjectField("Source Cubemap Image:", convertCube, typeof(Cubemap), false) as Cubemap;
            EditorGUILayout.Space();
            generateSkybox = EditorGUILayout.Toggle("Create Skybox:", generateSkybox);
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(generateSkybox == false);
                skyboxOption = (BabylonSkyboxOption)EditorGUILayout.EnumPopup("Skybox Option:", skyboxOption, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                if (skyboxOption == BabylonSkyboxOption.LowDynamicRange) {
                    skyboxSplitter = (BabylonImageFormat)EditorGUILayout.EnumPopup("Skybox Format:", skyboxSplitter, GUILayout.ExpandWidth(true));
                    EditorGUILayout.Space();
                } else if (skyboxOption == BabylonSkyboxOption.HighDynamicRange) {
                    skyboxConverter = (BabylonFilterFormat)EditorGUILayout.EnumPopup("Skybox Format:", skyboxConverter, GUILayout.ExpandWidth(true));
                    EditorGUILayout.Space();
                }
            EditorGUI.EndDisabledGroup();
            generateRadiance = EditorGUILayout.Toggle("Generate Radiance:", generateRadiance);
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(generateRadiance == false);
                radianceSize = (BabylonReflectionProbe)EditorGUILayout.EnumPopup("Radiance Size:", radianceSize, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                excludeBase = EditorGUILayout.Toggle("Exclude Base:", excludeBase);
                EditorGUILayout.Space();
                glossScale = (int)EditorGUILayout.Slider("Gloss Scale", glossScale, 0, 20);
                EditorGUILayout.Space();
                gloassBias = (int)EditorGUILayout.Slider("Gloss Bias", gloassBias, 0, 10);
                EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.LabelField(new GUIContent("Filtering Options"), EditorStyles.boldLabel);
            EditorGUILayout.Space();
            filterOutput = (BabylonCubemapFormat)EditorGUILayout.EnumPopup("Filter Output:", filterOutput, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            filterLighting = (BabylonCubemapLight)EditorGUILayout.EnumPopup("Filter Lighting:", filterLighting, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Processing Options"), EditorStyles.boldLabel);
            EditorGUILayout.Space();
            inputGammaNumerator = (float)EditorGUILayout.Slider("Input Numerator", inputGammaNumerator, 0.0f, 10.0f);
            EditorGUILayout.Space();
            inputGammaDenominator = (float)EditorGUILayout.Slider("Input Denominator", inputGammaDenominator, 0.0f, 10.0f);
            EditorGUILayout.Space();
            outputGammaNumerator = (float)EditorGUILayout.Slider("Output Numerator", outputGammaNumerator, 0.0f, 10.0f);
            EditorGUILayout.Space();
            outputGammaDenominator = (float)EditorGUILayout.Slider("Output Denominator", outputGammaDenominator, 0.0f, 10.0f);
            EditorGUILayout.Space();
            numberOfCpus = (int)EditorGUILayout.Slider("Number Of Processors", numberOfCpus, 1, 32);
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(generateSkybox == false);
                createSkyboxMaterial = EditorGUILayout.Toggle("Create Skybox Material:", createSkyboxMaterial);
                EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();
            keepGeneratorOpen = EditorGUILayout.Toggle("Keep Generator Open:", keepGeneratorOpen);
            EditorGUILayout.Space();
            if (GUILayout.Button(splitLabel)) {
                if (convertCube) {
                    Bake();
                }
                if (!convertCube) {
                    ExporterWindow.ShowMessage("You must select a cubemap");
                }
            }
        }

        public void Bake()
        {
            // Validate Project Platform
            if (generateSkybox == false && generateRadiance == false) {
                ExporterWindow.ShowMessage("You must select generate skybox and/or radiance");
                return;
            }
            if (!Unity3D2Babylon.Tools.ValidateProjectPlatform()) return;
            try
            {
                string inputFile = AssetDatabase.GetAssetPath(convertCube);
                string inputExt = Path.GetExtension(inputFile);
                if (skyboxOption == BabylonSkyboxOption.LowDynamicRange) {
                    Split(inputFile, inputExt, true);
                    Filter(inputFile, inputExt, false, true);
                } else if (skyboxOption == BabylonSkyboxOption.HighDynamicRange) {
                    Filter(inputFile, inputExt, true, true);
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

        public void Split(string inputFile, string inputExt, bool createSkybox)
        {
            bool jpeg = (skyboxSplitter == BabylonImageFormat.JPEG);
            string faceExt = (jpeg) ? ".jpg" : ".png";
            var splitterOpts = new BabylonSplitterOptions();
            var outputFile = inputFile.Replace(inputExt, faceExt);
            if (createSkybox == true && generateSkybox == true) {
                ExporterWindow.ReportProgress(1, "Splitting cubemap texture faces... This may take a while.");
                Tools.ExportSkybox(convertCube, outputFile, splitterOpts, skyboxSplitter);
            }
            if (createSkybox == true && generateSkybox == true && createSkyboxMaterial == true) {
                ExporterWindow.ReportProgress(1, "Generating skybox material assets... This may take a while.");
                AssetDatabase.Refresh();
                Material skyboxMaterial = new Material(Shader.Find("Skybox/6 Sided"));
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
                    
                    string outputMaterialName = Path.GetFileNameWithoutExtension(outputFile);
                    string outputMaterialPath = Path.GetDirectoryName(outputFile);
                    string outputMaterialFile =  outputMaterialPath + "/" + outputMaterialName + ".mat";
                    AssetDatabase.CreateAsset(skyboxMaterial, outputMaterialFile);
                } else {
                    UnityEngine.Debug.LogError("CMFT: Failed to locate 'Skybox/6 Sided' shader material");
                }
            }
        }

        public void Filter(string inputFile, string inputExt, bool createSkybox, bool createRadiance)
        {
            string faceExt = ".dds";
            int cubeSize = convertCube.height;
            string outputFile = inputFile.Replace(inputExt, faceExt);
            string outputSkybox = outputFile.Replace(".dds", "_sky.dds");
            string outputRadiance = outputFile.Replace(".dds", "_env.dds");
            if (inputExt.Equals(".hdr", StringComparison.OrdinalIgnoreCase) || inputExt.Equals(".exr", StringComparison.OrdinalIgnoreCase)) {
                string hdrTexturePath = null;                 
                if (inputExt.Equals(".exr", StringComparison.OrdinalIgnoreCase)) {
                    ExporterWindow.ReportProgress(1, "Parsing source cubemap texture... This may take a while.");
                    hdrTexturePath = (Path.GetTempFileName() + ".hdr");
                    string srcTexturePath = Tools.GetNativePath(inputFile);
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
                            tempTexture.WriteImageHDR(hdrTexturePath);
                        } else {
                            UnityEngine.Debug.LogError("Failed to convert exr/hdr file");
                        }
                    } catch (Exception ex) {
                        UnityEngine.Debug.LogException(ex);
                    } finally {
                        sourceStream.Close();
                    }
                    inputFile = hdrTexturePath;
                    inputExt = Path.GetExtension(inputFile);
                }
                if (createSkybox == true && generateSkybox == true) {
                    ExporterWindow.ReportProgress(1, "Baking skybox cubemap texture... This may take a while.");
                    Tools.ConvertCubemap(inputFile, outputSkybox, cubeSize, filterOutput, filterLighting, false, glossScale, gloassBias, excludeBase, numberOfCpus, inputGammaNumerator, inputGammaDenominator, outputGammaNumerator, outputGammaDenominator);
                    AssetDatabase.ImportAsset(outputSkybox, ImportAssetOptions.ForceUpdate);
                }
                if (createRadiance == true && generateRadiance == true) {
                    ExporterWindow.ReportProgress(1, "Baking radiance cubemap texture... This may take a while.");
                    Tools.ConvertCubemap(inputFile, outputRadiance, (int)radianceSize, filterOutput, filterLighting, true, glossScale, gloassBias, excludeBase, numberOfCpus, inputGammaNumerator, inputGammaDenominator, outputGammaNumerator, outputGammaDenominator);
                    AssetDatabase.ImportAsset(outputRadiance, ImportAssetOptions.ForceUpdate);
                }
                if (!String.IsNullOrEmpty(hdrTexturePath) && File.Exists(hdrTexturePath)) {
                    try{ File.Delete(hdrTexturePath); } catch{}
                }
                if (createSkybox == true && generateSkybox == true && createSkyboxMaterial == true) {
                    ExporterWindow.ReportProgress(1, "Generating skybox material assets... This may take a while.");
                    AssetDatabase.Refresh();
                    Material skyboxMaterial = new Material(Shader.Find("Skybox/Cubemap"));
                    if (skyboxMaterial != null) {
                        Cubemap ddsTexture = (Cubemap)AssetDatabase.LoadAssetAtPath(outputSkybox, typeof(Cubemap));
                        if (ddsTexture != null) skyboxMaterial.SetTexture("_Tex", ddsTexture);

                        string outputMaterialName = Path.GetFileNameWithoutExtension(outputFile);
                        string outputMaterialPath = Path.GetDirectoryName(outputFile);
                        string outputMaterialFile =  outputMaterialPath + "/" + outputMaterialName + ".mat";
                        AssetDatabase.CreateAsset(skyboxMaterial, outputMaterialFile);
                    } else {
                        UnityEngine.Debug.LogError("CMFT: Failed to locate 'Skybox/Cubemap' shader material");
                    }
                }
            } else {
                UnityEngine.Debug.LogError("CMFT: Unsupported cubemap file type " + inputExt);
            }
        }

        public void OnInspectorUpdate()
        {
            this.Repaint();
        }
    }
}
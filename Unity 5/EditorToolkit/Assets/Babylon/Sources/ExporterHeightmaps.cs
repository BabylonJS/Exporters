using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using Unity3D2Babylon;
using FreeImageAPI;

namespace Unity3D2Babylon
{
    public class ExporterHeightmaps : EditorWindow
    {
        private static string[] IMAGE_FORMATS = new string[] { "Heightmap Images", "raw,r16,exr,png,tif,tiff,jpg,jpeg", "All Files", "*" };
        string heightmapFile = String.Empty;
        string heightmapLabel = String.Empty;
        int heightmapResolution = 0;
        Texture2D heightmapTexture = null;
        //////////////////////////////////
        bool enableResolution = false;
        int exportResolution = 0;
        BabylonHeightmapFormat exportFormat = BabylonHeightmapFormat.RAW;
        BabylonTextureScale exportScaling = BabylonTextureScale.Bilinear;
        //////////////////////////////////
        // Free Image Library           //
        //////////////////////////////////
        bool freeImageAvailable = false;
        //////////////////////////////////
        bool keepGeneratorOpen = true;

        [MenuItem("Babylon/Height Mapping", false, 206)]
        public static void InitConverter()
        {
            ExporterHeightmaps mapper = ScriptableObject.CreateInstance<ExporterHeightmaps>();
            mapper.OnInitialize();
            mapper.ShowUtility();
        }

        public void OnInitialize()
        {
            maxSize = new Vector2(520.0f, 714.0f);
            minSize = this.maxSize;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Height Mapping");
            freeImageAvailable = Tools.IsFreeImageAvailable();
            if (!freeImageAvailable) UnityEngine.Debug.LogWarning("Free Image Library Not Available");
        }

        public void OnGUI()
        {
            GUILayout.Label("Heightmap File", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            heightmapLabel = EditorGUILayout.TextField(String.Empty, heightmapLabel);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Load Heightmap Data")) {
                LoadHeightmap();
            }
            EditorGUILayout.Space();
            exportFormat = (BabylonHeightmapFormat)EditorGUILayout.EnumPopup("Export Format:", exportFormat, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            // ..
            EditorGUILayout.BeginHorizontal();
            enableResolution = EditorGUILayout.Toggle("Scale Resolution:", enableResolution);
            if (enableResolution == false) exportResolution = heightmapResolution;
            EditorGUI.BeginDisabledGroup(enableResolution == false);
            exportResolution = (int)EditorGUILayout.Slider("", exportResolution, 0, heightmapResolution);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            exportScaling = (BabylonTextureScale)EditorGUILayout.EnumPopup("Map Image Scaling:", (BabylonTextureScale)exportScaling, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();
            // ..
            EditorGUILayout.BeginHorizontal();
            keepGeneratorOpen = EditorGUILayout.Toggle("Keep Generator Open:", keepGeneratorOpen);
            int vertexCount = (exportResolution * exportResolution);
            string vertextLabel = String.Format("Estimated Mesh Density - {0}", (vertexCount > 0) ? vertexCount.ToString("#,#") : "0");
            GUILayout.Label(vertextLabel, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            // ..
            if (heightmapTexture == null)
            {
                heightmapTexture = new Texture2D(500, 500, TextureFormat.RGBA32, false);
                heightmapTexture.Clear(Color.black);
            }
            if (heightmapTexture != null)
            {
    			GUI.DrawTexture(new Rect(10.0f, 180.0f, 500.0f, 500.0f), heightmapTexture, ScaleMode.ScaleToFit);
            }
            GUILayout.Space(510.0f);
            if (GUILayout.Button("Export Heightmap"))
            {
                if (heightmapTexture != null && !String.IsNullOrEmpty(heightmapFile)) {
                    ExportHeightmap();
                } else {
                    ExporterWindow.ShowMessage("No heightmap data generated.");
                }
            }
        }

        public void LoadHeightmap()
        {
            // Validate Project Platform
            if (!Unity3D2Babylon.Tools.ValidateProjectPlatform()) return;
            string filename = EditorUtility.OpenFilePanelWithFilters("Select Heightmap Image", String.Empty, IMAGE_FORMATS);
            if (String.IsNullOrEmpty(filename)) return;
            // ..
            ResetHeightmap();
            heightmapFile = Tools.GetNativePath(filename);
            heightmapLabel = Path.GetFileName(heightmapFile);
            // ..
            ExporterWindow.ReportProgress(1, "Loading heightmap image data... This may take a while.");
            string heightmapExt = Path.GetExtension(heightmapFile);
            bool heightmapRaw = (heightmapExt.Equals(".raw", StringComparison.OrdinalIgnoreCase) || heightmapExt.Equals(".r16", StringComparison.OrdinalIgnoreCase));
            try {
                try {
                    bool readResult = false;
                    int readWidth = 0;
                    int readHeight = 0;
                    int readBitsPerPixel = 0;
                    Color[] pixels = Tools.ReadRawHeightmapImage(heightmapFile, heightmapRaw, ref readResult, ref readWidth, ref readHeight, ref readBitsPerPixel);
                    if (readResult == true && pixels != null) {
                        int resolution = (int)(Math.Sqrt(pixels.Length));
                        exportResolution = resolution;
                        heightmapResolution = resolution;
                        // ..
                        Texture2D workTexture = new Texture2D(resolution, resolution, TextureFormat.RGBAHalf, false);
                        workTexture.SetPixels(pixels);
                        workTexture.Apply();
                        workTexture.MakeGrayscale();
                        // ..
                        if (heightmapRaw) workTexture = Tools.FlipTexture(workTexture);
                        heightmapTexture = workTexture;
                    }
                } catch(Exception ex) {
                    UnityEngine.Debug.LogException(ex);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            finally
            {
                ExporterWindow.ReportProgress(1, "Heightmap conversion complete.");
                EditorUtility.ClearProgressBar();
            }
        }

        public void ExportHeightmap()
        {
            // Validate Project Platform
            if (!Unity3D2Babylon.Tools.ValidateProjectPlatform()) return;
            if (heightmapTexture == null || String.IsNullOrEmpty(heightmapFile)) return;
            // ..
            bool exportRaw = (exportFormat == BabylonHeightmapFormat.RAW);
            string exportExt = (exportRaw) ? "raw" : "png";
            string exportDir = Path.GetDirectoryName(heightmapFile);
            string exportFile = Path.GetFileNameWithoutExtension(heightmapFile);
            // ..
            string filename = EditorUtility.SaveFilePanel("Export Heightmap Image", exportDir, exportFile, exportExt);
            if (String.IsNullOrEmpty(filename)) return;
            if (File.Exists(filename)) {
                if (!ExporterWindow.ShowMessage("Overwrite the selected file?", "Babylon.js", "Overwrite", "Cancel")) return;
            }
            // ..
            try {
                ExporterWindow.ReportProgress(1, "Baking heightmap image data... This may take a while.");
                Texture2D exportTexture = heightmapTexture.Copy(heightmapTexture.format);
                if (enableResolution == true && exportResolution != heightmapResolution) {
                    int saveResolution = exportResolution;
                    if (saveResolution <= 0) saveResolution = 1;
                    exportTexture.Scale(saveResolution, saveResolution, (exportScaling == BabylonTextureScale.Bilinear));
                }
                if (exportRaw) exportTexture = Tools.FlipTexture(exportTexture);
                if (exportRaw) exportTexture.WriteHeightmapRAW(filename);
                else exportTexture.WriteImagePNG16(filename, true);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            finally
            {
                ExporterWindow.ReportProgress(1, "Refresing assets database...");
                AssetDatabase.Refresh();
                ExporterWindow.ReportProgress(1, "Heightmap conversion complete.");
                EditorUtility.ClearProgressBar();
            }
            if (this.keepGeneratorOpen) {
                ExporterWindow.ShowMessage("Heightmap exportation complete.", "Babylon.js");
            } else {
                this.Close();
            }
        }

        public void ResetHeightmap()
        {
            heightmapTexture = null;
            heightmapResolution = 0;
            exportResolution = 0;
        }

        public void OnInspectorUpdate()
        {
            this.Repaint();
        }
    }
}

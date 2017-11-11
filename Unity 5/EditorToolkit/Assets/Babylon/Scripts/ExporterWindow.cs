using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.SceneManagement;
using BabylonHosting;

namespace Unity3D2Babylon
{
    [ExecuteInEditMode]
    public class ExporterWindow : EditorWindow
    {
        public const double BabylonVersion = 3.1;
        public const string ToolkitVersion = "3.1.044 (Alpha)";
        public const string DefaultHost = "localhost";
        public const string StaticLabel = "Babylon Static";
        public const string PrefabLabel = "Babylon Prefab";
        public const string PixelPrefect = "http://mackey.cloud/tools/splitter/index.html";
        public const int StaticIndex = 30;
        public const int PrefabIndex = 31;
        public const int MaxVerticies = 65535;
        public const int DefaultPort = 8888;
        public const float DefaultAmbientScale = 1.0f;
        public const float DefaultReflectScale = 1.0f;
        public const float DefaultIntensityScale = 1.0f;
        public static Vector3 DefaultRotationOffset = new Vector3(0,0,0);
        public static ExportationOptions exportationOptions = null;
        public static readonly List<string> logs = new List<string>();
        int adaptToDeviceRatio = 0;
        Vector2 scrollPosMain;
        bool showEngine = false;
        bool showTerrain = false;
        bool showLighting = false;
        bool showCollision = false;
        bool showPreview = false;
        bool showCompiler = false;
        int buildResult = 0;
        string defaultProjectFolder = String.Empty;
        string guiProjectFolder = String.Empty;
        public bool previewThread = false;

        public static void ReportProgress(float value, string message = "")
        {
            EditorUtility.DisplayProgressBar("Babylon.js", message, value);

            if (!string.IsNullOrEmpty(message))
            {
                logs.Add(message);
            }
        }

        public static bool ShowMessage(string message, string title = "Babylon.js", string ok = "OK", string cancel = "")
        {
            return EditorUtility.DisplayDialog(title, message, ok, cancel);
        }

        public ExportationOptions CreateSettings()
        {
            ExportationOptions result = new ExportationOptions();
            string ufile = Path.Combine(Application.dataPath, "Babylon/Template/Config/settings.json");
            if (File.Exists(ufile))
            {
                string json = File.ReadAllText(ufile);
                result = Tools.FromJson<ExportationOptions>(json);
            }
            return result;
        }

        public void SaveSettings(bool refresh = false)
        {
            if (refresh) GetSceneInfomation(false);
            string ufile = Path.Combine(Application.dataPath, "Babylon/Template/Config/settings.json");
            if (File.Exists(ufile))
            {
                string json = Tools.ToJson(exportationOptions, true);
                File.WriteAllText(ufile, json);
            }
        }

        [MenuItem("BabylonJS/Scene Exporter", false, 0)]
        public static void InitExporter()
        {
            var exporter = (ExporterWindow)GetWindow(typeof(ExporterWindow));
            exporter.minSize = new Vector2(420.0f, 480.0f);
        }

        [MenuItem("BabylonJS/Output Window", false, 1)]
        public static void InitOutput()
        {
            var output = (ExporterOutput)GetWindow(typeof(ExporterOutput));
            output.OnInitialize();
        }

        [MenuItem("BabylonJS/Update Libraries", false, 2)]
        public static void InitUpdate()
        {
            Tools.EnableRemoteCertificates();
            string updateMsg = (exportationOptions.DefaultUpdateOptions == (int)BabylonUpdateOptions.PreviewRelease) ? "Are you sure you want to update libraries using the github preview release version?" : "Are you sure you want to update libraries using the github stable release version?";
            if (ExporterWindow.ShowMessage(updateMsg, "Babylon.js", "Update", "Cancel"))
            {
                EditorUtility.DisplayProgressBar("Babylon.js", "Updating github editor toolkit library files...", 1);

                string libPath = Path.Combine(Application.dataPath, "Babylon/Library/");
                string bjsPath = (exportationOptions.DefaultUpdateOptions == (int)BabylonUpdateOptions.PreviewRelease) ? "https://raw.githubusercontent.com/BabylonJS/Babylon.js/master/dist/preview%20release/babylon.js" : "https://raw.githubusercontent.com/BabylonJS/Babylon.js/master/dist/babylon.js";
                string bjsTsPath = (exportationOptions.DefaultUpdateOptions == (int)BabylonUpdateOptions.PreviewRelease) ? "https://raw.githubusercontent.com/BabylonJS/Babylon.js/master/dist/preview%20release/babylon.d.ts" : "https://raw.githubusercontent.com/BabylonJS/Babylon.js/master/dist/babylon.d.ts";

                string inpsectorPath = (exportationOptions.DefaultUpdateOptions == (int)BabylonUpdateOptions.PreviewRelease) ? "https://raw.githubusercontent.com/BabylonJS/Babylon.js/master/dist/preview%20release/inspector/babylon.inspector.bundle.js" : "https://raw.githubusercontent.com/BabylonJS/Babylon.js/master/dist/inspector/babylon.inspector.bundle.js";

                string cannonPath = (exportationOptions.DefaultUpdateOptions == (int)BabylonUpdateOptions.PreviewRelease) ? "https://raw.githubusercontent.com/BabylonJS/Babylon.js/master/dist/preview%20release/cannon.js" : "https://raw.githubusercontent.com/BabylonJS/Babylon.js/master/dist/cannon.js";

                string oimoPath = (exportationOptions.DefaultUpdateOptions == (int)BabylonUpdateOptions.PreviewRelease) ? "https://raw.githubusercontent.com/BabylonJS/Babylon.js/master/dist/preview%20release/Oimo.js" : "https://raw.githubusercontent.com/BabylonJS/Babylon.js/master/dist/Oimo.js";

                try
                {
                    EditorUtility.DisplayProgressBar("Babylon.js", "Updating babylon.bjs...", 0.10f);
                    Tools.DownloadFile(bjsPath, Path.Combine(libPath, "babylon.bjs"));
                    EditorUtility.DisplayProgressBar("Babylon.js", "Updating babylon.d.ts...", 0.20f);
                    Tools.DownloadFile(bjsTsPath, Path.Combine(libPath, "babylon.d.ts"));

                    EditorUtility.DisplayProgressBar("Babylon.js", "Updating cannon.bjs...", 0.30f);
                    Tools.DownloadFile(cannonPath, Path.Combine(libPath, "cannon.bjs"));
                    EditorUtility.DisplayProgressBar("Babylon.js", "Updating oimo.bjs...", 0.40f);
                    Tools.DownloadFile(oimoPath, Path.Combine(libPath, "oimo.bjs"));

                    EditorUtility.DisplayProgressBar("Babylon.js", "Updating inspector.bjs...", 0.60f);
                    Tools.DownloadFile(inpsectorPath, Path.Combine(libPath, "inspector.bjs"));
                    EditorUtility.DisplayProgressBar("Babylon.js", "Updating navmesh.bjs...", 0.70f);
                    Tools.DownloadFile("https://raw.githubusercontent.com/BabylonJS/Extensions/master/SceneManager/dist/babylon.navigation.mesh.js", Path.Combine(libPath, "navmesh.bjs"));
                    
                    EditorUtility.DisplayProgressBar("Babylon.js", "Updating manager.bjs...", 0.80f);
                    Tools.DownloadFile("https://raw.githubusercontent.com/BabylonJS/Extensions/master/SceneManager/dist/babylon.scenemanager.js", Path.Combine(libPath, "manager.bjs"));
                    EditorUtility.DisplayProgressBar("Babylon.js", "Updating manager.d.ts...", 0.90f);
                    Tools.DownloadFile("https://raw.githubusercontent.com/BabylonJS/Extensions/master/SceneManager/dist/babylon.scenemanager.d.ts", Path.Combine(libPath, "manager.d.ts"));
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
                finally
                {
                    EditorUtility.DisplayProgressBar("Babylon.js", "Refresing assets database...", 1.0f);
                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        [MenuItem("Assets/Create/BabylonJS/Babylon JavaScript/JavaScript Class", false, 101)]
        public static void CreateJavascript_JS()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewJavascript.bjs");
            string template = "Assets/Babylon/Template/Sources/js_javascript.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Javascript File";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Babylon JavaScript/Mesh Component", false, 301)]
        public static void CreateMeshComponent_JS()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewMeshComponent.bjs");
            string template = "Assets/Babylon/Template/Sources/js_mesh.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Mesh Class";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Babylon JavaScript/Light Component", false, 302)]
        public static void CreateLightComponent_JS()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewLightComponent.bjs");
            string template = "Assets/Babylon/Template/Sources/js_light.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Light Class";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Babylon JavaScript/Camera Component", false, 303)]
        public static void CreateCameraComponent_JS()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewCameraComponent.bjs");
            string template = "Assets/Babylon/Template/Sources/js_camera.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Camera Class";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Babylon JavaScript/Shader Controller", false, 401)]   
        public static void CreateShaderController_JS()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewShaderController.bjs");
            string template = "Assets/Babylon/Template/Sources/js_shader.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Shader Controller";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Babylon JavaScript/Global Startup Script", false, 501)]
        public static void CreateApplicationScript_JS()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "Global.app.bjs");
            string template = "Assets/Babylon/Template/Sources/js_global.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Global Application Script";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Babylon TypeScript/TypeScript Class", false, 101)]
        public static void CreateTypescript_TS()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewTypescript.ts");
            string template = "Assets/Babylon/Template/Sources/ts_typescript.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Typescript File";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Babylon TypeScript/Mesh Component", false, 301)]
        public static void CreateMeshComponent_TS()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewMeshComponent.ts");
            string template = "Assets/Babylon/Template/Sources/ts_mesh.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Mesh Class";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Babylon TypeScript/Light Component", false, 302)]
        public static void CreateLightComponent_TS()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewLightComponent.ts");
            string template = "Assets/Babylon/Template/Sources/ts_light.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Light Class";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Babylon TypeScript/Camera Component", false, 303)]
        public static void CreateCameraComponent_TS()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewCameraComponent.ts");
            string template = "Assets/Babylon/Template/Sources/ts_camera.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Camera Class";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Babylon TypeScript/Shader Controller", false, 401)]
        public static void CreateShaderController_TS()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewShaderController.ts");
            string template = "Assets/Babylon/Template/Sources/ts_shader.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Shader Controller";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }
        

        [MenuItem("Assets/Create/BabylonJS/Babylon TypeScript/Global Startup Script", false, 501)]
        public static void CreateApplicationScript_TS()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "Global.app.ts");
            string template = "Assets/Babylon/Template/Sources/ts_global.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Global Application Script";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Html Markup Template", false, 601)]
        public static void CreateHtmlMarkup()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewMarkup.html");
            string template = "Assets/Babylon/Template/Config/markup.html";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Markup File";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Editor Script Component", false, 801)]
        public static void CreateEditorMeshComponent_CS()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewScriptComponent.cs");
            string template = "Assets/Babylon/Template/Sources/editora.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Script Class";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Universal Shader Material/Custom Shader Material", false, 901)]
        public static void CreateAmigaShader()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewShaderMaterial.shader");
            string template = "Assets/Babylon/Template/Sources/ux_shader.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Shader Class";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("Assets/Create/BabylonJS/Universal Shader Material/Particle System Fragment", false, 951)]
        public static void CreateParticleShader()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (String.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string filename = Path.Combine(path, "NewParticleShader.particle.fx");
            string template = "Assets/Babylon/Template/Sources/ux_particle.template";
            if (!File.Exists(template))
            {
                string defaultTemplate = "// Babylon Shader Class";
                File.WriteAllText(template, defaultTemplate);
            }
            var icon = EditorGUIUtility.FindTexture("ScriptableObject Icon") as Texture2D;
            var DoCreateScriptAsset = Type.GetType("UnityEditor.ProjectWindowCallback.DoCreateScriptAsset, UnityEditor");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance(DoCreateScriptAsset) as UnityEditor.ProjectWindowCallback.EndNameEditAction, filename, icon, template);
        }

        [MenuItem("BabylonJS/Babylon Dashboard", false, 9999)]
        public static void InitDashboard()
        {
            Application.OpenURL("http://www.babylonjs.com");
        }

        public void OnEnable()
        {
            this.titleContent = new GUIContent("Exporter");
            this.defaultProjectFolder = Tools.GetDefaultProjectFolder();
            this.guiProjectFolder = this.defaultProjectFolder;
            if (ExporterWindow.exportationOptions == null) {
                ExporterWindow.exportationOptions = CreateSettings();
            }

            // Validate project layers
            Tools.ValidateProjectLayers();

            // Validate compiler locations
            if (String.IsNullOrEmpty(exportationOptions.DefaultTypeScriptPath)) {
                exportationOptions.DefaultTypeScriptPath = Tools.GetDefaultTypeScriptPath();
            }
            if (String.IsNullOrEmpty(exportationOptions.DefaultNodeRuntimePath)) {
                exportationOptions.DefaultNodeRuntimePath = Tools.GetDefaultNodeRuntimePath();
            }
            
            // Attach unity editor buttons
            UnityEditor.EditorApplication.playModeStateChanged += (PlayModeStateChange change) =>
            {
                if (exportationOptions != null && exportationOptions.AttachUnityEditor)
                {
                    bool wantsToPlay = UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
                    bool wantsToPause = UnityEditor.EditorApplication.isPaused;
                    if (wantsToPlay || wantsToPause)
                    {
                        UnityEditor.EditorApplication.isPlaying = false;
                        UnityEditor.EditorApplication.isPaused = false;
                    }
                    if (wantsToPlay) Execute(false);
                }
            };
        }

        public void InitServer()
        {
            if (WebServer.IsStarted == false && exportationOptions.HostPreviewPage)
            {
                // Validate default project folder selected
                if (String.IsNullOrEmpty(this.defaultProjectFolder))
                {
                    UnityEngine.Debug.LogWarning("No default project file selected. Web server not started.");
                    return;
                }

                // Validate default project folder exists
                if (!Directory.Exists(this.defaultProjectFolder))
                {
                    UnityEngine.Debug.LogWarning("No default project file created. Web server not started.");
                    return;
                }

                // Validate local web server supported
                if (HttpListener.IsSupported)
                {
                    string prefix = "http://*:";
                    string unity = Tools.GetAssetsRootPath();
                    string root = this.defaultProjectFolder;
                    int port = exportationOptions.DefaultServerPort;
                    bool started = WebServer.Activate(prefix, root, port, unity);
                    if (started) UnityEngine.Debug.Log("Babylon.js web server started on port: " + port.ToString());
                    else UnityEngine.Debug.LogWarning("Babylon.js web server failed to start on port: " + port.ToString());
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Http listener services are not supported.");   
                }
            }
        }

        public void OnGUI()
        {
            GUILayout.Label("BabylonJS Toolkit - Version: " + ExporterWindow.ToolkitVersion, EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            guiProjectFolder = EditorGUILayout.TextField("", guiProjectFolder);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Save Export Settings"))
            {
                SaveSettings();
                ShowMessage("Export settings saved.");
            }

            scrollPosMain = EditorGUILayout.BeginScrollView(scrollPosMain, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
            EditorGUILayout.Space();
            exportationOptions.DefaultBinPath = EditorGUILayout.TextField("Project Bin Path", exportationOptions.DefaultBinPath);
            EditorGUILayout.Space();
            exportationOptions.DefaultBuildPath = EditorGUILayout.TextField("Project Build Path", exportationOptions.DefaultBuildPath);
            EditorGUILayout.Space();
            exportationOptions.DefaultScenePath = EditorGUILayout.TextField("Project Scene Path", exportationOptions.DefaultScenePath);
            EditorGUILayout.Space();
            exportationOptions.DefaultScriptPath = EditorGUILayout.TextField("Project Script Path", exportationOptions.DefaultScriptPath);
            EditorGUILayout.Space();
            exportationOptions.DefaultIndexPage = EditorGUILayout.TextField("Project Index Page", exportationOptions.DefaultIndexPage);
            EditorGUILayout.Space();

            showEngine = EditorGUILayout.Foldout(showEngine, "Default Engine Options");
            if (showEngine)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                exportationOptions.EnableAntiAliasing = EditorGUILayout.Toggle("Enable Engine Antialias", exportationOptions.EnableAntiAliasing);
                this.adaptToDeviceRatio = (int)(BabylonLargeEnabled)EditorGUILayout.EnumPopup("Adapt To Device Ratio", (BabylonLargeEnabled)this.adaptToDeviceRatio, GUILayout.ExpandWidth(true));
                exportationOptions.AdaptToDeviceRatio = (this.adaptToDeviceRatio == 0);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                exportationOptions.ExportPhysics = EditorGUILayout.Toggle("Enable Physics Engine", exportationOptions.ExportPhysics);
                exportationOptions.DefaultPhysicsEngine = (int)(BabylonPhysicsEngine)EditorGUILayout.EnumPopup("Default Physics Engine", (BabylonPhysicsEngine)exportationOptions.DefaultPhysicsEngine, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                exportationOptions.EnforceImageEncoding = EditorGUILayout.Toggle("Default Image Encoding", exportationOptions.EnforceImageEncoding);
                exportationOptions.ImageEncodingOptions = (int)(BabylonImageFormat)EditorGUILayout.EnumPopup("Default Texture Format", (BabylonImageFormat)exportationOptions.ImageEncodingOptions, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                exportationOptions.DefaultTextureQuality = (int)EditorGUILayout.Slider("Default Texture Quality", exportationOptions.DefaultTextureQuality, 1, 100);
                EditorGUILayout.Space();
                exportationOptions.CameraDistanceFactor = EditorGUILayout.Slider("Camera Distance Factor", exportationOptions.CameraDistanceFactor, 0.1f, 1.0f);
                EditorGUILayout.Space();
                exportationOptions.StaticVertexLimit = EditorGUILayout.Toggle("Static Mesh Vertex Limit", exportationOptions.StaticVertexLimit);
                EditorGUILayout.Space();
                EditorGUI.indentLevel -= 1;
            }

            showTerrain = EditorGUILayout.Foldout(showTerrain, "Terrain Builder Options");
            if (showTerrain)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.Space();
                exportationOptions.TerrainScaleFactor = EditorGUILayout.Slider("Terrain Scale Factor", exportationOptions.TerrainScaleFactor, 1.0f, 100.0f);
                EditorGUILayout.Space();
                exportationOptions.TerrainAtlasSize = (int)EditorGUILayout.Slider("Terrain Atlas Size", exportationOptions.TerrainAtlasSize, 128, 8192);
                EditorGUILayout.Space();
                exportationOptions.TerrainMaxImageSize = (int)EditorGUILayout.Slider("Terrain Image Max", exportationOptions.TerrainMaxImageSize, 0, 4096);
                EditorGUILayout.Space();
                exportationOptions.TerrainImageScaling = (int)(BabylonTextureScale)EditorGUILayout.EnumPopup("Terrain Image Scaling", (BabylonTextureScale)exportationOptions.TerrainImageScaling, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                exportationOptions.TerrainMeshSegemnts = (int)(BabylonTerrainColliders)EditorGUILayout.EnumPopup("Terrain Mesh Colliders", (BabylonTerrainColliders)exportationOptions.TerrainMeshSegemnts, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                exportationOptions.TerrainCoordinatesIndex = (int)EditorGUILayout.Slider("Terrain Lightmap Index", exportationOptions.TerrainCoordinatesIndex, 0, 1);
                EditorGUILayout.Space();
                exportationOptions.TerrainReceiveShadows = EditorGUILayout.Toggle("Terrain Receive Shadow", exportationOptions.TerrainReceiveShadows);
                EditorGUILayout.Space();
                EditorGUI.indentLevel -= 1;
            }

            showCollision = EditorGUILayout.Foldout(showCollision, "Collision Engine Options");
            if (showCollision)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.Space();
                exportationOptions.ExportCollisions = EditorGUILayout.Toggle("Enable Collisions", exportationOptions.ExportCollisions);
                EditorGUILayout.Space();
                exportationOptions.GenerateColliders = EditorGUILayout.Toggle("Generate Colliders", exportationOptions.GenerateColliders);
                EditorGUILayout.Space();
                exportationOptions.ColliderVisibility = (float)EditorGUILayout.Slider("Collision Visibility", exportationOptions.ColliderVisibility, 0.1f, 1.0f);
                EditorGUILayout.Space();
                exportationOptions.ShowDebugColliders = EditorGUILayout.Toggle("Show Debug Colliders", exportationOptions.ShowDebugColliders);
                EditorGUILayout.Space();
                exportationOptions.ShowDebugSockets = EditorGUILayout.Toggle("Show Debug Sockets", exportationOptions.ShowDebugSockets);
                EditorGUILayout.Space();
                exportationOptions.SocketColliderSize = (float)EditorGUILayout.Slider("Socket Collider Size", exportationOptions.SocketColliderSize, 0.01f, 1.0f);
                EditorGUILayout.Space();
                exportationOptions.WorkerCollisions = EditorGUILayout.Toggle("Proxy Worker Threads", exportationOptions.WorkerCollisions);
                EditorGUILayout.Space();
                exportationOptions.DefaultColliderDetail = (int)(BabylonColliderDetail)EditorGUILayout.EnumPopup("Default Collider Details", (BabylonColliderDetail)exportationOptions.DefaultColliderDetail, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                EditorGUI.indentLevel -= 1;
            }

            showLighting = EditorGUILayout.Foldout(showLighting, "Lightmap Baking Options");
            if (showLighting)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                exportationOptions.ExportLightmaps = EditorGUILayout.Toggle("Export Lightmaps", exportationOptions.ExportLightmaps);
                exportationOptions.DefaultLightmapBaking = (int)(BabylonLightmapBaking)EditorGUILayout.EnumPopup("Bake Iterative Maps", (BabylonLightmapBaking)exportationOptions.DefaultLightmapBaking, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                exportationOptions.DefaultCoordinatesIndex = (int)EditorGUILayout.Slider("Coordinates Index", exportationOptions.DefaultCoordinatesIndex, 0, 1);
                EditorGUILayout.Space();
                exportationOptions.LightmapMapFactor = (float)EditorGUILayout.Slider("Shadow Map Factor", exportationOptions.LightmapMapFactor, 1.0f, 25.0f);
                EditorGUILayout.Space();
                exportationOptions.CreateMaterialInstance = EditorGUILayout.Toggle("Use Material Instance", exportationOptions.CreateMaterialInstance);
                EditorGUILayout.Space();
                EditorGUI.indentLevel -= 1;
            }

            showCompiler = EditorGUILayout.Foldout(showCompiler, "Project Compiler Options");
            if (showCompiler)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.Space();
                exportationOptions.BuildJavaScript = EditorGUILayout.Toggle("Build Javascript Files", exportationOptions.BuildJavaScript);
                EditorGUILayout.Space();
                exportationOptions.CompileTypeScript = EditorGUILayout.Toggle("Build Typescript Files", exportationOptions.CompileTypeScript);
                EditorGUILayout.Space();
                if (exportationOptions.CompileTypeScript == true)
                {
                    exportationOptions.DefaultTypeScriptPath = EditorGUILayout.TextField("Typescript Compiler", exportationOptions.DefaultTypeScriptPath);
                    EditorGUILayout.Space();
                    exportationOptions.DefaultNodeRuntimePath = EditorGUILayout.TextField("Node Runtime System", exportationOptions.DefaultNodeRuntimePath);
                    EditorGUILayout.Space();
                }
                exportationOptions.DefaultUpdateOptions = (int)(BabylonUpdateOptions)EditorGUILayout.EnumPopup("Github Update Version", (BabylonUpdateOptions)exportationOptions.DefaultUpdateOptions, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                if (exportationOptions.CompileTypeScript == true)
                {
                    if (GUILayout.Button("Detect Runtime Compiler Locations"))
                    {
                        exportationOptions.DefaultTypeScriptPath = Tools.GetDefaultTypeScriptPath();
                        exportationOptions.DefaultNodeRuntimePath = Tools.GetDefaultNodeRuntimePath();
                    }
                    EditorGUILayout.Space();
                }
                EditorGUI.indentLevel -= 1;
            }            

            showPreview = EditorGUILayout.Foldout(showPreview, "Toolkit Exporter Options");
            if (showPreview)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.Space();
                exportationOptions.ScenePackingOptions = (int)(BabylonPackingOption)EditorGUILayout.EnumPopup("Scene Packing Type", (BabylonPackingOption)exportationOptions.ScenePackingOptions, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                exportationOptions.HostPreviewPage = EditorGUILayout.Toggle("Host Preview Server", exportationOptions.HostPreviewPage);
                EditorGUILayout.Space();
                if (exportationOptions.HostPreviewPage == true) {
                    exportationOptions.DefaultServerPort = EditorGUILayout.IntField("Default Server Port", exportationOptions.DefaultServerPort);
                    EditorGUILayout.Space();
                } else {
                    exportationOptions.RemoteServerPath = EditorGUILayout.TextField("Remote Server Path", exportationOptions.RemoteServerPath);
                    EditorGUILayout.Space();
                }
                exportationOptions.AttachUnityEditor = EditorGUILayout.Toggle("Attach Unity Editor", exportationOptions.AttachUnityEditor);
                EditorGUILayout.Space();
                exportationOptions.ExportHttpModule = EditorGUILayout.Toggle("Export Http Module", exportationOptions.ExportHttpModule);
                EditorGUILayout.Space();
                exportationOptions.ShowDebugControls = EditorGUILayout.Toggle("Show Debug Controls", exportationOptions.ShowDebugControls);
                EditorGUILayout.Space();
                exportationOptions.ExportMetadata = EditorGUILayout.Toggle("Export Unity Metadata", exportationOptions.ExportMetadata);
                EditorGUILayout.Space();
                exportationOptions.MinifyScriptFiles = EditorGUILayout.Toggle("Minify Project Scripts", exportationOptions.MinifyScriptFiles);
                EditorGUILayout.Space();
                exportationOptions.PrecompressContent = EditorGUILayout.Toggle("Precompress Contents", exportationOptions.PrecompressContent);
                EditorGUILayout.Space();
                exportationOptions.PrettyPrintExport = EditorGUILayout.Toggle("Debug Exporter Output", exportationOptions.PrettyPrintExport);
                EditorGUILayout.Space();
                EditorGUI.indentLevel -= 1;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Export Scene"))
            {
                Export(false);
            }
            if (GUILayout.Button("Build And Preview"))
            {
                Export(true);
            }
            if (GUILayout.Button("Rebuild Web Server"))
            {
                if (UnityEditor.EditorApplication.isCompiling == false) {
                    Tools.RebuildProjectSourceCode();
                    UnityEngine.Debug.Log("Queued project web server rebuild.");
                } else {
                    string msg = "There is a project compile in progress.";
                    UnityEngine.Debug.LogWarning(msg);
                    ShowMessage(msg);
                }
            }
            if (GUILayout.Button("Launch Preview Window"))
            {
                Execute(true);
            }
            EditorGUILayout.Space();
        }

        public void OnInspectorUpdate()
        {
            this.Repaint();
        }

        public string Build(string[] info = null)
        {
            string javascriptFile = null;
            try
            {
                var sceneInfo = info ?? GetSceneInfomation(false);
                string scriptPath = sceneInfo[2];
                string projectScript = sceneInfo[4];

                // Assemble javascript files
                javascriptFile = Tools.FormatProjectJavaScript(scriptPath, projectScript);
                if (exportationOptions.BuildJavaScript)
                {
                    ReportProgress(1, "Building project javascript files...");
                    Tools.BuildProjectJavaScript(scriptPath, javascriptFile);
                }

                // Compile typescript files
                if (exportationOptions.CompileTypeScript)
                {
                    ReportProgress(1, "Compiling project typescript files... This may take a while.");
                    string config = String.Empty;
                    string options = Path.Combine(Application.dataPath, "Babylon/Template/Config/options.json");
                    if (File.Exists(options)) config = File.ReadAllText(options);
                    this.buildResult = Tools.BuildProjectTypeScript(exportationOptions.DefaultNodeRuntimePath, exportationOptions.DefaultTypeScriptPath, scriptPath, javascriptFile, config);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            return javascriptFile;
        }

        public void Export(bool preview)
        {
            // Clear Console Log
             Tools.ClearConsoleLog(true);
            
            // Validate Project Platform
            if (!Tools.ValidateProjectPlatform()) return;

            // Validate lightmap bake in progress
            if (UnityEditor.EditorApplication.isCompiling == true)
            {
                string msg = "There is a project compile in progress.";
                UnityEngine.Debug.LogWarning(msg);
                ShowMessage(msg);
                return;
            }
            
            // Validate lightmap bake in progress
            if (Lightmapping.isRunning)
            {
                string msg = "There is a lightmap bake in progress.";
                UnityEngine.Debug.LogWarning(msg);
                ShowMessage(msg);
                return;
            }

            // Validate default project folder selected
            if (String.IsNullOrEmpty(this.defaultProjectFolder))
            {
                string msg = "No default project file selected.";
                UnityEngine.Debug.LogWarning(msg);
                ShowMessage(msg);
                return;
            }

            // Validate default project folder exists
            if (!Directory.Exists(this.defaultProjectFolder))
            {
                Directory.CreateDirectory(this.defaultProjectFolder);
                if (!Directory.Exists(this.defaultProjectFolder))
                {
                    string msg = "Failed to create default project file created.";
                    UnityEngine.Debug.LogWarning(msg);
                    ShowMessage(msg);
                    return;
                }
            }
            this.previewThread = preview;
            this.ExportSceneFiles();
        }

        public void ExportSceneFiles()
        {
            bool preview = this.previewThread;
            try
            {
                // Get validate scene path info
                string[] sceneInfo = GetSceneInfomation(true);
                string sceneName = sceneInfo[0];
                string scenePath = sceneInfo[1];
                string scriptPath = sceneInfo[2];
                string outputFile = sceneInfo[3];
                string projectScript = sceneInfo[4];
                string exportMessage = "Export current babylon scene: " + sceneName;
                if (preview) exportMessage = "Build current babylon scene: " + sceneName;
                if (!ExporterWindow.ShowMessage(exportMessage, "Babylon.js", (preview) ? "Build" : "Export", "Cancel"))
                {
                    return;
                }

                // Validate preview server initialized
                if (preview == true && exportationOptions.HostPreviewPage == true)
                {
                    this.InitServer();
                }

                // Save current scene info
                SaveSettings();
                ExporterWindow.logs.Clear();
                Stopwatch watch = new Stopwatch();
                watch.Start();
                ReportProgress(0, "Preparing scene assets: " + scenePath);
                AssetDatabase.Refresh(ImportAssetOptions.Default);
                ReportProgress(0, "Exporting scene assets: " + scenePath);

                // Lightmap shadow baking
                if (exportationOptions.ExportLightmaps)
                {
                    Tools.ValidateLightmapSettings();
                    if (Lightmapping.giWorkflowMode == Lightmapping.GIWorkflowMode.Iterative)
                    {
                        ReportProgress(1, "Baking iterative lightmap textures... This may take a while.");
                        try {
                            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
                            Lightmapping.Bake();
                        } catch(Exception baker) {
                            UnityEngine.Debug.LogException(baker);
                        } finally {
                            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.Iterative;
                        }
                    }
                }

                // Save all open scenes
                ReportProgress(1, "Saving open scene information...");
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

                // Build project preview
                ReportProgress(1, "Generating index page libraries...  This may take a while.");
                this.buildResult = 0;
                string javascriptFile = null;
                // Full build and preview 
                if (preview)
                {
                    Tools.GenerateProjectIndexPage(this.defaultProjectFolder, exportationOptions.ShowDebugControls, exportationOptions.DefaultScenePath, Path.GetFileName(outputFile), exportationOptions.DefaultScriptPath, Path.GetFileName(projectScript), exportationOptions.DefaultBinPath, exportationOptions.EnableAntiAliasing, exportationOptions.AdaptToDeviceRatio);
                    if (exportationOptions.BuildJavaScript || exportationOptions.CompileTypeScript)
                    {
                        javascriptFile = Build(sceneInfo);
                    }
                }
                if (this.buildResult == 0)
                {
                    // Build current scene
                    ReportProgress(1, "Parsing scene object information...");
                    SceneController sceneController = Tools.GetSceneController();
                    var sceneBuilder = new SceneBuilder(scenePath, sceneName, exportationOptions, sceneController, scriptPath);
                    sceneBuilder.ConvertFromUnity();
                    // Full build and preview 
                    if (preview)
                    {
                        // Pack project shaders
                        ReportProgress(1, "Parsing project shader information...");
                        string shaderScript = String.Empty;
                        DefaultAsset[] shaderVxs = Tools.GetAssetsOfType<DefaultAsset>(".vertex.fx");
                        if (shaderVxs != null && shaderVxs.Length > 0)
                        {
                            foreach (var shaderVx in shaderVxs)
                            {
                                // Validate Custom Vertex Program
                                if (!shaderVx.name.Equals("splatmap.vertex", StringComparison.OrdinalIgnoreCase)) {
                                    string basenameVx = shaderVx.name.Replace(".vertex", "").Replace("/", "_").Replace(" ", "_");
                                    string filenameVx = AssetDatabase.GetAssetPath(shaderVx);
                                    string programVx = Tools.LoadTextAsset(filenameVx);
                                    if (!String.IsNullOrEmpty(programVx)) {
                                        string programNameVx = basenameVx + "VertexShader";
                                        shaderScript += String.Format("BABYLON.Effect.ShadersStore['{0}'] = window.atob(\"{1}\");\n\n", programNameVx, Tools.FormatBase64(programVx));
                                    }
                                }
                            }
                        }
                        DefaultAsset[] shaderFxs = Tools.GetAssetsOfType<DefaultAsset>(".fragment.fx");
                        if (shaderFxs != null && shaderFxs.Length > 0)
                        {
                            foreach (var shaderFx in shaderFxs)
                            {
                                // Validate Custom Fragment Program
                                if (!shaderFx.name.Equals("splatmap.fragment", StringComparison.OrdinalIgnoreCase)) {
                                    string basenameFx = shaderFx.name.Replace(".fragment", "").Replace("/", "_").Replace(" ", "_");
                                    string filenameFx = AssetDatabase.GetAssetPath(shaderFx);
                                    string programFx = Tools.LoadTextAsset(filenameFx);
                                    if (!String.IsNullOrEmpty(programFx)) {
                                        string programNameFx = basenameFx + "PixelShader";
                                        shaderScript += String.Format("BABYLON.Effect.ShadersStore['{0}'] = window.atob(\"{1}\");\n\n", programNameFx, Tools.FormatBase64(programFx));
                                    }
                                }
                            }
                        }
                        DefaultAsset[] particleFxs = Tools.GetAssetsOfType<DefaultAsset>(".particle.fx");
                        if (particleFxs != null && particleFxs.Length > 0)
                        {
                            foreach (var particleFx in particleFxs)
                            {
                                string basenamePx = particleFx.name.Replace(".particle", "").Replace("/", "_").Replace(" ", "_");
                                string filenamePx = AssetDatabase.GetAssetPath(particleFx);
                                string programPx = Tools.LoadTextAsset(filenamePx);
                                if (!String.IsNullOrEmpty(programPx)) {
                                    string programNamePx = basenamePx + "FragmentShader";
                                    shaderScript += String.Format("BABYLON.Effect.ShadersStore['{0}'] = window.atob(\"{1}\");\n\n", programNamePx, Tools.FormatBase64(programPx));
                                }
                            }
                        }
                        if (!String.IsNullOrEmpty(shaderScript))
                        {
                            ReportProgress(1, "Packing project shaders files...");
                            if (File.Exists(javascriptFile))
                            {
                                string existingScript = File.ReadAllText(javascriptFile);
                                File.WriteAllText(javascriptFile, shaderScript + existingScript); 
                            }
                            else
                            {
                                File.WriteAllText(javascriptFile, shaderScript); 
                            }
                        }
                    }
                    // Write babylon scenes file(s)
                    sceneBuilder.WriteToBabylonFile(outputFile, javascriptFile);
                    watch.Stop();
                    string finished = String.Format("Exportation complete in {0:0.00} seconds.", watch.Elapsed.TotalSeconds);
                    ReportProgress(1, finished);
                    EditorUtility.ClearProgressBar();
                    sceneBuilder.GenerateStatus(logs);

                    string finish = preview ? "Preview" : "OK";
                    string close = preview ? "Done" : "";
                    bool ok = ShowMessage(finished, "Export Complete", finish, close);
                    if (preview && ok)
                    {
                        Preview();
                    }
                }
                else
                {
                    watch.Stop();
                    string failed = String.Format("Failed to build project in {0:0.00} seconds.", watch.Elapsed.TotalSeconds);
                    ReportProgress(1, failed);
                    EditorUtility.ClearProgressBar();
                    ShowMessage(failed, "Build Failed", "Cancel");
                }
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                ShowMessage("A problem occurred: " + ex.Message + ex.StackTrace, "Error");
            }
        }

        public string[] GetSceneInfomation(bool validate)
        {
            string[] result = new string[6];
            string sceneName = SceneManager.GetActiveScene().name;
            if (String.IsNullOrEmpty(exportationOptions.DefaultBinPath))
            {
                exportationOptions.DefaultBinPath = "bin";
            }
            string binPath = Tools.FormatSafePath(Path.Combine(this.defaultProjectFolder, exportationOptions.DefaultBinPath));
            if (validate && !Directory.Exists(binPath))
            {
                Directory.CreateDirectory(binPath);
            }
            if (String.IsNullOrEmpty(exportationOptions.DefaultBuildPath))
            {
                exportationOptions.DefaultBuildPath = "build";
            }
            string buildPath = Tools.FormatSafePath(Path.Combine(this.defaultProjectFolder, exportationOptions.DefaultBuildPath));
            if (validate && !Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }
            if (String.IsNullOrEmpty(exportationOptions.DefaultScenePath))
            {
                exportationOptions.DefaultScenePath = "scenes";
            }
            string scenePath = Tools.FormatSafePath(Path.Combine(this.defaultProjectFolder, exportationOptions.DefaultScenePath));
            if (validate && !Directory.Exists(scenePath))
            {
                Directory.CreateDirectory(scenePath);
            }
            if (String.IsNullOrEmpty(exportationOptions.DefaultScriptPath))
            {
                exportationOptions.DefaultScriptPath = "scripts";
            }
            string scriptPath = Tools.FormatSafePath(Path.Combine(this.defaultProjectFolder, exportationOptions.DefaultScriptPath));
            if (validate && !Directory.Exists(scriptPath))
            {
                Directory.CreateDirectory(scriptPath);
            }
            if (String.IsNullOrEmpty(exportationOptions.DefaultIndexPage))
            {
                exportationOptions.DefaultIndexPage = "index.html";
            }
            if (String.IsNullOrEmpty(exportationOptions.DefaultTypeScriptPath))
            {
                exportationOptions.DefaultTypeScriptPath = Tools.GetDefaultTypeScriptPath();
            }
            if (String.IsNullOrEmpty(exportationOptions.DefaultNodeRuntimePath))
            {
                exportationOptions.DefaultNodeRuntimePath = Tools.GetDefaultNodeRuntimePath();
            }
            if (exportationOptions.DefaultServerPort < 1024)
            {
                exportationOptions.DefaultServerPort = ExporterWindow.DefaultPort;
            }
            string projectName = Application.productName;
            if (String.IsNullOrEmpty(projectName))
            {
                projectName = "Application";
            }
            string outputFile = Tools.FormatSafePath(Path.Combine(scenePath, sceneName.Replace(" ", "") + Tools.GetSceneFileExtension()));
            string projectScript = Tools.FormatSafePath(Path.Combine(scenePath, projectName.Replace(" ", "") + ".babylon"));
            result[0] = sceneName;
            result[1] = scenePath;
            result[2] = buildPath;
            result[3] = outputFile;
            result[4] = projectScript;
            return result;
        }

        public void Preview()
        {
            string hostProtocol = "http://";
            string previewUrl = hostProtocol + "localhost:" + exportationOptions.DefaultServerPort.ToString() + "/" + exportationOptions.DefaultIndexPage;
            if (exportationOptions.HostPreviewPage == false) {
                previewUrl = exportationOptions.RemoteServerPath.TrimEnd('/') + "/" + exportationOptions.DefaultIndexPage;
            }
            Application.OpenURL(previewUrl);
        }

        public void Execute(bool preview)
        {
            if (preview) {
                Preview();
            } else {
                Export(true);
            }
        }
    }
}


using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using BabylonHosting;

namespace Unity3D2Babylon
{
    public class ExportationOptions
    {
        public string AlternateExport { get; set; }
        public int HostPreviewType { get; set; }
        public bool BuildJavaScript { get; set; }
        public bool CompileTypeScript { get; set; }
        public bool PrettyPrintExport { get; set; }
        public bool EnableAntiAliasing { get; set; }
        public bool AdaptToDeviceRatio { get; set; }
        public string RemoteServerPath { get; set; }
        public bool AttachUnityEditor { get; set; }
        public bool ShowDebugSockets { get; set; }
        public bool ShowDebugColliders { get; set; }
        public float ColliderVisibility { get; set; }
        public float SocketColliderSize { get; set; }
        public bool StaticVertexLimit { get; set; }
        public bool MinifyScriptFiles { get; set; }
        public bool PrecompressContent { get; set; }
        public float CameraDistanceFactor { get; set; }
        public int TerrainCoordinatesIndex { get; set; }
        public int TerrainAtlasSize { get; set; }
        public int TerrainImageScaling { get; set; }
        public bool EnableDefaultScene { get; set; }
        public bool RunWindowsPlatform { get; set; }
        public bool EnableWebAssembly { get; set; }
        public bool EnableXboxLive { get; set; }
        public bool EnableMainMenu { get; set; }
        public bool ExportMetadata { get; set; }
        public bool ExportLightmaps { get; set; }
        public bool ExportCollisions { get; set; }
        public int BakedLightsMode { get; set; }
        public bool ExportPhysics { get; set; }
        public bool ExportHttpModule { get; set; }
        public bool EmbedHtmlMarkup { get; set; }
        public bool GenerateColliders { get; set; }
        public bool EnforceImageEncoding { get; set; }
        public bool CreateMaterialInstance { get; set; }
        public string CustomWindowsSandbox { get; set; }
        public bool ShowRenderStats { get; set; }
        public int SceneRenderStats { get; set; }
        public int ImageEncodingOptions { get; set; }
        public int DefaultTextureQuality { get; set; }
        public int DefaultPhysicsEngine { get; set; }
        public int DefaultLightmapBaking { get; set; }
        public int DefaultCoordinatesIndex { get; set; }
        public int DefaultColliderDetail { get; set; }
        public string DefaultPlatformApp { get; set; }
        public string DefaultGamePage { get; set; }
        public string DefaultIndexPage { get; set; }
        public string DefaultBinPath { get; set; }
        public string DefaultBuildPath { get; set; }
        public string DefaultScenePath { get; set; }
        public string DefaultScriptPath { get; set; }
        public int DefaultServerPort { get; set; }
        public string DefaultSceneName { get; set; }
        public string DefaultTypeScriptPath { get; set; }
        public string DefaultNodeRuntimePath { get; set; }
        public int DefaultWindowsLaunchMode { get; set; }

        public ExportationOptions()
        {
            HostPreviewType = 0;
            RemoteServerPath = "http://localhost/project";
            AlternateExport = null;
            EnableDefaultScene = false;
            BuildJavaScript = true;
            CompileTypeScript = false;
            EnableMainMenu = false;
            EnableXboxLive = false;
            ShowRenderStats = false;
            SceneRenderStats = 0;
            RunWindowsPlatform = false;
            EnableAntiAliasing = true;
            AdaptToDeviceRatio = true;
            AttachUnityEditor = true;
            ShowDebugSockets = false;
            ShowDebugColliders = false;
            ColliderVisibility = 0.25f;
            SocketColliderSize = 0.125f;
            CameraDistanceFactor = 1.0f;
            StaticVertexLimit = false;
            TerrainAtlasSize = 4096;
            TerrainCoordinatesIndex = 0;
            TerrainImageScaling = 1;
            EnableWebAssembly = true;
            ExportMetadata = true;
            ExportLightmaps = true;
            ExportCollisions = true;
            ExportPhysics = true;
            ExportHttpModule = true;
            EmbedHtmlMarkup = true;
            BakedLightsMode = 0;
            GenerateColliders = true;
            EnforceImageEncoding = true;
            CreateMaterialInstance = true;
            ImageEncodingOptions = 0;
            MinifyScriptFiles = false;
            PrecompressContent = false;
            PrettyPrintExport = false;
            CustomWindowsSandbox = String.Empty;
            DefaultTextureQuality = 100;
            DefaultLightmapBaking = 0;
            DefaultCoordinatesIndex = 1;
            DefaultColliderDetail = 2;
            DefaultPhysicsEngine = 0;
            DefaultServerPort = 8888;
            DefaultBinPath = "bin";
            DefaultBuildPath = "debug";
            DefaultScenePath = "scenes";
            DefaultScriptPath = "scripts";
            DefaultIndexPage = "index.html";
            DefaultGamePage = "game.html";
            DefaultSceneName = "default";
            DefaultPlatformApp = "my-babylontoolkit://app";
            DefaultTypeScriptPath = Tools.GetDefaultTypeScriptPath();
            DefaultNodeRuntimePath = Tools.GetDefaultNodeRuntimePath();
            DefaultWindowsLaunchMode = 0;
        }
    }
}

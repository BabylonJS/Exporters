using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using BabylonHosting;

namespace Unity3D2Babylon
{
    public class ExportationOptions
    {
        public bool HostPreviewPage { get; set; }
        public bool BuildJavaScript { get; set; }
        public bool CompileTypeScript { get; set; }
        public bool PrettyPrintExport { get; set; }
        public bool EnableAntiAliasing { get; set; }
        public bool AdaptToDeviceRatio { get; set; }
        public string RemoteServerPath { get; set; }
        public bool AttachUnityEditor { get; set; }
        public int ScenePackingOptions { get; set; }
        public bool ShowDebugControls { get; set; }
        public bool ShowDebugSockets { get; set; }
        public bool ShowDebugColliders { get; set; }
        public float ColliderVisibility { get; set; }
        public float SocketColliderSize { get; set; }
        public bool StaticVertexLimit { get; set; }
        public bool MinifyScriptFiles { get; set; }
        public bool PrecompressContent { get; set; }
        public float CameraDistanceFactor { get; set; }
        public float TerrainScaleFactor { get; set; }
        public int TerrainCoordinatesIndex { get; set; }
        public bool TerrainReceiveShadows { get; set; }
        public int TerrainAtlasSize { get; set; }
        public int TerrainMaxImageSize { get; set; }
        public int TerrainImageScaling { get; set; }
        public int TerrainMeshSegemnts { get; set; }
        public bool ExportMetadata { get; set; }
        public bool ExportLightmaps { get; set; }
        public bool ExportCollisions { get; set; }
        public bool ExportPhysics { get; set; }
        public bool ExportHttpModule { get; set; }
        public bool GenerateColliders { get; set; }
        public bool WorkerCollisions { get; set; }
        public bool EnforceImageEncoding { get; set; }
        public bool CreateMaterialInstance { get; set; }
        public float LightmapMapFactor { get; set; }
        public int ImageEncodingOptions { get; set; }
        public int DefaultTextureQuality { get; set; }
        public int DefaultUpdateOptions { get; set; }
        public int DefaultPhysicsEngine { get; set; }
        public int DefaultLightmapBaking { get; set; }
        public int DefaultCoordinatesIndex { get; set; }
        public int DefaultColliderDetail { get; set; }
        public string DefaultIndexPage { get; set; }
        public string DefaultBinPath { get; set; }
        public string DefaultBuildPath { get; set; }
        public string DefaultScenePath { get; set; }
        public string DefaultScriptPath { get; set; }
        public int DefaultServerPort { get; set; }
        public string DefaultTypeScriptPath { get; set; }
        public string DefaultNodeRuntimePath { get; set; }

        public ExportationOptions()
        {
            RemoteServerPath = "http://localhost/project";
            ScenePackingOptions = 0;
            HostPreviewPage = true;
            BuildJavaScript = true;
            EnableAntiAliasing = true;
            AdaptToDeviceRatio = true;
            CompileTypeScript = false;
            AttachUnityEditor = true;
            ShowDebugControls = true;
            ShowDebugSockets = false;
            ShowDebugColliders = false;
            ColliderVisibility = 0.25f;
            SocketColliderSize = 0.125f;
            CameraDistanceFactor = 1.0f;
            StaticVertexLimit = false;
            TerrainAtlasSize = 4096;
            TerrainScaleFactor = 10.0f;
            TerrainMaxImageSize = 0;
            TerrainReceiveShadows = true;
            TerrainCoordinatesIndex = 0;
            TerrainImageScaling = 1;
            TerrainMeshSegemnts = 0;
            ExportMetadata = true;
            ExportLightmaps = true;
            ExportCollisions = true;
            ExportPhysics = true;
            ExportHttpModule = true;
            GenerateColliders = true;
            WorkerCollisions = false;
            EnforceImageEncoding = true;
            LightmapMapFactor = 5.0f;
            CreateMaterialInstance = true;
            ImageEncodingOptions = 0;
            MinifyScriptFiles = false;
            PrecompressContent = false;
            PrettyPrintExport = false;
            DefaultTextureQuality = 100;
            DefaultUpdateOptions = 0;
            DefaultLightmapBaking = 0;
            DefaultCoordinatesIndex = 1;
            DefaultColliderDetail = 2;
            DefaultPhysicsEngine = 0;
            DefaultServerPort = 8888;
            DefaultBinPath = "bin";
            DefaultBuildPath = "build";
            DefaultScenePath = "scenes";
            DefaultScriptPath = "scripts";
            DefaultIndexPage = "index.html";
            DefaultTypeScriptPath = Tools.GetDefaultTypeScriptPath();
            DefaultNodeRuntimePath = Tools.GetDefaultNodeRuntimePath();
        }
    }
}

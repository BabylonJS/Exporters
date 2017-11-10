using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using BabylonExport.Entities;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Object = UnityEngine.Object;
using Unity3D2Babylon;

namespace Unity3D2Babylon
{
    #region Unity Components
    [DataContract, Serializable]
    public class UnityNameString
    {
        [DataMember]
        public string name;

        [DataMember]
        public string value;
    }
    
    [DataContract, Serializable]
    public class UnityScriptFile
    {
        [DataMember]
        public int order;

        [DataMember]
        public string name;

        [DataMember]
        public string script;
    }

    [DataContract, Serializable]
    public class UnityFlareSystem
    {
        [DataMember]
        public string name;

        [DataMember]
        public string emitterId;

        [DataMember]
        public int borderLimit;

        [DataMember]
        public UnityFlareItem[] lensFlares;
    }

    [DataContract, Serializable]
    public class UnityFlareItem
    {
        [DataMember]
        public float size;

        [DataMember]
        public float position;

        [DataMember]
        public float[] color;

        [DataMember]
        public string textureName;
    }

    [DataContract, Serializable]
    public class UnityScriptComponent
    {
        [DataMember]
        public int order;

        [DataMember]
        public string name;

        [DataMember]
        public string klass;

        [DataMember]
        public bool update;

        [DataMember]
        public Dictionary<string, object> properties;

        [DataMember]
        public object instance;

        [DataMember]
        public object tag;

        public UnityScriptComponent()
        {
            this.order = 0;
            this.name = String.Empty;
            this.klass = String.Empty;
            this.update = true;
            this.properties = new Dictionary<string, object>();
            this.instance = null;
            this.tag = null;
        }
    }
    #endregion

    #region Read Only Attribute
    public class ReadOnlyAttribute : PropertyAttribute { }
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
    #endregion
}

namespace UnityEditor
{
    [DataContract, Serializable]
    public class SocketData
    {
        [DataMember]
        public int boneIndex;
        [DataMember]
        public string boneName;
        [DataMember]
        public object socketMesh;
        [DataMember]
        public float positionX;
        [DataMember]
        public float positionY;
        [DataMember]
        public float positionZ;
        [DataMember]
        public float rotationX;
        [DataMember]
        public float rotationY;
        [DataMember]
        public float rotationZ;
        [DataMember]
        public string prefabName;
        [DataMember]
        public float prefabPositionX;
        [DataMember]
        public float prefabPositionY;
        [DataMember]
        public float prefabPositionZ;
        [DataMember]
        public float prefabRotationX;
        [DataMember]
        public float prefabRotationY;
        [DataMember]
        public float prefabRotationZ;
    }

    [DataContract, Serializable]
    public class EmbeddedAsset
    {
        [DataMember]
        public BabylonTextEncoding encoding = BabylonTextEncoding.RawBytes;
        [DataMember]
        public TextAsset textAsset;
    }

    [DataContract, Serializable]
    public class SceneMetaData
    {
        [DataMember]
        public bool api;
        [DataMember]
        public List<string> imports;
        [DataMember]
        public Dictionary<string, object> properties;

        public SceneMetaData()
        {
            this.api = true;
            this.imports = new List<string>();
            this.properties = new Dictionary<string, object>();
        }
    }

    [DataContract, Serializable]
    public class UnityMetaData
    {
        [DataMember]
        public bool api;
        [DataMember]
        public string type;
        [DataMember]
        public bool prefab;
        [DataMember]
        public object state;
        [DataMember]
        public string objectName;
        [DataMember]
        public string objectId;
        [DataMember]
        public string tagName;
        [DataMember]
        public int layerIndex;
        [DataMember]
        public string layerName;
        [DataMember]
        public int areaIndex;
        [DataMember]
        public object navAgent;
        [DataMember]
        public object meshLink;
        [DataMember]
        public object meshObstacle;
        [DataMember]
        public List<SocketData> socketList;
        [DataMember]
        public List<object> animationClips;
        [DataMember]
        public List<object> animationEvents;
        [DataMember]
        public object collisionEvent;
        [DataMember]
        public List<object> components;
        [DataMember]
        public Dictionary<string, object> properties;

        public UnityMetaData()
        {
            this.api = true;
            this.type = "Unity";
            this.prefab = false;
            this.state = new object();
            this.objectName = String.Empty;
            this.objectId = String.Empty;
            this.tagName = String.Empty;
            this.layerIndex = -1;
            this.layerName = String.Empty;
            this.areaIndex = -1;
            this.navAgent = null;
            this.meshLink = null;
            this.meshObstacle = null;
            this.socketList = new List<SocketData>();
            this.animationClips = new List<object>();
            this.animationEvents = new List<object>();
            this.collisionEvent = null;
            this.components = new List<object>();
            this.properties = new Dictionary<string, object>();
        }
    }

    [DataContract, Serializable]
    public sealed class UnityLensFlareItem
    {
        [DataMember]
        public float size;
        [DataMember]
        public float position;
        [DataMember]
        public Color color = Color.white;
        [DataMember]
        public Texture2D texture;
    }

    [DataContract, Serializable]
    public class BabylonConfigOptions
    {
        [DataMember]
        public bool enableInput = true;
        [DataMember]
        public bool captureInput = false;
        [DataMember]
        public bool preventDefault = false;
        [DataMember]
        public BabylonInputOptions inputProperties = null;
    }

    [DataContract, Serializable]
    public class BabylonInputOptions
    {
        [DataMember, Range(0.0f, 0.9f)]
        public float padDeadStick = 0.25f;
        [DataMember]
        public bool padLStickXInvert = false;
        [DataMember]
        public bool padLStickYInvert = false;
        [DataMember]
        public bool padRStickXInvert = false;
        [DataMember]
        public bool padRStickYInvert = false;
        [DataMember, Range(0.0f, 10.0f)]
        public float padLStickLevel = 1.0f;
        [DataMember, Range(0.0f, 10.0f)]
        public float padRStickLevel = 1.0f;
        [DataMember, Range(0.0f, 5.0f)]
        public float wheelDeadZone = 0.1f;
        [DataMember, Range(0.0f, 10.0f)]
        public float mouseAngularLevel = 1.0f;
        
        [Header("[Virtual Joystick]")]

        [IgnoreDataMember]
        public BabylonJoystickOptions joystickInputMode = BabylonJoystickOptions.None;
        [DataMember]
        public bool disableRightStick = false;
        [DataMember, Range(0.0f, 0.9f)]
        public float joystickDeadStick = 0.1f;
        [DataMember, Range(0.0f, 10.0f)]
        public float joystickLeftLevel = 1.0f;
        [DataMember, Range(0.0f, 10.0f)]
        public float joystickRightLevel = 1.0f;
        [IgnoreDataMember]
        public Color joystickRightColor = Color.yellow;

        // Hidden Properties

        [DataMember, HideInInspector]
        public string joystickRightColorText = null;
        [DataMember, HideInInspector]
        public int joystickInputValue = 0;
    }

    [DataContract, Serializable]
    public class BabylonSceneOptions
    {
        [DataMember]
        public bool enableTime = true;
        [DataMember]
        public bool clearCanvas = true;
        [DataMember]
        public Vector3 defaultGravity = new Vector3(0.0f, -9.81f, 0.0f);
        [DataMember]
        public Vector3 defaultEllipsoid = new Vector3(0.5f, 1.0f, 0.5f);
        [DataMember]
        public BabylonNavigationMesh navigationMesh = BabylonNavigationMesh.EnableNavigation;
        [DataMember]
        public bool audioSources = true;
        [DataMember]
        public bool resizeCameras = true;
        [DataMember]
        public bool lensFlareSystems = true;
        [DataMember]
        public bool autoDrawInterface = true;
        [DataMember]
        public SceneAsset[] importSceneMeshes = null;
        [DataMember]
        public EmbeddedAsset graphicUserInterface = null;
    }

    [DataContract, Serializable]
    public class BabylonSkyboxOptions
    {
        [DataMember]
        public BabylonSkyboxType meshType = BabylonSkyboxType.Cube;
        [DataMember]
        public string meshTags = "WATER_TAG_0";
        [DataMember, Range(100, 2000)]
        public int skyMeshSize = 1000;
        [DataMember, Range(0.0f, 10.0f)]
        public float skyTextureLevel = 1;
        [DataMember]
        public bool highDynamicRange = false;
    }

    [DataContract, Serializable]
    public class BabylonLightOptions
    {
        [DataMember, Range(0.0f, 10.0f)]
        public float intensityScale = 1.0f;
        [DataMember]
        public Vector3 rotationOffset = new Vector3(0,0,0);
        [DataMember, Range(0.0f, 5.0f)]
        public float textureLevel = 1.0f;
        [DataMember]
        public BabylonAmbientLighting ambientLight = BabylonAmbientLighting.UnityAmbientLighting;
        [DataMember, Range(0.0f, 10.0f)]
        public float ambientScale = 1.0f;
        [DataMember]
        public Color ambientSpecular = Color.white;
        [DataMember]
        public bool enableReflections = true;
        [DataMember, Range(0.0f, 10.0f)]
        public float reflectionScale = 1.0f;
        [DataMember, Range(0.0f, 10.0f)]
        public float lightmapScale = 1.0f;
    }

    [DataContract, Serializable]
    public class BabylonSoundTrack
    {
        [DataMember]
        public float volume = 1;
        [DataMember]
        public float playbackRate = 1;
        [DataMember]
        public bool autoplay = false;
        [DataMember]
        public bool loop = false;
        [DataMember]
        public int soundTrackId = -1;
        [DataMember]
        public bool spatialSound = false;
        [DataMember]
        public Vector3 position = new Vector3(0, 0, 0);
        [DataMember]
        public float refDistance = 1;
        [DataMember]
        public float rolloffFactor = 1;
        [DataMember]
        public float maxDistance = 100;
        [DataMember]
        public string distanceModel = "linear";
        [DataMember]
        public string panningModel = "equalpower";
        [DataMember]
        public bool isDirectional = false;
        [DataMember, Range(0.0f, 360.0f)]
        public float coneInnerAngle = 360;
        [DataMember, Range(0.0f, 360.0f)]
        public float coneOuterAngle = 360;
        [DataMember]
        public float coneOuterGain = 0;
        [DataMember]
        public Vector3 directionToMesh = new Vector3(1, 0, 0);
    }

    [DataContract, Serializable]
    public class BabylonManifestOptions
    {
        [DataMember]
        public bool exportManifest = true;
        [DataMember]
        public int manifestVersion = 1;
        [DataMember]
        public bool storeSceneOffline = false;
        [DataMember]
        public bool storeTextureOffline = false;
    }

    [DataContract, Serializable]
    public class BabylonSplitterOptions
    {
        [DataMember]
        public bool progress = true;
        [DataMember]
        public bool bilinear = true;
        [DataMember]
        public int resolution = 0;
    }

    [DataContract, Serializable]
    public class BabylonWebVirtualReality
    {
        [DataMember]
        public bool trackPosition = false;
        [DataMember]
        public float positionScale = 1.0f;
        [DataMember]
        public string displayName = "";
    }

    [DataContract, Serializable]
    public class BabylonVirtualReality
    {
        [DataMember]
        public bool compensateDistortion = true;
        [DataMember]
        public int horizontalResolution = 1280;
        [DataMember]
        public int verticalResolution = 800;
        [DataMember, Range(0.0f, 1.0f)]
        public float horizontalScreen = 0.1497f;
        [DataMember, Range(0.0f, 1.0f)]
        public float verticalScreen = 0.0935f;
        [DataMember, Range(0.0f, 0.5f)]
        public float screenCenter = 0.0468f;
        [DataMember, Range(0.0f, 1.0f)]
        public float cameraBridge = 0f;
        [DataMember, Range(0.0f, 2.0f)]
        public float eyeToScreen = 0.0410f;
        [DataMember, Range(0.0f, 2.0f)]
        public float interpupillary = 0.0640f;
        [DataMember, Range(0.0f, 5.0f)]
        public float lensSeparation = 0.0635f;
        [DataMember, Range(0.0f, 5.0f)]
        public float lensCenterOffset = 0.1520f;
        [DataMember, Range(0.0f, 10.0f)]
        public float postProcessScale = 1.7146f;
    }

    [DataContract, Serializable]
    public class BabylonRenderPipeline
    {
        [DataMember]
        public float ratio = 1.0f;
        [DataMember]
        public float exposure = 1.0f;
        [DataMember]
        public float gaussCoeff = 0.3f;
        [DataMember]
        public float gaussMean = 1.0f;
        [DataMember]
        public float gaussStandDev = 0.8f;
        [DataMember]
        public float gaussMultiplier = 4.0f;
        [DataMember]
        public float brightThreshold = 0.8f;
        [DataMember]
        public float minimumLuminance = 1.0f;
        [DataMember]
        public float maximumLuminance = 1e20f;
        [DataMember]
        public float luminanceIncrease = 0.5f;
        [DataMember]
        public float luminanceDecrease = 0.5f;
    }

    [Serializable]
    public enum MotionType {
        Clip = 0,
        Tree = 1
    }

    [Serializable]
    public enum BabylonPackingOption
    {
        Json = 0,
        Binary = 1
    }

    [Serializable]
    public enum BabylonCameraOptions
    {
        UniversalCamera = 0,
        ArcRotateCamera = 1,
        FollowCamera = 2,
        ArcFollowCamera = 3,
        HolographicCamera = 4,
        WebVRFreeCamera = 5,
        WebVRGamepadCamera = 6,
        DeviceOrientationCamera = 7,
        VirtualJoysticksCamera = 8,
        VRDeviceOrientationFreeCamera = 9,
        VRDeviceOrientationGamepadCamera = 10,
        AnaglyphArcRotateCamera = 11,
        AnaglyphUniversalCamera = 12,
        StereoscopicArcRotateCamera = 13,
        StereoscopicUniversalCamera = 14
    }

    [Serializable]
    public enum BabylonEnabled
    {
        Enabled = 0,
        Disabled = 1
    }

    [Serializable]
    public enum BabylonLargeEnabled
    {
        ENABLED = 0,
        DISABLED = 1
    }

    [Serializable]
    public enum BabylonPhysicsEngine
    {
        CANNON = 0,
        OIMO = 1
    }

    [Serializable]
    public enum BabylonImageFormat
    {
        JPEG = 0,
        PNG = 1
    }

    [Serializable]
    public enum BabylonJoystickOptions
    {
        None = 0,
        Always = 1,
        Mobile = 2
    }

    [Serializable]
    public enum BabylonFogMode
    {
        None = 0,
        Exponential = 1,
        FastExponential = 2,
        Linear = 3
    }

    [Serializable]
    public enum BabylonGuiMode
    {
        None = 0,
        Html = 1
    }

    [Serializable]
    public enum BabylonSkyboxType
    {
        Cube = 0,
        Sphere = 1
    }

    [Serializable]
    public enum BabylonTickOptions
    {
        EnableTick = 0,
        DisableTick = 1
    }

    [Serializable]
    public enum BabylonTextEncoding
    {
        RawBytes = 0,
        EncodedText = 1
    }

    [Serializable]
    public enum BabylonShadowOptions
    {
        Baked = 0,
        Realtime = 1
    }

    [Serializable]
    public enum BabylonLightmapBaking
    {
        Auto = 0
    }

    [Serializable]
    public enum BabylonLoopBehavior
    {
        Relative = 0,
        Cycle = 1,
        Constant = 2
    }

    [Serializable]
    public enum BabylonParticleBlend
    {
        OneOne = 0,
        Standard = 1
    }

    [Serializable]
    public enum BabylonTextureExport
    {
        AlwaysExport = 0,
        IfNotExists = 1
    }

    [Serializable]
    public enum BabylonToolkitType
    {
        CreatePrimitive = 0,
        CombineMeshes = 1,
        SeperateMeshes = 2,
        BlockingVolumes = 3,
        BakeTextureAtlas = 4
    }

    [Serializable]
    public enum BabylonBlockingVolume
    {
        BakeColliders = 0,
        RemoveColliders = 1
    }

    [Serializable]
    public enum BabylonPrimitiveType
    {
        Ground = 0,
        Cube = 1,
        Cone = 2,
        Tube = 3,
        Wheel = 4,
        Torus = 5,
        Sphere = 6,
        Capsule = 7
    }

    [Serializable]
    public enum BabylonTextureMode
    {
        CombineMeshes = 0,
        SeperateMeshes = 1
    }

    [Serializable]
    public enum BabylonTextureScale
    {
        Point = 0,
        Bilinear = 1
    }
    
    [Serializable]
    public enum BabylonAnimationMode
    {
        None = 0,
        Transform = 1,
        Skeleton = 2
    }

    [Serializable]
    public enum BabylonProgramSection
    {
        Babylon = 0,
        Vertex = 1,
        Fragment = 2
    }


    [Serializable]
    public enum BabylonPhysicsImposter
    {
        None = 0,
        Sphere = 1,
        Box = 2,
        Plane = 3,
        Mesh = 4,
        Cylinder = 7,
        Particle = 8,
        HeightMap = 9
    }

    [Serializable]
    public enum BabylonPhysicsRotation
    {
        Normal = 0,
        Fixed = 1
    }

    public enum BabylonMovementType
    {
        DirectVelocity = 0,
        AppliedForces = 1
    }

    [Serializable]
    public enum BabylonCollisionType
    {
        Collider = 0,
        Trigger = 1
    }

    [Serializable]
    public enum BabylonAmbientLighting
    {
        NoAmbientLighting = 0,
        UnityAmbientLighting = 1
    }

    [Serializable]
    public enum BabylonNavigationMesh
    {
        DisableNavigation = 0,
        EnableNavigation = 1
    }

    [Serializable]
    public enum BabylonToneLibrary
    {
        FreeImageLibrary = 0
    }

    [Serializable]
    public enum BabylonToneMapping
    {
        Drago = 0,
        Fattal = 1,
        Reinhard = 2
    }

    [Serializable]
    public enum BabylonUpdateOptions
    {
        StableProduction = 0,
        PreviewRelease = 1,
    }

    [Serializable]
    public enum BabylonLightingFilter
    {
        NoFilter = 0,
        PoissonSampling = 1,
        ExponentialShadowMap = 2,
        BlurExponentialShadowMap = 3
    }

    [Serializable]
    public class BabylonTerrainData
    {
        public int width;
        public int height;
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector2[] uvs;
        public int[] triangles;
    }

    [Serializable]
    public enum BabylonProbeFormat
    {
        Probe128 = 128,
        Probe256 = 256,
        Probe512 = 512
    }

    [Serializable]
    public enum BabylonImageLibrary
    {
        UnityImageLibrary = 0
    }

    [Serializable]
    public enum BabylonCubemapTool
    {
        CubemapSplitter = 0,
        ReflectionProbes = 1,
        PixelPerfectTools = 2
    }

    [Serializable]
    public enum BabylonTerrainFormat
    {
        Triangles = 0
    }

    [Serializable]
    public enum BabylonTerrainResolution
    {
        FullResolution = 0
    }

    [Serializable]
    public enum BabylonTerrainColliders
    {
        TerrainMesh = 1,
        TwoByTwo = 2,
        FourByFour = 4,
        SixBySix = 6,
        EightByEight = 8,
        TwelveByTwelve = 12,
        SixteenBySixteen = 16
    }

    [Serializable]
    public enum BabylonColliderDetail
    {
        HighResolution = 0,
        MediumResolution = 1,
        LowResolution = 2,
        VeryLowResolution = 3,
        MinimumResolution = 4
    }

    [Serializable]
    public enum BabylonPreviewWindow
    {
        OpenDefaultBrowser = 0,
        AttachUnityBrowser = 1
    }

    [Serializable]
    public enum BabylonCameraInput
    {
        NoCameraUserInput = 0,
        AttachRenderCanvas = 1,
        UpdateCameraUserInput = 2
    }

    [DataContract, Serializable]
    public class BabylonEllipsoidProperties
    {
        [DataMember]
        public Vector3 defaultEllipsoid = new Vector3(0.5f, 1.0f, 0.5f);

        [DataMember]
        public Vector3 ellipsoidOffset = new Vector3(0.0f, 0.0f, 0.0f);
    }

    [DataContract, Serializable]
    public class BabylonPerformanceProperties
    {
        [DataMember]
        public bool detachFromParent = false;
        
        [DataMember]
        public bool freezeWorldMatrix = false;

        [DataMember]
        public bool convertToUnIndexed = false;

        [DataMember]
        public bool convertToFlatShaded = false;
    }

    [DataContract, Serializable]
    public class BabylonPrefabProperties
    {
        [DataMember]
        public bool makePrefabInstance = false;
        [DataMember]
        public bool offsetPrefabPosition = false;
        [DataMember]
        public Vector3 prefabOffsetPosition = Vector3.zero;
    }

    [DataContract, Serializable]
    public class BabylonSocketProperties
    {
        [DataMember]
        public bool defaultSocketPrefab = false;
        [DataMember]
        public string socketPrefabName = null;
        [DataMember]
        public Vector3 socketPrefabPosition = Vector3.zero;
        [DataMember]
        public Vector3 socketPrefabRotation = Vector3.zero;
    }

    [DataContract, Serializable]
    public class BabylonOverrideVisibility
    {
        [DataMember]
        public bool overrideVisibility = false;
        [DataMember]
        public bool makeMeshVisible = false;
        [DataMember, Range(0.0f, 1.0f)]
        public float newVisibilityLevel = 1.0f;
    }

    [DataContract, Serializable]
    public class BabylonCurveKeyframe
    {
        [DataMember]
        public float time { get; set; }
        [DataMember]
        public float value { get; set; }
        [DataMember]
        public float inTangent { get; set; }
        [DataMember]
        public float outTangent { get; set; }
        [DataMember]
        public int tangentMode { get; set; }
    }

    [DataContract, Serializable]
    public class BabylonTerrainSplat
    {
        [DataMember]
        public Color32[] Splat { get; private set; }
        [DataMember]
        public int Width { get; private set; }
        [DataMember]
        public int Height { get; private set; }
        [DataMember]
        public Vector2 TileSize { get; private set; }
        [DataMember]
        public Vector2 TileOffset { get; private set; }
        public BabylonTerrainSplat(Texture2D splat, Vector2 tile, Vector2 offset)
        {
            this.Splat = (splat != null) ? splat.GetPixels32() : null;
            this.Width = (splat != null) ? splat.width : 0;
            this.Height = (splat != null) ? splat.height : 0;
            this.TileSize = tile;
            this.TileOffset = offset;
        }
    }

    [DataContract, Serializable]
    public class BabylonCombineInstance
    {
        [DataMember]
        public MeshFilter filter { get; private set; }
        [DataMember]
        public string material { get; private set; }
        [DataMember]
        public Matrix4x4 transform { get; private set; }
        [DataMember]
        public Matrix4x4[] bindposes { get; private set; }
        [DataMember]
        public BoneWeight[] boneWeights { get; private set; }
        [DataMember]
        public int subMeshCount { get; private set; }
        [DataMember]
        public Bounds bounds { get; private set; }
        [DataMember]
        public int[] triangles { get; private set; }
        [DataMember]
        public Color32[] colors32 { get; private set; }
        [DataMember]
        public Color[] colors { get; private set; }
        [DataMember]
        public Vector2[] uv4 { get; private set; }
        [DataMember]
        public Vector2[] uv3 { get; private set; }
        [DataMember]
        public Vector2[] uv2 { get; private set; }
        [DataMember]
        public Vector2[] uv { get; private set; }
        [DataMember]
        public Vector4[] tangents { get; private set; }
        [DataMember]
        public Vector3[] normals { get; private set; }
        [DataMember]
        public Vector3[] vertices { get; private set; }

        public BabylonCombineInstance(Mesh source, Matrix4x4 transform, MeshFilter filter)
        {
            this.filter = filter;
            this.transform = transform;
            this.vertices = source.vertices;
            this.triangles = source.triangles;
            this.uv = source.uv;
            this.uv2 = source.uv2;
            this.uv3 = source.uv3;
            this.uv4 = source.uv4;
            this.bounds = source.bounds;
            this.normals = source.normals;
            this.tangents = source.tangents;
            this.colors = source.colors;
            this.colors32 = source.colors32;
            this.bindposes = source.bindposes;
            this.boneWeights = source.boneWeights;
            this.subMeshCount = source.subMeshCount;
        }

        public CombineInstanceFilter CreateCombineInstance()
        {
            Mesh mesh = new Mesh();
            mesh.name = this.filter.name;
            mesh.vertices = this.vertices;
            mesh.triangles = this.triangles;
            mesh.uv = this.uv;
            mesh.uv2 = this.uv2;
            mesh.uv3 = this.uv3;
            mesh.uv4 = this.uv4;
            mesh.bounds = this.bounds;
            mesh.normals = this.normals;
            mesh.tangents = this.tangents;
            mesh.colors = this.colors;
            mesh.colors32 = this.colors32;
            mesh.bindposes = this.bindposes;
            mesh.boneWeights = this.boneWeights;
            mesh.subMeshCount = this.subMeshCount;

            CombineInstance result = new CombineInstance();
            result.mesh = mesh;
            result.transform = this.transform;
            return new CombineInstanceFilter(result, this.filter);
        }
    }    

    [DataContract, Serializable]
    public class CombineInstanceFilter
    {
        [DataMember]
        public CombineInstance combine { get; private set; }
        [DataMember]
        public MeshFilter filter { get; private set; }

        public CombineInstanceFilter(CombineInstance combine, MeshFilter filter)
        {
            this.combine = combine;
            this.filter = filter;
        }
    }
    

    [DataContract, Serializable]
    public class BabylonDefaultMaterial: BabylonStandardMaterial
    {
        public BabylonDefaultMaterial() : base()
        {
            this.SetCustomType("BABYLON.StandardMaterial");
            this.ambient = new[] {1.0f, 1.0f, 1.0f};
            this.diffuse = new[] { 1.0f, 1.0f, 1.0f };
            this.specular = new[] { 1.0f, 1.0f, 1.0f };
            this.emissive = new[] { 0f, 0f, 0f };
            this.specularPower = 64;
            this.useSpecularOverAlpha = true;
            this.useEmissiveAsIllumination = false;
            this.linkEmissiveWithDiffuse = false;
            this.twoSidedLighting = false;
            this.maxSimultaneousLights = 4;
        }
    }

    [DataContract, Serializable]
    public class BabylonSystemMaterial : BabylonPBRMaterial
    {
        public BabylonSystemMaterial() : base()
        {
            this.SetCustomType("BABYLON.PBRMaterial");
            this.directIntensity = 1.0f;
            this.emissiveIntensity = 1.0f;
            this.environmentIntensity = 1.0f;
            this.specularIntensity = 1.0f;
            this.cameraExposure = 1.0f;
            this.cameraContrast = 1.0f;
            this.indexOfRefraction = 0.66f;
            this.twoSidedLighting = false;
            this.maxSimultaneousLights = 4;
            this.useRadianceOverAlpha = true;
            this.useSpecularOverAlpha = true;
            this.usePhysicalLightFalloff = true;
            this.useEmissiveAsIllumination = false;

            this.metallic = null;
            this.roughness = null;
            this.useRoughnessFromMetallicTextureAlpha = true;
            this.useRoughnessFromMetallicTextureGreen = false;

            this.microSurface = 0.9f;
            this.useMicroSurfaceFromReflectivityMapAplha = false;

            this.ambient = new[] { 0f, 0f, 0f };
            this.albedo = new[] { 1f, 1f, 1f };
            this.reflectivity = new[] { 1f, 1f, 1f };
            this.reflection = new[] { 0.5f, 0.5f, 0.5f };
            this.emissive = new[] { 0f, 0f, 0f };
        }
    }

   [DataContract, Serializable]
    public class BabylonUniversalMaterial : BabylonDefaultMaterial
    {
        [DataMember]
        public Dictionary<string, object> textures;

        [DataMember]
        public Dictionary<string, object[]> textureArrays;

        [DataMember]
        public Dictionary<string, object> floats;

        [DataMember]
        public Dictionary<string, object[]> floatArrays;

        [DataMember]
        public Dictionary<string, object> colors3;

        [DataMember]
        public Dictionary<string, object> colors4;

        [DataMember]
        public Dictionary<string, object> vectors2;

        [DataMember]
        public Dictionary<string, object> vectors3;

        [DataMember]
        public Dictionary<string, object> vectors4;

        [DataMember]
        public Dictionary<string, object> matrices;

        [DataMember]
        public Dictionary<string, object> matrices2x2;

        [DataMember]
        public Dictionary<string, object> matrices3x3;

        [DataMember]
        public Dictionary<string, object[]> vectors3Arrays;

        public BabylonUniversalMaterial()
        {
            this.SetCustomType("BABYLON.UniversalShaderMaterial");
            this.textures = new Dictionary<string, object>();
            this.textureArrays = new Dictionary<string, object[]>();
            this.floats = new Dictionary<string, object>();
            this.floatArrays = new Dictionary<string, object[]>();
            this.colors3 = new Dictionary<string, object>();
            this.colors4 = new Dictionary<string, object>();
            this.vectors2 = new Dictionary<string, object>();
            this.vectors3 = new Dictionary<string, object>();
            this.vectors4 = new Dictionary<string, object>();
            this.matrices = new Dictionary<string, object>();
            this.matrices2x2 = new Dictionary<string, object>();
            this.matrices3x3 = new Dictionary<string, object>();
            this.vectors3Arrays = new Dictionary<string, object[]>();
        }
    }
    
   [DataContract, Serializable]
    public class BabylonLDRCubeTexture : BabylonTexture
    {
        [DataMember]
        public string customType { get; private set; }

        [DataMember]
        public int size { get; set; }

        [DataMember]
        public bool useInGammaSpace { get; set; }

        public BabylonLDRCubeTexture()
        {
            this.customType = "BABYLON.LDRCubeTexture";
            this.size = 0;
            this.isCube = true;
            this.useInGammaSpace = false;
        }
    }

    [Serializable]
    public class MachineState
    {
        public string tag;
        public float time;
        public string name;
        public int type;
        public string motion;
        public string branch;
        public float rate;
        public float length;
        public string layer;
        public int index;
        public string machine;
        public bool interupted;
        public float apparentSpeed;
        public float averageAngularSpeed;
        public float averageDuration;
        public float[] averageSpeed;
        public float cycleOffset;
        public string cycleOffsetParameter;
        public bool cycleOffsetParameterActive;
        public bool iKOnFeet;
        public bool mirror;
        public string mirrorParameter;
        public bool mirrorParameterActive;
        public float speed;
        public string speedParameter;
        public bool speedParameterActive;
        public Dictionary<string, object> blendtree;
        public List<object> animations;
        public List<Dictionary<string, object>> transitions;
        public List<Dictionary<string, BabylonAnimation>> customCurves;
    }

    [Serializable]
    public enum BabylonAnimationLooping
    {
        AnimationClip = 0
    }

    [Serializable]
    public enum BabylonAnimationBaking
    {
        GameBlend = 0,
        AnimationClip = 1
    }

    [Serializable]
    public class AnimationParameters
    {
        public bool defaultBool;
        public float defaultFloat;
        public int defaultInt;
        public string name;
        public int type;
        public bool curve;
    }

    [DataContract, Serializable, AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class BabylonClassAttribute : Attribute { }

    [DataContract, Serializable, AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public sealed class BabylonPropertyAttribute : PropertyAttribute { }

    /////////////////////////////////////////////////////
    // Note: The Only Supported Toolkit Script Component
    /////////////////////////////////////////////////////

    [Serializable, CanEditMultipleObjects]
    public abstract class EditorScriptComponent : MonoBehaviour
    {
        [Unity3D2Babylon.ReadOnly]
        public string babylonClass;
        [HideInInspector]
        public Action<GameObject, Dictionary<string, object>> OnExportProperties { get; set; }
        void Start(){}
        protected EditorScriptComponent()
        {
            this.babylonClass = "BABYLON.SceneComponent";
        }
    }
}

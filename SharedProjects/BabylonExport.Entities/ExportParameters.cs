using Utilities;
using GLTFExport.Entities;

namespace BabylonExport.Entities
{
    public enum NormalMapFormat
    {
        unknown = 1,
        directx = 2,
        opengl = 3
    }

    public enum NormalMapY
    {
        unknown = 1, positiv = 2, negativ = 3
    }

    public enum MapCoordinate
    {
        unknown = 1, right = 2, left= 3
    }


    public class NormalMapParameters
    {
        public const NormalMapY YDefault = NormalMapY.unknown;
        public const MapCoordinate CoordinateDefault = MapCoordinate.unknown;

        public NormalMapY Y = YDefault;
        public MapCoordinate Coordinate = CoordinateDefault;

        public bool IsDirectX => Y == NormalMapY.negativ && Coordinate == MapCoordinate.left;
        public bool IsOpenGL => Y == NormalMapY.positiv && Coordinate == MapCoordinate.right;
        public bool IsBabylon => IsDirectX;
        public bool IsGLTF => IsOpenGL;
    }

    // Define the policy use to assign format to aggregated texture such ORM
    public enum TextureFormatExportPolicy
    {
        QUALITY, // we want the best quality
        CONSERVATIV, // we try to keep the source format if possible
        SIZE // we try to minimize the size of texture as much as possibe.
    }
    public class DracoParameters
    {
        public const string dracoPrefix = "draco.";
        public static readonly string compressionLevel_param_name = $"{dracoPrefix}compressionLevel";
        public static readonly string quantizePositionBits_param_name = $"{dracoPrefix}quantizePositionBits";
        public static readonly string quantizeNormalBits_param_name = $"{dracoPrefix}quantizeNormalBits";
        public static readonly string quantizeTexcoordBits_param_name = $"{dracoPrefix}quantizeTexcoordBits";
        public static readonly string quantizeColorBits_param_name = $"{dracoPrefix}quantizeColorBits";
        public static readonly string quantizeGenericBits_param_name = $"{dracoPrefix}quantizeGenericBits";
        public static readonly string unifiedQuantization_param_name = $"{dracoPrefix}unifiedQuantization";

        
        // default values are defined from https://github.com/CesiumGS/gltf-pipeline#command-line-flags
        public const int compressionLevel_default = 7;
        public const int quantizePositionBits_default = 14;
        public const int quantizeNormalBits_default = 10;
        public const int quantizeTexcoordBits_default = 12;
        public const int quantizeColorBits_default = 8;
        public const int quantizeGenericBits_default = 12;
        public const bool unifiedQuantization_default = false;

        public int compressionLevel = compressionLevel_default;
        public int quantizePositionBits = quantizePositionBits_default;
        public int quantizeNormalBits = quantizeNormalBits_default;
        public int quantizeTexcoordBits = quantizeTexcoordBits_default;
        public int quantizeColorBits = quantizeColorBits_default;
        public int quantizeGenericBits = quantizeGenericBits_default;
        public bool unifiedQuantization = unifiedQuantization_default;

        public string toCLIArgs()
        {
            return $"--{compressionLevel_param_name} {compressionLevel} --{quantizePositionBits_param_name} {quantizePositionBits}  --{quantizeNormalBits_param_name} {quantizeNormalBits} --{quantizeTexcoordBits_param_name} {quantizeTexcoordBits} --{quantizeColorBits_param_name} {quantizeColorBits} --{quantizeGenericBits_param_name} {quantizeGenericBits} --{unifiedQuantization_param_name} {unifiedQuantization}";
        }
    }

    public class ExportParameters
    {
        public string softwarePackageName;
        public string softwareVersion;
        public string exporterVersion;
        public string outputPath; // The directory to store the generated files
        public string outputFormat;
        public string textureFolder;
        public float scaleFactor = 1.0f;
        public bool writeTextures = true;
        public bool overwriteTextures = true;
        public bool exportHiddenObjects = false;
        public bool exportMaterials = true;
        public bool exportOnlySelected = false;
        public bool bakeAnimationFrames = false;
        public bool optimizeAnimations = true;
        public bool optimizeVertices = true;
        public bool animgroupExportNonAnimated = false;
        public bool generateManifest = false;
        public bool autoSaveSceneFile = false;
        public bool exportTangents = true;
        public bool exportSkins = true;
        public long txtQuality = 100;
        public bool mergeAO = true;
        public bool enableKHRLightsPunctual = false;
        public bool enableKHRTextureTransform = false;
        public bool enableKHRMaterialsUnlit = false;
        public bool pbrFull = false;
        public bool pbrNoLight = false;
        public bool createDefaultSkybox = false;
        public string pbrEnvironment;
        public bool exportAnimations = true;
        public bool exportAnimationsOnly = false;
        public bool exportTextures = true;
        // try to optimize the output reu-sing opaque and blend texture.
        public bool tryToReuseOpaqueAndBlendTexture = false;
        public TextureFormatExportPolicy textureFormatExportPolicy = TextureFormatExportPolicy.CONSERVATIV;

        public IGLTFMaterialExporter customGLTFMaterialExporter;
        public bool useMultiExporter = false;

        public const string ModelFilePathProperty = "modelFilePathProperty";
        public const string TextureFolderPathProperty = "textureFolderPathProperty";

        public const string PBRFullPropertyName = "babylonjs_pbr_full";
        public const string PBRNoLightPropertyName = "babylonjs_pbr_nolight";
        public const string PBREnvironmentPathPropertyName = "babylonjs_pbr_environmentPathProperty";

        #region DRACO
        public bool dracoCompression = false;
        public DracoParameters dracoParams = null;
        #endregion

        #region Morph
        public bool rebuildMorphTarget = true;
        public bool exportMorphTangents = true;
        public bool exportMorphNormals = true;
        #endregion
    }
}

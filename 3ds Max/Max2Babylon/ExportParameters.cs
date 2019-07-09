namespace Max2Babylon
{
    public class ExportParameters
    {
        public string outputPath;
        public string outputFormat;
        public string textureFolder;
        public string scaleFactor = "1";
        public bool writeTextures = true;
        public bool overwriteTextures = true;
        public bool exportHiddenObjects = false;
        public bool exportMaterials = true;
        public bool exportOnlySelected = false;
        public bool generateManifest = false;
        public bool autoSave3dsMaxFile = false;
        public bool exportTangents = true;
        public string txtQuality = "100";
        public bool mergeAOwithMR = true;
        public bool dracoCompression = false;
        public bool enableKHRLightsPunctual = false;
        public bool enableKHRTextureTransform = false;
        public bool enableKHRMaterialsUnlit = false;
        public bool pbrFull = false;
        public bool pbrNoLight = false;
        public string pbrEnvironment;

        public Autodesk.Max.IINode exportNode;

        public const string ModelFilePathProperty = "modelFilePathProperty";
        public const string TextureFolderPathProperty = "textureFolderPathProperty";

        public const string PBRFullPropertyName = "babylonjs_pbr_full";
        public const string PBRNoLightPropertyName = "babylonjs_pbr_nolight";
        public const string PBREnvironmentPathPropertyName = "babylonjs_pbr_environmentPathProperty";
    }
}

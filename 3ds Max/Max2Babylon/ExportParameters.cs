namespace Max2Babylon
{
    public class ExportParameters
    {
        public string outputPath;
        public string outputFormat;
        public string scaleFactor = "1";
        public bool writeTextures = true;
        public bool overwriteTextures = true;
        public bool exportHiddenObjects = false;
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
    }
}

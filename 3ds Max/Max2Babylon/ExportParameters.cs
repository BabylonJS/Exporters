namespace Max2Babylon
{
    public class ExportParameters
    {
        public string outputPath;
        public string outputFormat;
        public string scaleFactor = "1";
        public bool copyTexturesToOutput = true;
        public bool exportHiddenObjects = false;
        public bool exportOnlySelected = false;
        public bool generateManifest = false;
        public bool autoSave3dsMaxFile = false;
    }
}

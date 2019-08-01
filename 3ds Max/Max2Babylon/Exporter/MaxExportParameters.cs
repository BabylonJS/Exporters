using BabylonExport.Entities;

namespace Max2Babylon
{
    public class MaxExportParameters : ExportParameters
    {
        public Autodesk.Max.IINode exportNode;
        public bool useHoldFetchLogig = true; // some script could use the babaylon exporter and override this value
        public bool mergeInheritedContainers = false;
    }
}

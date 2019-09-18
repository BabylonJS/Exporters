using System.Collections.Generic;
using BabylonExport.Entities;

namespace Max2Babylon
{
    public class MaxExportParameters : ExportParameters
    {
        public Autodesk.Max.IINode exportNode;
        public List<Autodesk.Max.IILayer> exportLayers;
        public bool usePreExportProcess = false;
        public bool mergeContainersAndXRef = false;
        public bool flattenScene = false;
        
    }
}

using Autodesk.Max;
using BabylonExport.Entities;

namespace Max2Babylon
{
    public interface IMaterialExporter
    {
        IClass_ID MaterialClassID { get; }

		bool IsBabylonExporter { get; }
		bool IsGltfExporter { get; }

        BabylonMaterial ExportBabylonMaterial(IIGameMaterial material);
    }
}
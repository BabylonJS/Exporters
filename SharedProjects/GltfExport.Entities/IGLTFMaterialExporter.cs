using BabylonExport.Entities;
using Utilities;

namespace GLTFExport.Entities
{
    public interface IGLTFMaterialExporter
    {
        /// <summary>
        /// Returns true 
        /// </summary>
        /// <param name="babylonMaterial"></param>
        /// <param name="gltfMaterial"></param>
        /// <returns></returns>
        bool GetGltfMaterial(BabylonMaterial babylonMaterial, GLTF gltf, ILoggingProvider logger, out GLTFMaterial gltfMaterial);
    }
}

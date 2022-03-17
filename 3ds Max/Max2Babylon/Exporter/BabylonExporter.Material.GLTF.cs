using System.Collections.Generic;
using System.Linq;
using Autodesk.Max;
using BabylonExport.Entities;

namespace Max2Babylon
{
    /// <summary>
    /// The Exporter part dedicated to Autodesk Gltf Material.
    /// </summary>
    partial class BabylonExporter
    {
        /// <summary>
        /// Export dedicated to Autodesk GLTF Material
        /// </summary>
        /// <param name="materialNode">the material node interface</param>
        /// <param name="babylonScene">the scene to export the material</param>
        private void ExportGLTFMaterial(IIGameMaterial materialNode, BabylonScene babylonScene)
        {
 
        }

        public bool isGLTFMaterial(IIGameMaterial materialNode)
        {
            return ClassIDWrapper.Gltf_Material.Equals(materialNode.MaxMaterial.ClassID);
        }
    }
}
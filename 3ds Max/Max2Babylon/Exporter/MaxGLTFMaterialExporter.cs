using Autodesk.Max;
using Max2Babylon;
using System.Drawing;
using System.Collections.Generic;
using BabylonExport.Entities;
using GLTFExport.Entities;
using Babylon2GLTF;
using Utilities;

internal class MaxGLTFMaterialExporter : IGLTFMaterialExporter
{
    private Dictionary<ClassIDWrapper, IMaxMaterialExporter> materialExporters;
    private ExportParameters exportParameters;
    private GLTFExporter gltfExporter;

    public MaxGLTFMaterialExporter(Dictionary<ClassIDWrapper, IMaxMaterialExporter> materialExporters, ExportParameters exportParameters, GLTFExporter gltfExporter)
    {
        this.materialExporters = materialExporters;
        this.exportParameters = exportParameters;
        this.gltfExporter = gltfExporter;
    }

    public bool GetGltfMaterial(BabylonMaterial babylonMaterial, GLTF gltf, ILoggingProvider logger, out GLTFMaterial gltfMaterial)
    {
        gltfMaterial = null;
        IIGameMaterial gameMtl = babylonMaterial.maxGameMaterial;
        IMtl maxMtl = gameMtl.MaxMaterial;

        IMaxMaterialExporter materialExporter;
        if (materialExporters.TryGetValue(new ClassIDWrapper(maxMtl.ClassID), out materialExporter)
            && materialExporter is IMaxGLTFMaterialExporter)
        {
            gltfMaterial = ((IMaxGLTFMaterialExporter)materialExporter).ExportGLTFMaterial(this.exportParameters, gltf, gameMtl,
                (string sourcePath, string textureName) => { return gltfExporter.TryWriteImage(gltf, sourcePath, textureName); },
                (string message, Color color) => { logger.RaiseMessage(message, color, 2); },
                (string message) => { logger.RaiseWarning(message, 2); },
                (string message) => { logger.RaiseError(message, 2); });

            if (gltfMaterial == null)
            {
                string message = string.Format("Custom glTF material exporter failed to export | Exporter: '{0}' | Material Name: '{1}' | Material Class: '{2}'",
                    materialExporter.GetType().ToString(), gameMtl.MaterialName, gameMtl.ClassName);
                logger.RaiseWarning(message, 2);
            }
            return true;
        }
        return false;
    }
}
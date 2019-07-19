using Autodesk.Max;
using Max2Babylon;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Reflection;
using BabylonExport.Entities;
using GLTFExport.Entities;
using Babylon2GLTF;
using Utilities;

internal class MaxGLTFMaterialExporter : IGLTFMaterialExporter
{
    private Dictionary<ClassIDWrapper, IMaxMaterialExporter> materialExporters;
    private ExportParameters exportParameters;
    private GLTFExporter gltfExporter;

    public MaxGLTFMaterialExporter(ExportParameters exportParameters, GLTFExporter gltfExporter, ILoggingProvider logger)
    {
        // Instantiate custom material exporters
        this.materialExporters = new Dictionary<ClassIDWrapper, IMaxMaterialExporter>();
        foreach (Type type in Tools.GetAllLoadableTypes())
        {
            if (type.IsAbstract || type.IsInterface || !typeof(IMaxMaterialExporter).IsAssignableFrom(type))
                continue;

            IMaxMaterialExporter exporter = Activator.CreateInstance(type) as IMaxMaterialExporter;

            if (exporter == null)
                logger.RaiseWarning("Creating exporter instance failed: " + type.Name, 1);

            materialExporters.Add(exporter.MaterialClassID, exporter);
        }
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
                return false;
            }
            return true;
        }
        return false;
    }
}
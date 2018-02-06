using System;
using System.Collections.Generic;
using Autodesk.Max;
using BabylonExport.Entities;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        private void ExportWorldModifiers(IIGameNode meshNode, BabylonScene babylonScene, BabylonMesh babylonMesh)
        {
            var derivedObject = meshNode.MaxNode.WSMDerivedObject;
            if (derivedObject == null)
            {
                RaiseMessage("derivedOvject is null", 2);
            }
            else
            {
                foreach (var modifier in derivedObject.Modifiers)
                {
                    // TODO - check translations
                    if (modifier.Name == "Hair and Fur")
                    {
                        var babylonFurMaterial = ExportFurModifier(modifier, babylonMesh.name, babylonScene);
                        babylonScene.MaterialsList.Add(babylonFurMaterial);
                        babylonMesh.materialId = babylonFurMaterial.id;
                    }
                    else
                    {
                        RaiseWarning("Modifier or Language" + modifier.Name + " is not supported");
                    }
                }
            }
        }

        private BabylonFurMaterial ExportFurModifier(IModifier modifier, String sourceMeshName, BabylonScene babylonScene)
        {
            // Default:
            int furLength = 1;
            int density = 20;
            int spacing = 12;
            BabylonTexture diffuseTexture = null;

            for (int i = 0; i < modifier.NumParamBlocks; i++)
            {
                var paramBlock = modifier.GetParamBlock(i);

                for (short paramId = 0; paramId < paramBlock.NumParams; paramId++)
                {
                    var name = paramBlock.GetLocalName(paramId, 0);

                    // TODO - check translation
                    switch (name)
                    {
                        case "Cut Length":
                            // 3dsMax Cut Length is in percentages -
                            // "100" Cut length (longest hair) will be translated to "10" babylon furLength
                            furLength = (int)(paramBlock.GetFloat(paramId, 0, 0) / 10);
                            break;
                        case "Density":
                            // 3dsMax Density is in percentages -
                            // "100" density in 3dsmax will be translated to "20" babylon density 
                            density = (int)(paramBlock.GetFloat(paramId, 0, 0) / 5);
                            break;
                        case "Hair Segments":
                            spacing = paramBlock.GetInt(paramId, 0, 0);
                            break;
                        case "Maps":
                            if (paramBlock.GetTexmap(paramId, 0, 11) != null)
                            {
                                RaiseWarning("tip color is not supported in exporter - babylon Hair And Fur support only one color, use root color instead");
                            }

                            ITexmap rootColorTexmap = paramBlock.GetTexmap(paramId, 0, 14);
                            diffuseTexture = ExportTexture(rootColorTexmap, 0f, babylonScene);
                            diffuseTexture.level = 1;
                            break;
                    }
                }
            }

            return new BabylonFurMaterial
            {
                id = modifier.GetGuid().ToString(),
                name = modifier.GetGuid().ToString(),
                sourceMeshName = sourceMeshName,
                furLength = furLength,
                furDensity = density,
                furSpacing = spacing,
                diffuseTexture = diffuseTexture,
            };   
        }
    }
}
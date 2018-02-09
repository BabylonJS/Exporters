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
            // Defaults:
            int density = 20;
            int spacing = 12;
            float[] furColor = new[] { 1f, 1f, 1f };
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
                            // 3dsMax "Cut Length" is in percentages -
                            // "100%" will be "33" babylon spacing 
                            spacing = (int)Math.Round(paramBlock.GetFloat(paramId, 0, 0) / 3);
                            break;
                        case "Density":
                            // 3dsMax Density is in percentages -
                            // "100%" will be "20" babylon density 
                            density = (int)(paramBlock.GetFloat(paramId, 0, 0) / 5);
                            break;
                        case "Root Color":
                            var rootColor = paramBlock.GetColor(paramId, 0, 0);
                            furColor = new float[] {
                                rootColor.R,
                                rootColor.G,
                                rootColor.B
                            };
                            break;
                        case "Tip Color":
                            if (paramBlock.GetColor(paramId, 0, 0) != null)
                            {
                                RaiseWarning("tip color is not supported - use root color instead");
                            }
                            break;
                        case "Hair Segments":
                        // TODO - need to affect "quality"?
                        case "Maps":
                            if (paramBlock.GetTexmap(paramId, 0, 11) != null)
                            {
                                RaiseWarning("tip texture is not supported - use root texture instead");
                            }

                            ITexmap rootColorTexmap = paramBlock.GetTexmap(paramId, 0, 14);
                            if (rootColorTexmap != null)
                            {
                                diffuseTexture = ExportTexture(rootColorTexmap, 0f, babylonScene);
                                diffuseTexture.level = 1;
                            }
                            break;
                    }
                }
            }

            return new BabylonFurMaterial
            {
                id = modifier.GetGuid().ToString(),
                name = modifier.GetGuid().ToString(),
                sourceMeshName = sourceMeshName,
                furDensity = density,
                furSpacing = spacing,
                diffuseTexture = diffuseTexture,
                furColor = furColor,
            };   
        }
    }
}
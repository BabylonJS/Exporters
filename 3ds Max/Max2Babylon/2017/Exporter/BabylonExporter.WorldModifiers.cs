using System;
using System.Collections.Generic;
using Autodesk.Max;
using BabylonExport.Entities;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        const int CUT_LENGTH_PARAM_ID = 3;
        const int ROOT_THICKNESS_PARAM_ID = 7;
        const int ROOT_COLOR_PARAM_ID = 19;
        const int MAPS_PARAM_ID = 51;

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
                    if (modifier.Name == "Hair and Fur" || modifier.Name == "Chevelure et Pelage")
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
            var paramBlock = modifier.GetParamBlock(0);
            
            // 3dsMax "Cut Length" is in percentages - "100%" will be "20" babylon spacing 
            // (babylon Fur length means the distance from the obj, while the length of the hair is the spacing)
            var cutLength = paramBlock.GetFloat(CUT_LENGTH_PARAM_ID, 0, 0);
            var spacing = (int)Math.Round(cutLength / 5);

            // 3dsMax "Root Thick" is in percentages - "100%" will be "1" babylon density 
            // (lower density in babylon is thicker hair - lower root thick in 3dsMax is thinner)
            var rootThickness = paramBlock.GetFloat(ROOT_THICKNESS_PARAM_ID, 0, 0);
            var density = (int)Math.Ceiling((100.1f - rootThickness) / 5);

            var rootColor = paramBlock.GetColor(ROOT_COLOR_PARAM_ID, 0, 0);
            var furColor = new float[] { rootColor.R, rootColor.G, rootColor.B };

            if (paramBlock.GetTexmap(MAPS_PARAM_ID, 0, 11) != null)
            {
                RaiseWarning("tip texture is not supported - use root texture instead");
            }

            BabylonTexture diffuseTexture = null;
            ITexmap rootColorTexmap = paramBlock.GetTexmap(MAPS_PARAM_ID, 0, 14);
            if (rootColorTexmap != null)
            {
                diffuseTexture = ExportTexture(rootColorTexmap, 0f, babylonScene);
                diffuseTexture.level = 1;
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
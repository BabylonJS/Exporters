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
            if (derivedObject != null)
            {
                foreach (var modifier in derivedObject.Modifiers)
                {
                    // TODO - Find another way to detect if modifier is a HairAndFur
                    if (modifier.Name == "Hair and Fur" || // English
                        modifier.Name == "Haar und Fell" || // German
                        modifier.Name == "Chevelure et Pelage") // French
                    {
                        var babylonFurMaterial = ExportFurModifier(modifier, babylonMesh.name, babylonScene);
                        babylonScene.MaterialsList.Add(babylonFurMaterial);
                        babylonMesh.materialId = babylonFurMaterial.id;
                    }
                    else
                    {
                        RaiseWarning("Modifier or Language '" + modifier.Name + "' is not supported", 2);
                    }
                }
            }
        }

        private BabylonFurMaterial ExportFurModifier(IModifier modifier, String sourceMeshName, BabylonScene babylonScene)
        {
            RaiseMessage("Export Fur Modifier", 2);
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
                RaiseWarning("Tip texture is not supported. Use root texture instead", 2);
            }

            BabylonTexture diffuseTexture = null;
            ITexmap rootColorTexmap = paramBlock.GetTexmap(MAPS_PARAM_ID, 0, 14);
            if (rootColorTexmap != null)
            {
                diffuseTexture = ExportTexture(rootColorTexmap, babylonScene, 0f);
                diffuseTexture.level = 1;
            }

            return new BabylonFurMaterial(modifier.GetGuid().ToString())
            {
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
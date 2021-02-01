using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;

[assembly: MPxNodeClass(typeof(Maya2Babylon.babylonAiStandardSurfaceMaterialNode), "babylonAiStandardSurfaceMaterialNode", 0x0008106d, //528493 in decimal // TODO - Ask Autodesk for a unique ID
    NodeType = MPxNode.NodeType.kHardwareShader, Classification = "shader/surface/utility")]

namespace Maya2Babylon
{
    public class babylonAiStandardSurfaceMaterialNode : babylonMaterialNodeBase {
        public static int id = 528493;

        public static void Create(MFnDependencyNode materialDependencyNode)
        {
            // Create Babylon Material dependency node
            string babylonMaterialNodeName;
            MGlobal.executeCommand($"shadingNode -asShader babylonAiStandardSurfaceMaterialNode;", out babylonMaterialNodeName);
            MGlobal.executeCommand($"connectAttr -f {materialDependencyNode.name}.outColor {babylonMaterialNodeName}.outTransparency;");
        }

        public static void Init(MFnDependencyNode babylonAttributesDependencyNode, BabylonPBRMetallicRoughnessMaterial babylonMaterial = null)
        {
            babylonMaterialNodeBase.Init(babylonAttributesDependencyNode, babylonMaterial);
            
            if (babylonAttributesDependencyNode.hasAttribute("babylonTransparencyMode") == false) {
                MGlobal.executeCommand($"addAttr -ln \"babylonTransparencyMode\" -nn \"Transparency Mode\" -at \"enum\" -en \"Opaque:Cutoff:Blend:\" {babylonAttributesDependencyNode.name};");
            }

            // Initialise attributes according to babylon material
            if (babylonMaterial != null) {
                // Init alpha mode value based on material opacity
                setAttributeValue(babylonAttributesDependencyNode.name + ".babylonTransparencyMode", babylonMaterial.transparencyMode);
            }
        }
    }
}

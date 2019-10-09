using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;

[assembly: MPxNodeClass(typeof(Maya2Babylon.babylonStandardMaterialNode), "babylonStandardMaterialNode", 0x0008106b,
    NodeType = MPxNode.NodeType.kHardwareShader, Classification = "shader/surface/utility")]

namespace Maya2Babylon
{
    public class babylonStandardMaterialNode : MPxNode
    {
        public static MTypeId sId = new MTypeId(0xF3560C30); // TODO - Ask Autodesk for a unique ID

        [MPxNodeInitializer()]
        public static bool initialize()
        {
            return true;
        }

        public override void postConstructor()
        {
        }

        public override bool getInternalValue(MPlug plug, MDataHandle dataHandle)
        {
            return base.getInternalValue(plug, dataHandle);
        }

        public override bool setInternalValue(MPlug plug, MDataHandle dataHandle)
        {
            return base.setInternalValue(plug, dataHandle);
        }

        public static void Create(MFnDependencyNode materialDependencyNode)
        {
            // Create Babylon Material dependency node
            string babylonMaterialNodeName;
            MGlobal.executeCommand($"shadingNode -asShader babylonStandardMaterialNode;", out babylonMaterialNodeName);
            MGlobal.executeCommand($"connectAttr -f {materialDependencyNode.name}.outColor {babylonMaterialNodeName}.outTransparency;");
        }

        public static void Init(MFnDependencyNode babylonAttributesDependencyNode, BabylonStandardMaterial babylonMaterial)
        {
            // Ensure all attributes are setup
            if (babylonAttributesDependencyNode.hasAttribute("babylonTransparencyMode") == false)
            {
                MGlobal.executeCommand($"addAttr -ln \"babylonTransparencyMode\" -nn \"Transparency Mode\" - at \"enum\" -en \"Opaque:Cutoff:Blend:\" {babylonAttributesDependencyNode.name};");

                // Init alpha mode value based on material opacity
                if (babylonMaterial.alpha != 1.0f || (babylonMaterial.diffuseTexture != null && babylonMaterial.diffuseTexture.hasAlpha) || babylonMaterial.opacityTexture != null)
                {
                    MGlobal.executeCommand($"setAttr \"{babylonAttributesDependencyNode.name}.babylonTransparencyMode\" 2;");
                }
            }
            if (babylonAttributesDependencyNode.hasAttribute("babylonBackfaceCulling") == false)
            {
                MGlobal.executeCommand($"addAttr -ln \"babylonBackfaceCulling\" -nn \"Backface Culling\" - at bool {babylonAttributesDependencyNode.name};");
                MGlobal.executeCommand($"setAttr \"{babylonAttributesDependencyNode.name}.babylonBackfaceCulling\" 1;");
            }
            if (babylonAttributesDependencyNode.hasAttribute("babylonUnlit") == false)
            {
                MGlobal.executeCommand($"addAttr -ln \"babylonUnlit\" -nn \"Unlit\" - at bool {babylonAttributesDependencyNode.name};");
            }
            if (babylonAttributesDependencyNode.hasAttribute("babylonMaxSimultaneousLights") == false)
            {
                MGlobal.executeCommand($"addAttr -ln \"babylonMaxSimultaneousLights\" -nn \"Max Simultaneous Lights\" - at long  -min 1 -dv 4 {babylonAttributesDependencyNode.name};");
            }
        }
    }
}

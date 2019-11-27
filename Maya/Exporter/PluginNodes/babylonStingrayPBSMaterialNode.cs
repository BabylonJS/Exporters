using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;

[assembly: MPxNodeClass(typeof(Maya2Babylon.babylonStingrayPBSMaterialNode), "babylonStingrayPBSMaterialNode", 0x0008106c, //528492 in decimal // TODO - Ask Autodesk for a unique ID
    NodeType = MPxNode.NodeType.kHardwareShader, Classification = "shader/surface/utility")]

namespace Maya2Babylon
{
    public class babylonStingrayPBSMaterialNode : MPxNode
    {
        public static int id = 528492;

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
            MGlobal.executeCommand($"shadingNode -asShader babylonStingrayPBSMaterialNode;", out babylonMaterialNodeName);
            MGlobal.executeCommand($"connectAttr -f {materialDependencyNode.name}.outColor {babylonMaterialNodeName}.outTransparency;");
        }

        /// <summary>
        /// Ensure all attributes are setup
        /// </summary>
        /// <param name="babylonAttributesDependencyNode"></param>
        /// <param name="babylonMaterial"></param>
        public static void Init(MFnDependencyNode babylonAttributesDependencyNode, BabylonPBRMetallicRoughnessMaterial babylonMaterial = null)
        {
            if (babylonAttributesDependencyNode.hasAttribute("babylonTransparencyMode") == false)
            {
                MGlobal.executeCommand($"addAttr -ln \"babylonTransparencyMode\" -nn \"Transparency Mode\" - at \"enum\" -en \"Opaque:Cutoff:Blend:\" {babylonAttributesDependencyNode.name};");

                // Init alpha mode value based on material opacity
                if (babylonMaterial != null)
                    MGlobal.executeCommand($"setAttr \"{babylonAttributesDependencyNode.name}.babylonTransparencyMode\" {babylonMaterial.transparencyMode};");
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

        public override bool connectionMade(MPlug plug, MPlug otherPlug, bool asSrc)
        {
            MFnDependencyNode sourceNodePlug = new MFnDependencyNode(plug.node);

            Init(sourceNodePlug);

            return base.connectionMade(plug, otherPlug, asSrc);
        }
    }
}

using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;

[assembly: MPxNodeClass(typeof(Maya2Babylon.babylonAiStandardSurfaceMaterialNode), "babylonAiStandardSurfaceMaterialNode", 0x0008106d, //528493 in decimal // TODO - Ask Autodesk for a unique ID
    NodeType = MPxNode.NodeType.kHardwareShader, Classification = "shader/surface/utility")]

namespace Maya2Babylon
{
    public class babylonAiStandardSurfaceMaterialNode : MPxNode
    {
        public static int id = 528493;

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
            MGlobal.executeCommand($"shadingNode -asShader babylonAiStandardSurfaceMaterialNode;", out babylonMaterialNodeName);
            MGlobal.executeCommand($"connectAttr -f {materialDependencyNode.name}.outColor {babylonMaterialNodeName}.outTransparency;");
        }

        public static void Init(MFnDependencyNode babylonAttributesDependencyNode, BabylonPBRMetallicRoughnessMaterial babylonMaterial = null)
        {
            // Ensure all attributes are setup
            if (babylonAttributesDependencyNode.hasAttribute("babylonTransparencyMode") == false)
            {
                MGlobal.executeCommand($"addAttr -ln \"babylonTransparencyMode\" -nn \"Opacity Mode\" -at \"enum\" -en \"Opaque:Cutoff:Blend:\" {babylonAttributesDependencyNode.name};");
            }
            if (babylonAttributesDependencyNode.hasAttribute("babylonBackfaceCulling") == false)
            {
                MGlobal.executeCommand($"addAttr -ln \"babylonBackfaceCulling\" -nn \"Backface Culling\" -at bool {babylonAttributesDependencyNode.name};");
            }
            if (babylonAttributesDependencyNode.hasAttribute("babylonUnlit") == false)
            {
                MGlobal.executeCommand($"addAttr -ln \"babylonUnlit\" -nn \"Unlit\" -at bool {babylonAttributesDependencyNode.name};");
            }
            if (babylonAttributesDependencyNode.hasAttribute("babylonMaxSimultaneousLights") == false)
            {
                MGlobal.executeCommand($"addAttr -ln \"babylonMaxSimultaneousLights\" -nn \"Max Simultaneous Lights\" -at long  -min 1 -dv 4 {babylonAttributesDependencyNode.name};");
            }

            // Initialise attributes according to babylon material
            if (babylonMaterial != null) {
                // Init alpha mode value based on material opacity
                setAttributeValue(babylonAttributesDependencyNode.name + ".babylonTransparencyMode", babylonMaterial.transparencyMode);

                setAttributeValue(babylonAttributesDependencyNode.name + ".babylonBackfaceCulling", Convert.ToInt32(babylonMaterial.backFaceCulling));

                setAttributeValue(babylonAttributesDependencyNode.name + ".babylonUnlit", Convert.ToInt32(babylonMaterial.isUnlit));

                setAttributeValue(babylonAttributesDependencyNode.name + ".babylonMaxSimultaneousLights", babylonMaterial.maxSimultaneousLights);
            }
        }

        public override bool connectionMade(MPlug plug, MPlug otherPlug, bool asSrc)
        {
            MFnDependencyNode sourceNodePlug = new MFnDependencyNode(plug.node);

            Init(sourceNodePlug);

            return base.connectionMade(plug, otherPlug, asSrc);
        }

        /// <summary>
        /// Returns true if the attribute is locked.
        /// </summary>
        /// <param name="name">The name of the attribute in the format: {nodeName}.{attributeName}</param>
        public static bool isAttributeLocked(string name) {
            int isUnlocked;
            MGlobal.executeCommand($"getAttr -settable \"{name}\";", out isUnlocked);
            return isUnlocked == 1 ? false : true;
        }

        /// <summary>
        /// Set the value of the given attribute.
        /// </summary>
        /// <param name="name">The name of the attribute in the format: {nodeName}.{attributeName}</param>
        /// <param value="value">The new value of the attribute</param>
        public static void setAttributeValue(string name, int value) {
            bool isLocked = isAttributeLocked(name);
            if (isLocked) {
                MGlobal.executeCommand($"setAttr -l false \"{name}\";");
            }

            MGlobal.executeCommand($"setAttr \"{name}\" {value};");

            if (isLocked) {
                MGlobal.executeCommand($"setAttr -l true \"{name}\";");
            }
        }
    }
}

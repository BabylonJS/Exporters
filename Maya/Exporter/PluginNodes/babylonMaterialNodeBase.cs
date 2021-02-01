using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;

namespace Maya2Babylon {
    public abstract class babylonMaterialNodeBase : babylonMPxNode {

        public override bool connectionMade(MPlug plug, MPlug otherPlug, bool asSrc) {
            MFnDependencyNode sourceNodePlug = new MFnDependencyNode(plug.node);

            Init(sourceNodePlug);

            return base.connectionMade(plug, otherPlug, asSrc);
        }

        protected static void Init(MFnDependencyNode babylonAttributesDependencyNode, BabylonMaterial babylonMaterial = null) {
            
            // Ensure all attributes are setup            
            if (babylonAttributesDependencyNode.hasAttribute("babylonBackfaceCulling") == false) {
                MGlobal.executeCommand($"addAttr -ln \"babylonBackfaceCulling\" -nn \"Backface Culling\" -at bool {babylonAttributesDependencyNode.name};");
            }
            if (babylonAttributesDependencyNode.hasAttribute("babylonUnlit") == false) {
                MGlobal.executeCommand($"addAttr -ln \"babylonUnlit\" -nn \"Unlit\" -at bool {babylonAttributesDependencyNode.name};");
            }
            if (babylonAttributesDependencyNode.hasAttribute("babylonMaxSimultaneousLights") == false) {
                MGlobal.executeCommand($"addAttr -ln \"babylonMaxSimultaneousLights\" -nn \"Max Simultaneous Lights\" -at long  -min 1 -dv 4 {babylonAttributesDependencyNode.name};");
            }

            // Initialise attributes according to babylon material
            if (babylonMaterial != null) {
                setAttributeValue(babylonAttributesDependencyNode.name + ".babylonBackfaceCulling", Convert.ToInt32(babylonMaterial.backFaceCulling));

                setAttributeValue(babylonAttributesDependencyNode.name + ".babylonUnlit", Convert.ToInt32(babylonMaterial.isUnlit));

                setAttributeValue(babylonAttributesDependencyNode.name + ".babylonMaxSimultaneousLights", babylonMaterial.maxSimultaneousLights);
            }
        }
    }
}

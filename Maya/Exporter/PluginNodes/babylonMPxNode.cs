using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;

namespace Maya2Babylon {
    public abstract class babylonMPxNode : MPxNode {

        [MPxNodeInitializer()]
        public static bool initialize() {
            return true;
        }

        public override void postConstructor() {
        }

        public override bool getInternalValue(MPlug plug, MDataHandle dataHandle) {
            return base.getInternalValue(plug, dataHandle);
        }

        public override bool setInternalValue(MPlug plug, MDataHandle dataHandle) {
            return base.setInternalValue(plug, dataHandle);
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

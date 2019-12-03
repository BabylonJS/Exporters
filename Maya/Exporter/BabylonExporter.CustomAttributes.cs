using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using MayaBabylon;

namespace Maya2Babylon
{
    internal partial class BabylonExporter
    {

        private class BaseObject
        {
            public MFnTransform mFnTransform;
            public BabylonStandardMaterial babylonMaterial;
        }

        public Dictionary<string, object> ExportCustomAttributeFromTransform(MFnTransform mfnTransform) {
            var baseObject = new BaseObject();
            baseObject.mFnTransform = mfnTransform;
            baseObject.babylonMaterial = null;
            return _ExportCustomUserAttributes(baseObject);
        }

        public Dictionary<string, object> ExportCustomAttributeFromMaterial(BabylonStandardMaterial babylonMaterial)
        {
            var baseObject = new BaseObject();
            baseObject.mFnTransform = null;
            baseObject.babylonMaterial = babylonMaterial;
            return _ExportCustomUserAttributes(baseObject);
        }

        private Dictionary<string,object> _ExportCustomUserAttributes(BaseObject baseObject)
        {
            var objectName = "";

            if (baseObject.mFnTransform != null)
            {
                objectName = baseObject.mFnTransform.name;
            }
            else if(baseObject.babylonMaterial != null)
            {
                objectName = baseObject.babylonMaterial.name;
            }

            MStringArray customAttributeNames = new MStringArray();
            Dictionary<string, object> customsAttributes = new Dictionary<string, object>();

            MGlobal.executeCommand($"listAttr -ud {objectName}", customAttributeNames);

            foreach (string name in customAttributeNames)
            {
                MStringArray type = new MStringArray();

                MGlobal.executeCommand($"getAttr -type {objectName}.{name}", type);

                switch (type[0])
                {
                    case "double":
                        double floatValue = 0;
                        MGlobal.executeCommand($"getAttr {objectName}.{name}", out floatValue);
                        customsAttributes.Add(name, floatValue);
                        break;
                    case "bool":
                        int boolBinValue = 0;
                        MGlobal.executeCommand($"getAttr {objectName}.{name}", out boolBinValue);
                        customsAttributes.Add(name, boolBinValue);
                        break;
                    case "long":
                        int intValue = 0;
                        MGlobal.executeCommand($"getAttr {objectName}.{name}", out intValue);
                        customsAttributes.Add(name, intValue);
                        break;
                    case "string":
                        string stringValue = "";
                        MGlobal.executeCommand($"getAttr {objectName}.{name}", out stringValue);
                        customsAttributes.Add(name, stringValue);
                        break;
                    case "enum":
                        int enumValue = 0;
                        MGlobal.executeCommand($"getAttr {objectName}.{name}", out enumValue);
                        customsAttributes.Add(name, enumValue);
                        break;
                    case "double3":
                        MDoubleArray vectorValue = new MDoubleArray();
                        MGlobal.executeCommand($"getAttr {objectName}.{name}", vectorValue);
                        customsAttributes.Add(name, vectorValue);
                        break;
                    default:
                        MCommandResult attrValue = new MCommandResult();
                        MGlobal.executeCommand($"getAttr {objectName}.{name}", attrValue);
                        customsAttributes.Add(name, attrValue);
                        break;
                }
            }

            foreach (string name in customAttributeNames)
            {
                if (customsAttributes.ContainsKey(name + "X") && customsAttributes.ContainsKey(name + "Y") && customsAttributes.ContainsKey(name + "Z"))
                {
                    customsAttributes.Remove(name + "X");
                    customsAttributes.Remove(name + "Y");
                    customsAttributes.Remove(name + "Z");
                }
            }

            return customsAttributes;
        }
    }
}

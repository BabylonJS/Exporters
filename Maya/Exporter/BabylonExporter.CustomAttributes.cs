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
            public BabylonMaterial babylonMaterial;
        }

        public Dictionary<string, object> ExportCustomAttributeFromTransform(MFnTransform mfnTransform) {
            var baseObject = new BaseObject();
            baseObject.mFnTransform = mfnTransform;
            baseObject.babylonMaterial = null;
            return _ExportCustomUserAttributes(baseObject);
        }

        public Dictionary<string, object> ExportCustomAttributeFromMaterial(BabylonMaterial babylonMaterial)
        {
            var baseObject = new BaseObject();
            baseObject.mFnTransform = null;
            baseObject.babylonMaterial = babylonMaterial;
            return _ExportCustomUserAttributes(baseObject);
        }

        private Dictionary<string, object> _ExportCustomUserAttributes(BaseObject baseObject)
        {
            var objectName = "";

            if (baseObject.mFnTransform != null)
            {
                objectName = baseObject.mFnTransform.name;
            }
            else if (baseObject.babylonMaterial != null)
            {
                objectName = baseObject.babylonMaterial.name;
            }

            MStringArray customAttributeNamesMStringArray = new MStringArray();
            Dictionary<string, object> customsAttributes = new Dictionary<string, object>();

            try
            {
                MGlobal.executeCommand($"listAttr -ud {objectName}", customAttributeNamesMStringArray);
            }
            catch (Exception e)
            {
                //do nothing...
            }

            var customAttributeNames = customAttributeNamesMStringArray.Where((attributeName) => { return !_DisallowedCustomAttributeNames.Contains(attributeName); });

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

        /**
         *  This is a list of attributes that will not be exported as custom attributes, as the StingrayPBS material uses custom attributes to implement their node based shader.
         *  Any custom attribute that uses these default names will not be included in export.
         */
        public List<string> _DisallowedCustomAttributeNames = new List<string>()
        {
            "TEX_global_diffuse_cube",  
            "TEX_global_diffuse_cubeX", 
            "TEX_global_diffuse_cubeY", 
            "TEX_global_diffuse_cubeZ", 
            "TEX_global_specular_cube", 
            "TEX_global_specular_cubeX",
            "TEX_global_specular_cubeY",
            "TEX_global_specular_cubeZ",
            "TEX_brdf_lut",
            "TEX_brdf_lutX",
            "TEX_brdf_lutY",
            "TEX_brdf_lutZ",
            "use_normal_map",
            "uv_offset",
            "uv_offsetX",
            "uv_offsetY",
            "uv_scale",
            "uv_scaleX",
            "uv_scaleY",
            "TEX_normal_map",
            "TEX_normal_mapX",
            "TEX_normal_mapY",
            "TEX_normal_mapZ",
            "use_color_map",
            "TEX_color_map",
            "TEX_color_mapX",
            "TEX_color_mapY",
            "TEX_color_mapZ",
            "base_color",
            "base_colorR",
            "base_colorG",
            "base_colorB",
            "use_metallic_map",
            "TEX_metallic_map",
            "TEX_metallic_mapX",
            "TEX_metallic_mapY",
            "TEX_metallic_mapZ",
            "metallic",
            "use_roughness_map",
            "TEX_roughness_map",
            "TEX_roughness_mapX",
            "TEX_roughness_mapY",
            "TEX_roughness_mapZ",
            "roughness",
            "use_emissive_map",
            "TEX_emissive_map",
            "TEX_emissive_mapX",
            "TEX_emissive_mapY",
            "TEX_emissive_mapZ",
            "emissive",
            "emissiveR",
            "emissiveG",
            "emissiveB",
            "emissive_intensity",
            "use_ao_map",
            "TEX_ao_map",
            "TEX_ao_mapX",
            "TEX_ao_mapY",
            "TEX_ao_mapZ"
        };
    }
}

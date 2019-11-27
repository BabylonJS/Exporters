using Autodesk.Max;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        /// <summary>
        /// Return the custom attributes of a material
        /// </summary>
        /// <param name="materialNode"></param>
        /// <param name="babylonScene"></param>
        /// <param name="excludeAttributes">Attribute names to not export</param>
        public Dictionary<string, object> ExportExtraAttributes(IIGameMaterial gameMaterial, BabylonScene babylonScene, List<string> excludeAttributes = null)
        {
            // Retreive the max object
            ManagedServices.MaxscriptSDK.ExecuteMaxscriptCommand("obj = sceneMaterials[\"" + gameMaterial.MaterialName + "\"];");

            return _ExportExtraAttributes(gameMaterial.IPropertyContainer, babylonScene, excludeAttributes);
        }

        /// <summary>
        /// Return the custom attributes of a material
        /// </summary>
        /// <param name="materialNode"></param>
        /// <param name="babylonScene"></param>
        /// <param name="excludeAttributes">Attribute names to not export</param>
        public Dictionary<string, object> ExportExtraAttributes(IIGameNode gameNode, BabylonScene babylonScene, List<string> excludeAttributes = null)
        {
            // Retreive the max object
            ManagedServices.MaxscriptSDK.ExecuteMaxscriptCommand("obj = execute(\"$'" + gameNode.Name + "'\");");

            return _ExportExtraAttributes(gameNode.IGameObject.IPropertyContainer, babylonScene, excludeAttributes);
        }

        /// <summary>
        /// Return custom attributes retreive from a max object named "obj"
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="propertyContainer"></param>
        /// <param name="babylonScene"></param>
        /// <param name="excludeAttributes">Attribute names to not export</param>
        private Dictionary<string, object> _ExportExtraAttributes(IIPropertyContainer propertyContainer, BabylonScene babylonScene, List<string> excludeAttributes = null)
        {
            RaiseMessage("ExportExtraAttributes", 2);

            // Return a string encoded with 2 separators
            // Parameter separator: _$€PParam_
            // Name/Type separator: _$€PType_
            string cmd = "s = \"\""
                + "\r\n" + "for objDef in (custAttributes.getDefs obj) do"
                + "\r\n" + "("
                    + "\r\n" + "pbArray = custAttributes.getPBlockDefs objdef"
                    + "\r\n" + "for indexPBlock = 1 to pbArray.count do"
                    + "\r\n" + "("
                        + "\r\n" + "itms = pbArray[indexPBlock]"
                        + "\r\n" + "for y = 5 to itms.Count do"
                        + "\r\n" + "("
                            + "\r\n" + "s = s + \"_$€PParam_\" + itms[y][1]"
                            + "\r\n" + "for z = 1 to itms[y][2].Count by 2 do"
                            + "\r\n" + "("
                                + "\r\n" + "key = itms[y][2][z] as string"
                                + "\r\n" + "if (findString key \"type\") != undefined then"
                                + "\r\n" + "("
                                    + "\r\n" + "s = s + \"_$€PType_\" + itms[y][2][z+1]"
                                + "\r\n" + ")"
                            + "\r\n" + ")"
                        + "\r\n" + ")"
                    + "\r\n" + ")"
                + "\r\n" + ")"
                + "\r\n" + "s";
            string result = ManagedServices.MaxscriptSDK.ExecuteStringMaxscriptQuery(cmd);

            if (result == null || result == "")
            {
                return null;
            }

            // Parse the result into a dictionary
            string[] parameters = result.Split(new string[] { "_$€PParam_" }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> customAttributesTypeByName = new Dictionary<string, string>();
            foreach (string parameter in parameters)
            {
                string[] customAttribute = parameter.Split(new string[] { "_$€PType_" }, StringSplitOptions.RemoveEmptyEntries);
                string key = customAttribute[0];
                if (customAttributesTypeByName.ContainsKey(key) == false)
                {
                    customAttributesTypeByName.Add(key, customAttribute[1]);
                }
            }

            // Remove preset custom attributes
            customAttributesTypeByName.Remove("presetName_str");
            customAttributesTypeByName.Remove("preset_str");
            customAttributesTypeByName.Remove("rampOn");

            // Remove specified attributes
            if (excludeAttributes != null)
            {
                foreach (string excludeAttribute in excludeAttributes)
                {
                    customAttributesTypeByName.Remove(excludeAttribute);
                }
            }

            // Handle each attribute type
            Dictionary<string, object> metadata = new Dictionary<string, object>();
            foreach (KeyValuePair<string, string> entry in customAttributesTypeByName)
            {
                object obj = null;

                RaiseMessage(entry.Key + "=" + entry.Value, 2);

                switch (entry.Value.ToLowerInvariant())
                {
                    case "float":
                    case "angle": // in rad units
                    case "worldunits":
                        obj = propertyContainer.GetFloatProperty(entry.Key);
                        break;
                    case "percent": // in base 1 (80% => 0.8)
                        obj = propertyContainer.GetFloatProperty(entry.Key) / 100f;
                        break;
                    case "boolean":
                        obj = propertyContainer.GetBoolProperty(entry.Key);
                        break;
                    case "integer":
                    case "array": // selected enum value expressed as int starting from 1
                        obj = propertyContainer.GetIntProperty(entry.Key);
                        break;
                    case "string":
                        obj = propertyContainer.GetStringProperty(entry.Key);
                        break;
                    case "color": // Color RGB in base 1 (not 255)
                        obj = propertyContainer.GetPoint3Property(entry.Key).ToArray();
                        break;
                    case "frgba": // Color RGBA in base 1 (not 255)
                        obj = propertyContainer.GetPoint4Property(entry.Key).ToArray();
                        break;
                    case "texturemap":
                        IIGameProperty gameProperty = propertyContainer.QueryProperty(entry.Key);
                        ITexmap texmap = gameProperty.MaxParamBlock2.GetTexmap(gameProperty.ParamID, 0, 0);
                        obj = ExportTexture(texmap, babylonScene);
                        break;
                    case "node":
                        // Currently not exported
                        break;
                    case "material":
                        // Currently not exported
                        break;
                    default:
                        RaiseWarning("Unknown type '" + entry.Value + "' for custom attribute named '" + entry.Key + "'", 2);
                        break;
                }

                if (obj != null)
                {
                    metadata.Add(entry.Key, obj);
                }
            }

            // Print all extra attributes
            foreach (KeyValuePair<string, object> entry in metadata)
            {
                RaiseVerbose(entry.Key + "=" + entry.Value, 2);
            }

            return metadata;
        }
    }
}

using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Maya2Babylon
{
    partial class BabylonExporter
    {
        /// <summary>
        /// List of simple materials
        /// </summary>
         List<MFnDependencyNode> referencedMaterials = new List<MFnDependencyNode>();

        /// <summary>
        /// List of sub materials binded to each multimaterial
        /// </summary>
        readonly Dictionary<string, List<MFnDependencyNode>> multiMaterials = new Dictionary<string, List<MFnDependencyNode>>();
        
        private void ExportMultiMaterial(string uuidMultiMaterial, List<MFnDependencyNode> materials, BabylonScene babylonScene)
        {
            var babylonMultimaterial = new BabylonMultiMaterial { id = uuidMultiMaterial };

            // Name
            string nameConcatenated = "";
            bool isFirstTime = true;
            List<MFnDependencyNode> materialsSorted = new List<MFnDependencyNode>(materials);
            materialsSorted.Sort(new MFnDependencyNodeComparer());
            foreach (MFnDependencyNode material in materialsSorted)
            {
                if (!isFirstTime)
                {
                    nameConcatenated += "_";
                }
                isFirstTime = false;

                nameConcatenated += material.name;
            }
            babylonMultimaterial.name = nameConcatenated;

            // Materials
            var uuids = new List<string>();
            foreach (MFnDependencyNode subMat in materials)
            {
                string uuidSubMat = subMat.uuid().asString();
                uuids.Add(uuidSubMat);

                if (!referencedMaterials.Contains(subMat, new MFnDependencyNodeEqualityComparer()))
                {
                    // Export sub material
                    referencedMaterials.Add(subMat);
                    ExportMaterial(subMat, babylonScene);
                }
            }
            babylonMultimaterial.materials = uuids.ToArray();

            babylonScene.MultiMaterialsList.Add(babylonMultimaterial);
        }

        private void ExportMaterial(MFnDependencyNode materialDependencyNode, BabylonScene babylonScene)
        {
            MObject materialObject = materialDependencyNode.objectProperty;
            var name = materialDependencyNode.name;
            var id = materialDependencyNode.uuid().asString();

            RaiseMessage(name, 1);

            RaiseVerbose("materialObject.hasFn(MFn.Type.kBlinn)=" + materialObject.hasFn(MFn.Type.kBlinn), 2);
            RaiseVerbose("materialObject.hasFn(MFn.Type.kPhong)=" + materialObject.hasFn(MFn.Type.kPhong), 2);
            RaiseVerbose("materialObject.hasFn(MFn.Type.kPhongExplorer)=" + materialObject.hasFn(MFn.Type.kPhongExplorer), 2);

            if (materialObject.hasFn(MFn.Type.kLambert))
            {
                var lambertShader = new MFnLambertShader(materialObject);
                
                RaiseVerbose("typeId=" + lambertShader.typeId, 2);
                RaiseVerbose("typeName=" + lambertShader.typeName, 2);
                RaiseVerbose("color=" + lambertShader.color.toString(), 2);
                RaiseVerbose("transparency=" + lambertShader.transparency.toString(), 2);
                RaiseVerbose("ambientColor=" + lambertShader.ambientColor.toString(), 2);
                RaiseVerbose("incandescence=" + lambertShader.incandescence.toString(), 2);
                RaiseVerbose("diffuseCoeff=" + lambertShader.diffuseCoeff, 2);
                RaiseVerbose("translucenceCoeff=" + lambertShader.translucenceCoeff, 2);

                var babylonMaterial = new BabylonStandardMaterial
                {
                    name = name,
                    id = id,
                    ambient = lambertShader.ambientColor.toArrayRGB(),
                    diffuse = lambertShader.color.toArrayRGB(),
                    emissive = lambertShader.incandescence.toArrayRGB(),
                    alpha = 1.0f - lambertShader.transparency[0]
                };
                
                // If transparency is not a shade of grey (shade of grey <=> R=G=B)
                if (lambertShader.transparency[0] != lambertShader.transparency[1] ||
                    lambertShader.transparency[0] != lambertShader.transparency[2])
                {
                    RaiseWarning("Transparency color is not a shade of grey. Only it's R channel is used.", 2);
                }

                // Specular power
                if (materialObject.hasFn(MFn.Type.kReflect))
                {
                    var reflectShader = new MFnReflectShader(materialObject);

                    RaiseVerbose("specularColor=" + reflectShader.specularColor.toString(), 2);
                    RaiseVerbose("reflectivity=" + reflectShader.reflectivity, 2);
                    RaiseVerbose("reflectedColor=" + reflectShader.reflectedColor.toString(), 2);

                    babylonMaterial.specular = reflectShader.specularColor.toArrayRGB();

                    if (materialObject.hasFn(MFn.Type.kBlinn))
                    {
                        MFnBlinnShader blinnShader = new MFnBlinnShader(materialObject);
                        babylonMaterial.specularPower = (1.0f - blinnShader.eccentricity) * 256;
                    }
                    else if (materialObject.hasFn(MFn.Type.kPhong))
                    {
                        MFnPhongShader phongShader = new MFnPhongShader(materialObject);
                        babylonMaterial.specularPower = phongShader.cosPower;
                    }
                    else if (materialObject.hasFn(MFn.Type.kPhongExplorer))
                    {
                        MFnPhongEShader phongEShader = new MFnPhongEShader(materialObject);
                        // No use of phongE.whiteness and phongE.highlightSize
                        babylonMaterial.specularPower = (1.0f - phongEShader.roughness) * 256;
                    }
                    else
                    {
                        RaiseMessage("Unknown reflect shader type: " + reflectShader.typeName + ". Specular power is default 64. Consider using a Blinn or Phong shader instead.", 2);
                    }
                }

                // TODO
                //babylonMaterial.backFaceCulling = !stdMat.TwoSided;
                //babylonMaterial.wireframe = stdMat.Wire;

                // TODO - Textures
                //BabylonFresnelParameters fresnelParameters;

                //babylonMaterial.ambientTexture = ExportTexture(stdMat, 0, out fresnelParameters, babylonScene);                // Ambient
                //babylonMaterial.diffuseTexture = ExportTexture(stdMat, 1, out fresnelParameters, babylonScene);                // Diffuse
                //if (fresnelParameters != null)
                //{
                //    babylonMaterial.diffuseFresnelParameters = fresnelParameters;
                //}

                //babylonMaterial.specularTexture = ExportTexture(stdMat, 2, out fresnelParameters, babylonScene);               // Specular
                //babylonMaterial.emissiveTexture = ExportTexture(stdMat, 5, out fresnelParameters, babylonScene);               // Emissive
                //if (fresnelParameters != null)
                //{
                //    babylonMaterial.emissiveFresnelParameters = fresnelParameters;
                //    if (babylonMaterial.emissive[0] == 0 &&
                //        babylonMaterial.emissive[1] == 0 &&
                //        babylonMaterial.emissive[2] == 0 &&
                //        babylonMaterial.emissiveTexture == null)
                //    {
                //        babylonMaterial.emissive = new float[] { 1, 1, 1 };
                //    }
                //}

                //babylonMaterial.opacityTexture = ExportTexture(stdMat, 6, out fresnelParameters, babylonScene, false, true);   // Opacity
                //if (fresnelParameters != null)
                //{
                //    babylonMaterial.opacityFresnelParameters = fresnelParameters;
                //    if (babylonMaterial.alpha == 1 &&
                //         babylonMaterial.opacityTexture == null)
                //    {
                //        babylonMaterial.alpha = 0;
                //    }
                //}

                //babylonMaterial.bumpTexture = ExportTexture(stdMat, 8, out fresnelParameters, babylonScene);                   // Bump
                //babylonMaterial.reflectionTexture = ExportTexture(stdMat, 9, out fresnelParameters, babylonScene, true);       // Reflection
                //if (fresnelParameters != null)
                //{
                //    if (babylonMaterial.reflectionTexture == null)
                //    {
                //        RaiseWarning("Fallout cannot be used with reflection channel without a texture", 2);
                //    }
                //    else
                //    {
                //        babylonMaterial.reflectionFresnelParameters = fresnelParameters;
                //    }
                //}

                //// Constraints
                //if (babylonMaterial.diffuseTexture != null)
                //{
                //    babylonMaterial.diffuse = new[] { 1.0f, 1.0f, 1.0f };
                //}

                //if (babylonMaterial.emissiveTexture != null)
                //{
                //    babylonMaterial.emissive = new float[] { 0, 0, 0 };
                //}

                //if (babylonMaterial.opacityTexture != null && babylonMaterial.diffuseTexture != null &&
                //    babylonMaterial.diffuseTexture.name == babylonMaterial.opacityTexture.name &&
                //    babylonMaterial.diffuseTexture.hasAlpha && !babylonMaterial.opacityTexture.getAlphaFromRGB)
                //{
                //    // This is a alpha testing purpose
                //    babylonMaterial.opacityTexture = null;
                //    babylonMaterial.diffuseTexture.hasAlpha = true;
                //    RaiseWarning("Opacity texture was removed because alpha from diffuse texture can be use instead", 2);
                //    RaiseWarning("If you do not want this behavior, just set Alpha Source = None on your diffuse texture", 2);
                //}

                babylonScene.MaterialsList.Add(babylonMaterial);
            }
            //// TODO - Find another way to detect if material is physical
            //else if (materialNode.MaterialClass.ToLower() == "physical material" || // English
            //         materialNode.MaterialClass.ToLower() == "physisches material" || // German // TODO - check if translation is ok
            //         materialNode.MaterialClass.ToLower() == "matériau physique") // French
            //{
            //    var propertyContainer = materialNode.IPropertyContainer;

            //    var babylonMaterial = new BabylonPBRMetallicRoughnessMaterial
            //    {
            //        name = name,
            //        id = id
            //    };

            //    // --- Global ---

            //    // Alpha
            //    //var alphaFromXParency = 1.0f - materialNode.MaxMaterial.GetXParency(0, false);
            //    var alphaFromPropertyContainer = 1.0f - propertyContainer.GetFloatProperty(17);
            //    //RaiseMessage("alphaFromXParency=" + alphaFromXParency, 2);
            //    //RaiseMessage("alphaFromPropertyContainer=" + alphaFromPropertyContainer, 2);
            //    babylonMaterial.alpha = alphaFromPropertyContainer;

            //    babylonMaterial.baseColor = materialNode.MaxMaterial.GetDiffuse(0, false).ToArray();

            //    babylonMaterial.metallic = propertyContainer.GetFloatProperty(6);

            //    babylonMaterial.roughness = propertyContainer.GetFloatProperty(4);
            //    var invertRoughness = propertyContainer.GetBoolProperty(5);
            //    if (invertRoughness)
            //    {
            //        // Inverse roughness
            //        babylonMaterial.roughness = 1 - babylonMaterial.roughness;
            //    }

            //    // Self illumination is computed from emission color, luminance, temperature and weight
            //    babylonMaterial.emissive = materialNode.MaxMaterial.GetSelfIllumColorOn(0, false)
            //                                    ? materialNode.MaxMaterial.GetSelfIllumColor(0, false).ToArray()
            //                                    : materialNode.MaxMaterial.GetDiffuse(0, false).Scale(materialNode.MaxMaterial.GetSelfIllum(0, false));

            //    // --- Textures ---

            //    babylonMaterial.baseTexture = ExportBaseColorAlphaTexture(materialNode, babylonMaterial.baseColor, babylonMaterial.alpha, babylonScene, name);

            //    babylonMaterial.metallicRoughnessTexture = ExportMetallicRoughnessTexture(materialNode, babylonMaterial.metallic, babylonMaterial.roughness, babylonScene, name, invertRoughness);

            //    var normalMapAmount = propertyContainer.GetFloatProperty(91);
            //    babylonMaterial.normalTexture = ExportPBRTexture(materialNode, 30, babylonScene, normalMapAmount);

            //    babylonMaterial.emissiveTexture = ExportPBRTexture(materialNode, 17, babylonScene);

            //    // Use diffuse roughness map as ambient occlusion
            //    babylonMaterial.occlusionTexture = ExportPBRTexture(materialNode, 6, babylonScene);

            //    // Constraints
            //    if (babylonMaterial.baseTexture != null)
            //    {
            //        babylonMaterial.baseColor = new[] { 1.0f, 1.0f, 1.0f };
            //        babylonMaterial.alpha = 1.0f;
            //    }

            //    if (babylonMaterial.alpha != 1.0f || (babylonMaterial.baseTexture != null && babylonMaterial.baseTexture.hasAlpha))
            //    {
            //        babylonMaterial.transparencyMode = (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHABLEND;
            //    }

            //    if (babylonMaterial.emissiveTexture != null)
            //    {
            //        babylonMaterial.emissive = new[] { 1.0f, 1.0f, 1.0f };
            //    }

            //    if (babylonMaterial.metallicRoughnessTexture != null)
            //    {
            //        babylonMaterial.metallic = 1.0f;
            //        babylonMaterial.roughness = 1.0f;
            //    }

            //    babylonScene.MaterialsList.Add(babylonMaterial);
            //}
            else
            {
                RaiseWarning("Unsupported material type '" + materialObject.apiType + "' for material named '" + materialDependencyNode.name + "'", 2);
            }
        }

        public string GetMultimaterialUUID(List<MFnDependencyNode> materials)
        {
            List<MFnDependencyNode> materialsSorted = new List<MFnDependencyNode>(materials);
            materialsSorted.Sort(new MFnDependencyNodeComparer());

            string uuidConcatenated = "";
            bool isFirstTime = true;
            foreach(MFnDependencyNode material in materialsSorted)
            {
                if (!isFirstTime)
                {
                    uuidConcatenated += "_";
                }
                isFirstTime = false;

                uuidConcatenated += material.uuid().asString();
            }
            
            return uuidConcatenated;
        }

        public class MFnDependencyNodeComparer : IComparer<MFnDependencyNode>
        {
            public int Compare(MFnDependencyNode x, MFnDependencyNode y)
            {
                return x.uuid().asString().CompareTo(y.uuid().asString());
            }
        }

        public class MFnDependencyNodeEqualityComparer : IEqualityComparer<MFnDependencyNode>
        {
            public bool Equals(MFnDependencyNode x, MFnDependencyNode y)
            {
                return x.uuid().asString() == y.uuid().asString();
            }

            public int GetHashCode(MFnDependencyNode obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}

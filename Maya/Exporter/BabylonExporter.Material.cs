using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        private void ExportMultiMaterial(string uuidMultiMaterial, List<MFnDependencyNode> materials, BabylonScene babylonScene, bool fullPBR)
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
                    ExportMaterial(subMat, babylonScene, fullPBR);
                }
            }
            babylonMultimaterial.materials = uuids.ToArray();

            babylonScene.MultiMaterialsList.Add(babylonMultimaterial);
        }

        private void ExportMaterial(MFnDependencyNode materialDependencyNode, BabylonScene babylonScene, bool fullPBR)
        {
            MObject materialObject = materialDependencyNode.objectProperty;
            var name = materialDependencyNode.name;
            var id = materialDependencyNode.uuid().asString();

            RaiseMessage(name, 1);
            RaiseMessage(materialObject.apiType.ToString(), 1);

            RaiseVerbose("materialObject.hasFn(MFn.Type.kBlinn)=" + materialObject.hasFn(MFn.Type.kBlinn), 2);
            RaiseVerbose("materialObject.hasFn(MFn.Type.kPhong)=" + materialObject.hasFn(MFn.Type.kPhong), 2);
            RaiseVerbose("materialObject.hasFn(MFn.Type.kPhongExplorer)=" + materialObject.hasFn(MFn.Type.kPhongExplorer), 2);

            Print(materialDependencyNode, 2, "Print ExportMaterial materialDependencyNode");

            // Retreive Babylon Material dependency node
            MFnDependencyNode babylonAttributesDependencyNode = getBabylonMaterialNode(materialDependencyNode);

            // Standard material
            if (materialObject.hasFn(MFn.Type.kLambert))
            {
                if (materialObject.hasFn(MFn.Type.kBlinn))
                {
                    RaiseMessage("Blinn shader", 2);
                }
                else if (materialObject.hasFn(MFn.Type.kPhong))
                {
                    RaiseMessage("Phong shader", 2);
                }
                else if (materialObject.hasFn(MFn.Type.kPhongExplorer))
                {
                    RaiseMessage("Phong E shader", 2);
                }
                else
                {
                    RaiseMessage("Lambert shader", 2);
                }

                var lambertShader = new MFnLambertShader(materialObject);

                RaiseVerbose("typeId=" + lambertShader.typeId, 2);
                RaiseVerbose("typeName=" + lambertShader.typeName, 2);
                RaiseVerbose("color=" + lambertShader.color.toString(), 2);
                RaiseVerbose("transparency=" + lambertShader.transparency.toString(), 2);
                RaiseVerbose("ambientColor=" + lambertShader.ambientColor.toString(), 2);
                RaiseVerbose("incandescence=" + lambertShader.incandescence.toString(), 2);
                RaiseVerbose("diffuseCoeff=" + lambertShader.diffuseCoeff, 2);
                RaiseVerbose("translucenceCoeff=" + lambertShader.translucenceCoeff, 2);

                BabylonStandardMaterial babylonMaterial = new BabylonStandardMaterial(id)
                {
                    name = name,
                    diffuse = lambertShader.color.toArrayRGB()
                };

                // User custom attributes
                babylonMaterial.metadata = ExportCustomAttributeFromMaterial(babylonMaterial);

                bool isTransparencyModeFromBabylonMaterialNode = false;
                if (babylonAttributesDependencyNode != null)
                {
                    // Common attributes
                    ExportCommonBabylonAttributes(babylonAttributesDependencyNode, babylonMaterial);

                    isTransparencyModeFromBabylonMaterialNode = babylonAttributesDependencyNode.hasAttribute("babylonTransparencyMode");
                }

                // Maya ambient <=> babylon emissive
                babylonMaterial.emissive = lambertShader.ambientColor.toArrayRGB();
                babylonMaterial.linkEmissiveWithDiffuse = true; // Incandescence (or Illumination) is not exported

                if (isTransparencyModeFromBabylonMaterialNode == false || babylonMaterial.transparencyMode != 0)
                {
                    // If transparency is not a shade of grey (shade of grey <=> R=G=B)
                    if (lambertShader.transparency[0] != lambertShader.transparency[1] ||
                        lambertShader.transparency[0] != lambertShader.transparency[2])
                    {
                        RaiseWarning("Transparency color is not a shade of grey. Only it's R channel is used.", 2);
                    }
                    // Convert transparency to opacity
                    babylonMaterial.alpha = 1.0f - lambertShader.transparency[0];
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

                        float glossiness = (float)Math.Log(phongShader.cosPower, 2) * 10;
                        babylonMaterial.specularPower = glossiness / 100 * 256;
                    }
                    else if (materialObject.hasFn(MFn.Type.kPhongExplorer))
                    {
                        MFnPhongEShader phongEShader = new MFnPhongEShader(materialObject);
                        // No use of phongE.whiteness and phongE.highlightSize
                        babylonMaterial.specularPower = (1.0f - phongEShader.roughness) * 256;
                    }
                    else
                    {
                        RaiseWarning("Unknown reflect shader type: " + reflectShader.typeName + ". Specular power is default 64. Consider using a Blinn or Phong shader instead.", 2);
                    }
                }

                // TODO
                //babylonMaterial.wireframe = stdMat.Wire;

                // --- Textures ---

                if (exportParameters.exportTextures)
                {
                    babylonMaterial.diffuseTexture = ExportTexture(materialDependencyNode, "color", babylonScene);
                    babylonMaterial.emissiveTexture = ExportTexture(materialDependencyNode, "ambientColor", babylonScene); // Maya ambient <=> babylon emissive
                    babylonMaterial.bumpTexture = ExportTexture(materialDependencyNode, "normalCamera", babylonScene);
                    if (isTransparencyModeFromBabylonMaterialNode == false || babylonMaterial.transparencyMode != 0)
                    {
                        babylonMaterial.opacityTexture = ExportTexture(materialDependencyNode, "transparency", babylonScene, false, true);
                    }
                    if (materialObject.hasFn(MFn.Type.kReflect))
                    {
                        babylonMaterial.specularTexture = ExportTexture(materialDependencyNode, "specularColor", babylonScene);
                        babylonMaterial.reflectionTexture = ExportTexture(materialDependencyNode, "reflectedColor", babylonScene, true, false, true);
                    }
                }

                if (isTransparencyModeFromBabylonMaterialNode == false && (babylonMaterial.alpha != 1.0f || (babylonMaterial.diffuseTexture != null && babylonMaterial.diffuseTexture.hasAlpha) || babylonMaterial.opacityTexture != null))
                {
                    babylonMaterial.transparencyMode = (int)BabylonMaterial.TransparencyMode.ALPHABLEND;
                }

                // Constraints
                if (babylonMaterial.diffuseTexture != null)
                {
                    babylonMaterial.diffuse = new[] { 1.0f, 1.0f, 1.0f };
                }

                if (babylonMaterial.emissiveTexture != null)
                {
                    babylonMaterial.emissive = new float[] { 0, 0, 0 };
                }

                if (babylonMaterial.transparencyMode == (int)BabylonMaterial.TransparencyMode.ALPHATEST)
                {
                    // Set the alphaCutOff value explicitely to avoid different interpretations on different engines
                    // Use the glTF default value rather than the babylon one
                    babylonMaterial.alphaCutOff = 0.5f;
                }

                if (babylonAttributesDependencyNode == null)
                {
                    // Create Babylon Material dependency node
                    babylonStandardMaterialNode.Create(materialDependencyNode);

                    // Retreive Babylon Material dependency node
                    babylonAttributesDependencyNode = getBabylonMaterialNode(materialDependencyNode);
                }

                if (babylonAttributesDependencyNode != null)
                {
                    // Ensure all attributes are setup
                    babylonStandardMaterialNode.Init(babylonAttributesDependencyNode, babylonMaterial);

                    RaiseVerbose("Babylon Attributes of " + babylonAttributesDependencyNode.name, 2);
                    
                    // Common attributes
                    ExportCommonBabylonAttributes(babylonAttributesDependencyNode, babylonMaterial);

                    // Special treatment for Unlit
                    if (babylonMaterial.isUnlit)
                    {
                        if ((babylonMaterial.emissive != null && (babylonMaterial.emissive[0] != 0 || babylonMaterial.emissive[1] != 0 || babylonMaterial.emissive[2] != 0))
                            || (babylonMaterial.emissiveTexture != null)
                            || (babylonMaterial.emissiveFresnelParameters != null))
                        {
                            RaiseWarning("Material is unlit. Emission is discarded and replaced by diffuse.", 2);
                        }
                        // Copy diffuse to emissive
                        babylonMaterial.emissive = babylonMaterial.diffuse;
                        babylonMaterial.emissiveTexture = babylonMaterial.diffuseTexture;
                        babylonMaterial.emissiveFresnelParameters = babylonMaterial.diffuseFresnelParameters;

                        babylonMaterial.disableLighting = true;
                        babylonMaterial.linkEmissiveWithDiffuse = false;
                    }
                    // Special treatment for "Alpha test" transparency mode
                    if (exportParameters.exportTextures
                        && babylonMaterial.transparencyMode == (int)BabylonMaterial.TransparencyMode.ALPHATEST 
                        && ((babylonMaterial.diffuseTexture != null && babylonMaterial.opacityTexture != null && babylonMaterial.diffuseTexture.originalPath != babylonMaterial.opacityTexture.originalPath)
                            || (babylonMaterial.diffuseTexture == null && babylonMaterial.opacityTexture != null)))
                    {
                        // Base color and alpha files need to be merged into a single file
                        Color defaultColor = Color.FromArgb((int)(babylonMaterial.diffuse[0] * 255), (int)(babylonMaterial.diffuse[1] * 255), (int)(babylonMaterial.diffuse[2] * 255));
                        MFnDependencyNode baseColorTextureDependencyNode = getTextureDependencyNode(materialDependencyNode, "color");
                        MFnDependencyNode opacityTextureDependencyNode = getTextureDependencyNode(materialDependencyNode, "transparency");
                        babylonMaterial.diffuseTexture = ExportBaseColorAlphaTexture(baseColorTextureDependencyNode, opacityTextureDependencyNode, babylonScene, name, defaultColor, babylonMaterial.alpha);
                        babylonMaterial.opacityTexture = null;
                        babylonMaterial.alpha = 1.0f;
                    }
                }

                babylonScene.MaterialsList.Add(babylonMaterial);
            }
            // Stingray PBS material
            else if (isStingrayPBSMaterial(materialDependencyNode))
            {
                RaiseMessage("Stingray shader", 2);

                var babylonMaterial = new BabylonPBRMetallicRoughnessMaterial(id)
                {
                    name = name
                };

                if (babylonAttributesDependencyNode != null)
                {
                    // Common attributes
                    ExportCommonBabylonAttributes(babylonAttributesDependencyNode, babylonMaterial);
                }

                // --- Global ---

                // Color3
                babylonMaterial.baseColor = materialDependencyNode.findPlug("base_color").asFloatArray();

                // Alpha
                string opacityAttributeName = "opacity";
                if (materialDependencyNode.hasAttribute(opacityAttributeName))
                {
                    float opacityAttributeValue = materialDependencyNode.findPlug(opacityAttributeName).asFloat();
                    babylonMaterial.alpha = 1.0f - opacityAttributeValue;
                }

                // Metallic & roughness
                babylonMaterial.metallic = materialDependencyNode.findPlug("metallic").asFloat();
                babylonMaterial.roughness = materialDependencyNode.findPlug("roughness").asFloat();

                // Emissive
                float emissiveIntensity = materialDependencyNode.findPlug("emissive_intensity").asFloat();
                // Factor emissive color with emissive intensity
                emissiveIntensity = Tools.Clamp(emissiveIntensity, 0f, 1f);
                babylonMaterial.emissive = materialDependencyNode.findPlug("emissive").asFloatArray().Multiply(emissiveIntensity);

                // --- Textures ---

                bool useColorMap = false;
                bool useOpacityMap = false;
                bool useMetallicMap = false;
                bool useRoughnessMap = false;
                bool useEmissiveMap = false;

                if (exportParameters.exportTextures)
                {
                    // Base color & alpha
                    useColorMap = materialDependencyNode.findPlug("use_color_map").asBool();
                    useOpacityMap = false;
                    string useOpacityMapAttributeName = "use_opacity_map";
                    if (materialDependencyNode.hasAttribute(useOpacityMapAttributeName))
                    {
                        useOpacityMap = materialDependencyNode.findPlug(useOpacityMapAttributeName).asBool();
                    }
                    if (materialDependencyNode.hasAttribute("mask_threshold")) // Preset "Masked"
                    {
                        if (useColorMap && useOpacityMap)
                        {
                            // Texture is assumed to be already merged
                            babylonMaterial.baseTexture = ExportTexture(materialDependencyNode, "TEX_color_map", babylonScene, false, true);
                        }
                        else if (useColorMap || useOpacityMap)
                        {
                            // Merge Diffuse and Mask
                            Color defaultColor = Color.FromArgb((int)(babylonMaterial.baseColor[0] * 255), (int)(babylonMaterial.baseColor[1] * 255), (int)(babylonMaterial.baseColor[2] * 255));
                            // In Maya, a Masked StingrayPBS material without opacity or mask textures is counted as being fully transparent
                            // Such material is visible only when the mask threshold is set to 0
                            float defaultOpacity = 0;

                            // Either use the color map
                            MFnDependencyNode baseColorTextureDependencyNode = null;
                            if (useColorMap)
                            {
                                baseColorTextureDependencyNode = getTextureDependencyNode(materialDependencyNode, "TEX_color_map");
                            }
                            // Or the opacity map
                            MFnDependencyNode opacityTextureDependencyNode = null;
                            if (useOpacityMap)
                            {
                                opacityTextureDependencyNode = getTextureDependencyNode(materialDependencyNode, "TEX_color_map");
                            }

                            // Merge default value and texture
                            babylonMaterial.baseTexture = ExportBaseColorAlphaTexture(baseColorTextureDependencyNode, opacityTextureDependencyNode, babylonScene, babylonMaterial.name, defaultColor, defaultOpacity);
                        }
                        else
                        {
                            // In Maya, a Masked StingrayPBS material without opacity or mask textures is counted as being fully transparent
                            // Such material is visible only when the mask threshold is set to 0
                            babylonMaterial.alpha = 0;
                        }
                    }
                    else
                    {
                        if (useColorMap || useOpacityMap)
                        {
                            // Force non use map to default value
                            // Ex: if useOpacityMap == false, force alpha = 255 for all pixels.
                            babylonMaterial.baseTexture = ExportTexture(materialDependencyNode, "TEX_color_map", babylonScene, false, useOpacityMap);
                        }
                    }

                    if (babylonMaterial.transparencyMode == (int)BabylonMaterial.TransparencyMode.ALPHATEST)
                    {
                        // Set the alphaCutOff value explicitely to avoid different interpretations on different engines
                        // Use the glTF default value rather than the babylon one
                        babylonMaterial.alphaCutOff = 0.5f;
                    }

                    // Alpha cuttoff
                    if (materialDependencyNode.hasAttribute("mask_threshold")) // Preset "Masked"
                    {
                        babylonMaterial.alphaCutOff = materialDependencyNode.findPlug("mask_threshold").asFloat();
                    }

                    // Metallic, roughness, ambient occlusion
                    useMetallicMap = materialDependencyNode.findPlug("use_metallic_map").asBool();
                    useRoughnessMap = materialDependencyNode.findPlug("use_roughness_map").asBool();
                    string useAOMapAttributeName = "use_ao_map";
                    bool useAOMap = materialDependencyNode.hasAttribute(useAOMapAttributeName) && materialDependencyNode.findPlug(useAOMapAttributeName).asBool();

                    MFnDependencyNode metallicTextureDependencyNode = useMetallicMap ? getTextureDependencyNode(materialDependencyNode, "TEX_metallic_map") : null;
                    MFnDependencyNode roughnessTextureDependencyNode = useRoughnessMap ? getTextureDependencyNode(materialDependencyNode, "TEX_roughness_map") : null;
                    MFnDependencyNode ambientOcclusionTextureDependencyNode = useAOMap ? getTextureDependencyNode(materialDependencyNode, "TEX_ao_map") : null;

                    // Check if MR or ORM textures are already merged
                    bool areTexturesAlreadyMerged = false;
                    if (metallicTextureDependencyNode != null && roughnessTextureDependencyNode != null)
                    {
                        string sourcePathMetallic = getSourcePathFromFileTexture(metallicTextureDependencyNode);
                        string sourcePathRoughness = getSourcePathFromFileTexture(roughnessTextureDependencyNode);

                        if (sourcePathMetallic == sourcePathRoughness)
                        {
                            if (ambientOcclusionTextureDependencyNode != null)
                            {
                                string sourcePathAmbientOcclusion = getSourcePathFromFileTexture(ambientOcclusionTextureDependencyNode);
                                if (sourcePathMetallic == sourcePathAmbientOcclusion)
                                {
                                    // Metallic, roughness and ambient occlusion are already merged
                                    RaiseVerbose("Metallic, roughness and ambient occlusion are already merged", 2);
                                    BabylonTexture ormTexture = ExportTexture(metallicTextureDependencyNode, babylonScene);
                                    babylonMaterial.metallicRoughnessTexture = ormTexture;
                                    babylonMaterial.occlusionTexture = ormTexture;
                                    areTexturesAlreadyMerged = true;
                                }
                            }
                            else
                            {
                                // Metallic and roughness are already merged
                                RaiseVerbose("Metallic and roughness are already merged", 2);
                                BabylonTexture ormTexture = ExportTexture(metallicTextureDependencyNode, babylonScene);
                                babylonMaterial.metallicRoughnessTexture = ormTexture;
                                areTexturesAlreadyMerged = true;
                            }
                        }
                    }
                    if (areTexturesAlreadyMerged == false)
                    {
                        if (metallicTextureDependencyNode != null || roughnessTextureDependencyNode != null)
                        {
                            // Merge metallic, roughness and ambient occlusion
                            RaiseVerbose("Merge metallic, roughness and ambient occlusion", 2);
                            BabylonTexture ormTexture = ExportORMTexture(babylonScene, metallicTextureDependencyNode, roughnessTextureDependencyNode, ambientOcclusionTextureDependencyNode, babylonMaterial.metallic, babylonMaterial.roughness);
                            babylonMaterial.metallicRoughnessTexture = ormTexture;

                            if (ambientOcclusionTextureDependencyNode != null)
                            {
                                babylonMaterial.occlusionTexture = ormTexture;
                            }
                        }
                        else if (ambientOcclusionTextureDependencyNode != null)
                        {
                            // Simply export occlusion texture
                            RaiseVerbose("Simply export occlusion texture", 2);
                            babylonMaterial.occlusionTexture = ExportTexture(ambientOcclusionTextureDependencyNode, babylonScene);
                        }
                    }

                    // Normal
                    if (materialDependencyNode.findPlug("use_normal_map").asBool())
                    {
                        babylonMaterial.normalTexture = ExportTexture(materialDependencyNode, "TEX_normal_map", babylonScene);
                    }

                    // Emissive
                    useEmissiveMap = materialDependencyNode.findPlug("use_emissive_map").asBool();
                    if (useEmissiveMap)
                    {
                        babylonMaterial.emissiveTexture = ExportTexture(materialDependencyNode, "TEX_emissive_map", babylonScene, false, false, false, emissiveIntensity);
                    }
                }

                // Constraints
                if (useColorMap)
                {
                    babylonMaterial.baseColor = new[] { 1.0f, 1.0f, 1.0f };
                }
                if (useOpacityMap)
                {
                    babylonMaterial.alpha = 1.0f;
                }
                if (babylonMaterial.alpha != 1.0f || (babylonMaterial.baseTexture != null && babylonMaterial.baseTexture.hasAlpha))
                {
                    if (materialDependencyNode.hasAttribute("mask_threshold"))
                    {
                        babylonMaterial.transparencyMode = (int)BabylonMaterial.TransparencyMode.ALPHATEST;
                    }
                    else
                    {
                        babylonMaterial.transparencyMode = (int)BabylonMaterial.TransparencyMode.ALPHABLEND;
                    }
                }
                if (useMetallicMap)
                {
                    babylonMaterial.metallic = 1.0f;
                }
                if (useRoughnessMap)
                {
                    babylonMaterial.roughness = 1.0f;
                }
                if (useEmissiveMap)
                {
                    babylonMaterial.emissive = new[] { 1.0f, 1.0f, 1.0f };
                }

                // User custom attributes
                babylonMaterial.metadata = ExportCustomAttributeFromMaterial(babylonMaterial);

                if (babylonAttributesDependencyNode == null)
                {
                    // Create Babylon Material dependency node
                    babylonStingrayPBSMaterialNode.Create(materialDependencyNode);

                    // Retreive Babylon Material dependency node
                    babylonAttributesDependencyNode = getBabylonMaterialNode(materialDependencyNode);
                }

                if (babylonAttributesDependencyNode != null)
                {
                    // Ensure all attributes are setup
                    babylonStingrayPBSMaterialNode.Init(babylonAttributesDependencyNode, babylonMaterial);

                    RaiseVerbose("Babylon Attributes of " + babylonAttributesDependencyNode.name, 2);

                    // Common attributes
                    ExportCommonBabylonAttributes(babylonAttributesDependencyNode, babylonMaterial);
                    babylonMaterial.doubleSided = !babylonMaterial.backFaceCulling;
                    babylonMaterial._unlit = babylonMaterial.isUnlit;

                    // Update displayed Transparency mode value based on StingrayPBS preset material
                    int babylonTransparencyMode = 0;
                    if (materialDependencyNode.hasAttribute("mask_threshold"))
                    {
                        babylonTransparencyMode = 1;
                    }
                    else if (materialDependencyNode.hasAttribute("use_opacity_map"))
                    {
                        babylonTransparencyMode = 2;
                    }
                    babylonStingrayPBSMaterialNode.setAttributeValue(babylonAttributesDependencyNode.name + ".babylonTransparencyMode", babylonTransparencyMode);
                }

                babylonScene.MaterialsList.Add(babylonMaterial);
            }
            // Arnold Ai Standard Surface
            else if (isAiStandardSurface(materialDependencyNode))
            {
                RaiseMessage("Ai Standard Surface shader", 2);

                var babylonMaterial = new BabylonPBRMetallicRoughnessMaterial(id)
                {
                    name = name
                };

                // User custom attributes
                babylonMaterial.metadata = ExportCustomAttributeFromMaterial(babylonMaterial);

                // --- Global ---
                bool isTransparencyModeFromBabylonMaterialNode = false;
                if (babylonAttributesDependencyNode != null)
                {
                    // Common attributes
                    ExportCommonBabylonAttributes(babylonAttributesDependencyNode, babylonMaterial);

                    isTransparencyModeFromBabylonMaterialNode = babylonAttributesDependencyNode.hasAttribute("babylonTransparencyMode");
                }

                // Color3
                float baseWeight = materialDependencyNode.findPlug("base").asFloat();
                float[] baseColor = materialDependencyNode.findPlug("baseColor").asFloatArray();
                babylonMaterial.baseColor = baseColor.Multiply(baseWeight);

                // Alpha
                MaterialDuplicationData materialDuplicationData = materialDuplicationDatas[id];
                // If at least one mesh is Transparent and is using this material either directly or as a sub material
                if ((isTransparencyModeFromBabylonMaterialNode == false || babylonMaterial.transparencyMode != 0) && materialDuplicationData.isArnoldTransparent())
                {
                    float[] opacityAttributeValue = materialDependencyNode.findPlug("opacity").asFloatArray();
                    babylonMaterial.alpha = opacityAttributeValue[0];
                }
                else
                {
                    // Do not bother about alpha
                    babylonMaterial.alpha = 1.0f;
                }

                // Metallic & roughness
                babylonMaterial.metallic = materialDependencyNode.findPlug("metalness").asFloat();
                babylonMaterial.roughness = materialDependencyNode.findPlug("specularRoughness").asFloat();

                // Emissive
                float emissionWeight = materialDependencyNode.findPlug("emission").asFloat();
                babylonMaterial.emissive = materialDependencyNode.findPlug("emissionColor").asFloatArray().Multiply(emissionWeight);

                var list = new List<string>();

                for (int i = 0; i < materialDependencyNode.attributeCount; i++)
                {
                    var attr = materialDependencyNode.attribute((uint)i);
                    var plug = materialDependencyNode.findPlug(attr);
                    //string aliasName;
                    //materialDependencyNode.getPlugsAlias(plug, out aliasName);
                    System.Diagnostics.Debug.WriteLine(plug.name + i.ToString());
                }

                // --- Clear Coat ---
                float coatWeight = materialDependencyNode.findPlug("coat").asFloat();
                MFnDependencyNode intensityCoatTextureDependencyNode = getTextureDependencyNode(materialDependencyNode, "coat");
                if (coatWeight > 0.0f || intensityCoatTextureDependencyNode != null)
                {
                    babylonMaterial.clearCoat.isEnabled = true;
                    babylonMaterial.clearCoat.indexOfRefraction = materialDependencyNode.findPlug("coatIOR").asFloat();

                    var coatRoughness = materialDependencyNode.findPlug("coatRoughness").asFloat();
                    MFnDependencyNode roughnessCoatTextureDependencyNode = getTextureDependencyNode(materialDependencyNode, "coatRoughness");
                    var coatTexture = exportParameters.exportTextures ? ExportCoatTexture(intensityCoatTextureDependencyNode, roughnessCoatTextureDependencyNode, babylonScene, name, coatWeight, coatRoughness) : null;
                    if (coatTexture != null)
                    {
                        babylonMaterial.clearCoat.texture = coatTexture;
                        babylonMaterial.clearCoat.roughness = 1.0f;
                        babylonMaterial.clearCoat.intensity = 1.0f;
                    }
                    else
                    {
                        babylonMaterial.clearCoat.intensity = coatWeight;
                        babylonMaterial.clearCoat.roughness = coatRoughness;
                    }

                    float[] coatColor = materialDependencyNode.findPlug("coatColor").asFloatArray();
                    if (coatColor[0] != 1.0f || coatColor[1] != 1.0f || coatColor[2] != 1.0f)
                    {
                        babylonMaterial.clearCoat.isTintEnabled = true;
                        babylonMaterial.clearCoat.tintColor = coatColor;
                    }

                    babylonMaterial.clearCoat.tintTexture = exportParameters.exportTextures ? ExportTexture(materialDependencyNode, "coatColor", babylonScene) : null;
                    if (babylonMaterial.clearCoat.tintTexture != null)
                    {
                        babylonMaterial.clearCoat.tintColor = new[] { 1.0f, 1.0f, 1.0f };
                        babylonMaterial.clearCoat.isTintEnabled = true;
                    }

                    // EyeBall deduction...
                    babylonMaterial.clearCoat.tintThickness = 0.65f;

                    babylonMaterial.clearCoat.bumpTexture = exportParameters.exportTextures ? ExportTexture(materialDependencyNode, "coatNormal", babylonScene) : null;
                }

                // --- Textures ---

                if (exportParameters.exportTextures)
                {
                    // Base color & alpha
                    if ((isTransparencyModeFromBabylonMaterialNode == false || babylonMaterial.transparencyMode != 0) && materialDuplicationData.isArnoldTransparent())
                    {
                        MFnDependencyNode baseColorTextureDependencyNode = getTextureDependencyNode(materialDependencyNode, "baseColor");
                        MFnDependencyNode opacityTextureDependencyNode = getTextureDependencyNode(materialDependencyNode, "opacity");
                        if (baseColorTextureDependencyNode != null && opacityTextureDependencyNode != null &&
                            getSourcePathFromFileTexture(baseColorTextureDependencyNode) == getSourcePathFromFileTexture(opacityTextureDependencyNode))
                        {
                            // If the same file is used for base color and opacity
                            // Base color and alpha are already merged into a single file
                            babylonMaterial.baseTexture = ExportTexture(baseColorTextureDependencyNode, babylonScene, false, true);
                        }
                        else
                        {
                            // Base color and alpha files need to be merged into a single file
                            Color _baseColor = Color.FromArgb((int)(baseColor[0] * 255), (int)(baseColor[1] * 255), (int)(baseColor[2] * 255));
                            babylonMaterial.baseTexture = ExportBaseColorAlphaTexture(baseColorTextureDependencyNode, opacityTextureDependencyNode, babylonScene, name, _baseColor, babylonMaterial.alpha);
                        }
                    }
                    else
                    {
                        // Base color only
                        // Do not bother about alpha
                        babylonMaterial.baseTexture = ExportTexture(materialDependencyNode, "baseColor", babylonScene);
                    }

                    // Metallic & roughness
                    MFnDependencyNode metallicTextureDependencyNode = getTextureDependencyNode(materialDependencyNode, "metalness");
                    MFnDependencyNode roughnessTextureDependencyNode = getTextureDependencyNode(materialDependencyNode, "specularRoughness");
                    if (metallicTextureDependencyNode != null && roughnessTextureDependencyNode != null &&
                        getSourcePathFromFileTexture(metallicTextureDependencyNode) == getSourcePathFromFileTexture(roughnessTextureDependencyNode))
                    {
                        // If the same file is used for metallic and roughness
                        // Then we assume it's an ORM file (Red=Occlusion, Green=Roughness, Blue=Metallic)

                        // Metallic and roughness are already merged into a single file
                        babylonMaterial.metallicRoughnessTexture = ExportTexture(metallicTextureDependencyNode, babylonScene);

                        // Use same file for Ambient occlusion
                        babylonMaterial.occlusionTexture = babylonMaterial.metallicRoughnessTexture;
                    }
                    else
                    {
                        // Metallic and roughness files need to be merged into a single file
                        // Occlusion texture is not exported since aiStandardSurface material doesn't provide input for it
                        babylonMaterial.metallicRoughnessTexture = ExportORMTexture(babylonScene, metallicTextureDependencyNode, roughnessTextureDependencyNode, null, babylonMaterial.metallic, babylonMaterial.roughness);
                    }

                    // Normal
                    babylonMaterial.normalTexture = ExportTexture(materialDependencyNode, "normalCamera", babylonScene);

                    // Emissive
                    babylonMaterial.emissiveTexture = ExportTexture(materialDependencyNode, "emissionColor", babylonScene);

                    // Constraints
                    if (babylonMaterial.baseTexture != null)
                    {
                        babylonMaterial.baseColor = new[] { baseWeight, baseWeight, baseWeight };
                        babylonMaterial.alpha = 1.0f;
                    }
                    if (babylonMaterial.metallicRoughnessTexture != null)
                    {
                        babylonMaterial.metallic = 1.0f;
                        babylonMaterial.roughness = 1.0f;
                    }
                    if (babylonMaterial.emissiveTexture != null)
                    {
                        babylonMaterial.emissive = new[] { emissionWeight, emissionWeight, emissionWeight };
                    }
                }

                // If this material is containing alpha data
                if (babylonMaterial.alpha != 1.0f || (babylonMaterial.baseTexture != null && babylonMaterial.baseTexture.hasAlpha))
                {
                    if (isTransparencyModeFromBabylonMaterialNode == false)
                    {
                        babylonMaterial.transparencyMode = (int)BabylonMaterial.TransparencyMode.ALPHABLEND;
                    }

                    // If this material is assigned to both Transparent and Opaque meshes (either directly or as a sub material)
                    if (materialDuplicationData.isDuplicationRequired())
                    {
                        // Duplicate material
                        BabylonPBRMetallicRoughnessMaterial babylonMaterialCloned = DuplicateMaterial(babylonMaterial, materialDuplicationData);

                        // Store duplicated material too
                        babylonScene.MaterialsList.Add(babylonMaterialCloned);
                    }
                }

                if (babylonMaterial.transparencyMode == (int)BabylonMaterial.TransparencyMode.ALPHATEST)
                {
                    // Set the alphaCutOff value explicitely to avoid different interpretations on different engines
                    // Use the glTF default value rather than the babylon one
                    babylonMaterial.alphaCutOff = 0.5f;
                }

                if (babylonAttributesDependencyNode == null)
                {
                    // Create Babylon Material dependency node
                    babylonAiStandardSurfaceMaterialNode.Create(materialDependencyNode);

                    // Retreive Babylon Material dependency node
                    babylonAttributesDependencyNode = getBabylonMaterialNode(materialDependencyNode);
                }

                if (babylonAttributesDependencyNode != null)
                {
                    // Ensure all attributes are setup
                    babylonAiStandardSurfaceMaterialNode.Init(babylonAttributesDependencyNode, babylonMaterial);

                    RaiseVerbose("Babylon Attributes of " + babylonAttributesDependencyNode.name, 2);

                    // Common attributes
                    ExportCommonBabylonAttributes(babylonAttributesDependencyNode, babylonMaterial);
                    babylonMaterial.doubleSided = !babylonMaterial.backFaceCulling;
                    babylonMaterial._unlit = babylonMaterial.isUnlit;
                }

                if (fullPBR)
                {
                    var fullPBRMaterial = new BabylonPBRMaterial(babylonMaterial);
                    babylonScene.MaterialsList.Add(fullPBRMaterial);
                }
                else
                {
                    babylonScene.MaterialsList.Add(babylonMaterial);
                }
            }
            else
            {
                RaiseWarning("Unsupported material type '" + materialObject.apiType + "' for material named '" + materialDependencyNode.name + "'", 2);
            }
        }

        private void ExportCommonBabylonAttributes0(MFnDependencyNode babylonAttributesDependencyNode, BabylonMaterial babylonMaterial)
        {
            // Backface Culling
            if (babylonAttributesDependencyNode.hasAttribute("babylonBackfaceCulling"))
            {
                bool v = babylonAttributesDependencyNode.findPlug("babylonBackfaceCulling").asBool();
                RaiseVerbose($"backfaceCulling={v}", 3);
                babylonMaterial.backFaceCulling = v;
            }
            // unlit
            if (babylonAttributesDependencyNode.hasAttribute("babylonUnlit"))
            {
                bool v = babylonAttributesDependencyNode.findPlug("babylonUnlit").asBool();
                RaiseVerbose($"isUnlit={v}", 3);
                babylonMaterial.isUnlit = v;
            }
            // max light
            if (babylonAttributesDependencyNode.hasAttribute("babylonMaxSimultaneousLights"))
            {
                int v = babylonAttributesDependencyNode.findPlug("babylonMaxSimultaneousLights").asInt();
                RaiseVerbose($"maxSimultaneousLights={v}", 3);
                babylonMaterial.maxSimultaneousLights = v;
            }
        }

        private void ExportCommonBabylonAttributes(MFnDependencyNode babylonAttributesDependencyNode, BabylonStandardMaterial babylonMaterial)
        {
            ExportCommonBabylonAttributes0(babylonAttributesDependencyNode, babylonMaterial);
            
            if (babylonAttributesDependencyNode.hasAttribute("babylonTransparencyMode"))
            {
                int v = babylonAttributesDependencyNode.findPlug("babylonTransparencyMode").asInt();
                RaiseVerbose($"babylonTransparencyMode={v}", 3);
                babylonMaterial.transparencyMode = v;
            }
        }

        private void ExportCommonBabylonAttributes(MFnDependencyNode babylonAttributesDependencyNode, BabylonPBRMetallicRoughnessMaterial babylonMaterial)
        {

            ExportCommonBabylonAttributes0(babylonAttributesDependencyNode, babylonMaterial);

            if (babylonAttributesDependencyNode.hasAttribute("babylonTransparencyMode"))
            {
                int v = babylonAttributesDependencyNode.findPlug("babylonTransparencyMode").asInt();
                RaiseVerbose($"babylonTransparencyMode={v}", 3);
                babylonMaterial.transparencyMode = v;
            }
        }

        private bool isStingrayPBSMaterial(MFnDependencyNode materialDependencyNode)
        {
            // TODO - Find a better way to identify material type
            string graphAttribute = "graph";
            if (materialDependencyNode.hasAttribute(graphAttribute))
            {
                string graphValue = materialDependencyNode.findPlug(graphAttribute).asString();
                return graphValue.Contains("stingray");
            }
            else
            {
                return false;
            }
        }

        private bool isAiStandardSurface(MFnDependencyNode materialDependencyNode)
        {
            // TODO - Find a better way to identify material type
            return materialDependencyNode.hasAttribute("baseColor") &&
                materialDependencyNode.hasAttribute("metalness") &&
                materialDependencyNode.hasAttribute("normalCamera") &&
                materialDependencyNode.hasAttribute("specularRoughness") &&
                materialDependencyNode.hasAttribute("emissionColor");
        }

        private MFnDependencyNode getBabylonMaterialNode(MFnDependencyNode materialDependencyNode)
        {
            // Retreive connection
            MPlug connectionOutColor = materialDependencyNode.getConnection("outColor");
            MPlugArray destinations = new MPlugArray();
            connectionOutColor.destinations(destinations);

            // Retreive all Babylon Attributes dependency nodes
            List<MFnDependencyNode> babylonAttributesNodes = new List<MFnDependencyNode>();
            
            foreach (MPlug destination in destinations)
            {
                MObject destinationObject = destination.node;

                if (destinationObject != null && destinationObject.hasFn(MFn.Type.kPluginHardwareShader))
                {
                    MFnDependencyNode babylonAttributesDependencyNode = new MFnDependencyNode(destinationObject);

                    if (babylonAttributesDependencyNode.typeId.id == babylonStandardMaterialNode.id
                    || babylonAttributesDependencyNode.typeId.id == babylonStingrayPBSMaterialNode.id
                    || babylonAttributesDependencyNode.typeId.id == babylonAiStandardSurfaceMaterialNode.id)
                    {
                        babylonAttributesNodes.Add(babylonAttributesDependencyNode);
                    }
                }
            }

            if(babylonAttributesNodes.Count == 0)
            {
                return null;
            }
            else
            {
                // Occurs only if the user explicitly create a connection with several babylon attributes nodes.
                if(babylonAttributesNodes.Count > 1)
                {
                    RaiseWarning("Babylon attributes node attached to a material should be unique. The first one is going to be used.", 2);
                }

                // Return the first shading engine node
                return babylonAttributesNodes[0];
            }
        }

        public string GetMultimaterialUUID(List<MFnDependencyNode> materials)
        {
            List<MFnDependencyNode> materialsSorted = new List<MFnDependencyNode>(materials);
            materialsSorted.Sort(new MFnDependencyNodeComparer());

            string uuidConcatenated = "";
            bool isFirstTime = true;
            foreach (MFnDependencyNode material in materialsSorted)
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

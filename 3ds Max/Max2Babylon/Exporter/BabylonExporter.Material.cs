using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Max;
using BabylonExport.Entities;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        readonly List<IIGameMaterial> referencedMaterials = new List<IIGameMaterial>();

        private void ExportMaterial(IIGameMaterial materialNode, BabylonScene babylonScene)
        {
            var name = materialNode.MaterialName;
            var id = materialNode.MaxMaterial.GetGuid().ToString();

            // Check if the material was already exported. The material id is unique.
            if (babylonScene.MaterialsList.FirstOrDefault(m => m.id == id) != null)
            {
                return;
            }

            RaiseMessage(name, 1);

            // --- prints ---
            #region prints
            {
                RaiseVerbose("materialNode.MaterialClass=" + materialNode.MaterialClass, 2);
                RaiseVerbose("materialNode.NumberOfTextureMaps=" + materialNode.NumberOfTextureMaps, 2);

                var propertyContainer = materialNode.IPropertyContainer;
                RaiseVerbose("propertyContainer=" + propertyContainer, 2);
                if (propertyContainer != null)
                {
                    RaiseVerbose("propertyContainer.NumberOfProperties=" + propertyContainer.NumberOfProperties, 3);
                    for (int i = 0; i < propertyContainer.NumberOfProperties; i++)
                    {
                        var prop = propertyContainer.GetProperty(i);
                        if (prop != null)
                        {
                            RaiseVerbose("propertyContainer.GetProperty(" + i + ")=" + prop.Name, 3);
                            switch (prop.GetType_)
                            {
                                case PropType.StringProp:
                                    string propertyString = "";
                                    RaiseVerbose("prop.GetPropertyValue(ref propertyString, 0)=" + prop.GetPropertyValue(ref propertyString, 0), 4);
                                    RaiseVerbose("propertyString=" + propertyString, 4);
                                    break;
                                case PropType.IntProp:
                                    int propertyInt = 0;
                                    RaiseVerbose("prop.GetPropertyValue(ref propertyInt, 0)=" + prop.GetPropertyValue(ref propertyInt, 0), 4);
                                    RaiseVerbose("propertyInt=" + propertyInt, 4);
                                    break;
                                case PropType.FloatProp:
                                    float propertyFloat = 0;
                                    RaiseVerbose("prop.GetPropertyValue(ref propertyFloat, 0, true)=" + prop.GetPropertyValue(ref propertyFloat, 0, true), 4);
                                    RaiseVerbose("propertyFloat=" + propertyFloat, 4);
                                    RaiseVerbose("prop.GetPropertyValue(ref propertyFloat, 0, false)=" + prop.GetPropertyValue(ref propertyFloat, 0, false), 4);
                                    RaiseVerbose("propertyFloat=" + propertyFloat, 4);
                                    break;
                                case PropType.Point3Prop:
                                    IPoint3 propertyPoint3 = Loader.Global.Point3.Create(0, 0, 0);
                                    RaiseVerbose("prop.GetPropertyValue(ref propertyPoint3, 0)=" + prop.GetPropertyValue(propertyPoint3, 0), 4);
                                    RaiseVerbose("propertyPoint3=" + Point3ToString(propertyPoint3), 4);
                                    break;
                                case PropType.Point4Prop:
                                    IPoint4 propertyPoint4 = Loader.Global.Point4.Create(0, 0, 0, 0);
                                    RaiseVerbose("prop.GetPropertyValue(ref propertyPoint4, 0)=" + prop.GetPropertyValue(propertyPoint4, 0), 4);
                                    RaiseVerbose("propertyPoint4=" + Point4ToString(propertyPoint4), 4);
                                    break;
                                case PropType.UnknownProp:
                                default:
                                    RaiseVerbose("Unknown property type", 4);
                                    break;
                            }
                        }
                        else
                        {
                            RaiseVerbose("propertyContainer.GetProperty(" + i + ") IS NULL", 3);
                        }
                    }
                }
            }
            #endregion

            if (materialNode.SubMaterialCount > 0)
            {
                var babylonMultimaterial = new BabylonMultiMaterial { name = name, id = id };

                var guids = new List<string>();

                for (var index = 0; index < materialNode.SubMaterialCount; index++)
                {
                    var subMat = materialNode.GetSubMaterial(index);

                    if (subMat != null)
                    {
                        guids.Add(subMat.MaxMaterial.GetGuid().ToString());

                        if (!referencedMaterials.Contains(subMat))
                        {
                            referencedMaterials.Add(subMat);
                            ExportMaterial(subMat, babylonScene);
                        }
                    }
                    else
                    {
                        guids.Add(Guid.Empty.ToString());
                    }
                }

                babylonMultimaterial.materials = guids.ToArray();

                babylonScene.MultiMaterialsList.Add(babylonMultimaterial);
                return;
            }

            var unlitProperty = materialNode.IPropertyContainer.QueryProperty("BabylonUnlit");
            bool isUnlit = unlitProperty != null ? unlitProperty.GetBoolValue() : false;

            var stdMat = materialNode.MaxMaterial.GetParamBlock(0).Owner as IStdMat2;

            if (stdMat != null)
            {
                var babylonMaterial = new BabylonStandardMaterial
                {
                    name = name,
                    id = id,
                    isUnlit = isUnlit,
                    diffuse = materialNode.MaxMaterial.GetDiffuse(0, false).ToArray(),
                    alpha = 1.0f - materialNode.MaxMaterial.GetXParency(0, false)
                };

                babylonMaterial.backFaceCulling = !stdMat.TwoSided;
                babylonMaterial.wireframe = stdMat.Wire;

                var isSelfIllumColor = materialNode.MaxMaterial.GetSelfIllumColorOn(0, false);
                var maxSpecularColor = materialNode.MaxMaterial.GetSpecular(0, false).ToArray();

                if (isUnlit == false)
                {
                    babylonMaterial.ambient = materialNode.MaxMaterial.GetAmbient(0, false).ToArray();
                    babylonMaterial.specular = maxSpecularColor.Multiply(materialNode.MaxMaterial.GetShinStr(0, false));
                    babylonMaterial.specularPower = materialNode.MaxMaterial.GetShininess(0, false) * 256;
                    babylonMaterial.emissive =
                        isSelfIllumColor
                            ? materialNode.MaxMaterial.GetSelfIllumColor(0, false).ToArray()
                            : materialNode.MaxMaterial.GetDiffuse(0, false).Scale(materialNode.MaxMaterial.GetSelfIllum(0, false)); // compute the pre-multiplied emissive color

                    // If Self-Illumination color checkbox is checked
                    // Then self-illumination is assumed to be pre-multiplied
                    // Otherwise self-illumination needs to be multiplied with diffuse
                    // linkEmissiveWithDiffuse attribute tells the Babylon engine to perform such multiplication
                    babylonMaterial.linkEmissiveWithDiffuse = !isSelfIllumColor;
                    // useEmissiveAsIllumination attribute tells the Babylon engine to use pre-multiplied emissive as illumination
                    babylonMaterial.useEmissiveAsIllumination = isSelfIllumColor;
                
                    // Store the emissive value (before multiplication) for gltf
                    babylonMaterial.selfIllum = materialNode.MaxMaterial.GetSelfIllum(0, false);
                }

                // Textures

                BabylonFresnelParameters fresnelParameters;
                babylonMaterial.diffuseTexture = ExportTexture(stdMat, 1, out fresnelParameters, babylonScene);                // Diffuse
                if (fresnelParameters != null)
                {
                    babylonMaterial.diffuseFresnelParameters = fresnelParameters;
                }
                if ((babylonMaterial.alpha == 1.0f && babylonMaterial.opacityTexture == null) &&
                    babylonMaterial.diffuseTexture != null &&
                    (babylonMaterial.diffuseTexture.originalPath.EndsWith(".tif") || babylonMaterial.diffuseTexture.originalPath.EndsWith(".tiff")) &&
                    babylonMaterial.diffuseTexture.hasAlpha)
                {
                    RaiseWarning($"Diffuse texture named {babylonMaterial.diffuseTexture.originalPath} is a .tif file and its Alpha Source is 'Image Alpha' by default.", 2);
                    RaiseWarning($"If you don't want material to be in BLEND mode, set diffuse texture Alpha Source to 'None (Opaque)'", 2);
                }

                babylonMaterial.opacityTexture = ExportTexture(stdMat, 6, out fresnelParameters, babylonScene, false, true);   // Opacity
                if (fresnelParameters != null)
                {
                    babylonMaterial.opacityFresnelParameters = fresnelParameters;
                    if (babylonMaterial.alpha == 1 &&
                         babylonMaterial.opacityTexture == null)
                    {
                        babylonMaterial.alpha = 0;
                    }
                }

                if (isUnlit == false)
                {
                    babylonMaterial.ambientTexture = ExportTexture(stdMat, 0, out fresnelParameters, babylonScene);                // Ambient

                    babylonMaterial.specularTexture = ExportSpecularTexture(materialNode, maxSpecularColor, babylonScene);

                    babylonMaterial.emissiveTexture = ExportTexture(stdMat, 5, out fresnelParameters, babylonScene);               // Emissive
                    if (fresnelParameters != null)
                    {
                        babylonMaterial.emissiveFresnelParameters = fresnelParameters;
                        if (babylonMaterial.emissive[0] == 0 &&
                            babylonMaterial.emissive[1] == 0 &&
                            babylonMaterial.emissive[2] == 0 &&
                            babylonMaterial.emissiveTexture == null)
                        {
                            babylonMaterial.emissive = new float[] { 1, 1, 1 };
                        }
                    }

                    babylonMaterial.bumpTexture = ExportTexture(stdMat, 8, out fresnelParameters, babylonScene);                   // Bump
                    babylonMaterial.reflectionTexture = ExportTexture(stdMat, 9, out fresnelParameters, babylonScene, true);       // Reflection
                    if (fresnelParameters != null)
                    {
                        if (babylonMaterial.reflectionTexture == null)
                        {
                            RaiseWarning("Fallout cannot be used with reflection channel without a texture", 2);
                        }
                        else
                        {
                            babylonMaterial.reflectionFresnelParameters = fresnelParameters;
                        }
                    }
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

                if (babylonMaterial.opacityTexture != null && babylonMaterial.diffuseTexture != null &&
                    babylonMaterial.diffuseTexture.name == babylonMaterial.opacityTexture.name &&
                    babylonMaterial.diffuseTexture.hasAlpha && !babylonMaterial.opacityTexture.getAlphaFromRGB)
                {
                    // This is a alpha testing purpose
                    babylonMaterial.opacityTexture = null;
                    babylonMaterial.diffuseTexture.hasAlpha = true;
                    RaiseWarning("Opacity texture was removed because alpha from diffuse texture can be use instead", 2);
                    RaiseWarning("If you do not want this behavior, just set Alpha Source = None on your diffuse texture", 2);
                }

                babylonScene.MaterialsList.Add(babylonMaterial);
            }
            else if (isPhysicalMaterial(materialNode))
            {
                var propertyContainer = materialNode.IPropertyContainer;

                var babylonMaterial = new BabylonPBRMetallicRoughnessMaterial
                {
                    name = name,
                    id = id,
                    isUnlit = isUnlit
                };

                // --- Global ---

                // Alpha
                //var alphaFromXParency = 1.0f - materialNode.MaxMaterial.GetXParency(0, false);
                var alphaFromPropertyContainer = 1.0f - propertyContainer.GetFloatProperty(17);
                //RaiseMessage("alphaFromXParency=" + alphaFromXParency, 2);
                //RaiseMessage("alphaFromPropertyContainer=" + alphaFromPropertyContainer, 2);
                babylonMaterial.alpha = alphaFromPropertyContainer;

                babylonMaterial.baseColor = materialNode.MaxMaterial.GetDiffuse(0, false).ToArray();

                var invertRoughness = propertyContainer.GetBoolProperty(5);
                if (isUnlit == false)
                {
                    babylonMaterial.metallic = propertyContainer.GetFloatProperty(6);

                    babylonMaterial.roughness = propertyContainer.GetFloatProperty(4);
                    if (invertRoughness)
                    {
                        // Inverse roughness
                        babylonMaterial.roughness = 1 - babylonMaterial.roughness;
                    }

                    // Self illumination is computed from emission color, luminance, temperature and weight
                    babylonMaterial.emissive = materialNode.MaxMaterial.GetSelfIllumColorOn(0, false)
                                                    ? materialNode.MaxMaterial.GetSelfIllumColor(0, false).ToArray()
                                                    : materialNode.MaxMaterial.GetDiffuse(0, false).Scale(materialNode.MaxMaterial.GetSelfIllum(0, false));
                }
                else
                {
                    // Ignore specified roughness and metallic values
                    babylonMaterial.metallic = 0;
                    babylonMaterial.roughness = 0.9f;
                }

                // --- Textures ---
                // 1 - base color ; 9 - transparancy weight
                ITexmap colorTexmap = _getTexMap(materialNode, 1);
                ITexmap alphaTexmap = _getTexMap(materialNode, 9);
                babylonMaterial.baseTexture = ExportBaseColorAlphaTexture(colorTexmap, alphaTexmap, babylonMaterial.baseColor, babylonMaterial.alpha, babylonScene, name);

                if (isUnlit == false)
                {
                    // Metallic, roughness, ambient occlusion
                    ITexmap metallicTexmap = _getTexMap(materialNode, 5);
                    ITexmap roughnessTexmap = _getTexMap(materialNode, 4);
                    ITexmap ambientOcclusionTexmap = _getTexMap(materialNode, 6); // Use diffuse roughness map as ambient occlusion

                    // Check if MR or ORM textures are already merged
                    bool areTexturesAlreadyMerged = false;
                    if (metallicTexmap != null && roughnessTexmap != null)
                    {
                        string sourcePathMetallic = getSourcePath(metallicTexmap);
                        string sourcePathRoughness = getSourcePath(roughnessTexmap);

                        if (sourcePathMetallic == sourcePathRoughness)
                        {
                            if (ambientOcclusionTexmap != null && exportParameters.mergeAOwithMR)
                            {
                                string sourcePathAmbientOcclusion = getSourcePath(ambientOcclusionTexmap);
                                if (sourcePathMetallic == sourcePathAmbientOcclusion)
                                {
                                    // Metallic, roughness and ambient occlusion are already merged
                                    RaiseVerbose("Metallic, roughness and ambient occlusion are already merged", 2);
                                    BabylonTexture ormTexture = ExportTexture(metallicTexmap, babylonScene);
                                    babylonMaterial.metallicRoughnessTexture = ormTexture;
                                    babylonMaterial.occlusionTexture = ormTexture;
                                    areTexturesAlreadyMerged = true;
                                }
                            }
                            else
                            {
                                // Metallic and roughness are already merged
                                RaiseVerbose("Metallic and roughness are already merged", 2);
                                BabylonTexture ormTexture = ExportTexture(metallicTexmap, babylonScene);
                                babylonMaterial.metallicRoughnessTexture = ormTexture;
                                areTexturesAlreadyMerged = true;
                            }
                        }
                    }
                    if (areTexturesAlreadyMerged == false)
                    {
                        if (metallicTexmap != null || roughnessTexmap != null)
                        {
                            // Merge metallic, roughness and ambient occlusion
                            RaiseVerbose("Merge metallic and roughness (and ambient occlusion if `mergeAOwithMR` is enabled)", 2);
                            BabylonTexture ormTexture = ExportORMTexture(exportParameters.mergeAOwithMR ? ambientOcclusionTexmap : null, roughnessTexmap, metallicTexmap, babylonMaterial.metallic, babylonMaterial.roughness, babylonScene, invertRoughness);
                            babylonMaterial.metallicRoughnessTexture = ormTexture;

                            if (ambientOcclusionTexmap != null)
                            {
                                if (exportParameters.mergeAOwithMR)
                                {
                                    babylonMaterial.occlusionTexture = ormTexture;
                                }
                                else
                                {
                                    babylonMaterial.occlusionTexture = ExportPBRTexture(materialNode, 6, babylonScene);
                                }
                            }
                        }
                        else if (ambientOcclusionTexmap != null)
                        {
                            // Simply export occlusion texture
                            RaiseVerbose("Simply export occlusion texture", 2);
                            babylonMaterial.occlusionTexture = ExportTexture(ambientOcclusionTexmap, babylonScene);
                        }
                    }
                    if (ambientOcclusionTexmap != null && !exportParameters.mergeAOwithMR && babylonMaterial.occlusionTexture == null)
                    {
                        RaiseVerbose("Exporting occlusion texture without merging with metallic roughness", 2);
                        babylonMaterial.occlusionTexture = ExportTexture(ambientOcclusionTexmap, babylonScene);
                    }

                    var normalMapAmount = propertyContainer.GetFloatProperty(91);
                    babylonMaterial.normalTexture = ExportPBRTexture(materialNode, 30, babylonScene, normalMapAmount);

                    babylonMaterial.emissiveTexture = ExportPBRTexture(materialNode, 17, babylonScene);
                }

                // Constraints
                if (babylonMaterial.baseTexture != null)
                {
                    babylonMaterial.baseColor = new[] { 1.0f, 1.0f, 1.0f };
                    babylonMaterial.alpha = 1.0f;
                }

                if (babylonMaterial.alpha != 1.0f || (babylonMaterial.baseTexture != null && babylonMaterial.baseTexture.hasAlpha))
                {
                    var alphaTestProperty = materialNode.IPropertyContainer.QueryProperty("BabylonAlphaTest");
                    bool isAlphaTest = alphaTestProperty != null ? alphaTestProperty.GetBoolValue() : false;

                    babylonMaterial.transparencyMode = isAlphaTest ? (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHATEST : (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHABLEND;
                }

                if (babylonMaterial.emissiveTexture != null)
                {
                    babylonMaterial.emissive = new[] { 1.0f, 1.0f, 1.0f };
                }

                if (babylonMaterial.metallicRoughnessTexture != null)
                {
                    babylonMaterial.metallic = 1.0f;
                    babylonMaterial.roughness = 1.0f;
                }

                babylonScene.MaterialsList.Add(babylonMaterial);
            }
            else if (isArnoldMaterial(materialNode))
            {
                var propertyContainer = materialNode.IPropertyContainer;
                var babylonMaterial = new BabylonPBRMetallicRoughnessMaterial
                {
                    name = name,
                    id = id,
                    isUnlit = isUnlit
                };

                // Alpha
                babylonMaterial.alpha = 1.0f - propertyContainer.GetFloatProperty(32);

                // Color: base * weight
                float[] baseColor = propertyContainer.GetPoint3Property(5).ToArray();
                float baseWeight = propertyContainer.GetFloatProperty(2);
                babylonMaterial.baseColor = baseColor.Multiply(baseWeight);

                // Metallic & roughness
                bool invertRoughness = false;
                babylonMaterial.roughness = propertyContainer.GetFloatProperty(17); // specular_roughness
                babylonMaterial.metallic = propertyContainer.GetFloatProperty(29);

                // Emissive: emission_color * emission
                float[] emissionColor = propertyContainer.GetPoint3Property(94).ToArray();
                float emissionWeight = propertyContainer.GetFloatProperty(91);
                babylonMaterial.emissive = emissionColor.Multiply(emissionWeight);

                // --- Textures ---
                // 1 - base_color ; 5 - diffuse_roughness ; 9 - metalness ; 10 - transparent
                ITexmap colorTexmap = _getTexMap(materialNode, 1);
                ITexmap alphaTexmap = _getTexMap(materialNode, 10);
                babylonMaterial.baseTexture = ExportBaseColorAlphaTexture(colorTexmap, alphaTexmap, babylonMaterial.baseColor, babylonMaterial.alpha, babylonScene, name);
                
                if (isUnlit == false)
                {
                    // Metallic, roughness
                    ITexmap metallicTexmap = _getTexMap(materialNode, 9);
                    ITexmap roughnessTexmap = _getTexMap(materialNode, 5);

                    // Check if MR textures are already merged
                    bool areTexturesAlreadyMerged = false;
                    if (metallicTexmap != null && roughnessTexmap != null)
                    {
                        string sourcePathMetallic = getSourcePath(metallicTexmap);
                        string sourcePathRoughness = getSourcePath(roughnessTexmap);

                        if (sourcePathMetallic == sourcePathRoughness)
                        {
                            // Metallic and roughness are already merged
                            RaiseVerbose("Metallic and roughness are already merged", 2);
                            BabylonTexture ormTexture = ExportTexture(metallicTexmap, babylonScene);
                            babylonMaterial.metallicRoughnessTexture = ormTexture;
                            // The already merged map is assumed to contain Ambient Occlusion in R channel
                            babylonMaterial.occlusionTexture = ormTexture;
                            areTexturesAlreadyMerged = true;
                        }
                    }
                    if (areTexturesAlreadyMerged == false)
                    {
                        if (metallicTexmap != null || roughnessTexmap != null)
                        {
                            // Merge metallic, roughness
                            RaiseVerbose("Merge metallic and roughness", 2);
                            BabylonTexture ormTexture = ExportORMTexture(null, roughnessTexmap, metallicTexmap, babylonMaterial.metallic, babylonMaterial.roughness, babylonScene, invertRoughness);
                            babylonMaterial.metallicRoughnessTexture = ormTexture;
                        }
                    }

                    babylonMaterial.normalTexture = ExportPBRTexture(materialNode, 20, babylonScene);
                    babylonMaterial.emissiveTexture = ExportPBRTexture(materialNode, 30, babylonScene);
                }

                // Constraints
                if (babylonMaterial.baseTexture != null)
                {
                    babylonMaterial.baseColor = new[] { 1.0f, 1.0f, 1.0f };
                    babylonMaterial.alpha = 1.0f;
                }

                if (babylonMaterial.alpha != 1.0f || (babylonMaterial.baseTexture != null && babylonMaterial.baseTexture.hasAlpha))
                {
                    babylonMaterial.transparencyMode = (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHABLEND;
                }

                if (babylonMaterial.emissiveTexture != null)
                {
                    babylonMaterial.emissive = new[] { 1.0f, 1.0f, 1.0f };
                }

                if (babylonMaterial.metallicRoughnessTexture != null)
                {
                    babylonMaterial.metallic = 1.0f;
                    babylonMaterial.roughness = 1.0f;
                }

                // Add the material to the scene
                babylonScene.MaterialsList.Add(babylonMaterial);
            }
            else
            {
                // isMaterialExportable check should prevent this to happen
                RaiseError("Unsupported material type: " + materialNode.MaterialClass, 2);
            }
        }

        public bool isPhysicalMaterial(IIGameMaterial materialNode)
        {
            // TODO - Find another way to detect if material is physical
            return materialNode.MaterialClass.ToLower() == "physical material" || // English
                     materialNode.MaterialClass.ToLower() == "physikalisches material" || // German
                     materialNode.MaterialClass.ToLower() == "matériau physique"; // French
        }

        public bool isMultiSubObjectMaterial(IIGameMaterial materialNode)
        {
            // TODO - Find another way to detect if material is a multi/sub-object
            return materialNode.MaterialClass.ToLower() == "multi/sub-object" || // English
                     materialNode.MaterialClass.ToLower() == "multi-/unterobjekt" || // German
                     materialNode.MaterialClass.ToLower() == "multi/sous-objet"; // French
        }

        public bool isDirectXShaderMaterial(IIGameMaterial materialNode)
        {
            return materialNode.MaterialClass.ToLower() == "directx shader" ||    // English
                    materialNode.MaterialClass.ToLower() == "directx-shader" ||   // German
                    materialNode.MaterialClass.ToLower() == "ombrage directx";   // French
        }

        public bool isArnoldMaterial(IIGameMaterial materialNode)
        {
            return materialNode.MaterialClass.ToLower() == "standard surface"; // English, German and French
        }

        public bool isShellMaterial(IIGameMaterial materialNode)
        {
            return materialNode.MaterialClass.ToLower() == "shell material" ||    // English
                    materialNode.MaterialClass.ToLower() == "hüllenmaterial" ||   // German
                    materialNode.MaterialClass.ToLower() == "matériau coque";   // French
        }

        /// <summary>
        /// Return null if the material is supported.
        /// Otherwise return the unsupported material (himself or one of its sub-materials)
        /// </summary>
        /// <param name="materialNode"></param>
        /// <returns></returns>
        public IIGameMaterial isMaterialSupported(IIGameMaterial materialNode)
        {
            // Shell material
            if (isShellMaterial(materialNode))
            {
                var bakedMaterial = GetBakedMaterialFromShellMaterial(materialNode);
                if(bakedMaterial == null)
                {
                    return materialNode;
                }
                return isMaterialSupported(bakedMaterial);
            }

            if (materialNode.SubMaterialCount > 0)
            {
                // Check sub materials recursively
                for (int indexSubMaterial = 0; indexSubMaterial < materialNode.SubMaterialCount; indexSubMaterial++)
                {
                    IIGameMaterial subMaterialNode = materialNode.GetSubMaterial(indexSubMaterial);
                    IIGameMaterial unsupportedSubMaterial = isMaterialSupported(subMaterialNode);
                    if (unsupportedSubMaterial != null)
                    {
                        return unsupportedSubMaterial;
                    }
                }

                // Multi/sub-object material
                if (isMultiSubObjectMaterial(materialNode))
                {
                    return null;
                }
            }
            else
            {
                // Standard material
                var stdMat = materialNode.MaxMaterial.GetParamBlock(0).Owner as IStdMat2;
                if (stdMat != null)
                {
                    return null;
                }

                // Physical material
                if (isPhysicalMaterial(materialNode))
                {
                    return null;
                }

                // Arnold material
                if (isArnoldMaterial(materialNode))
                {
                    return null;
                }

                // DirectX Shader
                if (isDirectXShaderMaterial(materialNode))
                {
                    return isMaterialSupported(GetRenderMaterialFromDirectXShader(materialNode));
                }
            }
            return materialNode;
        }

        private IIGameMaterial GetRenderMaterialFromDirectXShader(IIGameMaterial materialNode)
        {
            IIGameMaterial renderMaterial = null;

            if (isDirectXShaderMaterial(materialNode))
            {
                var gameScene = Loader.Global.IGameInterface;
                IMtl renderMtl = materialNode.IPropertyContainer.GetProperty(35).MaxParamBlock2.GetMtl(4, 0, 0);
                if(renderMtl != null)
                {
                    renderMaterial = gameScene.GetIGameMaterial(renderMtl);
                }
            }

            return renderMaterial;
        }

        private IIGameMaterial GetBakedMaterialFromShellMaterial(IIGameMaterial materialNode)
        {
            if (isShellMaterial(materialNode))
            {
                // Shell Material Parameters
                // Original Material not exported => only for the offline rendering in 3DS Max
                // Baked Material => used for the export
                IMtl bakedMtl = materialNode.IPropertyContainer.GetProperty(1).MaxParamBlock2.GetMtl(3, 0, 0);

                if(bakedMtl != null)
                {
                    Guid guid = bakedMtl.GetGuid();

                    for (int indexSubMaterial = 0; indexSubMaterial < materialNode.SubMaterialCount; indexSubMaterial++)
                    {
                        IIGameMaterial subMaterialNode = materialNode.GetSubMaterial(indexSubMaterial);
                        if (guid.Equals(subMaterialNode.MaxMaterial.GetGuid()))
                        {
                            return subMaterialNode;
                        }
                    }
                }
            }

            return null;
        }

        // -------------------------
        // --------- Utils ---------
        // -------------------------

        private string ColorToString(IColor color)
        {
            if (color == null)
            {
                return "";
            }

            return "{ r=" + color.R + ", g=" + color.G + ", b=" + color.B + " }";
        }

        private string Point3ToString(IPoint3 point)
        {
            if (point == null)
            {
                return "";
            }

            return "{ x=" + point.X + ", y=" + point.Y + ", z=" + point.Z + " }";
        }

        private string Point4ToString(IPoint4 point)
        {
            if (point == null)
            {
                return "";
            }

            return "{ x=" + point.X + ", y=" + point.Y + ", z=" + point.Z + ", w=" + point.W + " }";
        }
    }
}

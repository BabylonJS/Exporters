using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Max;
using Utilities;
using BabylonExport.Entities;
using System.Drawing;

namespace BabylonExport.Entities
{
    partial class BabylonMaterial
    {
        public IIGameMaterial maxGameMaterial { get; set; }
    }
}

namespace Max2Babylon
{
    /// <summary>
    /// This is the base class used to decorate IIGameMaterial in order to access properties.
    /// </summary>
    public class MaterialDecorator
    {
        protected IIGameMaterial _node;

        public MaterialDecorator(IIGameMaterial node)
        {
            _node = node ?? throw new ArgumentNullException(nameof(node));
        }

        public IIPropertyContainer Properties => _node.IPropertyContainer;
        public string Id => _node.MaxMaterial.GetGuid().ToString();
        public string Name => _node.MaterialName;
    }

    /// <summary>
    /// Babylon custom attribute decorator
    /// </summary>
    public class BabylonCustomAttributeDecorator : MaterialDecorator
    {
        public const string UnlitStr = "babylonUnlit";
        public const string BackfaceCullingStr = "babylonBackfaceCulling";
        public const string SepCullingPassStr = "babylonSeparateCullingPass";
        public const string MaxSimultaneousLightsStr = "babylonMaxSimultaneousLights";
        public const string UseFactorsStr = "babylonUseFactors";
        public const string DirectIntensityStr = "babylonDirectIntensity";
        public const string EmissiveIntensityStr = "babylonEmissiveIntensity";
        public const string EnvironmentIntensityStr = "babylonEnvironmentIntensity";
        public const string SpecularIntensityStr = "babylonSpecularIntensity";
        public const string TransparencyModeStr = "babylonTransparencyMode";

        public static IEnumerable<string> ListPrivatePropertyNames()
        {
            yield return UnlitStr;
            yield return BackfaceCullingStr;
            yield return MaxSimultaneousLightsStr;
            yield return UseFactorsStr;
            yield return DirectIntensityStr;
            yield return EmissiveIntensityStr;
            yield return EnvironmentIntensityStr;
            yield return SpecularIntensityStr;
            yield return TransparencyModeStr;
        }

        public BabylonCustomAttributeDecorator(IIGameMaterial node) : base(node)
        {
        }

        public bool IsUnlit => Properties?.GetBoolProperty(UnlitStr, false) ?? false;
        public bool BackFaceCulling => Properties?.GetBoolProperty(BackfaceCullingStr, true) ?? true;
        public bool SeparateCullingPass => Properties?.GetBoolProperty(SepCullingPassStr, false) ?? false;
        public int MaxSimultaneousLights => Properties?.GetIntProperty(MaxSimultaneousLightsStr, 4) ?? 4;
        public bool UseMaxFactor => Properties?.GetBoolProperty(UseFactorsStr, true) ?? true;
        public float DirectIntensity => Properties?.GetFloatProperty(DirectIntensityStr, 1.0f) ?? 1.0f;
        public float EmissiveIntensity => Properties?.GetFloatProperty(EmissiveIntensityStr, 1.0f) ?? 1.0f;
        public float EnvironementIntensity => Properties?.GetFloatProperty(EnvironmentIntensityStr, 1.0f) ?? 1.0f;
        public float SpecularIntensity => Properties?.GetFloatProperty(SpecularIntensityStr, 1.0f) ?? 1.0f;
        public int TransparencyMode => Properties?.GetIntProperty(TransparencyModeStr, 0) ?? 0;
    }

    /// <summary>
    /// The Exporter
    /// </summary>
    partial class BabylonExporter
    {
        // use as scale for GetSelfIllumColor to convert [0,PI] to [O,1]
        private const float selfIllumScale = (float)(1.0 / Math.PI);

        readonly List<IIGameMaterial> referencedMaterials = new List<IIGameMaterial>();
        Dictionary<ClassIDWrapper, IMaxMaterialExporter> materialExporters;

        private static int STANDARD_MATERIAL_TEXTURE_ID_DIFFUSE = 1;
        private static int STANDARD_MATERIAL_TEXTURE_ID_OPACITY = 6;

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

                Print(materialNode.IPropertyContainer, 2);
                for (int i = 0; i < materialNode.MaxMaterial.NumSubTexmaps; i++)
                {
                    RaiseVerbose("Texture[" + i + "] is named '" + materialNode.MaxMaterial.GetSubTexmapSlotName(i) + "'", 2);
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
                        if (subMat.SubMaterialCount > 0)
                        {
                            RaiseError("MultiMaterials as inputs to other MultiMaterials are not supported!");
                        }
                        else
                        {
                            guids.Add(subMat.MaxMaterial.GetGuid().ToString());

                            if (!referencedMaterials.Contains(subMat))
                            {
                                referencedMaterials.Add(subMat);
                                ExportMaterial(subMat, babylonScene);
                            }
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

            // Retreive Attributes container
            IIPropertyContainer attributesContainer = materialNode.IPropertyContainer;

            bool isUnlit = attributesContainer?.GetBoolProperty("babylonUnlit", false)??false;

            // check custom exporters first, to allow custom exporters of supported material classes
            materialExporters.TryGetValue(new ClassIDWrapper(materialNode.MaxMaterial.ClassID), out IMaxMaterialExporter materialExporter);

            IStdMat2 stdMat = null;
            var maxMaterial = materialNode.MaxMaterial;
            if (maxMaterial != null && maxMaterial.NumParamBlocks > 0)
            {
                var paramBlock = maxMaterial.GetParamBlock(0);
                if (paramBlock != null && paramBlock.Owner != null)
                {
                    stdMat = maxMaterial.GetParamBlock(0).Owner as IStdMat2;
                }
            }

            if (isBabylonExported && materialExporter != null && materialExporter is IMaxBabylonMaterialExporter)
            {
                IMaxBabylonMaterialExporter babylonMaterialExporter = materialExporter as IMaxBabylonMaterialExporter;
                BabylonMaterial babylonMaterial = babylonMaterialExporter.ExportBabylonMaterial(materialNode);
                if (babylonMaterial == null)
                {
                    string message = string.Format("Custom Babylon material exporter failed to export | Exporter: '{0}' | Material Name: '{1}' | Material Class: '{2}'",
                        babylonMaterialExporter.GetType().ToString(), materialNode.MaterialName, materialNode.MaterialClass);
                    RaiseWarning(message, 2);
                }
                else babylonScene.MaterialsList.Add(babylonMaterial);
            }
            else if (isGltfExported && materialExporter != null && materialExporter is IMaxGLTFMaterialExporter)
            {
                // add a basic babylon material to the list to forward the max material reference
                var babylonMaterial = new BabylonMaterial(id)
                {
                    maxGameMaterial = materialNode,
                    name = name
                };
                babylonScene.MaterialsList.Add(babylonMaterial);
            }
            else if (stdMat != null)
            {
                var babylonMaterial = new BabylonStandardMaterial(id)
                {
                    maxGameMaterial = materialNode,
                    name = name,
                    isUnlit = isUnlit,
                    diffuse = materialNode.MaxMaterial.GetDiffuse(0, false).ToArray()
                };
                ExportStandardMaterial(materialNode, attributesContainer, stdMat, babylonScene, babylonMaterial);
            }
            else if (isPhysicalMaterial(materialNode))
            {
                var babylonMaterial = new BabylonPBRMetallicRoughnessMaterial(id)
                {
                    maxGameMaterial = materialNode,
                    name = name,
                    isUnlit = isUnlit
                };
                ExportPhysicalMaterial(materialNode, attributesContainer, babylonScene, babylonMaterial);
            }
            else if (isPbrMetalRoughMaterial(materialNode))
            {
                ExportPbrMetalRoughMaterial(materialNode, babylonScene);
            }
            else if (isPbrSpecGlossMaterial(materialNode))
            {
                ExportPbrSpecGlossMaterial(materialNode, babylonScene);

            }
            else if (isArnoldMaterial(materialNode))
            {
                var babylonMaterial = new BabylonPBRMetallicRoughnessMaterial(id)
                {
                    maxGameMaterial = materialNode,
                    name = name,
                    isUnlit = isUnlit
                };

                ExportArnoldMaterial(materialNode, attributesContainer, babylonScene, babylonMaterial);
            }
            else
            {
                // isMaterialExportable check should prevent this to happen
                RaiseError("Unsupported material type: " + materialNode.MaterialClass, 2);
            }
        }

        private void ExportStandardMaterial(IIGameMaterial materialNode, IIPropertyContainer attributesContainer, IStdMat2 stdMat, BabylonScene babylonScene, BabylonStandardMaterial babylonMaterial)
        {
            bool isTransparencyModeFromBabylonAttributes = false;
            if (attributesContainer != null)
            {
                IIGameProperty babylonTransparencyModeGameProperty = attributesContainer.QueryProperty("babylonTransparencyMode");
                if (babylonTransparencyModeGameProperty != null)
                {
                    babylonMaterial.transparencyMode = babylonTransparencyModeGameProperty.GetIntValue();
                    isTransparencyModeFromBabylonAttributes = true;
                }
            }

            if (isTransparencyModeFromBabylonAttributes == false || babylonMaterial.transparencyMode != 0)
            {
                // The user specified value in 3ds Max is opacity
                // The retreived value here is transparency
                // Convert transparency to opacity
                babylonMaterial.alpha = 1.0f - materialNode.MaxMaterial.GetXParency(0, false);
            }

            babylonMaterial.backFaceCulling = !stdMat.TwoSided;
            babylonMaterial.wireframe = stdMat.Wire;

            var isSelfIllumColor = materialNode.MaxMaterial.GetSelfIllumColorOn(0, false);
            var maxSpecularColor = materialNode.MaxMaterial.GetSpecular(0, false).ToArray();

            if (babylonMaterial.isUnlit == false)
            {
                babylonMaterial.ambient = materialNode.MaxMaterial.GetAmbient(0, false).ToArray();
                babylonMaterial.specular = maxSpecularColor.Multiply(materialNode.MaxMaterial.GetShinStr(0, false));
                babylonMaterial.specularPower = materialNode.MaxMaterial.GetShininess(0, false) * 256;
                babylonMaterial.emissive =
                    isSelfIllumColor
                        ? materialNode.MaxMaterial.GetSelfIllumColor(0, false).Scale(selfIllumScale).ToArray()
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

            float[] multiplyColor = null;
            if (exportParameters.exportTextures)
            {
                BabylonFresnelParameters fresnelParameters;
                babylonMaterial.diffuseTexture = ExportDiffuseTexture(stdMat, STANDARD_MATERIAL_TEXTURE_ID_DIFFUSE, out fresnelParameters, babylonScene, out multiplyColor);                // Diffuse
                if (fresnelParameters != null)
                {
                    babylonMaterial.diffuseFresnelParameters = fresnelParameters;
                }
                if (multiplyColor != null)
                {
                    babylonMaterial.diffuse = multiplyColor;
                }
                if ((babylonMaterial.alpha == 1.0f && babylonMaterial.opacityTexture == null) &&
                    babylonMaterial.diffuseTexture != null &&
                    (babylonMaterial.diffuseTexture.originalPath.EndsWith(".tif") || babylonMaterial.diffuseTexture.originalPath.EndsWith(".tiff")) &&
                    babylonMaterial.diffuseTexture.hasAlpha)
                {
                    RaiseWarning($"Diffuse texture named {babylonMaterial.diffuseTexture.originalPath} is a .tif file and its Alpha Source is 'Image Alpha' by default.", 2);
                    RaiseWarning($"If you don't want material to be in BLEND mode, set diffuse texture Alpha Source to 'None (Opaque)'", 2);
                }

                if (isTransparencyModeFromBabylonAttributes == false || babylonMaterial.transparencyMode != 0)
                {
                    // The map is opacity
                    babylonMaterial.opacityTexture = ExportTexture(stdMat, STANDARD_MATERIAL_TEXTURE_ID_OPACITY, out fresnelParameters, babylonScene, false, true);   // Opacity
                }

                if (fresnelParameters != null)
                {
                    babylonMaterial.opacityFresnelParameters = fresnelParameters;
                    if (babylonMaterial.alpha == 1 &&
                         babylonMaterial.opacityTexture == null)
                    {
                        babylonMaterial.alpha = 0;
                    }
                }

                if (babylonMaterial.isUnlit == false)
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
            }

            if (isTransparencyModeFromBabylonAttributes == false && (babylonMaterial.alpha != 1.0f || (babylonMaterial.diffuseTexture != null && babylonMaterial.diffuseTexture.hasAlpha) || babylonMaterial.opacityTexture != null))
            {
                babylonMaterial.transparencyMode = (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHABLEND;
            }

            // Constraints
            if (babylonMaterial.diffuseTexture != null && multiplyColor == null)
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


            if (babylonMaterial.transparencyMode == (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHATEST)
            {
                // Set the alphaCutOff value explicitely to avoid different interpretations on different engines
                // Use the glTF default value rather than the babylon one
                babylonMaterial.alphaCutOff = 0.5f;
            }

            // Add babylon attributes
            if (attributesContainer == null)
            {
                AddStandardBabylonAttributes(materialNode.MaterialName, babylonMaterial);
            }

            if (attributesContainer != null)
            {
                RaiseVerbose("Babylon Attributes of " + materialNode.MaterialName, 2);

                // Common attributes
                ExportCommonBabylonAttributes(attributesContainer, babylonMaterial);

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
                if (exportParameters.exportTextures && babylonMaterial.transparencyMode == (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHATEST)
                {
                    // Base color and alpha files need to be merged into a single file
                    Color defaultColor = Color.FromArgb((int)(babylonMaterial.diffuse[0] * 255), (int)(babylonMaterial.diffuse[1] * 255), (int)(babylonMaterial.diffuse[2] * 255));
                    ITexmap baseColorTextureMap = GetSubTexmap(stdMat, STANDARD_MATERIAL_TEXTURE_ID_DIFFUSE);
                    ITexmap opacityTextureMap = GetSubTexmap(stdMat, STANDARD_MATERIAL_TEXTURE_ID_OPACITY);
                    multiplyColor = null;
                    babylonMaterial.diffuseTexture = ExportBaseColorAlphaTexture(baseColorTextureMap, opacityTextureMap, babylonMaterial.diffuse, babylonMaterial.alpha, babylonScene, out multiplyColor, true);
                    if (multiplyColor != null)
                    {
                        babylonMaterial.diffuse = multiplyColor;
                    }
                    babylonMaterial.opacityTexture = null;
                    babylonMaterial.alpha = 1.0f;
                }
            }

            // List all babylon material attributes
            // Those attributes are currently stored into the native material
            // They should not be exported as extra attributes
            List<string> excludeAttributes = new List<string>();
            excludeAttributes.Add("babylonUnlit");
            excludeAttributes.Add("babylonMaxSimultaneousLights");
            excludeAttributes.Add("babylonTransparencyMode");

            // Export the custom attributes of this material
            babylonMaterial.metadata = ExportExtraAttributes(materialNode, babylonScene, excludeAttributes);

            babylonScene.MaterialsList.Add(babylonMaterial);
        }


        private void ExportPhysicalMaterial(IIGameMaterial materialNode, IIPropertyContainer attributesContainer, BabylonScene babylonScene, BabylonPBRMetallicRoughnessMaterial babylonMaterial)
        {
            var propertyContainer = materialNode.IPropertyContainer;

            bool isTransparencyModeFromBabylonAttributes = false;
            bool usePbrFactor = false; // this force the exporter to set the metallic or roughness even if the map are set
            if (attributesContainer != null)
            {
                IIGameProperty gameProperty = attributesContainer.QueryProperty("babylonTransparencyMode");
                if (gameProperty != null)
                {
                    babylonMaterial.transparencyMode = gameProperty.GetIntValue();
                    isTransparencyModeFromBabylonAttributes = true;
                }
                gameProperty = attributesContainer.QueryProperty("babylonUseFactors");
                if (gameProperty != null)
                {
                    usePbrFactor = gameProperty.GetBoolValue();
                }
            }

            // --- Global ---

            // Alpha
            if (isTransparencyModeFromBabylonAttributes == false || babylonMaterial.transparencyMode != 0)
            {
                // Convert transparency to opacity
                babylonMaterial.alpha = 1.0f - propertyContainer.GetFloatProperty(17);
            }

            babylonMaterial.baseColor = materialNode.MaxMaterial.GetDiffuse(0, false).ToArray();

            var invertRoughness = propertyContainer.GetBoolProperty(5);
            if (babylonMaterial.isUnlit == false)
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
                                                ? materialNode.MaxMaterial.GetSelfIllumColor(0, false).Scale(selfIllumScale).ToArray()
                                                : materialNode.MaxMaterial.GetDiffuse(0, false).Scale(materialNode.MaxMaterial.GetSelfIllum(0, false));
            }
            else
            {
                // Ignore specified roughness and metallic values
                babylonMaterial.metallic = 0;
                babylonMaterial.roughness = 0.9f;
            }

            // --- Textures ---
            float[] multiplyColor = null;
            if (exportParameters.exportTextures)
            {
                // 1 - base color ; 9 - transparency weight
                ITexmap colorTexmap = _getTexMap(materialNode, 1);
                ITexmap alphaTexmap = null;
                if (isTransparencyModeFromBabylonAttributes == false || babylonMaterial.transparencyMode != 0)
                {
                    alphaTexmap = _getTexMap(materialNode, 9);
                }
                babylonMaterial.baseTexture = ExportBaseColorAlphaTexture(colorTexmap, alphaTexmap, babylonMaterial.baseColor, babylonMaterial.alpha, babylonScene, out multiplyColor);
                if (multiplyColor != null)
                {
                    babylonMaterial.baseColor = multiplyColor;
                }

                if (babylonMaterial.isUnlit == false)
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
                            if (ambientOcclusionTexmap != null && exportParameters.mergeAO)
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
                            BabylonTexture ormTexture = ExportORMTexture(exportParameters.mergeAO ? ambientOcclusionTexmap : null, roughnessTexmap, metallicTexmap, babylonMaterial.metallic, babylonMaterial.roughness, babylonScene, invertRoughness);
                            babylonMaterial.metallicRoughnessTexture = ormTexture;

                            if (ambientOcclusionTexmap != null)
                            {
                                if (exportParameters.mergeAO)
                                {
                                    // if the ambient occlusion texture map uses a different set of texture coordinates than
                                    // metallic roughness, create a new instance of the ORM BabylonTexture with the different texture
                                    // coordinate indices
                                    var ambientOcclusionTexture = _getBitmapTex(ambientOcclusionTexmap);
                                    var texCoordIndex = ambientOcclusionTexture.UVGen.MapChannel - 1;
                                    if (texCoordIndex != ormTexture.coordinatesIndex)
                                    {
                                        babylonMaterial.occlusionTexture = new BabylonTexture(ormTexture);
                                        babylonMaterial.occlusionTexture.coordinatesIndex = texCoordIndex;
                                        // Set UVs/texture transform for the ambient occlusion texture
                                        var uvGen = _exportUV(ambientOcclusionTexture.UVGen, babylonMaterial.occlusionTexture);
                                    }
                                    else
                                    {
                                        babylonMaterial.occlusionTexture = ormTexture;
                                    }
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
                    if (ambientOcclusionTexmap != null && !exportParameters.mergeAO && babylonMaterial.occlusionTexture == null)
                    {
                        RaiseVerbose("Exporting occlusion texture without merging with metallic roughness", 2);
                        babylonMaterial.occlusionTexture = ExportTexture(ambientOcclusionTexmap, babylonScene);
                    }

                    var normalMapAmount = propertyContainer.GetFloatProperty(91);
                    babylonMaterial.normalTexture = ExportPBRTexture(materialNode, 30, babylonScene, normalMapAmount);

                    babylonMaterial.emissiveTexture = ExportPBRTexture(materialNode, 17, babylonScene);

                    if (babylonMaterial.metallicRoughnessTexture != null && !usePbrFactor)
                    {
                        // Change the factor to zero if combining partial channel to avoid issue (in case of image compression).
                        // ie - if no metallic map, then b MUSt be fully black. However channel of jpeg MAY not beeing fully black 
                        // cause of the compression algorithm. Keeping MetallicFactor to 1 will make visible artifact onto texture. So set to Zero instead.
                        babylonMaterial.metallic = areTexturesAlreadyMerged || metallicTexmap != null ? 1.0f : 0.0f;
                        babylonMaterial.roughness = areTexturesAlreadyMerged || roughnessTexmap != null ? 1.0f : 0.0f;
                    }
                }
            }

            // Constraints
            if (babylonMaterial.baseTexture != null && multiplyColor == null)
            {
                babylonMaterial.baseColor = new[] { 1.0f, 1.0f, 1.0f };
            }

            if (isTransparencyModeFromBabylonAttributes == false && (babylonMaterial.alpha != 1.0f || (babylonMaterial.baseTexture != null && babylonMaterial.baseTexture.hasAlpha)))
            {
                babylonMaterial.transparencyMode = (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHABLEND;
            }

            if (babylonMaterial.emissiveTexture != null)
            {
                babylonMaterial.emissive = new[] { 1.0f, 1.0f, 1.0f };
            }


            if (babylonMaterial.transparencyMode == (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHATEST)
            {
                // Set the alphaCutOff value explicitely to avoid different interpretations on different engines
                // Use the glTF default value rather than the babylon one
                babylonMaterial.alphaCutOff = 0.5f;
            }

            // Add babylon attributes
            if (attributesContainer == null)
            {
                AddPhysicalBabylonAttributes(materialNode.MaterialName, babylonMaterial);
            }

            if (attributesContainer != null)
            {
                RaiseVerbose("Babylon Attributes of " + materialNode.MaterialName, 2);

                // Common attributes
                ExportCommonBabylonAttributes(attributesContainer, babylonMaterial);
                babylonMaterial._unlit = babylonMaterial.isUnlit;

                // Backface culling
                bool backFaceCulling = attributesContainer.GetBoolProperty("babylonBackfaceCulling");
                RaiseVerbose("backFaceCulling=" + backFaceCulling, 3);
                babylonMaterial.backFaceCulling = backFaceCulling;
                babylonMaterial.doubleSided = !backFaceCulling;
            }

            // List all babylon material attributes
            // Those attributes are currently stored into the native material
            // They should not be exported as extra attributes
            List<string> excludeAttributes = new List<string>();
            excludeAttributes.Add("babylonUnlit");
            excludeAttributes.Add("babylonBackfaceCulling");
            excludeAttributes.Add("babylonMaxSimultaneousLights");
            excludeAttributes.Add("babylonTransparencyMode");
            excludeAttributes.Add("babylonUseFactors");
            excludeAttributes.Add("babylonDirectIntensity");
            excludeAttributes.Add("babylonEmissiveIntensity");
            excludeAttributes.Add("babylonEnvironmentIntensity");
            excludeAttributes.Add("babylonSpecularIntensity");

            // Export the custom attributes of this material
            babylonMaterial.metadata = ExportExtraAttributes(materialNode, babylonScene, excludeAttributes);

            babylonScene.MaterialsList.Add(babylonMaterial);
        }

        private void ExportArnoldMaterial(IIGameMaterial materialNode, IIPropertyContainer attributesContainer, BabylonScene babylonScene, BabylonPBRMetallicRoughnessMaterial babylonMaterial)
        {
            var propertyContainer = materialNode.IPropertyContainer;

            bool isTransparencyModeFromBabylonAttributes = false;
            if (attributesContainer != null)
            {
                IIGameProperty babylonTransparencyModeGameProperty = attributesContainer.QueryProperty("babylonTransparencyMode");
                if (babylonTransparencyModeGameProperty != null)
                {
                    babylonMaterial.transparencyMode = babylonTransparencyModeGameProperty.GetIntValue();
                    isTransparencyModeFromBabylonAttributes = true;
                }
            }

            // Alpha
            if (isTransparencyModeFromBabylonAttributes == false || babylonMaterial.transparencyMode != 0)
            {
                // Retreive alpha value from R channel of opacity color
                babylonMaterial.alpha = propertyContainer.GetPoint3Property("opacity")[0];
            }

            // Color: base * weight
            float[] baseColor = propertyContainer.GetPoint3Property(5).ToArray();
            float baseWeight = propertyContainer.GetFloatProperty(2);
            babylonMaterial.baseColor = baseColor.Multiply(baseWeight);

            // Metallic & roughness
            bool invertRoughness = false;
            babylonMaterial.roughness = propertyContainer.GetFloatProperty(17); // specular_roughness
            babylonMaterial.metallic = propertyContainer.GetFloatProperty(29);

            // Emissive: emission_color * emission
            float[] emissionColor = propertyContainer.QueryProperty("emission_color").GetPoint3Property().ToArray();
            float emissionWeight = propertyContainer.QueryProperty("emission").GetFloatValue();
            if (emissionColor != null && emissionWeight > 0f)
            {
                babylonMaterial.emissive = emissionColor.Multiply(emissionWeight);
            }

            // --- Clear Coat ---
            float coatWeight = propertyContainer.GetFloatProperty(75);
            if (coatWeight > 0.0f)
            {
                babylonMaterial.clearCoat.isEnabled = true;
                babylonMaterial.clearCoat.indexOfRefraction = propertyContainer.GetFloatProperty(84);

                ITexmap intensityTexmap = _getTexMap(materialNode, 23);
                ITexmap roughnessTexmap = _getTexMap(materialNode, 25);
                var coatRoughness = propertyContainer.GetFloatProperty(81);
                var coatTexture = exportParameters.exportTextures ? ExportClearCoatTexture(intensityTexmap, roughnessTexmap, coatWeight, coatRoughness, babylonScene, babylonMaterial.name, invertRoughness) : null;
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

                float[] coatColor = propertyContainer.GetPoint3Property(78).ToArray();
                if (coatColor[0] != 1.0f || coatColor[1] != 1.0f || coatColor[2] != 1.0f)
                {
                    babylonMaterial.clearCoat.isTintEnabled = true;
                    babylonMaterial.clearCoat.tintColor = coatColor;
                }

                babylonMaterial.clearCoat.tintTexture = exportParameters.exportTextures ? ExportPBRTexture(materialNode, 24, babylonScene) : null;
                if (babylonMaterial.clearCoat.tintTexture != null)
                {
                    babylonMaterial.clearCoat.tintColor = new[] { 1.0f, 1.0f, 1.0f };
                    babylonMaterial.clearCoat.isTintEnabled = true;
                }

                // EyeBall deduction...
                babylonMaterial.clearCoat.tintThickness = 0.65f;

                babylonMaterial.clearCoat.bumpTexture = exportParameters.exportTextures ? ExportPBRTexture(materialNode, 27, babylonScene) : null;
            }

            // --- Textures ---

            float[] multiplyColor = null;
            if (exportParameters.exportTextures)
            {
                // 1 - base_color ; 5 - specular_roughness ; 9 - metalness ; 40 - transparent
                ITexmap colorTexmap = _getTexMap(materialNode, 1);
                ITexmap alphaTexmap = null;
                if (isTransparencyModeFromBabylonAttributes == false || babylonMaterial.transparencyMode != 0)
                {
                    alphaTexmap = _getTexMap(materialNode, "opacity");
                }

                babylonMaterial.baseTexture = ExportBaseColorAlphaTexture(colorTexmap, alphaTexmap, babylonMaterial.baseColor, babylonMaterial.alpha, babylonScene, out multiplyColor, true);

                if (multiplyColor != null)
                {
                    babylonMaterial.baseColor = multiplyColor;
                }

                if (babylonMaterial.isUnlit == false)
                {
                    // Metallic, roughness
                    ITexmap metallicTexmap = _getTexMap(materialNode, 9);
                    ITexmap roughnessTexmap = _getTexMap(materialNode, 5);
                    ITexmap ambientOcclusionTexmap = _getTexMap(materialNode, 6); // Use diffuse roughness map as ambient occlusion

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

                            if (ambientOcclusionTexmap != null)
                            {
                                // if the ambient occlusion texture map uses a different set of texture coordinates than
                                // metallic roughness, create a new instance of the ORM BabylonTexture with the different texture
                                // coordinate indices

                                var ambientOcclusionTexture = _getBitmapTex(ambientOcclusionTexmap);
                                var texCoordIndex = ambientOcclusionTexture.UVGen.MapChannel - 1;
                                if (texCoordIndex != ormTexture.coordinatesIndex)
                                {
                                    babylonMaterial.occlusionTexture = new BabylonTexture(ormTexture);
                                    babylonMaterial.occlusionTexture.coordinatesIndex = texCoordIndex;
                                    // Set UVs/texture transform for the ambient occlusion texture
                                    var uvGen = _exportUV(ambientOcclusionTexture.UVGen, babylonMaterial.occlusionTexture);
                                }
                                else
                                {
                                    babylonMaterial.occlusionTexture = ormTexture;
                                }
                            }
                            else
                            {
                                babylonMaterial.occlusionTexture = ormTexture;
                            }
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

                    var numOfTexMapSlots = materialNode.MaxMaterial.NumSubTexmaps;

                    for (int i = 0; i < numOfTexMapSlots; i++)
                    {
                        if (materialNode.MaxMaterial.GetSubTexmapSlotName(i) == "normal")
                        {
                            babylonMaterial.normalTexture = ExportPBRTexture(materialNode, i, babylonScene);
                        }

                        else if (materialNode.MaxMaterial.GetSubTexmapSlotName(i) == "emission")
                        {
                            babylonMaterial.emissiveTexture = ExportPBRTexture(materialNode, i, babylonScene);
                        }
                    }

                    if (babylonMaterial.metallicRoughnessTexture != null)
                    {
                        // Change the factor to zero if combining partial channel to avoid issue (in case of image compression).
                        // ie - if no metallic map, then b MUSt be fully black. However channel of jpeg MAY not beeing fully black 
                        // cause of the compression algorithm. Keeping MetallicFactor to 1 will make visible artifact onto texture. So set to Zero instead.
                        babylonMaterial.metallic = areTexturesAlreadyMerged || metallicTexmap != null ? 1.0f : 0.0f;
                        babylonMaterial.roughness = areTexturesAlreadyMerged || roughnessTexmap != null ? 1.0f : 0.0f;
                    }
                }
            }

            // Constraints
            if (babylonMaterial.baseTexture != null)
            {
                if (multiplyColor == null)
                {
                    babylonMaterial.baseColor = new[] { 1.0f, 1.0f, 1.0f };
                }
                babylonMaterial.alpha = 1.0f;
            }

            if (isTransparencyModeFromBabylonAttributes == false && (babylonMaterial.alpha != 1.0f || (babylonMaterial.baseTexture != null && babylonMaterial.baseTexture.hasAlpha)))
            {
                babylonMaterial.transparencyMode = (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHABLEND;
            }


            if (babylonMaterial.transparencyMode == (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHATEST)
            {
                // Set the alphaCutOff value explicitely to avoid different interpretations on different engines
                // Use the glTF default value rather than the babylon one
                babylonMaterial.alphaCutOff = 0.5f;
            }

            // Add babylon attributes
            if (attributesContainer == null)
            {
                AddAiStandardSurfaceBabylonAttributes(materialNode.MaterialName, babylonMaterial);
            }

            if (attributesContainer != null)
            {
                RaiseVerbose("Babylon Attributes of " + materialNode.MaterialName, 2);

                // Common attributes
                ExportCommonBabylonAttributes(attributesContainer, babylonMaterial);
                babylonMaterial._unlit = babylonMaterial.isUnlit;

                // Backface culling
                bool backFaceCulling = attributesContainer.GetBoolProperty("babylonBackfaceCulling");
                RaiseVerbose("backFaceCulling=" + backFaceCulling, 3);
                babylonMaterial.backFaceCulling = backFaceCulling;
                babylonMaterial.doubleSided = !backFaceCulling;
            }

            // List all babylon material attributes
            // Those attributes are currently stored into the native material
            // They should not be exported as extra attributes
            List<string> excludeAttributes = new List<string>();
            excludeAttributes.Add("babylonUnlit");
            excludeAttributes.Add("babylonBackfaceCulling");
            excludeAttributes.Add("babylonMaxSimultaneousLights");
            excludeAttributes.Add("babylonTransparencyMode");

            // Export the custom attributes of this material
            babylonMaterial.metadata = ExportExtraAttributes(materialNode, babylonScene, excludeAttributes);

            if (exportParameters.pbrFull)
            {
                var fullPBR = new BabylonPBRMaterial(babylonMaterial)
                {
                    directIntensity = attributesContainer?.GetIntProperty("babylonDirectIntensity") ?? 1.0f,
                    emissiveIntensity = attributesContainer?.GetIntProperty("babylonEmissiveIntensity") ?? 1.0f,
                    environmentIntensity = attributesContainer?.GetIntProperty("babylonEnvironmentIntensity") ?? 1.0f,
                    specularIntensity = attributesContainer?.GetIntProperty("babylonSpecularIntensity") ?? 1.0f,
                    maxGameMaterial = babylonMaterial.maxGameMaterial
                };
                babylonScene.MaterialsList.Add(fullPBR);
            }
            else
            {
                // Add the material to the scene
                babylonScene.MaterialsList.Add(babylonMaterial);
            }
        }

        public bool isPhysicalMaterial(IIGameMaterial materialNode)
        {
            return ClassIDWrapper.Physical_Material.Equals(materialNode.MaxMaterial.ClassID);
        }

        public bool isDoubleSidedMaterial(IIGameMaterial materialNode)
        {
            return ClassIDWrapper.Double_Sided_Material.Equals(materialNode.MaxMaterial.ClassID);
        }

        public bool isMultiSubObjectMaterial(IIGameMaterial materialNode)
        {
            return ClassIDWrapper.Multi_Sub_Object_Material.Equals(materialNode.MaxMaterial.ClassID);
        }

        public bool isDirectXShaderMaterial(IIGameMaterial materialNode)
        {
            return ClassIDWrapper.DirectX_Shader_Material.Equals(materialNode.MaxMaterial.ClassID);
        }

        public bool isArnoldMaterial(IIGameMaterial materialNode)
        {
            return ClassIDWrapper.Standard_Surface_Material.Equals(materialNode.MaxMaterial.ClassID);
        }

        public bool isShellMaterial(IIGameMaterial materialNode)
        {
            return ClassIDWrapper.Shell_Material.Equals(materialNode.MaxMaterial.ClassID);
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
                if (bakedMaterial == null)
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

                // Double sided material
                if (isDoubleSidedMaterial(materialNode))
                {
                    return null;
                }
            }
            else
            {
                // Standard material
                IStdMat2 stdMat = null;
                if (materialNode.MaxMaterial != null && materialNode.MaxMaterial.NumParamBlocks > 0)
                {
                    var paramBlock = materialNode.MaxMaterial.GetParamBlock(0);
                    if (paramBlock != null && paramBlock.Owner != null)
                    {
                        stdMat = materialNode.MaxMaterial.GetParamBlock(0).Owner as IStdMat2;
                    }
                }

                if (stdMat != null)
                {
                    return null;
                }

                // Physical materials
                if (isPhysicalMaterial(materialNode) || isPbrMetalRoughMaterial(materialNode) || isPbrSpecGlossMaterial(materialNode))
                {
                    return null;
                }

                // Custom material exporters
                IMaxMaterialExporter materialExporter;
                if (materialExporters.TryGetValue(new ClassIDWrapper(materialNode.MaxMaterial.ClassID), out materialExporter))
                {
                    if (isGltfExported && materialExporter is IMaxGLTFMaterialExporter)
                        return null;
                    else if (isBabylonExported && materialExporter is IMaxBabylonMaterialExporter)
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
                IIGameProperty property = materialNode.IPropertyContainer.GetProperty(35);
                if (property != null)
                {
                    IMtl renderMtl = materialNode.IPropertyContainer.GetProperty(35).MaxParamBlock2.GetMtl(4, 0, 0);
                    if (renderMtl != null)
                    {
                        renderMaterial = gameScene.GetIGameMaterial(renderMtl);
                    }
                }
                else
                {
                    RaiseWarning($"DirectX material property for {materialNode.MaterialName} is null...", 2);
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

                if (bakedMtl != null)
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

        private void AddStandardBabylonAttributes(string attributesContainer, BabylonStandardMaterial babylonMaterial = null) => AddCustomAttributes(attributesContainer, MaterialScripts.StandardBabylonCAtDef, "STANDARD_MATERIAL_CAT_DEF");

        private void AddPhysicalBabylonAttributes(string attributesContainer, BabylonPBRMetallicRoughnessMaterial babylonMaterial = null) => AddCustomAttributes(attributesContainer, MaterialScripts.PhysicalBabylonCAtDef, "PHYSICAL_MATERIAL_CAT_DEF");

        private void AddAiStandardSurfaceBabylonAttributes(string attributesContainer, BabylonPBRMetallicRoughnessMaterial babylonMaterial = null) => AddCustomAttributes(attributesContainer, MaterialScripts.AIBabylonCAtDef, "ARNOLD_MATERIAL_CAT_DEF");

        private void AddCustomAttributes(string attributesContainer, string cmdCreateBabylonAttributes, string def) => ManagedServices.MaxscriptSDK.ExecuteMaxscriptCommand(MaterialScripts.AddCustomAttribute(cmdCreateBabylonAttributes, attributesContainer, def));


        private void ExportCommonBabylonAttributes(IIPropertyContainer babylonAttributesContainer, BabylonMaterial babylonMaterial)
        {
            int maxSimultaneousLights = babylonAttributesContainer.GetIntProperty("babylonMaxSimultaneousLights", 4);
            maxSimultaneousLights = Math.Min(100,Math.Max(maxSimultaneousLights,1));// force to be [1,100], because of 3DSMax Slider issue.
            RaiseVerbose($"maxSimultaneousLights={maxSimultaneousLights}", 3);
            babylonMaterial.maxSimultaneousLights = maxSimultaneousLights;

            bool unlit = babylonAttributesContainer.GetBoolProperty("babylonUnlit");
            RaiseVerbose($"unlit={unlit}", 3);
            babylonMaterial.isUnlit = unlit;
        }
    }
}
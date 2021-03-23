using System.Linq;
using System.Collections.Generic;
using Autodesk.Max;
using BabylonExport.Entities;
using System;

namespace Max2Babylon
{

    public class PbrGameMaterialDecorator : MaterialDecorator
    {
        private static readonly float[] Point3Default = new float[] { 0, 0, 0 };

        protected static ITexmap _getTexMap(IIGameMaterial materialNode, string name)
        {
            for (int i = 0; i < materialNode.MaxMaterial.NumSubTexmaps; i++)
            {
                if (materialNode.MaxMaterial.GetSubTexmapSlotName(i) == name)
                {
                    return materialNode.MaxMaterial.GetSubTexmap(i);
                }
            }
            return null;
        }

        BabylonCustomAttributeDecorator _babylonCAT;
        public PbrGameMaterialDecorator(IIGameMaterial node) : base(node)
        {
            _babylonCAT = new BabylonCustomAttributeDecorator(node);
        }

        public bool AmbientOcclusionAffectsDiffuse => Properties?.GetBoolProperty("ao_affects_diffuse", false) ?? false;
        public bool AmbientOcclusionAffectsReflection => Properties?.GetBoolProperty("ao_affects_reflection", false) ?? false;
        public bool NormalFlipRed => Properties?.GetBoolProperty("normal_flip_red", false) ?? false;
        public bool NormalFlipGreen => Properties?.GetBoolProperty("normal_flip_green", false) ?? false;
        public IColor BaseColor => _node.MaxMaterial.GetDiffuse(0, false);
        public ITexmap BaseColorMap => _getTexMap(_node, "base_color_map");
        public int UseGlossiness => Properties?.GetIntProperty("useGlossiness", 0) ?? 0;
        public ITexmap AmbientOcclusionMap => _getTexMap(_node, "ao_map");
        public float BumpMapAmount => Properties?.GetFloatProperty("bump_map_amt", 1.0f) ?? 1.0f;
        public ITexmap NormalMap => _getTexMap(_node, "norm_map");
        public IColor EmitColor => _node.MaxMaterial.GetSelfIllumColor(0, false); 
        public ITexmap EmitColormMap => _getTexMap(_node, "emit_color_map");
        public ITexmap OpacityMap => _getTexMap(_node, "opacity_map");

        public BabylonCustomAttributeDecorator BabylonCustomAttributes => _babylonCAT;
    }

    public class PbrMetalRoughDecorator : PbrGameMaterialDecorator
    {
        public PbrMetalRoughDecorator(IIGameMaterial node) : base(node)
        {
        }

        public float Metalness => Properties?.GetFloatProperty("metalness", 0) ?? 0;
        public ITexmap MetalnessMap => _getTexMap(_node, "metalness_map");
        public float Roughness => Properties?.GetFloatProperty("metalness", 0) ?? 0;
        public ITexmap RoughnessMap => _getTexMap(_node, "roughness_map");
    }

    public class PbrSpecGlossDecorator : PbrGameMaterialDecorator
    {
        public PbrSpecGlossDecorator(IIGameMaterial node) : base(node)
        {
        }

        public IPoint3 Specular => Properties?.GetPoint3Property("Specular");
        public ITexmap SpecularMap => _getTexMap(_node, "specular_map");
        public float Glossiness => Properties?.GetFloatProperty("glossiness", 0) ?? 0;
        public ITexmap GlossinessMap => _getTexMap(_node, "glossiness_map");
    }

    /// <summary>
    /// The Exporter
    /// </summary>
    partial class BabylonExporter
    {
        private void ExportPbrMetalRoughMaterial(IIGameMaterial materialNode, BabylonScene babylonScene)
        {
            PbrMetalRoughDecorator maxDecorator = new PbrMetalRoughDecorator(materialNode);
            BabylonCustomAttributeDecorator babylonDecorator = maxDecorator.BabylonCustomAttributes;

            var babylonMaterial = new BabylonPBRMetallicRoughnessMaterial(maxDecorator.Id)
            {
                maxGameMaterial = materialNode,
                name = maxDecorator.Name
            };

            bool usePbrFactor = babylonDecorator.UseMaxFactor; // this force the exporter to set the metallic or roughness even if the map are set

            // --- Global ---
            babylonMaterial.baseColor = maxDecorator.BaseColor.ToArray();
            babylonMaterial.isUnlit = maxDecorator.BabylonCustomAttributes.IsUnlit;

            if (babylonMaterial.isUnlit)
            {
                // Ignore specified roughness and metallic values
                babylonMaterial.metallic = 0;
                babylonMaterial.roughness = 0.9f;
            }
            else 
            { 
                babylonMaterial.metallic = maxDecorator.Metalness;
                babylonMaterial.roughness = maxDecorator.Roughness;
                babylonMaterial.emissive = maxDecorator.EmitColor.ToArray();
            }

            // --- Textures ---
            float[] multiplyColor = null;
            if (exportParameters.exportTextures)
            {
                // 1 - base color ; 0 - transparency weight
                ITexmap colorTexmap = maxDecorator.BaseColorMap;
                ITexmap alphaTexmap = null;

                babylonMaterial.baseTexture = ExportBaseColorAlphaTexture(colorTexmap, alphaTexmap, babylonMaterial.baseColor, babylonMaterial.alpha, babylonScene, out multiplyColor);
                if (multiplyColor != null)
                {
                    babylonMaterial.baseColor = multiplyColor;
                }

                if (!babylonMaterial.isUnlit)
                {
                    // Metallic, roughness, ambient occlusion
                    ITexmap metallicTexmap = maxDecorator.MetalnessMap;
                    ITexmap roughnessTexmap = maxDecorator.RoughnessMap;
                    ITexmap ambientOcclusionTexmap = maxDecorator.AmbientOcclusionMap;

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
                            BabylonTexture ormTexture = ExportORMTexture(exportParameters.mergeAOwithMR ? ambientOcclusionTexmap : null, roughnessTexmap, metallicTexmap, babylonMaterial.metallic, babylonMaterial.roughness, babylonScene, false);
                            babylonMaterial.metallicRoughnessTexture = ormTexture;

                            if (ambientOcclusionTexmap != null)
                            {
                                if (exportParameters.mergeAOwithMR)
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
                    if (ambientOcclusionTexmap != null && !exportParameters.mergeAOwithMR && babylonMaterial.occlusionTexture == null)
                    {
                        RaiseVerbose("Exporting occlusion texture without merging with metallic roughness", 2);
                        babylonMaterial.occlusionTexture = ExportTexture(ambientOcclusionTexmap, babylonScene);
                    }

                    var normalMapAmount = maxDecorator.BumpMapAmount;
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

            if (babylonMaterial.alpha != 1.0f || (babylonMaterial.baseTexture != null && babylonMaterial.baseTexture.hasAlpha))
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
            if (maxDecorator.Properties == null)
            {
                AddPhysicalBabylonAttributes(materialNode.MaterialName, babylonMaterial);
            }

            if (maxDecorator.Properties != null)
            {
                RaiseVerbose("Babylon Attributes of " + materialNode.MaterialName, 2);

                // Common attributes
                ExportCommonBabylonAttributes(maxDecorator.Properties, babylonMaterial);
                babylonMaterial._unlit = babylonMaterial.isUnlit;

                // Backface culling
                bool backFaceCulling = babylonDecorator.BackFaceCulling;
                RaiseVerbose($"backFaceCulling={backFaceCulling}", 3);
                babylonMaterial.backFaceCulling = backFaceCulling;
                babylonMaterial.doubleSided = !backFaceCulling;
            }

            // List all babylon material attributes
            // Those attributes are currently stored into the native material
            // They should not be exported as extra attributes
            var doNotExport = BabylonCustomAttributeDecorator.ListPropertyNames().ToList();

            // Export the custom attributes of this material
            babylonMaterial.metadata = ExportExtraAttributes(materialNode, babylonScene, doNotExport);

            if (exportParameters.pbrFull)
            {
                var fullPBR = new BabylonPBRMaterial(babylonMaterial)
                {
                    directIntensity = babylonDecorator.DirectIntensity,
                    emissiveIntensity = babylonDecorator.EmissiveIntensity,
                    environmentIntensity = babylonDecorator.EnvironementIntensity,
                    specularIntensity = babylonDecorator.SpecularIntensity,
                    maxGameMaterial = babylonMaterial.maxGameMaterial
                };
                babylonScene.MaterialsList.Add(fullPBR);
            }
            else
            {
                // Add the material to the scene
                babylonScene.MaterialsList.Add(babylonMaterial);
            }

            babylonScene.MaterialsList.Add(babylonMaterial);
        }
		
        public bool isPbrMetalRoughMaterial(IIGameMaterial materialNode)
        {
            return ClassIDWrapper.PBR_MetalRough_Material.Equals(materialNode.MaxMaterial.ClassID);
        }
    }
}
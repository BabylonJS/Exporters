using System.Collections.Generic;
using System.Linq;
using Autodesk.Max;
using BabylonExport.Entities;

namespace Max2Babylon
{
    /// <summary>
    /// PbrGameMaterial decorator homogenizes and simplifies PbrGameMaterial properties. 
    /// </summary>
    public class PbrGameMaterialDecorator : MaterialDecorator
    {


        // add temporary cache to optimize the map access.
        IDictionary<string, ITexmap> _mapCaches ;
        
        BabylonCustomAttributeDecorator _babylonCAT;

        public PbrGameMaterialDecorator(IIGameMaterial node) : base(node)
        {
            _babylonCAT = new BabylonCustomAttributeDecorator(node);
        }

        public virtual bool AmbientOcclusionAffectsDiffuse => Properties?.GetBoolProperty("ao_affects_diffuse", false) ?? false;
        public virtual bool AmbientOcclusionAffectsReflection => Properties?.GetBoolProperty("ao_affects_reflection", false) ?? false;
        public virtual bool NormalFlipRed => Properties?.GetBoolProperty("normal_flip_red", false) ?? false;
        public virtual bool NormalFlipGreen => Properties?.GetBoolProperty("normal_flip_green", false) ?? false;
        public virtual IColor BaseColor => _node.MaxMaterial.GetDiffuse(0, false);
        public virtual ITexmap BaseColorMap => _getTexMap(_node, "base_color_map");
        public virtual int UseGlossiness => Properties?.GetIntProperty("useGlossiness", 0) ?? 0;
        public virtual ITexmap AmbientOcclusionMap => _getTexMap(_node, "ao_map");
        public virtual float BumpMapAmount => Properties?.GetFloatProperty("bump_map_amt", 1.0f) ?? 1.0f;
        public virtual ITexmap NormalMap => _getTexMap(_node, "norm_map");
        public virtual IColor EmitColor => _node.MaxMaterial.GetSelfIllumColor(0, false); 
        public virtual ITexmap EmitColormMap => _getTexMap(_node, "emit_color_map");
        public ITexmap OpacityMap => _getTexMap(_node, "opacity_map");

        public BabylonCustomAttributeDecorator BabylonCustomAttributes => _babylonCAT;


        protected ITexmap _getTexMapWithCache(IIGameMaterial materialNode, string name)
        {
            var materialName = materialNode.MaterialName;
            if(_mapCaches == null)
            {
                _mapCaches = new Dictionary<string, ITexmap>();
                for (int i = 0; i < materialNode.MaxMaterial.NumSubTexmaps; i++)
                {
                    var mn = materialNode.MaxMaterial.GetSubTexmapSlotName(i);
                    _mapCaches.Add(mn, materialNode.MaxMaterial.GetSubTexmap(i));
                }
            }
            if (_mapCaches.TryGetValue(name, out ITexmap texmap))
            {
                return texmap;
            }
            // max 2022 maj introduce a change into the naming of the map.
            // the SDK do not return the name of the map anymore but a display name with camel style and space
            // Here a fix which maintain the old style and transform the name for a second try if failed.
            name = string.Join(" ", name.Split('_').Select(s => char.ToUpper(s[0]) + s.Substring(1)));
            return _mapCaches.TryGetValue(name, out texmap) ? texmap : null;
        }

        protected ITexmap _getTexMap(IIGameMaterial materialNode, string name, bool cache = true)
        {
            if (cache)
            {
                return _getTexMapWithCache(materialNode, name);
            }

            for (int k = 0; k < 2; k++)
            {
                for (int i = 0; i < materialNode.MaxMaterial.NumSubTexmaps; i++)
                {
                    if (materialNode.MaxMaterial.GetSubTexmapSlotName(i) == name)
                    {
                        return materialNode.MaxMaterial.GetSubTexmap(i);
                    }
                }
                // max 2022 maj introduce a change into the naming of the map.
                // the SDK do not return the name of the map anymore but a display name with camel style and space
                // Here a fix which maintain the old style and transform the name for a second try if failed.
                name = string.Join(" ", name.Split('_').Select(s => char.ToUpper(s[0]) + s.Substring(1)));
            }
            return null;
        }
    }

    public class PbrMetalRoughDecorator : PbrGameMaterialDecorator
    {
        public PbrMetalRoughDecorator(IIGameMaterial node) : base(node)
        {
        }

        public float Metalness => Properties?.GetFloatProperty("metalness", 0) ?? 0;
        public ITexmap MetalnessMap => _getTexMap(_node, "metalness_map");
        public float Roughness => Properties?.GetFloatProperty("roughness", 0) ?? 0;
        public ITexmap RoughnessMap => _getTexMap(_node, "roughness_map");
     }

    public class PbrSpecGlossDecorator : PbrGameMaterialDecorator
    {
        public PbrSpecGlossDecorator(IIGameMaterial node) : base(node)
        {
        }

        public IPoint3 SpecularColor => Properties?.GetPoint3Property("Specular");
        public ITexmap SpecularMap => _getTexMap(_node, "specular_map");
        public float Glossiness => Properties?.GetFloatProperty("glossiness", 0) ?? 0;
        public ITexmap GlossinessMap => _getTexMap(_node, "glossiness_map");
    }

    /// <summary>
    /// The Exporter
    /// </summary>
    partial class BabylonExporter
    {
        /// <summary>
        /// Export dedicated to PbrMetalRough Material
        /// </summary>
        /// <param name="materialNode">the material node interface</param>
        /// <param name="babylonScene">the scene to export the material</param>
        private void ExportPbrMetalRoughMaterial(IIGameMaterial materialNode, BabylonScene babylonScene)
        {
            // build material decorator
            PbrMetalRoughDecorator maxDecorator = new PbrMetalRoughDecorator(materialNode);
            // get the custom babylon attribute decorator
            BabylonCustomAttributeDecorator babylonDecorator = maxDecorator.BabylonCustomAttributes;

            // the target material
            var babylonMaterial = new BabylonPBRMetallicRoughnessMaterial(maxDecorator.Id)
            {
                maxGameMaterial = materialNode,
                name = maxDecorator.Name,
                backFaceCulling = babylonDecorator.BackFaceCulling,
                doubleSided = !babylonDecorator.BackFaceCulling,
                separateCullingPass = babylonDecorator.SeparateCullingPass,
                isUnlit = babylonDecorator.IsUnlit,
                _unlit = babylonDecorator.IsUnlit,
                baseColor = maxDecorator.BaseColor.ToArray()
            };

            // --- Global ---
            if (babylonMaterial.isUnlit)
            {
                // Ignore values
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
                ITexmap baseColorTexMap = maxDecorator.BaseColorMap;
                ITexmap alphaTexMap = maxDecorator.OpacityMap;
                bool isOpacity = true;
                babylonMaterial.baseTexture = ExportBaseColorAlphaTexture(baseColorTexMap, alphaTexMap, babylonMaterial.baseColor, babylonMaterial.alpha, babylonScene, out multiplyColor, isOpacity);

                if (multiplyColor != null)
                {
                    babylonMaterial.baseColor = multiplyColor;
                }

                if (!babylonMaterial.isUnlit)
                {
                    // Metallic, roughness, ambient occlusion
                    ITexmap metalnessTexMap = maxDecorator.MetalnessMap;
                    ITexmap roughnessTexMap = maxDecorator.RoughnessMap;
                    ITexmap ambientOcclusionTexMap = maxDecorator.AmbientOcclusionMap;

                    // Check if MR or ORM textures are already merged
                    bool areTexturesAlreadyMerged = false; 
                    if (metalnessTexMap != null && roughnessTexMap != null)
                    {
                        string sourcePathMetallic = getSourcePath(metalnessTexMap);
                        string sourcePathRoughness = getSourcePath(roughnessTexMap);

                        if (sourcePathMetallic == sourcePathRoughness)
                        {
                            if (ambientOcclusionTexMap != null && exportParameters.mergeAO)
                            {
                                string sourcePathAmbientOcclusion = getSourcePath(ambientOcclusionTexMap);
                                if (sourcePathMetallic == sourcePathAmbientOcclusion)
                                {
                                    // Metallic, roughness and ambient occlusion are already merged
                                    RaiseVerbose("Metallic, roughness and ambient occlusion are already merged", 2);
                                    BabylonTexture ormTexture = ExportTexture(metalnessTexMap, babylonScene);
                                    babylonMaterial.metallicRoughnessTexture = ormTexture;
                                    babylonMaterial.occlusionTexture = ormTexture;
                                    areTexturesAlreadyMerged = true;
                                }
                            }
                            else
                            {
                                // Metallic and roughness are already merged
                                RaiseVerbose("Metallic and roughness are already merged", 2);
                                BabylonTexture ormTexture = ExportTexture(metalnessTexMap, babylonScene);
                                babylonMaterial.metallicRoughnessTexture = ormTexture;
                                areTexturesAlreadyMerged = true;
                            }
                        }
                    }
                    if (areTexturesAlreadyMerged == false)
                    {
                        if (metalnessTexMap != null || roughnessTexMap != null)
                        {
                            // Merge metallic, roughness and ambient occlusion
                            RaiseVerbose("Merge metallic and roughness (and ambient occlusion if `mergeAOwithMR` is enabled)", 2);
                            BabylonTexture ormTexture = ExportORMTexture(exportParameters.mergeAO ? ambientOcclusionTexMap : null, roughnessTexMap, metalnessTexMap, babylonMaterial.metallic, babylonMaterial.roughness, babylonScene, false);
                            babylonMaterial.metallicRoughnessTexture = ormTexture;

                            if (ambientOcclusionTexMap != null)
                            {
                                if (exportParameters.mergeAO)
                                {
                                    // if the ambient occlusion texture map uses a different set of texture coordinates than
                                    // metallic roughness, create a new instance of the ORM BabylonTexture with the different texture
                                    // coordinate indices
                                    var ambientOcclusionTexture = _getBitmapTex(ambientOcclusionTexMap);
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
                                    babylonMaterial.occlusionTexture = ExportTexture(ambientOcclusionTexMap, babylonScene);
                                }
                            }
                        }
                        else if (ambientOcclusionTexMap != null)
                        {
                            // Simply export occlusion texture
                            RaiseVerbose("Simply export occlusion texture", 2);
                            babylonMaterial.occlusionTexture = ExportTexture(ambientOcclusionTexMap, babylonScene);
                        }
                    }
                    if (ambientOcclusionTexMap != null && !exportParameters.mergeAO && babylonMaterial.occlusionTexture == null)
                    {
                        RaiseVerbose("Exporting occlusion texture without merging with metallic roughness", 2);
                        babylonMaterial.occlusionTexture = ExportTexture(ambientOcclusionTexMap, babylonScene);
                    }

                    var normalMapAmount = maxDecorator.BumpMapAmount;
                    ITexmap normalTexMap = maxDecorator.NormalMap;
                    babylonMaterial.normalTexture = ExportTexture(normalTexMap, babylonScene, normalMapAmount);

                    ITexmap emitTexMap = maxDecorator.EmitColormMap;
                    babylonMaterial.emissiveTexture = ExportTexture(emitTexMap , babylonScene);

                    if (babylonMaterial.metallicRoughnessTexture != null && !babylonDecorator.UseMaxFactor)
                    {
                        // Change the factor to zero if combining partial channel to avoid issue (in case of image compression).
                        // ie - if no metallic map, then b MUST be fully black. However channel of jpeg MAY not beeing fully black 
                        // cause of the compression algorithm. Keeping MetallicFactor to 1 will make visible artifact onto texture. So set to Zero instead.
                        babylonMaterial.metallic = areTexturesAlreadyMerged || metalnessTexMap != null ? 1.0f : 0.0f;
                        babylonMaterial.roughness = areTexturesAlreadyMerged || roughnessTexMap != null ? 1.0f : 0.0f;
                    }
                }
            }

            if (babylonMaterial.alpha != 1.0f || (babylonMaterial.baseTexture != null && babylonMaterial.baseTexture.hasAlpha))
            {
                babylonMaterial.transparencyMode = (int)BabylonMaterial.TransparencyMode.ALPHABLEND;
            }

            if (babylonMaterial.transparencyMode == (int)BabylonMaterial.TransparencyMode.ALPHATEST)
            {
                // Set the alphaCutOff value explicitely to avoid different interpretations on different engines
                // Use the glTF default value rather than the babylon one
                babylonMaterial.alphaCutOff = 0.5f;
            }


            if (babylonMaterial.emissiveTexture != null)
            {
                babylonMaterial.emissive = new[] { 1.0f, 1.0f, 1.0f };
            }

            // Add babylon attributes
            if (babylonDecorator.Properties == null)
            {
                AddPhysicalBabylonAttributes(materialNode.MaterialName, babylonMaterial);
            }


            // List all babylon material attributes
            // Those attributes are currently stored into the native material
            // They should not be exported as extra attributes
            var doNotExport = BabylonCustomAttributeDecorator.ListPrivatePropertyNames().ToList();

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
        }

        /// <summary>
        /// Export dedicated to SpecGloss Material
        /// </summary>
        /// <param name="materialNode">the material node interface</param>
        /// <param name="babylonScene">the scene to export the material</param>
        private void ExportPbrSpecGlossMaterial(IIGameMaterial materialNode, BabylonScene babylonScene)
        {
            // build material decorator
            PbrSpecGlossDecorator maxDecorator = new PbrSpecGlossDecorator(materialNode);
            // get the custom babylon attribute decorator
            BabylonCustomAttributeDecorator babylonDecorator = maxDecorator.BabylonCustomAttributes;

            // the target material
            var babylonMaterial = new BabylonPBRSpecularGlossinessMaterial(maxDecorator.Id)
            {
                 maxGameMaterial = materialNode,
                 name = maxDecorator.Name,
                 backFaceCulling = babylonDecorator.BackFaceCulling,
                 doubleSided = !babylonDecorator.BackFaceCulling,
                 separateCullingPass = babylonDecorator.SeparateCullingPass,
                 isUnlit = babylonDecorator.IsUnlit,
                 baseColor = maxDecorator.BaseColor.ToArray(),
             };

            // --- Global ---
            if (babylonMaterial.isUnlit)
            {
                // Ignore values
                babylonMaterial.specularColor = BabylonPBRBaseSimpleMaterial.BlackColor();
                babylonMaterial.glossiness = 0;
            }
            else
            {
                babylonMaterial.glossiness = maxDecorator.Glossiness;
                babylonMaterial.specularColor = maxDecorator.SpecularColor.ToArray();
                babylonMaterial.emissive = maxDecorator.EmitColor.ToArray();
            }

            // --- Textures ---
            float[] multiplyColor = null;
            if (exportParameters.exportTextures)
            {
                ITexmap diffuseTexMap = maxDecorator.BaseColorMap;
                ITexmap alphaTexMap = maxDecorator.OpacityMap;
                bool isOpacity = true;
                babylonMaterial.diffuseTexture = ExportBaseColorAlphaTexture(diffuseTexMap, alphaTexMap, babylonMaterial.baseColor, babylonMaterial.alpha, babylonScene, out multiplyColor, isOpacity);
                if (multiplyColor != null)
                {
                    babylonMaterial.baseColor = multiplyColor;
                }

                if (!babylonMaterial.isUnlit)
                {
                    // Metallic, roughness, ambient occlusion
                    ITexmap specularTexMap = maxDecorator.SpecularMap;
                    ITexmap glossinessTexMap = maxDecorator.GlossinessMap;
                    ITexmap ambientOcclusionTexMap = maxDecorator.AmbientOcclusionMap;

                    if (specularTexMap != null || glossinessTexMap != null)
                    {
                        // Merge Specular and Glossiness
                        RaiseVerbose("Merge Specular and Glossiness", 2);
                        BabylonTexture specularGlossinessTexture = ExportSpecularGlossinessTexture(babylonMaterial.specularColor, specularTexMap, babylonMaterial.glossiness, glossinessTexMap, babylonScene);
                        babylonMaterial.specularGlossinessTexture = specularGlossinessTexture;

                    }
                    
                    if (ambientOcclusionTexMap != null)
                    {
                        // Simply export occlusion texture
                        RaiseVerbose("Export occlusion texture", 2);
                        babylonMaterial.occlusionTexture = ExportTexture(ambientOcclusionTexMap, babylonScene);
                    }

                    var normalMapAmount = maxDecorator.BumpMapAmount;
                    ITexmap normalTexMap = maxDecorator.NormalMap;
                    babylonMaterial.normalTexture = ExportTexture(normalTexMap, babylonScene, normalMapAmount);

                    ITexmap emitTexMap = maxDecorator.EmitColormMap;
                    babylonMaterial.emissiveTexture = ExportTexture(emitTexMap, babylonScene);

                    if (babylonMaterial.specularGlossinessTexture != null && !babylonDecorator.UseMaxFactor)
                    {
                        babylonMaterial.glossiness = glossinessTexMap != null ? 1.0f : 0.0f;
                        babylonMaterial.specularColor = specularTexMap != null ? BabylonPBRBaseSimpleMaterial.WhiteColor() : BabylonPBRBaseSimpleMaterial.BlackColor();
                    }
                }
            }


            // --- Finalize ---
            if (babylonMaterial.alpha != 1.0f || (babylonMaterial.diffuseTexture != null && babylonMaterial.diffuseTexture.hasAlpha))
            {
                babylonMaterial.transparencyMode = (int)BabylonMaterial.TransparencyMode.ALPHABLEND;
            }

            if (babylonMaterial.transparencyMode == (int)BabylonMaterial.TransparencyMode.ALPHATEST)
            {
                // Set the alphaCutOff value explicitely to avoid different interpretations on different engines
                // Use the glTF default value rather than the babylon one
                babylonMaterial.alphaCutOff = 0.5f;
            }

            if (babylonMaterial.emissiveTexture != null)
            {
                babylonMaterial.emissive = new[] { 1.0f, 1.0f, 1.0f };
            }


            // List all babylon material attributes
            // Those attributes are currently stored into the native material
            // They should not be exported as extra attributes
            var doNotExport = BabylonCustomAttributeDecorator.ListPrivatePropertyNames().ToList();

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
        }

        public bool isPbrMetalRoughMaterial(IIGameMaterial materialNode)
        {
            return ClassIDWrapper.PBR_MetalRough_Material.Equals(materialNode.MaxMaterial.ClassID);
        }

        public bool isPbrSpecGlossMaterial(IIGameMaterial materialNode)
        {
            return ClassIDWrapper.PBR_SpecGloss_Material.Equals(materialNode.MaxMaterial.ClassID);
        }
    }
}
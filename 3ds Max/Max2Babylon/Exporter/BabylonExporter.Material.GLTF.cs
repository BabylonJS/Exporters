using System;
using Autodesk.Max;
using BabylonExport.Entities;

namespace Max2Babylon
{
    /// <summary>
    /// Decorator is used to access properties in a cleaner and centralized way
    /// </summary>
    public class GltfMaterialDecorator : MaterialDecorator
    {
        /// <summary>
        /// the Alpha mode as defined into the GltfMaterial
        /// </summary>
        public enum AlphaModeCode
        {
            OPAQUE = 1, // Max indices always start at 1.
            MASK = 2,
            BLEND = 3
        }

        // use as scale for GetSelfIllumColor to convert [0,PI] to [O,1]
        private const float selfIllumScale = (float)(1.0 / Math.PI);

        public GltfMaterialDecorator(IIGameMaterial node) : base(node)
        {
        }
        // Theses lines are extracted from the Material_gltf.ms script 
        // ----------------------------------------------------------
        // baseColor type:#frgba default:white ui:cpBaseColor localizedname:~GLTF_MATERIAL_PARAM_BASECOLOR~ nonlocalizedName:"Base Color"
        // baseColorMap type:#TextureMap ui:btnBaseColorMap localizedname:~GLTF_MATERIAL_PARAM_BASECOLOR_MAP~ nonlocalizedName:"Base Color Map"
        // alphaMode type:#integer default:1 ui:cbAlphaMode localizedname:~GLTF_MATERIAL_PARAM_ALPHA_MODE~ nonlocalizedName:"Alpha"
        // alphaMap type:#textureMap ui:btnAlphaMap localizedname:~GLTF_MATERIAL_PARAM_ALPHA_MAP~ nonlocalizedName:"Alpha Map"
        // alphaCutoff type:#float default:0.5 ui:spnAlphaCutoff localizedname:~GLTF_MATERIAL_PARAM_ALPHA_CUTOFF~ nonlocalizedName:"Cutoff"
        // metalness type:#float default:0.0 ui:spnMetalness localizedname:~GLTF_MATERIAL_PARAM_METALNESS~
        // metalnessMap type:#textureMap ui:btnMetalnessMap localizedname:~GLTF_MATERIAL_PARAM_METALNESS_MAP~ nonlocalizedName:"Metalness Map"
        // roughness type:#float default:0.5 ui:spnRoughness localizedname:~GLTF_MATERIAL_PARAM_ROUGHNESS~
        // roughnessMap type:#textureMap ui:btnRoughnessMap localizedname:~GLTF_MATERIAL_PARAM_ROUGHNESS_MAP~ nonlocalizedName:"Roughness Map"
        // normal type:#float default:1.0 ui:spnNormal localizedname:~GLTF_MATERIAL_PARAM_NORMAL~
        // normalMap type:#textureMap ui:btnNormalMap localizedname:~GLTF_MATERIAL_PARAM_NORMAL_MAP~ nonlocalizedName:"Normal Map"
        // ambientOcclusion type:#float default:1.0 ui:spnAmbientOcclusion localizedname:~GLTF_MATERIAL_PARAM_AO~ nonlocalizedName:"Occlusion (AO)"
        // ambientOcclusionMap type:#textureMap ui:btnAmbientOcclusionMap localizedname:~GLTF_MATERIAL_PARAM_AO_MAP~ nonlocalizedName:"Occlusion (AO) Map"
        // emissionColor type:#frgba default:black ui:cpEmissionColor localizedname:~GLTF_MATERIAL_PARAM_EMISSION~ nonlocalizedName:"Emission"
        // emissionMap type:#textureMap ui:btnEmissionMap localizedname:~GLTF_MATERIAL_PARAM_EMISSION_MAP~ nonlocalizedName:"Emission Map"
        // doubleSided type:#boolean default:false ui:cbDoubleSided localizedname:~GLTF_MATERIAL_PARAM_DOUBLE_SIDED~ nonlocalizedName:"Double Sided"
        public IColor BaseColor => _node.MaxMaterial.GetDiffuse(0, false); // delegate.base_color = val
        public ITexmap BaseColorMap => _getTexMap(_node, "baseColorMap");
        public int AlphaMode => Properties?.GetIntProperty("alphaMode", 1) ?? 1;
        public ITexmap AlphaMap => _getTexMap(_node, "alphaMap");
        public float AlphaCutOff => Properties?.GetFloatProperty("alphaCutoff", 0.5f) ?? 0.5f;
        public float Metalness => Properties?.GetFloatProperty("metalness", 0.0f) ?? 0.0f;
        public ITexmap MetalnessMap => _getTexMap(_node, "metalnessMap");
        public float Roughness => Properties?.GetFloatProperty("roughness", 0.0f) ?? 0.0f;
        public ITexmap RoughnessMap => _getTexMap(_node, "roughnessMap");
        public float Normal => Properties?.GetFloatProperty("normal", 0.0f) ?? 0.0f;
        public ITexmap NormalMap => _getTexMap(_node, "normalMap");
        public float AmbientOcclusion => Properties?.GetFloatProperty("ambientOcclusion", 0.0f) ?? 0.0f;
        public ITexmap AmbientOcclusionMap => _getTexMap(_node, "ambientOcclusionMap");
        public IColor EmissionColor => Properties?.GetColorProperty("emissionColor", null) ?? null;
        public ITexmap EmissionMap => _getTexMap(_node, "emissionMap");
        public bool DoubleSided => Properties?.GetBoolProperty("doubleSided", false) ?? false;
        public bool Unlit => Properties?.GetBoolProperty("unlit", false) ?? false;

        // Extensions:
        // -----------
        // enableClearcoat type:#boolean default:false ui:cbEnableClearcoat localizedname:~GLTF_MATERIAL_PARAM_ENABLE_CLEARCOAT~ nonlocalizedName:"Enable Clearcoat"
        // clearcoat type:#float default:1.0 ui:spnClearcoatFactor localizedname:~GLTF_MATERIAL_PARAM_CLEARCOAT~
        // clearcoatMap type:#TextureMap ui:btnClearcoatMap localizedname:~GLTF_MATERIAL_PARAM_CLEARCOAT_MAP~ nonlocalizedName:"Clearcoat Map"
        // clearcoatRoughness type:#float default:0.0 ui:spnClearcoatRoughness localizedname:~GLTF_MATERIAL_PARAM_CLEARCOAT_ROUGHNESS~ nonlocalizedName:"Clearcoat Roughness"
        // clearcoatRoughnessMap type:#TextureMap ui:btnClearcoatRoughnessMap localizedname:~GLTF_MATERIAL_PARAM_CLEARCOAT_ROUGHNESS_MAP~ nonlocalizedName:"Clearcoat Roughness Map"
        // clearcoatNormal type:#float default:1.0 ui:spnClearcoatNormal localizedname:~GLTF_MATERIAL_PARAM_CLEARCOAT_NORMAL~ nonlocalizedName:"Clearcoat Normal"
        // clearcoatNormalMap type:#TextureMap ui:btnClearcoatNormalMap localizedname:~GLTF_MATERIAL_PARAM_CLEARCOAT_NORMAL_MAP~ nonlocalizedName:"Clearcoat Normal Map"
        public bool EnableClearcoat => Properties?.GetBoolProperty("enableClearcoat", false) ?? false;
        public float Clearcoat => Properties?.GetFloatProperty("clearcoat", 1.0f) ?? 1.0f;
        public ITexmap ClearcoatMap => _getTexMap(_node, "clearcoatMap");
        public float ClearcoatRoughness => Properties?.GetFloatProperty("clearcoatRoughness", 0.0f) ?? 0.0f;
        public ITexmap ClearcoatRoughnessMap => _getTexMap(_node, "clearcoatRoughnessMap");
        public float ClearcoatNormal => Properties?.GetFloatProperty("clearcoatNormal", 1.0f) ?? 1.0f;
        public ITexmap ClearcoatNormalMap => _getTexMap(_node, "clearcoatNormalMap");

        // enableSheen type:#boolean default:false ui:cbEnableSheen localizedname:~GLTF_MATERIAL_PARAM_ENABLE_SHEEN~ nonlocalizedName:"Enable Sheen"
        // sheenColor type:#frgba default:white ui:cpSheenColor localizedname:~GLTF_MATERIAL_PARAM_SHEEN_COLOR~ nonlocalizedName:"Sheen Color"
        // sheenColorMap type:#TextureMap ui:btnSheenColorMap localizedname:~GLTF_MATERIAL_PARAM_SHEEN_COLOR_MAP~ nonlocalizedName:"Sheen Color Map"
        // sheenRoughness type:#float default:0.0 ui:spnSheenRoughness localizedname:~GLTF_MATERIAL_PARAM_SHEEN_ROUGHNESS~ nonlocalizedName:"Sheen Roughness"
        // sheenRoughnessMap type:#TextureMap ui:btnSheenRoughnessMap localizedname:~GLTF_MATERIAL_PARAM_SHEEN_ROUGHNESS_MAP~ nonlocalizedName:"Sheen Roughness Map"
        public bool EnableSheen => Properties?.GetBoolProperty("enableSheen", false) ?? false;
        public IColor SheenColor => Properties?.GetColorProperty("sheenColor", null) ?? null;
        public ITexmap SheenColorMap => _getTexMap(_node, "sheenColorMap");
        public float SheenRoughness => Properties?.GetFloatProperty("sheenRoughness", 0.0f) ?? 0.0f;
        public ITexmap SheenRoughnessMap => _getTexMap(_node, "sheenRoughnessMap");

        // enableSpecular type:#boolean default:false ui:cbEnableSpecular localizedname:~GLTF_MATERIAL_PARAM_ENABLE_SPECULAR~ nonlocalizedName:"Enable Specular"
        // specular type:#float default:1.0 ui:spnSpecularFactor localizedname:~GLTF_MATERIAL_PARAM_SPECULAR~
        // specularMap type:#TextureMap ui:btnSpecularMap localizedname:~GLTF_MATERIAL_PARAM_SPECULAR_MAP~ nonlocalizedName:"Specular Map"
        // specularColor type:#frgba default:white ui:cpSpecularColor localizedname:~GLTF_MATERIAL_PARAM_SPECULAR_COLOR~ nonlocalizedName:"Specular Color"
        // specularColorMap type:#TextureMap ui:btnSpecularColorMap localizedname:~GLTF_MATERIAL_PARAM_SPECULAR_COLOR_MAP~ nonlocalizedName:"Specular Color Map"
        public bool EnableSpecular => Properties?.GetBoolProperty("enableSpecular", false) ?? false;
        public float Specular => Properties?.GetFloatProperty("specular", 1.0f) ?? 1.0f;
        public ITexmap SpecularMap => _getTexMap(_node, "specularMap");
        public IColor specularColor => Properties?.GetColorProperty("specularColor", null) ?? null;
        public ITexmap SpecularColorMap => _getTexMap(_node, "specularColorMap");

        // enableTransmission type:#boolean default:false ui:cbEnableTransmission localizedname:~GLTF_MATERIAL_PARAM_ENABLE_TRANSMISSION~ nonlocalizedName:"Enable Transmission"
        // transmission type:#float default:1.0 ui:spnTransmissionFactor localizedname:~GLTF_MATERIAL_PARAM_TRANSMISSION~
        // transmissionMap type:#TextureMap ui:btnTransmissionMap localizedname:~GLTF_MATERIAL_PARAM_TRANSMISSION_MAP~ nonlocalizedName:"Transmission Map"
        public bool EnableTransmission => Properties?.GetBoolProperty("enableTransmission", false) ?? false;
        public float Transmission => Properties?.GetFloatProperty("transmission", 1.0f) ?? 1.0f;
        public ITexmap TransmissionMap => _getTexMap(_node, "transmissionMap");

        // enableVolume type:#boolean default:false ui:cbEnableVolume localizedname:~GLTF_MATERIAL_PARAM_ENABLE_VOLUME~ nonlocalizedName:"Enable Volume"
        // volumeThickness type:#float default:0.0 ui:spnVolumeThickness localizedname:~GLTF_MATERIAL_PARAM_VOLUME_THICKNESS~ nonlocalizedName:"Volume Thickness"
        // volumeThicknessMap type:#TextureMap ui:btnVolumeThicknessMap localizedname:~GLTF_MATERIAL_PARAM_VOLUME_THICKNESS_MAP~ nonlocalizedName:"Volume Thickness Map"
        // volumeDistance type:#float default:0.0 ui:spnVolumeDistance localizedname:~GLTF_MATERIAL_PARAM_VOLUME_DISTANCE~ nonlocalizedName:"Volume Distance"
        // volumeColor type:#frgba default:white ui:cpVolumeColor localizedname:~GLTF_MATERIAL_PARAM_VOLUME_COLOR~ nonlocalizedName:"Volume Color"
        public bool EnableVolume => Properties?.GetBoolProperty("enableVolume", false) ?? false;
        public float VolumeThickness => Properties?.GetFloatProperty("volumeThickness", 0.0f) ?? 0.0f;
        public ITexmap VolumeThicknessMap => _getTexMap(_node, "volumeThicknessMap");
        public float VolumeDistance => Properties?.GetFloatProperty("volumeDistance", 0.0f) ?? 0.0f;
        public IColor VolumeColor => Properties?.GetColorProperty("volumeColor", null) ?? null;

        // enableIndexOfRefraction type:#boolean default:false ui:cbEnableIOR localizedname:~GLTF_MATERIAL_PARAM_ENABLE_IOR~ nonlocalizedName:"Enable IOR"
        // indexOfRefraction type:#float default:1.5 ui:spnIOR localizedname:~GLTF_MATERIAL_PARAM_IOR~ nonlocalizedName:"IOR"
        public bool EnableIndexOfRefraction => Properties?.GetBoolProperty("enableIndexOfRefraction", false) ?? false;
        public float indexOfRefraction => Properties?.GetFloatProperty("indexOfRefraction", 1.5f) ?? 1.5f;

        // For additional information, using the folowing script
        //
        // fn showSelectedProperties = ( node = SME.GetMtlinParamEditor(); print (classof node) ; showProperties node ;)
        //
        // we obtain the list of the properties as seen by the SDK
        //
        // .baseColor(Base_Color) : color
        // .baseColorMap(Base_Color_Map) : texturemap
        // .alphaMode(Alpha) : integer
        // .AlphaMap(Alpha_Map) : texturemap
        // .alphaCutoff(Cutoff) : float
        // .metalness : float
        // .metalnessMap(Metalness_Map) : texturemap
        // .roughness : float
        // .roughnessMap(Roughness_Map) : texturemap
        // .normal : float
        // .normalMap(Normal_Map) : texturemap
        // .ambientOcclusion(Occlusion__AO) : float
        // .ambientOcclusionMap(Occlusion__AO__Map) : texturemap
        // .emissionColor(Emission) : color
        // .emissionMap(Emission_Map) : texturemap
        // .DoubleSided(Double_Sided) : boolean
        // .unlit : boolean
        //  ----------- 
        //  Extensions
        //  -----------
        // .enableClearCoat(Enable_Clearcoat) : boolean
        // .clearcoat : float
        // .clearcoatMap(Clearcoat_Map) : texturemap
        // .clearcoatRoughness(Clearcoat_Roughness) : float
        // .clearcoatRoughnessMap(Clearcoat_Roughness_Map) : texturemap
        // .clearcoatNormal(Clearcoat_Normal) : float
        // .clearcoatNormalMap(Clearcoat_Normal_Map) : texturemap
        // .enableSheen(Enable_Sheen) : boolean
        // .sheenColor(Sheen_Color) : color
        // .sheenColorMap(Sheen_Color_Map) : texturemap
        // .sheenRoughness(Sheen_Roughness) : float
        // .sheenRoughnessMap(Sheen_Roughness_Map) : texturemap
        // .enableSpecular(Enable_Specular) : boolean
        // .Specular : float
        // .specularMap(Specular_Map) : texturemap
        // .specularcolor(Specular_Color) : color
        // .specularColorMap(Specular_Color_Map) : texturemap
        // .enableTransmission(Enable_Transmission) : boolean  
        // .transmission : float
        // .transmissionMap(Transmission_Map) : texturemap
        // .enableVolume(Enable_Volume) : boolean
        // .volumeThickness(Volume_Thickness) : float
        // .volumeThicknessMap(Volume_Thickness_Map) : texturemap
        // .volumeDistance(Volume_Distance) : float
        // .volumeColor(Volume_Color) : color
        // .enableIndexOfRefraction(Enable_IOR) : boolean
        // .indexOfRefraction(IOR) : float
    }

    /// <summary>
    /// The Exporter part dedicated to Autodesk Gltf Material.
    /// </summary>
    partial class BabylonExporter
    {
        /// <summary>
        /// Export dedicated to Autodesk GLTF Material
        /// </summary>
        /// <param name="materialNode">the material node interface</param>
        /// <param name="babylonScene">the scene to export the material</param>
        private void ExportGLTFMaterial(IIGameMaterial materialNode, BabylonScene babylonScene)
        {
            // the unobvious part is that we MUST first export the material as Babylon to allow
            // the Babylon2GLTF export to rebuild the coresponding material and extensions.
            GltfMaterialDecorator maxDecorator = new GltfMaterialDecorator(materialNode);

            ///////////////////////////////////////////
            // The target Metallic Rougness material //
            ///////////////////////////////////////////
            var babylonMaterial = new BabylonPBRMaterial(maxDecorator.Id)
            {
                maxGameMaterial = materialNode,
                name = maxDecorator.Name,
                albedo = maxDecorator.BaseColor.ToArray(),
                isUnlit = maxDecorator.Unlit
            };

            try
            {
                //////////////////////////////////////////////////////
                ///    the Physical base without KHR Extensions     //
                //////////////////////////////////////////////////////  

                ExportPhysicalBase(maxDecorator, babylonMaterial, babylonScene);

                ////////////////////////////////
                ///    the KHR Extensions     //
                ////////////////////////////////  

                ExportClearCoat(maxDecorator, babylonMaterial, babylonScene);
                ExportSheen(maxDecorator, babylonMaterial, babylonScene);
                ExportTransmission(maxDecorator, babylonMaterial, babylonScene);
                ExportIOR(maxDecorator, babylonMaterial, babylonScene);
                ExportSpecular(maxDecorator, babylonMaterial, babylonScene);
                ExportVolume(maxDecorator, babylonMaterial, babylonScene);
            }
            finally
            {
                // finally add the material to the list
                babylonScene.MaterialsList.Add(babylonMaterial);
            }
        }
        public bool isGLTFMaterial(IIGameMaterial materialNode) => ClassIDWrapper.Gltf_Material.Equals(materialNode.MaxMaterial.ClassID);
        internal void ExportPhysicalBase(GltfMaterialDecorator maxDecorator, BabylonPBRMaterial babylonMaterial, BabylonScene babylonScene, bool invertRoughness = false)
        {
            if (!babylonMaterial.isUnlit)
            {
                babylonMaterial.metallic = maxDecorator.Metalness;
                babylonMaterial.roughness = maxDecorator.Roughness;
                babylonMaterial.emissive = maxDecorator.EmissionColor?.ToArray();
            }
            else
            {
                // Ignore values
                babylonMaterial.metallic = 0;
                babylonMaterial.roughness = 0.9f;
            }

            ExportAlphaMode(maxDecorator, babylonMaterial);

            if (exportParameters.exportTextures)
            {
                ITexmap baseColorTexMap = maxDecorator.BaseColorMap;
                ITexmap alphaTexMap = maxDecorator.AlphaMap;
                bool isOpacity = true;
                babylonMaterial.albedoTexture = ExportBaseColorAlphaTexture(baseColorTexMap, alphaTexMap, babylonMaterial.albedo, babylonMaterial.alpha, babylonScene, out float[] multiplyColor, isOpacity);
                if (multiplyColor != null)
                {
                    babylonMaterial.albedo = multiplyColor;
                }

                if (!babylonMaterial.isUnlit)
                {
                    // ORM
                    ITexmap metalnessTexMap = maxDecorator.MetalnessMap;
                    ITexmap roughnessTexMap = maxDecorator.RoughnessMap;
                    ITexmap ambientOcclusionTexmap = maxDecorator.AmbientOcclusionMap;

                    // Check if MR or ORM textures are already merged
                    bool areTexturesAlreadyMerged = false;
                    if (metalnessTexMap != null && roughnessTexMap != null)
                    {
                        string sourcePathMetallic = getSourcePath(metalnessTexMap);
                        string sourcePathRoughness = getSourcePath(roughnessTexMap);

                        if (sourcePathMetallic == sourcePathRoughness)
                        {
                            if (ambientOcclusionTexmap != null && exportParameters.mergeAO)
                            {
                                string sourcePathAmbientOcclusion = getSourcePath(ambientOcclusionTexmap);
                                if (sourcePathMetallic == sourcePathAmbientOcclusion)
                                {
                                    // Metallic, roughness and ambient occlusion are already merged
                                    RaiseVerbose("Metallic, roughness and ambient occlusion are already merged", 2);
                                    BabylonTexture ormTexture = ExportTexture(metalnessTexMap, babylonScene);
                                    babylonMaterial.metallicTexture = ormTexture;
                                    babylonMaterial.ambientTexture = ormTexture;
                                    areTexturesAlreadyMerged = true;
                                }
                            }
                            else
                            {
                                // Metallic and roughness are already merged
                                RaiseVerbose("Metallic and roughness are already merged", 2);
                                BabylonTexture ormTexture = ExportTexture(metalnessTexMap, babylonScene);
                                babylonMaterial.metallicTexture = ormTexture;
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
                            BabylonTexture ormTexture = ExportORMTexture(exportParameters.mergeAO ? ambientOcclusionTexmap : null, 
                                roughnessTexMap, 
                                metalnessTexMap, 
                                babylonMaterial.metallic.Value, 
                                babylonMaterial.roughness.Value, 
                                babylonScene, 
                                invertRoughness);
                            babylonMaterial.metallicTexture = ormTexture;

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
                                        babylonMaterial.ambientTexture = new BabylonTexture(ormTexture);
                                        babylonMaterial.ambientTexture.coordinatesIndex = texCoordIndex;
                                        // Set UVs/texture transform for the ambient occlusion texture
                                        _exportUV(ambientOcclusionTexture.UVGen, babylonMaterial.ambientTexture);
                                    }
                                    else
                                    {
                                        babylonMaterial.ambientTexture = ormTexture;
                                    }
                                }
                                else
                                {
                                    babylonMaterial.ambientTexture = ExportPBRTexture(maxDecorator.Node, 6, babylonScene);
                                }
                            }
                        }
                        else if (ambientOcclusionTexmap != null)
                        {
                            // Simply export occlusion texture
                            RaiseVerbose("Simply export occlusion texture", 2);
                            babylonMaterial.ambientTexture = ExportTexture(ambientOcclusionTexmap, babylonScene);
                        }
                    }
                    if (ambientOcclusionTexmap != null && !exportParameters.mergeAO && babylonMaterial.ambientTexture == null)
                    {
                        RaiseVerbose("Exporting occlusion texture without merging with metallic roughness", 2);
                        babylonMaterial.ambientTexture = ExportTexture(ambientOcclusionTexmap, babylonScene);
                    }

                    // Normal
                    ITexmap NormalMap = maxDecorator.NormalMap;
                    if (NormalMap != null)
                    {
                        var normalMapAmount = maxDecorator.Normal;
                        babylonMaterial.bumpTexture = ExportTexture(NormalMap, babylonScene, normalMapAmount);
                    }
                }

                ITexmap EmmissionMap = maxDecorator.EmissionMap;
                if (EmmissionMap != null)
                {
                    babylonMaterial.emissiveTexture = ExportTexture(EmmissionMap, babylonScene);
                    babylonMaterial.emissive = new[] { 1.0f, 1.0f, 1.0f };
                }
            }

        }
        internal void ExportClearCoat(GltfMaterialDecorator maxDecorator, BabylonPBRMaterial material, BabylonScene babylonScene)
        {
            if (maxDecorator.EnableClearcoat)
            {
                // GLTF Material has the following properties
                //.enableClearCoat(Enable_Clearcoat) : boolean
                //.clearcoatMap(Clearcoat_Map) : texturemap
                //.clearcoatRoughness(Clearcoat_Roughness) : float
                //.clearcoatRoughnessMap(Clearcoat_Roughness_Map) : texturemap
                //.clearcoatNormal(Clearcoat_Normal) : float
                //.clearcoatNormalMap(Clearcoat_Normal_Map) : texturemap
                var target = material.clearCoat;

                target.isEnabled = maxDecorator.EnableClearcoat;
                // The clearcoat layer intensity. If is zero, the whole clearcoat layer is disabled.
                target.intensity = maxDecorator.Clearcoat;
                // The clearcoat layer roughness
                target.roughness = maxDecorator.ClearcoatRoughness;

                if (exportParameters.exportTextures)
                {
                    // The clearcoat layer intensity texture
                    if (maxDecorator.ClearcoatMap != null)
                    {
                        target.texture = ExportTexture(maxDecorator.ClearcoatMap, babylonScene);
                    }
                    // The clearcoat layer roughness texture
                    if (maxDecorator.ClearcoatRoughnessMap != null)
                    {
                        target.useRoughnessFromMainTexture = (maxDecorator.ClearcoatRoughnessMap == maxDecorator.ClearcoatMap);
                        if(!target.useRoughnessFromMainTexture)
                        {
                            target.textureRoughness = ExportTexture(maxDecorator.ClearcoatRoughnessMap, babylonScene);
                        }
                    }
                    // The clearcoat normal map texture
                    if (maxDecorator.ClearcoatNormalMap != null)
                    {
                        // here we use the ClearcoatNormal as BabylonTexture level property
                        // reason is GLTF 2.0 do NOT Define any scale parameter for Normal. So we only mimic Autodesk behavior.
                        target.bumpTexture = ExportTexture(maxDecorator.ClearcoatNormalMap, babylonScene, maxDecorator.ClearcoatNormal);
                    }
                }
            }
        }
        internal void ExportSheen(GltfMaterialDecorator maxDecorator, BabylonPBRMaterial material, BabylonScene babylonScene)
        {
            if (maxDecorator.EnableSheen)
            {
                // GLTF Material has the following properties
                // .enableSheen(Enable_Sheen) : boolean
                // .sheenColor(Sheen_Color) : color
                // .sheenColorMap(Sheen_Color_Map) : texturemap
                // .sheenRoughness(Sheen_Roughness) : float
                // .sheenRoughnessMap(Sheen_Roughness_Map) : texturemap
                var target = material.sheen;
                target.isEnabled = maxDecorator.EnableSheen;
                target.color = maxDecorator.SheenColor.ToArray();
                target.roughness = maxDecorator.SheenRoughness;
                if (exportParameters.exportTextures)
                {
                    if (maxDecorator.SheenColorMap != null)
                    {
                        target.texture = ExportTexture(maxDecorator.SheenColorMap, babylonScene);
                    }

                    if (maxDecorator.SheenRoughnessMap != null)
                    {
                        target.useRoughnessFromMainTexture = (maxDecorator.SheenRoughnessMap == maxDecorator.SheenColorMap);
                        if(!target.useRoughnessFromMainTexture)
                        {
                            target.textureRoughness = ExportTexture(maxDecorator.SheenRoughnessMap, babylonScene);
                        }
                    }
                }
            }
        }
        internal void ExportTransmission(GltfMaterialDecorator maxDecorator, BabylonPBRMaterial target, BabylonScene babylonScene)
        {
            if (maxDecorator.EnableTransmission)
            {
                // GLTF Material has the following properties
                // .enableTransmission(Enable_Transmission) : boolean  
                // .transmission : float -> map to Physical Material "transparency".
                // .transmissionMap(Transmission_Map) : texturemap -> map to Physical Material "transparency_map".
                var s = target.subSurface;
                s.isRefractionEnabled = true;
                s.refractionIntensity = maxDecorator.Transmission;
                if (exportParameters.exportTextures)
                {
                    if (maxDecorator.TransmissionMap != null)
                    {
                        s.refractionIntensityTexture = ExportTexture(maxDecorator.TransmissionMap, babylonScene);
                    }
                }
            }
        }
        internal void ExportIOR(GltfMaterialDecorator maxDecorator, BabylonPBRMaterial target, BabylonScene babylonScene)
        {
            if (maxDecorator.EnableIndexOfRefraction)
            {
                // GLTF Material has the following properties
                // .enableIndexOfRefraction(Enable_IOR) : boolean
                // .indexOfRefraction(IOR) : float -> map to Physical Material "trans_ior".
                target.indexOfRefraction = maxDecorator.indexOfRefraction;
            }
        }
        internal void ExportVolume(GltfMaterialDecorator maxDecorator, BabylonPBRMaterial target, BabylonScene babylonScene)
        {
            if (maxDecorator.EnableVolume)
            {
                // GLTF Material has the following properties
                // .enableVolume(Enable_Volume) : boolean
                // .volumeThickness(Volume_Thickness) : float -> map to Physical Material thin_walled, trans_depth and trans_color
                // .volumeThicknessMap(Volume_Thickness_Map) : texturemap -> NO mapping to physical Material.
                // .volumeDistance(Volume_Distance) : float -> map to Physical Material trans_depth
                // .volumeColor(Volume_Color) : color -> map to Physical Material trans_color
                var s = target.subSurface;
                s.maximumThickness = maxDecorator.VolumeThickness;
                s.tintColorAtDistance = maxDecorator.VolumeDistance;
                s.tintColor = maxDecorator.VolumeColor?.ToArray();
                if (exportParameters.exportTextures)
                {
                    if (maxDecorator.VolumeThicknessMap != null)
                    {
                        s.thicknessTexture = ExportTexture(maxDecorator.VolumeThicknessMap, babylonScene);
                    }
                }
            }
        }
        internal void ExportSpecular(GltfMaterialDecorator maxDecorator, BabylonPBRMaterial target, BabylonScene babylonScene)
        {
            if (maxDecorator.EnableSpecular)
            {
                // GLTF Material has the following properties
                // .enableSpecular(Enable_Specular) : boolean
                // .Specular : float -> map to Physical Material "reflectivity".
                // .specularMap(Specular_Map) : texturemap -> map to Physical Material "reflectivity_map".
                // .specularcolor(Specular_Color) : color -> map to Physical Material "refl_color".
                // .specularColorMap(Specular_Color_Map) : texturemap -> map to Physical Material "refl_color_map".

                target.metallicF0Factor = maxDecorator.Specular;
                target.metallicReflectanceColor = maxDecorator.specularColor.ToArray();
                if (exportParameters.exportTextures)
                {
                    if (maxDecorator.SpecularMap != null)
                    {
                        target.metallicReflectanceTexture = ExportTexture(maxDecorator.SpecularMap, babylonScene);
                    }

                    if (maxDecorator.SpecularColorMap != null)
                    {
                        target.reflectanceTexture = ExportTexture(maxDecorator.SpecularColorMap, babylonScene);
                    }
                }

            }
        }

        private void ExportAlphaMode(GltfMaterialDecorator maxDecorator, BabylonPBRMaterial target)
        {
            var a = maxDecorator.AlphaMode;
            switch (a)
            {
                case ((int)GltfMaterialDecorator.AlphaModeCode.OPAQUE):
                    {
                        target.transparencyMode = (int)BabylonMaterial.TransparencyMode.OPAQUE;
                        break;
                    }
                case ((int)GltfMaterialDecorator.AlphaModeCode.BLEND):
                    {
                        target.transparencyMode = (int)BabylonMaterial.TransparencyMode.ALPHABLEND;
                        break;
                    }
                case ((int)GltfMaterialDecorator.AlphaModeCode.MASK):
                    {
                        target.transparencyMode = (int)BabylonMaterial.TransparencyMode.ALPHATEST;
                        break;
                    }
            }
            // GltfMaterial default value is 0.5
            target.alphaCutOff = maxDecorator.AlphaCutOff;
        }
    }
}
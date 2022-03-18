using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Max;
using BabylonExport.Entities;

namespace Max2Babylon
{
    /// <summary>
    /// Decorator is used to access properties in a cleaner and centralized way
    /// </summary>
    public class GltfMaterialDecorator : MaterialDecorator
    {
        public GltfMaterialDecorator(IIGameMaterial node) : base(node)
        {
        }

        // Theses lines are extracted from the Material_gltf.ms script 
        // ----------------------------------------------------------
        //baseColor type:#frgba default:white ui:cpBaseColor localizedname:~GLTF_MATERIAL_PARAM_BASECOLOR~ nonlocalizedName:"Base Color"
        //baseColorMap type:#TextureMap ui:btnBaseColorMap localizedname:~GLTF_MATERIAL_PARAM_BASECOLOR_MAP~ nonlocalizedName:"Base Color Map"
        //alphaMode type:#integer default:1 ui:cbAlphaMode localizedname:~GLTF_MATERIAL_PARAM_ALPHA_MODE~ nonlocalizedName:"Alpha"
        //alphaMap type:#textureMap ui:btnAlphaMap localizedname:~GLTF_MATERIAL_PARAM_ALPHA_MAP~ nonlocalizedName:"Alpha Map"
        //alphaCutoff type:#float default:0.5 ui:spnAlphaCutoff localizedname:~GLTF_MATERIAL_PARAM_ALPHA_CUTOFF~ nonlocalizedName:"Cutoff"
        //metalness type:#float default:0.0 ui:spnMetalness localizedname:~GLTF_MATERIAL_PARAM_METALNESS~
        //metalnessMap type:#textureMap ui:btnMetalnessMap localizedname:~GLTF_MATERIAL_PARAM_METALNESS_MAP~ nonlocalizedName:"Metalness Map"
        //roughness type:#float default:0.5 ui:spnRoughness localizedname:~GLTF_MATERIAL_PARAM_ROUGHNESS~
        //roughnessMap type:#textureMap ui:btnRoughnessMap localizedname:~GLTF_MATERIAL_PARAM_ROUGHNESS_MAP~ nonlocalizedName:"Roughness Map"
        //normal type:#float default:1.0 ui:spnNormal localizedname:~GLTF_MATERIAL_PARAM_NORMAL~
        //normalMap type:#textureMap ui:btnNormalMap localizedname:~GLTF_MATERIAL_PARAM_NORMAL_MAP~ nonlocalizedName:"Normal Map"
        //ambientOcclusion type:#float default:1.0 ui:spnAmbientOcclusion localizedname:~GLTF_MATERIAL_PARAM_AO~ nonlocalizedName:"Occlusion (AO)"
        //ambientOcclusionMap type:#textureMap ui:btnAmbientOcclusionMap localizedname:~GLTF_MATERIAL_PARAM_AO_MAP~ nonlocalizedName:"Occlusion (AO) Map"
        //emissionColor type:#frgba default:black ui:cpEmissionColor localizedname:~GLTF_MATERIAL_PARAM_EMISSION~ nonlocalizedName:"Emission"
        //emissionMap type:#textureMap ui:btnEmissionMap localizedname:~GLTF_MATERIAL_PARAM_EMISSION_MAP~ nonlocalizedName:"Emission Map"
        //doubleSided type:#boolean default:false ui:cbDoubleSided localizedname:~GLTF_MATERIAL_PARAM_DOUBLE_SIDED~ nonlocalizedName:"Double Sided"
        public IColor BaseColor => _node.MaxMaterial.GetDiffuse(0, false); // delegate.base_color = val
        public ITexmap BaseColorMap => _getTexMap(_node, "baseColorMap");
        public int AlphaMode => Properties?.GetIntProperty("alphaMode", 1) ?? 1;
        public ITexmap AlphaMap => _getTexMap(_node, "AlphaMap");
        public float AlphaCutOff => Properties?.GetFloatProperty("alphaCutoff", 0.5f) ?? 0.5f;
        public float Metalness => Properties?.GetFloatProperty("metalness", 0.0f) ?? 0.0f;
        public ITexmap MetalnessMap => _getTexMap(_node, "metalnessMap");
        public float Roughness => Properties?.GetFloatProperty("roughness", 0.0f) ?? 0.0f;
        public ITexmap RoughnessMap => _getTexMap(_node, "roughnessMap");
        public float Normal => Properties?.GetFloatProperty("normal", 0.0f) ?? 0.0f;
        public ITexmap NormalMap => _getTexMap(_node, "normalMap");
        public float AmbientOcclusion => Properties?.GetFloatProperty("ambientOcclusion", 0.0f) ?? 0.0f;
        public ITexmap AmbientOcclusionMap => _getTexMap(_node, "ambientOcclusionMap");
        public IColor EmissionColor => _node.MaxMaterial.GetSelfIllumColor(0, false);
        public ITexmap EmissionMap => _getTexMap(_node, "emissionMap");
        public bool DoubleSided => Properties?.GetBoolProperty("doubleSided", false) ?? false;


        // Extensions:
        // -----------
        //enableClearcoat type:#boolean default:false ui:cbEnableClearcoat localizedname:~GLTF_MATERIAL_PARAM_ENABLE_CLEARCOAT~ nonlocalizedName:"Enable Clearcoat"
        //clearcoat type:#float default:1.0 ui:spnClearcoatFactor localizedname:~GLTF_MATERIAL_PARAM_CLEARCOAT~
        //clearcoatMap type:#TextureMap ui:btnClearcoatMap localizedname:~GLTF_MATERIAL_PARAM_CLEARCOAT_MAP~ nonlocalizedName:"Clearcoat Map"
        //clearcoatRoughness type:#float default:0.0 ui:spnClearcoatRoughness localizedname:~GLTF_MATERIAL_PARAM_CLEARCOAT_ROUGHNESS~ nonlocalizedName:"Clearcoat Roughness"
        //clearcoatRoughnessMap type:#TextureMap ui:btnClearcoatRoughnessMap localizedname:~GLTF_MATERIAL_PARAM_CLEARCOAT_ROUGHNESS_MAP~ nonlocalizedName:"Clearcoat Roughness Map"
        //clearcoatNormal type:#float default:1.0 ui:spnClearcoatNormal localizedname:~GLTF_MATERIAL_PARAM_CLEARCOAT_NORMAL~ nonlocalizedName:"Clearcoat Normal"
        //clearcoatNormalMap type:#TextureMap ui:btnClearcoatNormalMap localizedname:~GLTF_MATERIAL_PARAM_CLEARCOAT_NORMAL_MAP~ nonlocalizedName:"Clearcoat Normal Map"

        public bool EnableClearcoat => Properties?.GetBoolProperty("enableClearcoat", false) ?? false;
        public float Clearcoat => Properties?.GetFloatProperty("clearcoat", 1.0f) ?? 1.0f;
        public ITexmap ClearcoatMap => _getTexMap(_node, "clearcoatMap");
        public float ClearcoatRoughness => Properties?.GetFloatProperty("clearcoatRoughness", 0.0f) ?? 0.0f;
        public ITexmap ClearcoatRoughnessMap => _getTexMap(_node, "clearcoatRoughnessMap");
        public float ClearcoatNormal => Properties?.GetFloatProperty("clearcoatNormal", 1.0f) ?? 1.0f;
        public ITexmap ClearcoatNormalMap => _getTexMap(_node, "clearcoatNormalMap");

        //enableSheen type:#boolean default:false ui:cbEnableSheen localizedname:~GLTF_MATERIAL_PARAM_ENABLE_SHEEN~ nonlocalizedName:"Enable Sheen"
        //sheenColor type:#frgba default:white ui:cpSheenColor localizedname:~GLTF_MATERIAL_PARAM_SHEEN_COLOR~ nonlocalizedName:"Sheen Color"
        //sheenColorMap type:#TextureMap ui:btnSheenColorMap localizedname:~GLTF_MATERIAL_PARAM_SHEEN_COLOR_MAP~ nonlocalizedName:"Sheen Color Map"
        //sheenRoughness type:#float default:0.0 ui:spnSheenRoughness localizedname:~GLTF_MATERIAL_PARAM_SHEEN_ROUGHNESS~ nonlocalizedName:"Sheen Roughness"
        //sheenRoughnessMap type:#TextureMap ui:btnSheenRoughnessMap localizedname:~GLTF_MATERIAL_PARAM_SHEEN_ROUGHNESS_MAP~ nonlocalizedName:"Sheen Roughness Map"

        //enableSpecular type:#boolean default:false ui:cbEnableSpecular localizedname:~GLTF_MATERIAL_PARAM_ENABLE_SPECULAR~ nonlocalizedName:"Enable Specular"
        //specular type:#float default:1.0 ui:spnSpecularFactor localizedname:~GLTF_MATERIAL_PARAM_SPECULAR~
        //specularMap type:#TextureMap ui:btnSpecularMap localizedname:~GLTF_MATERIAL_PARAM_SPECULAR_MAP~ nonlocalizedName:"Specular Map"
        //specularColor type:#frgba default:white ui:cpSpecularColor localizedname:~GLTF_MATERIAL_PARAM_SPECULAR_COLOR~ nonlocalizedName:"Specular Color"
        //specularColorMap type:#TextureMap ui:btnSpecularColorMap localizedname:~GLTF_MATERIAL_PARAM_SPECULAR_COLOR_MAP~ nonlocalizedName:"Specular Color Map"

        //enableTransmission type:#boolean default:false ui:cbEnableTransmission localizedname:~GLTF_MATERIAL_PARAM_ENABLE_TRANSMISSION~ nonlocalizedName:"Enable Transmission"
        //transmission type:#float default:1.0 ui:spnTransmissionFactor localizedname:~GLTF_MATERIAL_PARAM_TRANSMISSION~
        //transmissionMap type:#TextureMap ui:btnTransmissionMap localizedname:~GLTF_MATERIAL_PARAM_TRANSMISSION_MAP~ nonlocalizedName:"Transmission Map"

        //enableVolume type:#boolean default:false ui:cbEnableVolume localizedname:~GLTF_MATERIAL_PARAM_ENABLE_VOLUME~ nonlocalizedName:"Enable Volume"
        //volumeThickness type:#float default:0.0 ui:spnVolumeThickness localizedname:~GLTF_MATERIAL_PARAM_VOLUME_THICKNESS~ nonlocalizedName:"Volume Thickness"
        //volumeThicknessMap type:#TextureMap ui:btnVolumeThicknessMap localizedname:~GLTF_MATERIAL_PARAM_VOLUME_THICKNESS_MAP~ nonlocalizedName:"Volume Thickness Map"
        //volumeDistance type:#float default:0.0 ui:spnVolumeDistance localizedname:~GLTF_MATERIAL_PARAM_VOLUME_DISTANCE~ nonlocalizedName:"Volume Distance"
        //volumeColor type:#frgba default:white ui:cpVolumeColor localizedname:~GLTF_MATERIAL_PARAM_VOLUME_COLOR~ nonlocalizedName:"Volume Color"

        //enableIndexOfRefraction type:#boolean default:false ui:cbEnableIOR localizedname:~GLTF_MATERIAL_PARAM_ENABLE_IOR~ nonlocalizedName:"Enable IOR"
        //indexOfRefraction type:#float default:1.5 ui:spnIOR localizedname:~GLTF_MATERIAL_PARAM_IOR~ nonlocalizedName:"IOR"


        // For information, using the folowing script
        // fn showSelectedProperties = ( node = SME.GetMtlinParamEditor(); print (classof node) ; showProperties node ;)
        // we obtain also the list of the properties as seen by the SDK
        //.baseColor(Base_Color) : color
        //.baseColorMap(Base_Color_Map) : texturemap
        //.alphaMode(Alpha) : integer
        //.AlphaMap(Alpha_Map) : texturemap
        //.alphaCutoff(Cutoff) : float
        //.metalness : float
        //.metalnessMap(Metalness_Map) : texturemap
        //.roughness : float
        //.roughnessMap(Roughness_Map) : texturemap
        //.normal : float
        //.normalMap(Normal_Map) : texturemap
        //.ambientOcclusion(Occlusion__AO) : float
        //.ambientOcclusionMap(Occlusion__AO__Map) : texturemap
        //.emissionColor(Emission) : color
        //.emissionMap(Emission_Map) : texturemap
        //.DoubleSided(Double_Sided) : boolean
        //.unlit : boolean
        //.enableClearCoat(Enable_Clearcoat) : boolean
        //.clearcoat : float
        //.clearcoatMap(Clearcoat_Map) : texturemap
        //.clearcoatRoughness(Clearcoat_Roughness) : float
        //.clearcoatRoughnessMap(Clearcoat_Roughness_Map) : texturemap
        //.clearcoatNormal(Clearcoat_Normal) : float
        //.clearcoatNormalMap(Clearcoat_Normal_Map) : texturemap
        //.enableSheen(Enable_Sheen) : boolean
        //.sheenColor(Sheen_Color) : color
        //.sheenColorMap(Sheen_Color_Map) : texturemap
        //.sheenRoughness(Sheen_Roughness) : float
        //.sheenRoughnessMap(Sheen_Roughness_Map) : texturemap
        //.enableSpecular(Enable_Specular) : boolean
        //.Specular : float
        //.specularMap(Specular_Map) : texturemap
        //.specularcolor(Specular_Color) : color
        //.specularColorMap(Specular_Color_Map) : texturemap
        //.enableTransmission(Enable_Transmission) : boolean  
        //.transmission : float
        //.transmissionMap(Transmission_Map) : texturemap
        //.enableVolume(Enable_Volume) : boolean
        //.volumeThickness(Volume_Thickness) : float
        //.volumeThicknessMap(Volume_Thickness_Map) : texturemap
        //.volumeDistance(Volume_Distance) : float
        //.volumeColor(Volume_Color) : color
        //.enableIndexOfRefraction(Enable_IOR) : boolean
        //.indexOfRefraction(IOR) : float
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
            // the not obvious part is that we MUST first export the material as Babylon to allow
            // the Babylon2GLTF export rebuild the coresponding material and extensions.
            GltfMaterialDecorator decorator = new GltfMaterialDecorator(materialNode);
        }

        public bool isGLTFMaterial(IIGameMaterial materialNode)
        {
            return ClassIDWrapper.Gltf_Material.Equals(materialNode.MaxMaterial.ClassID);
        }
    }
}
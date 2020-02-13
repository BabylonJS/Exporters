using System.Runtime.Serialization;
namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonPBRMaterial : BabylonMaterial
    {
        [DataMember]
        public string customType { get; private set; }

        [DataMember]
        public float directIntensity { get; set; }

        [DataMember]
        public float emissiveIntensity { get; set; }

        [DataMember]
        public float environmentIntensity { get; set; }

        [DataMember]
        public float specularIntensity { get; set; }

        [DataMember]
        public float cameraExposure { get; set; }

        [DataMember]
        public float cameraContrast { get; set; }

        [DataMember]
        public float microSurface { get; set; }

        [DataMember]
        public BabylonTexture albedoTexture { get; set; }

        [DataMember]
        public BabylonTexture ambientTexture { get; set; }

        [DataMember]
        public BabylonTexture opacityTexture { get; set; }

        [DataMember]
        public BabylonTexture reflectionTexture { get; set; }

        [DataMember]
        public BabylonTexture emissiveTexture { get; set; }

        [DataMember]
        public BabylonTexture reflectivityTexture { get; set; }

        [DataMember]
        public BabylonTexture bumpTexture { get; set; }

        [DataMember]
        public BabylonTexture lightmapTexture { get; set; }

        [DataMember]
        public BabylonTexture metallicTexture { get; set; }

        [DataMember]
        public bool useLightmapAsShadowmap { get; set; }

        [DataMember]
        public BabylonTexture refractionTexture { get; set; }

        [DataMember]
        public float[] ambient { get; set; }

        [DataMember]
        public float[] albedo { get; set; }

        [DataMember]
        public float[] reflectivity { get; set; }

        [DataMember]
        public float[] reflection { get; set; }

        [DataMember]
        public float[] emissive { get; set; }

        [DataMember]
        public float? roughness { get; set; }

        [DataMember]
        public float? metallic { get; set; }

        [DataMember]
        public bool useMicroSurfaceFromReflectivityMapAplha { get; set; }

        [DataMember]
        public bool linkRefractionWithTransparency { get; set; }

        [DataMember]
        public bool useRoughnessFromMetallicTextureAlpha { get; set; }

        [DataMember]
        public bool useRoughnessFromMetallicTextureGreen { get; set; }

        [DataMember]
        public bool useMetallnessFromMetallicTextureBlue { get; set; }

        [DataMember]
        public bool useAlphaFromAlbedoTexture { get; set; }

        [DataMember]
        public bool useEmissiveAsIllumination { get; set; }

        [DataMember]
        public bool useMicroSurfaceFromReflectivityMapAlpha { get; set; }

        [DataMember]
        public bool useSpecularOverAlpha { get; set; }

        [DataMember]
        public bool useRadianceOverAlpha { get; set; }

        [DataMember]
        public bool usePhysicalLightFalloff { get; set; }

        [DataMember]
        public float indexOfRefraction { get; set; }

        [DataMember]
        public bool invertRefractionY { get; set; }

        [DataMember]
        public BabylonFresnelParameters emissiveFresnelParameters { get; set; }

        [DataMember]
        public BabylonFresnelParameters opacityFresnelParameters { get; set; }

        [DataMember]
        public bool disableLighting { get; set; }

        [DataMember]
        public bool twoSidedLighting { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float? alphaCutOff { get; set; }

        [DataMember]
        public int transparencyMode { get; set; }

        [DataMember]
        public bool invertNormalMapX { get; set; }

        [DataMember]
        public bool invertNormalMapY { get; set; }

        [DataMember]
        public float ambientTextureStrength { get; set; }

        [DataMember]
        public BabylonPBRClearCoat clearCoat { get; set; }

        public BabylonPBRMaterial(string id) : base(id)
        {
            SetCustomType("BABYLON.PBRMaterial");
            directIntensity = 1.0f;
            emissiveIntensity = 1.0f;
            environmentIntensity = 1.0f;
            specularIntensity = 1.0f;
            cameraExposure = 1.0f;
            cameraContrast = 1.0f;
            indexOfRefraction = 0.66f;
            twoSidedLighting = false;
            useRadianceOverAlpha = true;
            useSpecularOverAlpha = true;
            usePhysicalLightFalloff = true;
            useEmissiveAsIllumination = false;

            // Default Null Metallic Workflow
            metallic = null;
            roughness = null;
            useRoughnessFromMetallicTextureAlpha = true;
            useRoughnessFromMetallicTextureGreen = false;
            useMetallnessFromMetallicTextureBlue = false;

            microSurface = 0.9f;
            useMicroSurfaceFromReflectivityMapAplha = false;

            ambient = new[] { 0f, 0f, 0f };
            albedo = new[] { 1f, 1f, 1f };
            reflectivity = new[] { 1f, 1f, 1f };
            reflection = new[] { 0.5f, 0.5f, 0.5f };
            emissive = new[] { 0f, 0f, 0f };

            invertNormalMapX = false;
            invertNormalMapY = false;
            ambientTextureStrength = 1.0f;
            transparencyMode = (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.OPAQUE;

            clearCoat = new BabylonPBRClearCoat();
        }

        public BabylonPBRMaterial(BabylonPBRMetallicRoughnessMaterial origin) : base(origin.id)
        {
            SetCustomType("BABYLON.PBRMaterial");
            directIntensity = 1.0f;
            emissiveIntensity = 1.0f;
            environmentIntensity = 1.0f;
            specularIntensity = 1.0f;
            cameraExposure = 1.0f;
            cameraContrast = 1.0f;

            useRadianceOverAlpha = true;
            useSpecularOverAlpha = true;
            usePhysicalLightFalloff = true;
            useEmissiveAsIllumination = true;

            useRoughnessFromMetallicTextureAlpha = false;
            useRoughnessFromMetallicTextureGreen = true;
            useMetallnessFromMetallicTextureBlue = true;

            ambient = new[] { 0f, 0f, 0f };
            reflectivity = new[] { 1f, 1f, 1f };
            reflection = new[] { 1f, 1f, 1f };

            albedoTexture = origin.baseTexture;
            alpha = origin.alpha;
            alphaCutOff = origin.alphaCutOff;
            alphaMode = origin.alphaMode;
            backFaceCulling = origin.backFaceCulling;
            albedo = origin.baseColor;
            albedoTexture = origin.baseTexture;
            clearCoat = origin.clearCoat;
            disableLighting = origin.disableLighting;
            twoSidedLighting = origin.doubleSided;
            emissive = origin.emissive;
            emissiveTexture = origin.emissiveTexture;
            invertNormalMapX = origin.invertNormalMapX;
            invertNormalMapY = origin.invertNormalMapY;
            isUnlit = origin.isUnlit;
            maxSimultaneousLights = origin.maxSimultaneousLights;
            metallic = origin.metallic;
            reflectivityTexture = origin.metallicRoughnessTexture;
            name = origin.name;
            bumpTexture = origin.normalTexture;
            ambientTextureStrength = origin.occlusionStrength;
            ambientTexture = origin.occlusionTexture;
            roughness = origin.roughness;
            transparencyMode = origin.transparencyMode;
            wireframe = origin.wireframe;
        }

        public void SetCustomType(string type)
        {
            customType = type;
        }
    }
}
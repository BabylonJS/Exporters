using System.Runtime.Serialization;
namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonPBRMaterial : BabylonMaterial
    {
        public const float DefaultIOR = 1.5f;
        public const float DefaultSpecularFactor = 1.0f;

        public static float[] BlackColor() => new[] { 0f, 0f, 0f };
        public static float[] GreyColor() => new[] { 0.5f, 0.5f, 0.5f };
        public static float[] WhiteColor() => new[] { 1f, 1f, 1f };

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
        public bool useAmbientOcclusionFromMetallicTextureRed { get; set; }

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
        public bool useAmbientInGrayScale { get; set; }

        [DataMember]
        public float? indexOfRefraction { get; set; }

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
        
        [DataMember]
        public BabylonPBRSheenConfiguration sheen { get; set; }
        
        [DataMember]
        public BabylonPBRSubSurfaceConfiguration subSurface { get; set; }

        // SPECULAR
        [DataMember]
        public float? metallicF0Factor { get; set; }
        [DataMember]
        public BabylonTexture metallicReflectanceTexture { get; set; }
        [DataMember]
        public float[] metallicReflectanceColor { get; set; } = WhiteColor();

        public BabylonTexture reflectanceTexture { get; set; }

        public BabylonPBRMaterial(string id) : base(id)
        {
            SetCustomType("BABYLON.PBRMaterial");
            directIntensity = 1.0f;
            emissiveIntensity = 1.0f;
            environmentIntensity = 1.0f;
            specularIntensity = 1.0f;
            cameraExposure = 1.0f;
            cameraContrast = 1.0f;
            indexOfRefraction = null;
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

            ambient = BlackColor();
            albedo = WhiteColor();
            reflectivity = WhiteColor();
            reflection = GreyColor();
            emissive = BlackColor();

            invertNormalMapX = false;
            invertNormalMapY = false;
            ambientTextureStrength = 1.0f;
            transparencyMode = (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.OPAQUE;

            clearCoat = new BabylonPBRClearCoat();
            sheen = new BabylonPBRSheenConfiguration();
            subSurface = new BabylonPBRSubSurfaceConfiguration();
        }

        public BabylonPBRMaterial(BabylonPBRBaseSimpleMaterial origin) : base(origin.id)
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

            ambient = BlackColor();
            reflectivity = WhiteColor();
            reflection = WhiteColor();

            albedoTexture = origin.baseTexture;
            alpha = origin.alpha;
            alphaCutOff = origin.alphaCutOff;
            alphaMode = origin.alphaMode;
            backFaceCulling = origin.backFaceCulling;
            albedo = origin.baseColor;
            albedoTexture = origin.baseTexture;
            clearCoat = origin.clearCoat;
            sheen = origin.sheen;
            disableLighting = origin.disableLighting;
            twoSidedLighting = origin.doubleSided;
            emissive = origin.emissive;
            emissiveTexture = origin.emissiveTexture;
            invertNormalMapX = origin.invertNormalMapX;
            invertNormalMapY = origin.invertNormalMapY;
            isUnlit = origin.isUnlit;
            maxSimultaneousLights = origin.maxSimultaneousLights;
            name = origin.name;
            bumpTexture = origin.normalTexture;
            ambientTextureStrength = origin.occlusionStrength;
            ambientTexture = origin.occlusionTexture;
            transparencyMode = origin.transparencyMode;
            wireframe = origin.wireframe;

            subSurface = new BabylonPBRSubSurfaceConfiguration();
        }
        public BabylonPBRMaterial(BabylonPBRMetallicRoughnessMaterial origin) : this((BabylonPBRBaseSimpleMaterial)origin)
        {
            useRoughnessFromMetallicTextureAlpha = false;
            useRoughnessFromMetallicTextureGreen = true;
            useMetallnessFromMetallicTextureBlue = true;
            useAmbientInGrayScale = true;

            roughness = origin.roughness;
            metallic = origin.metallic;
            metallicTexture = origin.metallicRoughnessTexture;
        }

        public BabylonPBRMaterial(BabylonPBRSpecularGlossinessMaterial origin) : this((BabylonPBRBaseSimpleMaterial)origin)
        {
        }

        public void SetCustomType(string type)
        {
            customType = type;
        }
    }
}
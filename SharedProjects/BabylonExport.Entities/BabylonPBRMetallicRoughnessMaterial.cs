using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonPBRMetallicRoughnessMaterial : BabylonMaterial
    {
        public enum TransparencyMode
        {
            OPAQUE = 0,
            ALPHATEST = 1,
            ALPHABLEND = 2,
            ALPHATESTANDBLEND = 3
        }

        [DataMember]
        public string customType { get; private set; }

        [DataMember]
        public float[] baseColor { get; set; }

        [DataMember]
        public BabylonTexture baseTexture { get; set; }

        [DataMember]
        public float metallic { get; set; }

        [DataMember]
        public float roughness { get; set; }

        [DataMember]
        public BabylonTexture metallicRoughnessTexture { get; set; }

        [DataMember]
        public bool disableLighting { get; set; }

        [DataMember]
        public bool invertNormalMapX { get; set; }

        [DataMember]
        public bool invertNormalMapY { get; set; }

        [DataMember]
        public BabylonTexture normalTexture { get; set; }

        [DataMember]
        public float[] emissive { get; set; }

        [DataMember]
        public BabylonTexture emissiveTexture { get; set; }

        [DataMember]
        public float occlusionStrength { get; set; }

        [DataMember]
        public BabylonTexture occlusionTexture { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float? alphaCutOff { get; set; }

        [DataMember]
        public int transparencyMode { get; set; }

        [DataMember]
        public bool doubleSided { get; set; }

        [DataMember]
        public BabylonPBRClearCoat clearCoat { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool _unlit { get; set; }

        public BabylonPBRMetallicRoughnessMaterial(string id) : base(id)
        {
            customType = "BABYLON.PBRMetallicRoughnessMaterial";
            
            emissive = new[] { 0f, 0f, 0f };
            occlusionStrength = 1.0f;
            transparencyMode = (int)TransparencyMode.OPAQUE;
            _unlit = false;

            clearCoat = new BabylonPBRClearCoat();
        }

        public BabylonPBRMetallicRoughnessMaterial(BabylonPBRMetallicRoughnessMaterial original) : base(original)
        {
            customType = original.customType;
            baseColor = original.baseColor;
            baseTexture = original.baseTexture;
            metallic = original.metallic;
            roughness = original.roughness;
            metallicRoughnessTexture = original.metallicRoughnessTexture;
            maxSimultaneousLights = original.maxSimultaneousLights;
            disableLighting = original.disableLighting;
            invertNormalMapX = original.invertNormalMapX;
            invertNormalMapY = original.invertNormalMapY;
            normalTexture = original.normalTexture;
            emissive = original.emissive;
            emissiveTexture = original.emissiveTexture;
            occlusionStrength = original.occlusionStrength;
            occlusionTexture = original.occlusionTexture;
            alphaCutOff = original.alphaCutOff;
            transparencyMode = original.transparencyMode;
            doubleSided = original.doubleSided;
            clearCoat = original.clearCoat;
            _unlit = original._unlit;
        }
    }
}

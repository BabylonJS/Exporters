using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    /// <summary>
    /// Base class of Metallic Roughness and Specular Glossiness material
    /// </summary>
    [DataContract]
    public class BabylonPBRBaseSimpleMaterial : BabylonMaterial
    {
        public static float[] BlackColor() => new[] { 0f, 0f, 0f };
        public static float[] WhiteColor() => new[] { 1f, 1f, 1f };

        public BabylonPBRBaseSimpleMaterial(string id) : base(id)
        {
            emissive = BlackColor();
            occlusionStrength = 1.0f;
            transparencyMode = (int)TransparencyMode.OPAQUE;
            _unlit = false;
            clearCoat = new BabylonPBRClearCoat();
        }
        public BabylonPBRBaseSimpleMaterial(BabylonPBRBaseSimpleMaterial original) : base(original)
        {
            customType = original.customType;
            baseColor = original.baseColor;
            baseTexture = original.baseTexture;
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
    
        [DataMember]
        public string customType { get; internal set; }

        [DataMember]
        public float[] baseColor { get; set; }

        [DataMember]
        public BabylonTexture baseTexture { get; set; }


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
    }
}

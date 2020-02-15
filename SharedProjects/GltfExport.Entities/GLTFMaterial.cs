using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFMaterial : GLTFIndexedChildRootProperty
    {
        public enum AlphaMode
        {
            OPAQUE,
            MASK,
            BLEND
        }

        [DataMember]
        public GLTFPBRMetallicRoughness pbrMetallicRoughness { get; set; }

        [DataMember]
        public GLTFTextureInfo normalTexture { get; set; }

        [DataMember]
        public GLTFTextureInfo occlusionTexture { get; set; }

        [DataMember]
        public GLTFTextureInfo emissiveTexture { get; set; }

        [DataMember]
        public float[] emissiveFactor { get; set; }

        [DataMember, JsonConverter(typeof(StringEnumConverter))]
        public AlphaMode alphaMode { get; set; }

        [DataMember]
        public float? alphaCutoff { get; set; }

        [DataMember]
        public bool doubleSided { get; set; }

        public string id;

        public bool ShouldSerializepbrMetallicRoughness()
        {
            return (this.pbrMetallicRoughness != null);
        }

        public bool ShouldSerializenormalTexture()
        {
            return (this.normalTexture != null);
        }

        public bool ShouldSerializeocclusionTexture()
        {
            return (this.occlusionTexture != null);
        }

        public bool ShouldSerializeemissiveTexture()
        {
            return (this.emissiveTexture != null);
        }

        public bool ShouldSerializeemissiveFactor()
        {
            return (this.emissiveFactor != null && !this.emissiveFactor.SequenceEqual(new float[] { 0f, 0f, 0f }));
        }

        public bool ShouldSerializealphaMode()
        {
            return (this.alphaMode != AlphaMode.OPAQUE);
        }

        public bool ShouldSerializealphaCutoff()
        {
            return (this.alphaCutoff != null && this.alphaCutoff != 0.5f);
        }

        public bool ShouldSerializedoubleSided()
        {
            return this.doubleSided;
        }
    }


    // https://github.com/AltspaceVR/glTF/blob/avr-sampler-offset-tile/extensions/2.0/Khronos/KHR_texture_transform/README.md
    [DataContract]
    public class KHR_texture_transform
    {
        [DataMember]
        public float[] offset { get; set; }     // array[2], default value [0,0]

        [DataMember]
        public float rotation { get; set; }     // in radian, default value 0

        [DataMember]
        public float[] scale { get; set; }      // array[2], default value [1,1]

        [DataMember]
        public int? texCoord { get; set; }       // min value 0, default null


        public bool ShouldSerializeoffset()
        {
            return (this.offset != null && this.offset != new float[] { 0f, 0f });

        }
        public bool ShouldSerializerotation()
        {
            return (this.rotation != 0f);
        }

        public bool ShouldSerializescale()
        {
            return (this.scale != null && this.scale != new float[] { 0f, 0f });
        }

        public bool ShouldSerializetexCoord()
        {
            return (this.texCoord != null);
        }
    }
}

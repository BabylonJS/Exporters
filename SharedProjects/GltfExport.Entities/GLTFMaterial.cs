using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Utilities;

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
            return (this.emissiveFactor != null && !this.emissiveFactor.IsAlmostEqualTo(0,float.Epsilon));
        }

        public bool ShouldSerializealphaMode()
        {
            return (this.alphaMode != AlphaMode.OPAQUE);
        }

        public bool ShouldSerializealphaCutoff()
        {
            return (this.alphaCutoff != null && !MathUtilities.IsAlmostEqualTo(this.alphaCutoff.Value, 0.5f, float.Epsilon));
        }

        public bool ShouldSerializedoubleSided()
        {
            return this.doubleSided;
        }
    }


 
}

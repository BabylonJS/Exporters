using System.Runtime.Serialization;
using System.Linq;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFPBRMetallicRoughness : GLTFProperty
    {
        [DataMember]
        public float[] baseColorFactor { get; set; }

        [DataMember]
        public GLTFTextureInfo baseColorTexture { get; set; }

        [DataMember]
        public float? metallicFactor { get; set; }

        [DataMember]
        public float? roughnessFactor { get; set; }

        [DataMember]
        public GLTFTextureInfo metallicRoughnessTexture { get; set; }

        public bool ShouldSerializebaseColorFactor()
        {
            return (this.baseColorFactor != null)  && !this.baseColorFactor.SequenceEqual(new float[] { 1f, 1f, 1f, 1f});
        }

        public bool ShouldSerializebaseColorTexture()
        {
            return (this.baseColorTexture != null);
        }

        public bool ShouldSerializemetallicFactor()
        {
            return (this.metallicFactor != 1f);
        }

        public bool ShouldSerializeroughnessFactor()
        {
            return (this.roughnessFactor != 1F);
        }

        public bool ShouldSerializemetallicRoughnessTexture()
        {
            return (this.metallicRoughnessTexture != null);
        }
    }
}

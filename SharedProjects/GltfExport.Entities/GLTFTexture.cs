using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFTexture : GLTFIndexedChildRootProperty
    {
        [DataMember]
        public int? sampler { get; set; }

        [DataMember]
        public int? source { get; set; }

        public bool ShouldSerializesampler()
        {
            return (this.sampler != null);
        }

        public bool ShouldSerializesource()
        {
            return (this.source != null);
        }
    }
}

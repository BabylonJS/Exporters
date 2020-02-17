using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFSampler : GLTFIndexedChildRootProperty
    {
        public enum TextureMagFilter
        {
            NEAREST = 9728,
            LINEAR = 9729
        }

        public enum TextureMinFilter
        {
            NEAREST = 9728,
            LINEAR = 9729,
            NEAREST_MIPMAP_NEAREST = 9984,
            LINEAR_MIPMAP_NEAREST = 9985,
            NEAREST_MIPMAP_LINEAR = 9986,
            LINEAR_MIPMAP_LINEAR = 9987
        }

        public enum TextureWrapMode
        {
            CLAMP_TO_EDGE = 33071,
            MIRRORED_REPEAT = 33648,
            REPEAT = 10497
        }

        [DataMember]
        public TextureMagFilter? magFilter { get; set; }

        [DataMember]
        public TextureMinFilter? minFilter { get; set; }

        [DataMember]
        public TextureWrapMode? wrapS { get; set; }

        [DataMember]
        public TextureWrapMode? wrapT { get; set; }

        public bool ShouldSerializemagFilter()
        {
            return (this.magFilter != null);
        }

        public bool ShouldSerializeminFilter()
        {
            return (this.minFilter != null);
        }

        public bool ShouldSerializewrapS()
        {
            return (this.wrapS != null && this.wrapS != TextureWrapMode.REPEAT);
        }

        public bool ShouldSerializewrapT()
        {
            return (this.wrapT != null && this.wrapT != TextureWrapMode.REPEAT);
        }
    }
}

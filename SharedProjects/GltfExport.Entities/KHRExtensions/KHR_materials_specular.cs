using System.Runtime.Serialization;
using Utilities;

namespace GLTFExport.Entities
{
    // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_specular/README.md
    [DataContract]
    public class KHR_materials_specular
    {
        // The strength of the specular reflection. Optional
        [DataMember]
        public float? specularFactor { get; set; }

        // A texture that defines the strength of the specular reflection,
        // stored in the alpha (A) channel. This will be multiplied by specularFactor.
        // Optional
        [DataMember]
        public GLTFTextureInfo specularTexture { get; set; }

        // The F0 color of the specular reflection (linear RGB). optional
        [DataMember]
        public float[] specularColorFactor { get; set; }

        // A texture that defines the F0 color of the specular reflection,
        // stored in the RGB channels and encoded in sRGB.
        // This texture will be multiplied by specularColorFactor.
        // Optional
        [DataMember]
        public GLTFTextureInfo specularColorTexture { get; set; }

        public bool ShouldSerializespecularFactor()
        {
            return this.specularFactor != null && !MathUtilities.IsAlmostEqualTo(this.specularFactor.Value, 1f, float.Epsilon);
        }
        public bool ShouldSerializespecularTexture()
        {
            return (this.specularTexture != null);
        }
        public bool ShouldSerializespecularColorFactor()
        {
            return this.specularColorFactor != null && !this.specularColorFactor.IsAlmostEqualTo(1.0f, float.Epsilon);
        }
        public bool ShouldSerializespecularColorTexture()
        {
            return (this.specularColorTexture != null);
        }
    }

}

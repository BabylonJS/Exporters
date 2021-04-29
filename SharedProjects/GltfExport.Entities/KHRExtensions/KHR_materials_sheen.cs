using System.Runtime.Serialization;
using Utilities;

namespace GLTFExport.Entities
{
    // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_sheen/README.md
    [DataContract]
    public class KHR_materials_sheen
    {
        // The sheen color in linear space, default value [0,0,0]
        [DataMember]
        public float[] sheenColorFactor { get; set; }

        // The sheen color (RGB). The sheen color is in sRGB transfer function
        [DataMember]
        public GLTFTextureInfo sheenColorTexture { get; set; }

        // The sheen roughness. Default is 0.0
        [DataMember]
        public float? sheenRoughnessFactor { get; set; }

        // The sheen roughness (Alpha) texture.
        [DataMember]
        public GLTFTextureInfo sheenRoughnessTexture { get; set; }

        public bool ShouldSerializesheenColorFactor()
        {
            return (this.sheenColorFactor != null && !this.sheenColorFactor.IsAlmostEqualTo(0, float.Epsilon));
        }

        public bool ShouldSerializesheenColorTexture()
        {
            return (this.sheenColorTexture != null);
        }

        public bool ShouldSerializesheenRoughnessFactor()
        {
            return this.sheenRoughnessFactor != null && !MathUtilities.IsAlmostEqualTo(this.sheenRoughnessFactor.Value, 0f, float.Epsilon);
        }

        public bool ShouldSerializesheenRoughnessTexture()
        {
            return (this.sheenRoughnessTexture != null);
        }
    }
}

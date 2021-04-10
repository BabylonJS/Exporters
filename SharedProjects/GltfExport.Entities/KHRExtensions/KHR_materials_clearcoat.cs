using System.Runtime.Serialization;
using Utilities;

namespace GLTFExport.Entities
{
    // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md
    [DataContract]
    public class KHR_materials_clearcoat
    {
        // The clearcoat layer intensity. optional, default 0.0
        [DataMember]
        public float? clearcoatFactor { get; set; }

        // The clearcoat layer intensity texture. Optional
        [DataMember]
        public GLTFTextureInfo clearcoatTexture { get; set; }

        // The clearcoat layer roughness. optional
        [DataMember]
        public float? clearcoatRoughnessFactor { get; set; }

        // The clearcoat layer roughness texture. optional, default 0.0
        [DataMember]
        public GLTFTextureInfo clearcoatRoughnessTexture { get; set; }

        // The clearcoat normal map texture. optional
        [DataMember]
        public GLTFTextureInfo clearcoatNormalTexture { get; set; }

        public bool ShouldSerializeclearcoatFactor()
        {
            return this.clearcoatFactor != null && !MathUtilities.IsAlmostEqualTo(this.clearcoatFactor.Value, 0f, float.Epsilon);
        }
        public bool ShouldSerializeclearcoatTexture()
        {
            return (this.clearcoatTexture != null);
        }
        public bool ShouldSerializeclearcoatRoughnessFactor()
        {
            return this.clearcoatRoughnessFactor != null && !MathUtilities.IsAlmostEqualTo(this.clearcoatRoughnessFactor.Value, 0f, float.Epsilon);
        }
        public bool ShouldSerializeclearcoatRoughnessTexture()
        {
            return (this.clearcoatRoughnessTexture != null);
        }
        public bool ShouldSerializeclearcoatNormalTexture()
        {
            return (this.clearcoatNormalTexture != null);
        }
    }

}

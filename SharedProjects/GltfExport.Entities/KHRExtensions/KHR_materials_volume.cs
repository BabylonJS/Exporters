using System.Runtime.Serialization;
using Utilities;

namespace GLTFExport.Entities
{
    // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_volume/README.md
    [DataContract]
    public class KHR_materials_volume
    {
        // The thickness of the volume beneath the surface. The value is given in the coordinate space of the mesh.
        // If the value is 0 the material is thin-walled. Otherwise the material is a volume boundary.
        // The doubleSided property has no effect on volume boundaries. Range is [0, +inf).
        // optional, default:0
        [DataMember]
        public float? thicknessFactor { get; set; }

        // A texture that defines the thickness, stored in the G channel. This will be multiplied by thicknessFactor. Range is [0, 1].
        // Optional
        [DataMember]
        public GLTFTextureInfo thicknessTexture { get; set; }

        // Density of the medium given as the average distance that light travels in the medium before interacting with a particle.
        // The value is given in world space. Range is (0, +inf).
        // optional, default +infinity
        [DataMember]
        public float? attenuationDistance { get; set; }

        // The color that white light turns into due to absorption when reaching the attenuation distance.
        // optional, default [1.0, 1.0,1.0]
        [DataMember]
        public float[] attenuationColor { get; set; }

        public bool ShouldSerializethicknessFactor()
        {
            return this.thicknessFactor != null && !MathUtilities.IsAlmostEqualTo(this.thicknessFactor.Value, 0f, float.Epsilon);
        }
        public bool ShouldSerializethicknessTexture()
        {
            return (this.thicknessTexture != null);
        }
        public bool ShouldSerializeattenuationDistance()
        {
            return this.attenuationDistance != null && this.attenuationDistance != 0 && this.attenuationDistance != float.PositiveInfinity;
        }
        public bool ShouldSerializeattenuationColor()
        {
            return this.attenuationColor != null && this.attenuationColor.Length == 3 && !this.attenuationColor.IsAlmostEqualTo(1.0f, float.Epsilon);
        }
    }

}

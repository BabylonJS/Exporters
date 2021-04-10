using System.Runtime.Serialization;
using Utilities;

namespace GLTFExport.Entities
{
    // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_transmission/README.md
    [DataContract]
    public class KHR_materials_transmission
    {
        // The base percentage of light that is transmitted through the surface. Default 0.0
        [DataMember]
        public float? transmissionFactor { get; set; }

        // A texture that defines the transmission percentage of the surface, stored in the R channel. 
        // This will be multiplied by transmissionFactor.
        [DataMember]
        public GLTFTextureInfo transmissionTexture { get; set; }

        public bool ShouldSerializetransmissionFactor()
        {
            return this.transmissionFactor != null && !MathUtilities.IsAlmostEqualTo(this.transmissionFactor.Value, 0f, float.Epsilon);
        }

        public bool ShouldSerializetransmissionTexture()
        {
            return (this.transmissionTexture != null);
        }
    }
}

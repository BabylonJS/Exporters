using System.Runtime.Serialization;
using Utilities;

namespace GLTFExport.Entities
{
    // https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_texture_transform
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
            return (this.offset != null && !this.offset.IsAlmostEqualTo(0, float.Epsilon));
        }
        public bool ShouldSerializerotation()
        {
            return !MathUtilities.IsAlmostEqualTo(this.rotation, 0f, float.Epsilon);
        }

        public bool ShouldSerializescale()
        {
            return this.scale != null && !this.scale.IsAlmostEqualTo(1, float.Epsilon);
        }

        public bool ShouldSerializetexCoord()
        {
            return (this.texCoord != null);
        }
    }
}

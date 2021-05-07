using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFMeshPrimitive : GLTFProperty
    {
        public enum FillMode
        {
            POINTS = 0,
            LINES = 1,
            LINE_LOOP = 2,
            LINE_STRIP = 3,
            TRIANGLES = 4,
            TRIANGLE_STRIP = 5,
            TRIANGLE_FAN = 6
        }

        public enum Attribute
        {
            POSITION,
            NORMAL,
            TANGENT,
            TEXCOORD_0,
            TEXCOORD_1,
            COLOR_0,
            JOINTS_0,
            JOINTS_1,
            WEIGHTS_0,
            WEIGHTS_1
        }

        [DataMember(IsRequired = true)]
        public Dictionary<string, int> attributes { get; set; }

        [DataMember]
        public int? indices { get; set; }

        [DataMember]
        public FillMode? mode { get; set; }

        [DataMember]
        public int? material { get; set; }

        [DataMember]
        public GLTFMorphTarget[] targets { get; set; }

        public bool ShouldSerializeattributes()
        {
            return (this.attributes != null);
        }

        public bool ShouldSerializeindices()
        {
            return (this.indices != null);
        }

        public bool ShouldSerializematerial()
        {
            return (this.material != null);
        }

        public bool ShouldSerializemode()
        {
            return (this.mode != null && this.mode != FillMode.TRIANGLES);
        }

        public bool ShouldSerializetargets()
        {
            return (this.targets != null);
        }
    }
}

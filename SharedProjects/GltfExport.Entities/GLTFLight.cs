using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Linq;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFLight : GLTFIndexedChildRootProperty
    {
        public enum LightType
        {
            point,      // BabylonLight.type == 0
            directional,// 1
            spot       // 2
        }


        // Property used by GLTFNodeExtension
        [DataMember]
        public int? light { get; set; }

        // Properties used by GLTFextension
        [DataMember]
        public float[] color { get; set; }

        [DataMember]
        public float intensity { get; set; }

        [DataMember]
        public string type { get; set; }         // ambient, directional, point or spot

        [DataMember]
        public float range { get; set; }            // point or spot

        [DataMember]
        public Spot spot { get; set; }              // spot


        public bool ShouldSerializecolor()
        {
            return (this.color != null && !this.color.SequenceEqual(new float[] {
                        1F,
                        1F,
                        1F}));
        }

        public bool ShouldSerializeintensity()
        {
            return (this.intensity != 1f);
        }

        public bool ShouldSerializetype()
        {
            return (this.type != null);
        }

        public bool ShouldSerializerange()
        {
            return (this.range != float.MaxValue);
        }

        public bool ShouldSerializename()
        {
            return (this.name != "");
        }


        [DataContract]
        public class Spot
        {
            // 0 < innerConeAngle < outerConeAngle < Math.PI /2.0
            [DataMember]
            public float? innerConeAngle { get; set; }

            [DataMember]
            public float? outerConeAngle { get; set; }

            public bool ShouldSerializeinnerConeAngle()
            {
                return (this.innerConeAngle != 0f);
            }

            public bool ShouldSerializeouterConeAngle()
            {
                return (this.outerConeAngle != 0.7853981633974483);
            }
        }
    }
}

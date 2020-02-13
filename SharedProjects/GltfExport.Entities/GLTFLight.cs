using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

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
        [DataMember(EmitDefaultValue = false)]
        public int? light { get; set; }

        // Properties used by GLTFextension
        [DataMember(EmitDefaultValue = false)]
        public float[] color { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float intensity { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string type { get; set; }         // ambient, directional, point or spot

        [DataMember(EmitDefaultValue = false)]
        public float range { get; set; }            // point or spot

        [DataMember(EmitDefaultValue = false)]
        public Spot spot { get; set; }              // spot



        [DataContract]
        public class Spot
        {
            // 0 < innerConeAngle < outerConeAngle < Math.PI /2.0
            [DataMember(EmitDefaultValue = false)]
            public float? innerConeAngle { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public float? outerConeAngle { get; set; }
        }
    }
}

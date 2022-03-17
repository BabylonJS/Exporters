using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonMorphTarget
    {
        [DataMember(EmitDefaultValue = false)]
        public string name { get; set; }

        [DataMember]
        public string id { get; set; }

        [DataMember(IsRequired = true)]
        public float influence { get; set; }

        [DataMember(IsRequired = true)]
        public float[] positions { get; set; }

        [DataMember(IsRequired = true)]
        public float[] normals { get; set; }

        [DataMember(IsRequired = false)]
        public float[] tangents { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public BabylonAnimation[] animations { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool autoAnimate { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public bool autoAnimateLoop { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public int? autoAnimateFrom { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? autoAnimateTo { get; set; }
    }
}

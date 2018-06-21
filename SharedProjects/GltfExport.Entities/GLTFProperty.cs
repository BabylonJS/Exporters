using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFProperty
    {
        [DataMember(EmitDefaultValue = false)]
        public GLTFExtensions extensions { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object extras { get; set; }
    }
}

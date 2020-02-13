using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFProperty
    {
        [DataMember(EmitDefaultValue = false)]
        public GLTFExtensions extensions { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Dictionary<string, object> extras { get; set; }
    }
}

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

        public bool ShouldSerializeextensions()
        {
            return this.extensions != null && this.extensions.Count > 0;
        }

        public bool ShouldSerializeextras()
        {
            return (this.extras != null && this.extras.Count > 0);
        }
    }
}

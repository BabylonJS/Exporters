using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFNodeExtensions
    {
        [DataMember(EmitDefaultValue = false)]
        public GLTFLight KHR_lights {get; set;}
    }
}

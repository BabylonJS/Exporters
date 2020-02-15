using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFAsset : GLTFProperty
    {
        [DataMember(IsRequired = true)]
        public string version { get; set; }

        [DataMember]
        public string generator { get; set; }

        [DataMember]
        public string copyright { get; set; }

        [DataMember]
        public string minVersion { get; set; }

        public bool ShouldSerializeversion()
        {
            return (this.version != null);
        }

        public bool ShouldSerializegenerator()
        {
            return (this.generator != null);
        }

        public bool ShouldSerializecopyright()
        {
            return (this.copyright != null);
        }

        public bool ShouldSerializeminVersion()
        {
            return (this.minVersion != null);
        }
    }
}

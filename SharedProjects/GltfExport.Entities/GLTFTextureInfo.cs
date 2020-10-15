using System.Runtime.Serialization;


namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFTextureInfo : GLTFChildRootProperty
    {
        [DataMember(IsRequired = true)]
        public int index { get; set; }

        [DataMember]
        public int? texCoord { get; set; }

        public bool ShouldSerializetexCoord()
        {
            return (this.texCoord != null && this.texCoord != 0);
        }
    }
}

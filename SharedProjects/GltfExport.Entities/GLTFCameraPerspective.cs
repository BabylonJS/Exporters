using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFCameraPerspective : GLTFProperty
    {
        [DataMember]
        public float? aspectRatio { get; set; }

        [DataMember(IsRequired = true)]
        public float yfov { get; set; }

        [DataMember]
        public float? zfar { get; set; }

        [DataMember(IsRequired = true)]
        public float znear { get; set; }

        public bool ShouldSerializeaspectRatio()
        {
            return (this.aspectRatio != null);
        }

        public bool ShouldSerializezfar()
        {
            return (this.zfar != null);
        }
    }
}

using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonPBRClearCoat
    {
        [DataMember]
        public bool isEnabled { get; set; } = false;

        [DataMember]
        public float intensity { get; set; } = 1.0f;

        [DataMember]
        public float roughness { get; set; } = 0.0f;

        [DataMember]
        public float indexOfRefraction { get; set; } = 1.5f;

        [DataMember]
        public BabylonTexture texture { get; set; }

        [DataMember]
        public BabylonTexture bumpTexture { get; set; }

        [DataMember]
        public bool isTintEnabled { get; set; } = false;

        [DataMember]
        public float[] tintColor { get; set; } = { 1.0f, 1.0f, 1.0f };

        [DataMember]
        public float tintThickness { get; set; } = 1.0f;

        [DataMember]
        public BabylonTexture tintTexture { get; set; }
    }
}
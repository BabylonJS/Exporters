using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonPBRSubSurfaceConfiguration
    {
        [DataMember]
        public bool isRefractionEnabled { get; set; } = false;
        [DataMember]
        public float refractionIntensity { get; set; }
        [DataMember]
        public BabylonTexture refractionIntensityTexture { get; set; }

        [DataMember]
        public float? maximumThickness { get; set; }

        [DataMember]
        public BabylonTexture thicknessTexture { get; set; }
        [DataMember]
        public float tintColorAtDistance { get; set; }
        [DataMember]
        public float[] tintColor { get; set; }
    }
}

using System.Runtime.Serialization;
namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonFurMaterial : BabylonMaterial
    {
        [DataMember]
        public string customType { get; set; }
        
        [DataMember]
        public int furLength { get; set; }

        [DataMember]
        public int furAngle { get; set; }

        [DataMember]
        public float[] furColor { get; set; }

        [DataMember]
        public float furOffset { get; set; }

        [DataMember]
        public int furSpacing { get; set; }

        [DataMember]
        public int[] furGravity { get; set; }

        [DataMember]
        public int furSpeed { get; set; }

        [DataMember]
        public int furDensity { get; set; }

        [DataMember]
        public int furTime { get; set; }

        [DataMember]
        public int quality { get; set; }

        [DataMember]
        public string sourceMeshName { get; set; }

        [DataMember]
        public BabylonTexture diffuseTexture { get; set; }

        public BabylonFurMaterial(string id) : base(id)
        {
            customType = "BABYLON.FurMaterial";
            furAngle = 0;
            furOffset = 0f;
            furGravity = new[] { 0, -1, 0 };
            furSpeed = 100;
            furTime = 0;
            quality = 30;
            furLength = 0;
        }
    }
}

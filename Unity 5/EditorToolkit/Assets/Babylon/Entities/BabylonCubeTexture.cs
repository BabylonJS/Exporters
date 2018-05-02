using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonCubeTexture : BabylonTexture
    {
        [DataMember]
        public string customType { get; private set; }

        [DataMember]
        public bool filtered { get; private set; }

        [DataMember]
        public float[] boundingBoxSize { get; set; }

        [DataMember]
        public float[] boundingBoxPosition { get; set; }
        [DataMember]
        public bool prefiltered = false;

        public BabylonCubeTexture()
        {
            SetCustomType("BABYLON.CubeTexture");
            isCube = true;
            filtered = true;
            prefiltered = false;
            boundingBoxSize = null;
            boundingBoxPosition = null;
        }

        public void SetCustomType(string type)
        {
            customType = type;
        }
    }
}

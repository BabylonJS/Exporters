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

        public BabylonCubeTexture()
        {
            SetCustomType("BABYLON.CubeTexture");
            this.filtered = true;
        }

        public void SetCustomType(string type)
        {
            customType = type;
        }
    }
}

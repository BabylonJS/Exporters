using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonBone
    {
        [DataMember]
        public string id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public int index { get; set; }

        [DataMember]
        public int parentBoneIndex { get; set; }

        public string parentNodeId { get; set; }

        [DataMember]
        public float[] matrix { get; set; }

        [DataMember]
        public BabylonAnimation animation { get; set; }

        [DataMember]
        public string linkedTransformNodeId { get; set; }

        public BabylonBone()
        {
            parentBoneIndex = -1;
            linkedTransformNodeId = null;
        }
    }
}

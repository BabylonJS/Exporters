using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonRange
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public int from { get; set; }

        [DataMember]
        public int to { get; set; }
    }
    
    [DataContract]
    public class BabylonSkeleton
    {
        [DataMember]
        public int id { get; set; }
        
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public BabylonBone[] bones { get; set; }

        [DataMember]
        public BabylonRange[] ranges { get; set; }

        [DataMember]
        public bool needInitialSkinMatrix { get; set; }
    }
}

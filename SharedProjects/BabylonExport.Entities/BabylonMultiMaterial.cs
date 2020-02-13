using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonMultiMaterial
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string id { get; set; }

        [DataMember]
        public string[] materials { get; set; }    
        
        public BabylonMultiMaterial()
        {

        }

        public BabylonMultiMaterial(BabylonMultiMaterial original)
        {
            name = original.name;
            id = original.id;
            materials = (string[]) original.materials.Clone();
        }
    }
}

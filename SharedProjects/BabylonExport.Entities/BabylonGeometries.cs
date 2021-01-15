using System.Linq;
using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonGeometries
    {
        [DataMember]
        public BabylonVertexData[] vertexData { get; set; }
    }
}

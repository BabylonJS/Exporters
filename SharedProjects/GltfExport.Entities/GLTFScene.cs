using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFScene : GLTFChildRootProperty
    {
        [DataMember]
        public int[] nodes { get; set; }

        public List<int> NodesList { get; private set; }

        public GLTFScene()
        {
            NodesList = new List<int>();
            extensions = new GLTFExtensions();
        }

        public void Prepare()
        {
            // Do not export empty arrays
            if (NodesList.Count > 0)
            {
                nodes = NodesList.ToArray();
            }
        }

        public bool ShouldSerializenodes()
        {
            return (this.nodes != null);
        }
    }
}

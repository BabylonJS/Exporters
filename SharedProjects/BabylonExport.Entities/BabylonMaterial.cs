using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public partial class BabylonMaterial
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string id { get; set; }

        [DataMember]
        public bool backFaceCulling { get; set; }

        [DataMember]
        public bool wireframe { get; set; }

        [DataMember]
        public float alpha { get; set; }

        [DataMember]
        public int alphaMode { get; set; }

        [DataMember]
        public int maxSimultaneousLights { get; set; }

        [DataMember]
        public Dictionary<string, object> metadata { get; set; }

        public bool isUnlit = false;

        public BabylonMaterial(string id)
        {
            this.id = id;
            backFaceCulling = true;
            alpha = 1.0f;
            alphaMode = 2;
            maxSimultaneousLights = 4;
        }

        public BabylonMaterial(BabylonMaterial original)
        {
            name = original.name;
            id = original.id;
            backFaceCulling = original.backFaceCulling;
            wireframe = original.wireframe;
            alpha = original.alpha;
            alphaMode = original.alphaMode;
            maxSimultaneousLights = 4;
            isUnlit = original.isUnlit;
            metadata = original.metadata != null ? new Dictionary<string, object>(original.metadata) : null;
        }
    }
}

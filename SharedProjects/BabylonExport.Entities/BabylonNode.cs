using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonNode : BabylonIAnimatable
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string id { get; set; }
        
        [DataMember]
        public string parentId { get; set; }

        [DataMember]
        public float[] position { get; set; }

        virtual public float[] rotation { get; set; }

        virtual public float[] scaling { get; set; }

        virtual public float[] rotationQuaternion { get; set; }

        [DataMember]
        public BabylonAnimation[] animations { get; set; }

        [DataMember]
        public bool autoAnimate { get; set; }

        [DataMember]
        public int autoAnimateFrom { get; set; }

        [DataMember]
        public int autoAnimateTo { get; set; }

        [DataMember]
        public bool autoAnimateLoop { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string tags { get; set; }

        [DataMember]
        public Dictionary<string, object> metadata { get; set; }

        // Animations exported for glTF but not for Babylon
        public List<BabylonAnimation> extraAnimations;
    }
}

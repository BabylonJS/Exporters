using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonTargetedAnimation
    {
        [DataMember]
        public BabylonAnimation animation { get; set; }

        [DataMember]
        public string targetId { get; set; }              // the id of the target. It can be a node or a bone.
    }

    [DataContract]
    public class BabylonAnimationGroup
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public float from { get; set; }

        [DataMember]
        public float to { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public IList<BabylonTargetedAnimation> targetedAnimations { get; set; }
    }
}

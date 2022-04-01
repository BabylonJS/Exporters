using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonPBRSheenConfiguration
    {
        [DataMember]
        public bool isEnabled { get; set; } = false;

        [DataMember]
        public float[] color { get; set; } 

        [DataMember]
        public BabylonTexture texture { get; set; }

        [DataMember]
        public float roughness { get; set; }

        [DataMember]
        public BabylonTexture textureRoughness { get; set; }

        [DataMember]
        public bool useRoughnessFromMainTexture { get; set; } = false;
    }
}

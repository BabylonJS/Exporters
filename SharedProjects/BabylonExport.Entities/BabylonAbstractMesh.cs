using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonAbstractMesh: BabylonNode
    {
        [DataMember]
        override public float[] rotation { get; set; }

        [DataMember]
        override public float[] scaling { get; set; }

        [DataMember]
        override public float[] rotationQuaternion { get; set; }

        [DataMember]
        public BabylonActions actions { get; set; }

        [DataMember]
        public bool checkCollisions { get; set; }

        [DataMember]
        public bool pickable { get; set; }

        [DataMember]
        public bool showBoundingBox { get; set; }

        // Identifier shared between a mesh and its instances
        public int idGroupInstance;
    }
}

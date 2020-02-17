using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFNode : GLTFIndexedChildRootProperty
    {
        [DataMember]
        public int? camera { get; set; }

        [DataMember]
        public int[] children { get; set; }

        [DataMember]
        public int? skin { get; set; }

        // Either matrix or Translation+Rotation+Scale
        //[DataMember]
        //public float[] matrix { get; set; }

        [DataMember]
        public int? mesh { get; set; }

        [DataMember]
        public float[] translation { get; set; }

        [DataMember]
        public float[] rotation { get; set; }

        [DataMember]
        public float[] scale { get; set; }

        [DataMember]
        public float[] weights { get; set; }

        public List<int> ChildrenList { get; private set; }

        // Used to compute transform world matrix
        public GLTFNode parent;

        public GLTFNode()
        {
            ChildrenList = new List<int>();
        }

        public void Prepare()
        {
            // Do not export empty arrays
            if (ChildrenList.Count > 0)
            {
                children = ChildrenList.ToArray();
            }
        }

        public bool ShouldSerializecamera() { 
            return (this.camera != null);
        }

        public bool ShouldSerializechildren()
        {
            return (this.children != null);
        }

        public bool ShouldSerializeskin()
        {
            return (this.skin != null);
        }

        public bool ShouldSerializemesh()
        {
            return (this.mesh != null);
        }

        //public bool ShouldSerializematrix() {
        //    return (this.matrix != null);
        //}

        public bool ShouldSerializetranslation()
        {
            return (this.translation != null) && !this.translation.SequenceEqual(new float[] { 0f, 0f, 0f});
        }

        public bool ShouldSerializerotation()
        {
            return (this.rotation != null) && !this.rotation.SequenceEqual(new float[] { 0f, 0f, 0f, 1f});
        }

        public bool ShouldSerializescale()
        {
            return (this.scale != null) && !this.scale.SequenceEqual(new float[] { 1f, 1f, 1f});
        }

        public bool ShouldSerializeweights()
        {
            return (this.weights != null);
        }
    }
}

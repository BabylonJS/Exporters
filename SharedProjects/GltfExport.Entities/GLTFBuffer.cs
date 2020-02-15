using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFBuffer : GLTFIndexedChildRootProperty
    {
        [DataMember]
        public string uri { get; set; }

        [DataMember(IsRequired = true)]
        public int byteLength { get; set; }

        public List<byte> bytesList = new List<byte>();
        public List<GLTFBufferView> BufferViews = new List<GLTFBufferView>();

        public bool ShouldSerializeuri()
        {
            return (this.uri != null);
        }
    }
}

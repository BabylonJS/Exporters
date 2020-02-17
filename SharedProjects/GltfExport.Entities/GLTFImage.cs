using System;
using System.IO;
using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFImage : GLTFIndexedChildRootProperty
    {
        [DataMember]
        public string uri
        {
            get { return _uri; }
            set
            {
                _uri = value;
            }
        }

        [DataMember]
        public string mimeType { get; set; } // "image/jpeg" or "image/png"

        [DataMember]
        public int? bufferView { get; set; }

        public string FileExtension;
        private string _uri;

        public bool ShouldSerializeuri()
        {
            return (this.uri != null);
        }

        public bool ShouldSerializemimeType()
        {
            return (this.mimeType != null);
        }

        public bool ShouldSerializebufferView()
        {
            return (this.bufferView != null);
        }
    }
}

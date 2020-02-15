using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFCamera : GLTFIndexedChildRootProperty
    {
        public enum CameraType
        {
            perspective,
            orthographic
        }

        [DataMember]
        public GLTFCameraOrthographic orthographic { get; set; }

        [DataMember]
        public GLTFCameraPerspective perspective { get; set; }

        [DataMember(IsRequired = true)]
        public string type { get; set; }

        public GLTFNode gltfNode;

        public bool ShouldSerializeorthographic()
        {
            return (this.orthographic != null);
        }

        public bool ShouldSerializeperspective()
        {
            return (this.perspective != null);
        }
    }
}

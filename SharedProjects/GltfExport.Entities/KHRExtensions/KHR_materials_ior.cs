using System.Runtime.Serialization;
using Utilities;

namespace GLTFExport.Entities
{
    // https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_materials_ior/README.md
    [DataContract]
    public class KHR_materials_ior
    {
        public const float DefaultIOR = 1.5f;

        // The index of refraction. optional, default 1.5
        [DataMember]
        public float? ior { get; set; }

        public bool ShouldSerializeior()
        {
            return (ior != null && ior != DefaultIOR);
        }
    }
}

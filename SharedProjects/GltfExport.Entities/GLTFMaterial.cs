using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFMaterial : GLTFIndexedChildRootProperty
    {
        public enum AlphaMode
        {
            OPAQUE,
            MASK,
            BLEND
        }

        [DataMember(EmitDefaultValue = false)]
        public GLTFPBRMetallicRoughness pbrMetallicRoughness { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFTextureInfo normalTexture { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFTextureInfo occlusionTexture { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFTextureInfo emissiveTexture { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float[] emissiveFactor { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string alphaMode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float? alphaCutoff { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool doubleSided { get; set; }

        public string id;
    }


    // https://github.com/AltspaceVR/glTF/blob/avr-sampler-offset-tile/extensions/2.0/Khronos/KHR_texture_transform/README.md
    [DataContract]
    public class KHR_texture_transform
    {
        [DataMember(EmitDefaultValue = false)]
        public float[] offset { get; set; }     // array[2], default value [0,0]

        [DataMember(EmitDefaultValue = false)]
        public float rotation { get; set; }     // in radian, default value 0

        [DataMember(EmitDefaultValue = false)]
        public float[] scale { get; set; }      // array[2], default value [1,1]

        [DataMember(EmitDefaultValue = false)]
        public int? texCoord { get; set; }       // min value 0, default null
    }
}

using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Utilities;

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

        [DataMember]
        public GLTFPBRMetallicRoughness pbrMetallicRoughness { get; set; }

        [DataMember]
        public GLTFTextureInfo normalTexture { get; set; }

        [DataMember]
        public GLTFTextureInfo occlusionTexture { get; set; }

        [DataMember]
        public GLTFTextureInfo emissiveTexture { get; set; }

        [DataMember]
        public float[] emissiveFactor { get; set; }

        [DataMember, JsonConverter(typeof(StringEnumConverter))]
        public AlphaMode alphaMode { get; set; }

        [DataMember]
        public float? alphaCutoff { get; set; }

        [DataMember]
        public bool doubleSided { get; set; }

        public string id;

        public bool ShouldSerializepbrMetallicRoughness()
        {
            return (this.pbrMetallicRoughness != null);
        }

        public bool ShouldSerializenormalTexture()
        {
            return (this.normalTexture != null);
        }

        public bool ShouldSerializeocclusionTexture()
        {
            return (this.occlusionTexture != null);
        }

        public bool ShouldSerializeemissiveTexture()
        {
            return (this.emissiveTexture != null);
        }

        public bool ShouldSerializeemissiveFactor()
        {
            return (this.emissiveFactor != null && !this.emissiveFactor.IsAlmostEqualTo(0,float.Epsilon));
        }

        public bool ShouldSerializealphaMode()
        {
            return (this.alphaMode != AlphaMode.OPAQUE);
        }

        public bool ShouldSerializealphaCutoff()
        {
            return (this.alphaCutoff != null && !MathUtilities.IsAlmostEqualTo(this.alphaCutoff.Value, 0.5f, float.Epsilon));
        }

        public bool ShouldSerializedoubleSided()
        {
            return this.doubleSided;
        }
    }


    // https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_texture_transform
    [DataContract]
    public class KHR_texture_transform
    {
        [DataMember]
        public float[] offset { get; set; }     // array[2], default value [0,0]

        [DataMember]
        public float rotation { get; set; }     // in radian, default value 0

        [DataMember]
        public float[] scale { get; set; }      // array[2], default value [1,1]

        [DataMember]
        public int? texCoord { get; set; }       // min value 0, default null


        public bool ShouldSerializeoffset()
        {
            return (this.offset != null && !this.offset.IsAlmostEqualTo(0, float.Epsilon));
        }
        public bool ShouldSerializerotation()
        {
            return !MathUtilities.IsAlmostEqualTo(this.rotation,0f,float.Epsilon);
        }

        public bool ShouldSerializescale()
        {
            return this.scale != null && !this.scale.IsAlmostEqualTo(1,float.Epsilon);
        }

        public bool ShouldSerializetexCoord()
        {
            return (this.texCoord != null);
        }
    }

    // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md
    [DataContract]
    public class KHR_materials_clearcoat
    {
        // The clearcoat layer intensity. optional, default 0.0
        [DataMember]
        public float? clearcoatFactor { get; set; }
        
        // The clearcoat layer intensity texture. Optional
        [DataMember]
        public GLTFTextureInfo clearcoatTexture { get; set; }
        
        // The clearcoat layer roughness. optional
        [DataMember]
        public float? clearcoatRoughnessFactor { get; set; }
        
        // The clearcoat layer roughness texture. optional, default 0.0
        [DataMember]
        public GLTFTextureInfo clearcoatRoughnessTexture { get; set; }
        
        // The clearcoat normal map texture. optional
        [DataMember]
        public GLTFTextureInfo clearcoatNormalTexture { get; set; }

        public bool ShouldSerializeclearcoatFactor()
        {
            return this.clearcoatFactor != null && !MathUtilities.IsAlmostEqualTo(this.clearcoatFactor.Value, 0f, float.Epsilon);
        }
        public bool ShouldSerializeclearcoatTexture()
        {
            return (this.clearcoatTexture != null );
        }
        public bool ShouldSerializeclearcoatRoughnessFactor()
        {
            return this.clearcoatRoughnessFactor != null && !MathUtilities.IsAlmostEqualTo(this.clearcoatRoughnessFactor.Value, 0f, float.Epsilon);
        }
        public bool ShouldSerializeclearcoatRoughnessTexture()
        {
            return (this.clearcoatRoughnessTexture != null);
        }
        public bool ShouldSerializeclearcoatNormalTexture()
        {
            return (this.clearcoatNormalTexture != null);
        }
    }

    // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_sheen/README.md
    [DataContract]
    public class KHR_materials_sheen
    {
        // The sheen color in linear space, default value [0,0,0]
        [DataMember]
        public float[] sheenColorFactor { get; set; }

        // The sheen color (RGB). The sheen color is in sRGB transfer function
        [DataMember]
        public GLTFTextureInfo sheenColorTexture { get; set; }
        
        // The sheen roughness. Default is 0.0
        [DataMember]
        public float? sheenRoughnessFactor { get; set; }

        // The sheen roughness (Alpha) texture.
        [DataMember]
        public GLTFTextureInfo sheenRoughnessTexture { get; set; }

        public bool ShouldSerializesheenColorFactor()
        {
            return (this.sheenColorFactor != null && !this.sheenColorFactor.IsAlmostEqualTo(0,float.Epsilon));
        }

        public bool ShouldSerializesheenColorTexture()
        {
            return (this.sheenColorTexture != null);
        }

        public bool ShouldSerializesheenRoughnessFactor()
        {
            return this.sheenRoughnessFactor != null && !MathUtilities.IsAlmostEqualTo(this.sheenRoughnessFactor.Value, 0f, float.Epsilon);
        }

        public bool ShouldSerializesheenRoughnessTexture()
        {
            return (this.sheenRoughnessTexture != null);
        }
    }

    // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_transmission/README.md
    [DataContract]
    public class KHR_materials_transmission
    {
        // The base percentage of light that is transmitted through the surface. Default 0.0
        [DataMember]
        public float? transmissionFactor { get; set; }

        // A texture that defines the transmission percentage of the surface, stored in the R channel. 
        // This will be multiplied by transmissionFactor.
        [DataMember]
        public GLTFTextureInfo transmissionTexture { get; set; }

        public bool ShouldSerializetransmissionFactor()
        {
            return this.transmissionFactor != null && !MathUtilities.IsAlmostEqualTo(this.transmissionFactor.Value, 0f, float.Epsilon);
        }

        public bool ShouldSerializetransmissionTexture()
        {
            return (this.transmissionTexture != null);
        }
    }
}

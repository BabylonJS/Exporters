using System.Linq;
using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    // https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_materials_pbrSpecularGlossiness
    [DataContract]
    public class KHR_materials_pbrSpecularGlossiness
    {
        /// <summary>
        /// The RGBA components of the reflected diffuse color of the material. Metals have a diffuse value of [0.0, 0.0, 0.0].
        /// The fourth component (A) is the opacity of the material. The values are linear.
        /// </summary>
        [DataMember]
        public float[] diffuseFactor { get; set; }

        /// <summary>
        /// The diffuse texture. This texture contains RGB components of the reflected diffuse color of the material encoded with
        /// the sRGB transfer function. If the fourth component (A) is present, it represents the linear alpha coverage of the material.
        /// Otherwise, an alpha of 1.0 is assumed. The alphaMode property specifies how alpha is interpreted. 
        /// The stored texels must not be premultiplied.
        /// </summary>
        [DataMember]
        public GLTFTextureInfo diffuseTexture { get; set; }

        /// <summary>
        /// The specular RGB color of the material. This value is linear.
        /// </summary>
        [DataMember]
        public float[] specularFactor { get; set; }

        /// <summary>
        /// The glossiness or smoothness of the material. A value of 1.0 means the material has full glossiness or is perfectly smooth.
        /// A value of 0.0 means the material has no glossiness or is perfectly rough. This value is linear.
        /// </summary>
        [DataMember]
        public float glossinessFactor { get; set; }

        /// <summary>
        /// The specular-glossiness texture is an RGBA texture, containing the specular color (RGB) encoded with the sRGB transfer
        /// function and the linear glossiness value (A).
        /// </summary>
        [DataMember]
        public GLTFTextureInfo specularGlossinessTexture { get; set; }

        public bool ShouldSerializediffuseFactor()
        {
            return (this.diffuseFactor != null && this.diffuseFactor.Length == 4 && this.diffuseFactor.Any(f => f != 1.0));
        }
        public bool ShouldSerializediffuseTexture()
        {
            return (this.diffuseTexture != null);
        }
        public bool ShouldSerializespecularFactor()
        {
            return (this.specularFactor != null && this.specularFactor.Length == 3 && this.specularFactor.Any(f=>f!=1.0));
        }
        public bool ShouldSerializeglossinessFactor()
        {
            return (this.glossinessFactor != 1.0);
        }
        public bool ShouldSerializespecularGlossinessTexture()
        {
            return (this.specularGlossinessTexture != null);
        }
    }

}

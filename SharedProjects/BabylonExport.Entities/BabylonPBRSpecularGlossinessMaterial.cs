using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    /// <summary>
    /// The PBR material of BJS following the specular glossiness convention.
    /// This fits to the PBR convention in the GLTF definition:
    /// https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_materials_pbrSpecularGlossiness
    /// </summary>
    [DataContract]
    public class BabylonPBRSpecularGlossinessMaterial : BabylonPBRBaseSimpleMaterial
    {
        public static float GlossinessDefault = 1f;

        public BabylonPBRSpecularGlossinessMaterial(string id) : base(id)
        {
            customType = "BABYLON.PBRSpecularGlossinessMaterial";
            glossiness = GlossinessDefault;
            specularColor = WhiteColor();
            specularGlossinessTexture = null;
        }

        public BabylonPBRSpecularGlossinessMaterial(BabylonPBRSpecularGlossinessMaterial original) : base(original)
        {
            glossiness = original.glossiness;
            specularColor = original.specularColor;
            specularGlossinessTexture = original.specularGlossinessTexture;
        }

        /// <summary>
        /// Specifies the glossiness of the material. This indicates "how sharp is the reflection".
        /// </summary>
        [DataMember]
        public float glossiness { get; set; }

        /// <summary>
        /// Specifies the specular color of the material. This indicates how reflective is the material (none to mirror).
        /// </summary>
        [DataMember]
        public float[] specularColor { get; set; }

        /// <summary>
        /// Specifies both the specular color RGB and the glossiness A of the material per pixels
        /// </summary>
        [DataMember]
        public BabylonTexture specularGlossinessTexture { get; set; }
        
        [DataMember]
        public BabylonTexture diffuseTexture { get; set; }
    }
}

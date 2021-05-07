using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    /// <summary>
    /// The PBR material of BJS following the metal roughness convention.
    /// This fits to the PBR convention in the GLTF definition:
    /// https://github.com/KhronosGroup/glTF/tree/2.0/specification/2.0
    /// </summary>
    [DataContract]
    public class BabylonPBRMetallicRoughnessMaterial : BabylonPBRBaseSimpleMaterial
    {

        public static float metallicDefault = 0;
        public static float RoughnessDefault = .9f;

        public BabylonPBRMetallicRoughnessMaterial(string id) : base(id)
        {
            customType = "BABYLON.PBRMetallicRoughnessMaterial";
            metallic = metallicDefault;
            roughness = RoughnessDefault;
            metallicRoughnessTexture = null;
        }

        public BabylonPBRMetallicRoughnessMaterial(BabylonPBRMetallicRoughnessMaterial original) : base(original)
        {
            metallic = original.metallic;
            roughness = original.roughness;
            metallicRoughnessTexture = original.metallicRoughnessTexture;
        }

        [DataMember]
        public float metallic { get; set; }

        [DataMember]
        public float roughness { get; set; }

        [DataMember]
        public BabylonTexture metallicRoughnessTexture { get; set; }

    }
}

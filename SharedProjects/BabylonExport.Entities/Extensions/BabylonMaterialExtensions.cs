using System.Linq;

namespace BabylonExport.Entities
{
    public static class BabylonPbrExtensions
    {
        private static float[] _white = BabylonPBRMaterial.WhiteColor();

        public static bool IsClearCoatEnabled(this BabylonPBRMaterial mat) => mat.clearCoat != null && mat.clearCoat.isEnabled;
        public static bool IsSheenEnabled(this BabylonPBRMaterial mat) => mat.sheen != null && mat.sheen.isEnabled;
        public static bool IsIorEnabled(this BabylonPBRMaterial mat) => mat.indexOfRefraction != null;
        public static bool IsSpecularEnabled(this BabylonPBRMaterial mat)
        {
            return (mat.metallicF0Factor != null && mat.metallicF0Factor != 1.0) ||
                   (mat.metallicReflectanceColor != null && !Enumerable.SequenceEqual(mat.metallicReflectanceColor, _white)) ||
                    mat.metallicReflectanceTexture != null ||
                    mat.reflectanceTexture != null;
        }
        public static bool IsVolumeEnabled(this BabylonPBRMaterial mat) => mat.subSurface != null && mat.subSurface.maximumThickness != null;
        public static bool IsMetallicWorkflow(this BabylonPBRMaterial mat) => mat.metallic != null || mat.roughness != null || mat.metallicTexture != null;
        public static bool IsRefractionEnabled(this BabylonPBRMaterial mat) => mat.subSurface != null && mat.subSurface.isRefractionEnabled;
    }
}

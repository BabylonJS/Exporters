namespace BabylonExport.Entities
{
    public static class BabylonPbrExtensions
    {
        public static bool IsClearCoatEnabled(this BabylonPBRMaterial mat) => mat.clearCoat != null && mat.clearCoat.isEnabled;
        public static bool IsSheenEnabled(this BabylonPBRMaterial mat) => mat.sheen != null && mat.sheen.isEnabled;
        public static bool IsIorEnabled(this BabylonPBRMaterial mat) => mat.indexOfRefraction != null;
        public static bool IsSpecularEnabled(this BabylonPBRMaterial mat) => mat.metallicF0Factor != null;
        public static bool IsVolumeEnabled(this BabylonPBRMaterial mat) => mat.subSurface.maximumThickness != null;
    }
}

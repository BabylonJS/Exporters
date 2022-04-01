namespace BabylonExport.Entities
{
    public static class BabylonPbrExtensions
    {
        public static bool IsIorEnabled(this BabylonPBRBaseSimpleMaterial mat) => mat.indexOfRefraction != null;
        public static bool IsSpecularEnabled(this BabylonPBRBaseSimpleMaterial mat) => mat.metallicF0Factor != null;
        public static bool IsVolumeEnabled(this BabylonPBRBaseSimpleMaterial mat) => mat.subSurface.maximumThickness != null;
    }
}

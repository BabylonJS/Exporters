using BabylonExport.Entities;

namespace GLTFExport.Entities
{
    public class GLTFGlobalVertex
    {
        public BabylonVector3 Position { get; set; }
        public BabylonVector3 Normal { get; set; }
        public BabylonQuaternion Tangent { get; set; }
        public BabylonVector2 UV { get; set; }
        public BabylonVector2 UV2 { get; set; }
        public float[] Color { get; set; }
        public ushort[] BonesIndices { get; set; }
        public float[] BonesWeights { get; set; }
    }
}

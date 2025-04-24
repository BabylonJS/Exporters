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
        public BabylonVector2 UV3 { get; set; }
        public BabylonVector2 UV4 { get; set; }
        public BabylonVector2 UV5 { get; set; }
        public BabylonVector2 UV6 { get; set; }
        public BabylonVector2 UV7 { get; set; }
        public BabylonVector2 UV8 { get; set; }
        public float[] Color { get; set; }
        public int[] BonesIndices { get; set; }
        public int[] BonesIndicesExtra { get; set; }
        public float[] BonesWeights { get; set; }
        public float[] BonesWeightsExtra { get; set; }
    }
}

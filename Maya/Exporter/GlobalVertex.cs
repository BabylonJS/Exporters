using Maya2Babylon;

namespace MayaBabylon
{
    public struct GlobalVertex
    {
        public int BaseIndex { get; set; }
        public int CurrentIndex { get; set; }
        public float[] Position { get; set; } // Vec3
        public float[] Normal { get; set; } // Vec3
        public float[] Tangent { get; set; } // Vec3
        public float[] UV { get; set; } // Vec2
        public float[] UV2 { get; set; } // Vec2
        public int BonesIndices { get; set; }
        public float[] Weights { get; set; } // Vec4
        public int BonesIndicesExtra { get; set; }
        public float[] WeightsExtra { get; set; } // Vec4
        public float[] Color { get; set; } // Vec4

        public GlobalVertex(GlobalVertex other)
        {
            this.BaseIndex = other.BaseIndex;
            this.CurrentIndex = other.CurrentIndex;
            this.Position = other.Position;
            this.Normal = other.Normal;
            this.Tangent = other.Tangent;
            this.UV = other.UV;
            this.UV2 = other.UV2;
            this.BonesIndices = other.BonesIndices;
            this.Weights = other.Weights;
            this.BonesIndicesExtra = other.BonesIndicesExtra;
            this.WeightsExtra = other.WeightsExtra;
            this.Color = other.Color;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is GlobalVertex other)
            {
                // Note the logic located into the Tools.IsAlmostEqualTo Extension which is also check for null parameters.
                return 
                    other.BaseIndex == BaseIndex &&
                    other.Position.IsAlmostEqualTo(Position, Tools.Epsilon) &&
                    other.Normal.IsAlmostEqualTo(Normal, Tools.Epsilon) &
                    other.UV.IsAlmostEqualTo(UV, Tools.Epsilon) &&
                    other.UV2.IsAlmostEqualTo(UV2, Tools.Epsilon) &&
                    other.Weights.IsAlmostEqualTo(Weights, Tools.Epsilon) &&
                    other.WeightsExtra.IsAlmostEqualTo(WeightsExtra, Tools.Epsilon) &&
                    other.Color.IsAlmostEqualTo(Color, Tools.Epsilon) && 
                    other.BonesIndices == BonesIndices;
            }
            return false;
        }
    }
}

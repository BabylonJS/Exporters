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
            if (!(obj is GlobalVertex))
            {
                return false;
            }

            var other = (GlobalVertex)obj;

            if (other.BaseIndex != BaseIndex)
            {
                return false;
            }

            if (!other.Position.IsAlmostEqualTo(Position, Tools.Epsilon))
            {
                return false;
            }

            if (!other.Normal.IsAlmostEqualTo(Normal, Tools.Epsilon))
            {
                return false;
            }

            if (UV != null && !other.UV.IsAlmostEqualTo(UV, Tools.Epsilon))
            {
                return false;
            }

            if (UV2 != null && !other.UV2.IsAlmostEqualTo(UV2, Tools.Epsilon))
            {
                return false;
            }

            if (Weights != null && !other.Weights.IsAlmostEqualTo(Weights, Tools.Epsilon))
            {
                return false;
            }

            if (WeightsExtra != null && !other.WeightsExtra.IsAlmostEqualTo(WeightsExtra, Tools.Epsilon))
            {
                return false;
            }

            if (Color != null && !other.Color.IsAlmostEqualTo(Color, Tools.Epsilon))
            {
                return false;
            }

            return other.BonesIndices == BonesIndices;
        }
    }
}

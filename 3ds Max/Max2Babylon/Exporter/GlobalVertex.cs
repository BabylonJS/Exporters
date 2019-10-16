using Autodesk.Max;
using Utilities;

namespace Max2Babylon
{
    public struct GlobalVertex
    {

        public int BaseIndex { get; set; }
        public int CurrentIndex { get; set; }
        public IPoint3 Position { get; set; }
        public IPoint3 Normal { get; set; }
        public float[] Tangent { get; set; }
        public IPoint2 UV { get; set; }
        public IPoint2 UV2 { get; set; }
        public int BonesIndices { get; set; }
        public IPoint4 Weights { get; set; }
        public int BonesIndicesExtra { get; set; }
        public IPoint4 WeightsExtra { get; set; }
        public float[] Color { get; set; }

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
            /*
            return string.Format("{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}",
                                    BaseIndex,
                                    CurrentIndex,
                                    Position != null ? Position.ToArray() : null,
                                    Normal != null ? Normal.ToArray() : null,
                                    Tangent,
                                    UV != null ? UV.ToArray() : null,
                                    UV2 != null ? UV.ToArray() : null,
                                    BonesIndices,
                                    Weights != null ? Weights.ToArray() : null,
                                    BonesIndicesExtra,
                                    WeightsExtra != null ? WeightsExtra.ToArray() : null,
                                    Color).GetHashCode();
            */
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

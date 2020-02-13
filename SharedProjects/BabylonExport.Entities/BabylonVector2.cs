namespace BabylonExport.Entities
{
    public class BabylonVector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public BabylonVector2() { }

        public BabylonVector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public float[] ToArray()
        {
            return new [] {X, Y};
        }

        /**
         * Returns a new Vector3 set from the index "countOffset" x 2 of the passed array.
         */
        public static BabylonVector2 FromArray(float[] array, int countOffset = 0)
        {
            var offset = countOffset * 2;
            return new BabylonVector2(array[offset], array[offset + 1]);
        }

        public override string ToString()
        {
            return "{ X=" + X + ", Y=" + Y + " }";
        }
    }
}

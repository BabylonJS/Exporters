using System;
using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonAnimationKey : IComparable<BabylonAnimationKey>, ICloneable
    {
        [DataMember]
        public int frame { get; set; }

        [DataMember]
        public float[] values { get; set; }

        public object Clone()
        {
            return new BabylonAnimationKey
            {
                frame = frame,
                values = (float[])values.Clone()
            };
        }

        public int CompareTo(BabylonAnimationKey other)
        {
            if (other == null)
                return 1;
            else
                return this.frame.CompareTo(other.frame);
        }
    }
}

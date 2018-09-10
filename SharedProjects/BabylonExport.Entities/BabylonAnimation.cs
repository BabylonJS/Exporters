using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonAnimation : ICloneable
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string property { get; set; }

        [DataMember]
        public int dataType { get; set; }

        [DataMember]
        public bool enableBlending { get; set; }

        [DataMember]
        public float blendingSpeed { get; set; }

        [DataMember]
        public int loopBehavior { get; set; }

        [DataMember]
        public int framePerSecond { get; set; }

        [DataMember]
        public BabylonAnimationKey[] keys { get; set; }

        public List<BabylonAnimationKey> keysFull { get; set; }

        public enum DataType
        {
            Float = 0,
            Vector3 = 1,
            Quaternion = 2,
            Matrix = 3,
            Color3 = 4,
        }

        public enum LoopBehavior
        {
            Relative = 0,
            Cycle = 1,
            Constant = 2
        }

        public BabylonAnimation()
        {
            enableBlending = false;
            blendingSpeed = 0.01f;
        }

        public object Clone()
        {
            return new BabylonAnimation
            {
                name = name,
                property = property,
                dataType = dataType,
                enableBlending = enableBlending,
                blendingSpeed = blendingSpeed,
                loopBehavior = loopBehavior,
                framePerSecond = framePerSecond,
                keys = (BabylonAnimationKey[])keys.Clone(),
                keysFull = new List<BabylonAnimationKey>(keysFull.Select(k => (BabylonAnimationKey)k.Clone()))
            };
        }
    }
}

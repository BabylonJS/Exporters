using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFAnimationSampler : GLTFProperty
    {
        public enum Interpolation
        {
            [EnumMember(Value = "LINEAR")]
            LINEAR,
            [EnumMember(Value = "STEP")]
            STEP,
            [EnumMember(Value = "CUBICSPLINE")]
            CUBICSPLINE
        }

        /// <summary>
        /// The index of an accessor containing keyframe input values, e.g., time. That accessor must have componentType FLOAT.
        /// The values represent time in seconds with time[0] >= 0.0, and strictly increasing values, i.e., time[n + 1] > time[n].
        /// </summary>
        [DataMember(IsRequired = true)]
        public int input { get; set; }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public Interpolation interpolation { get; private set; }

        /// <summary>
        /// The index of an accessor containing keyframe output values.
        /// When targeting TRS target, the accessor.componentType of the output values must be FLOAT.
        /// When targeting morph weights, the accessor.componentType of the output values must be FLOAT
        /// or normalized integer where each output element stores values with a count equal to the number of morph targets.
        /// </summary>
        [DataMember(IsRequired = true)]
        public int output { get; set; }

        public int index;

        public void SetInterpolation(Interpolation interpolation)
        {
            this.interpolation = interpolation;
        }

        public GLTFAnimationSampler()
        {
            // For GLTF, default value is LINEAR
            // but gltf loader of BABYLON doesn't handle missing interpolation value
            SetInterpolation(Interpolation.LINEAR);
        }

        public bool ShouldSerializeinterpolation()
        {
            return (this.interpolation != Interpolation.LINEAR);
        }
    }
}

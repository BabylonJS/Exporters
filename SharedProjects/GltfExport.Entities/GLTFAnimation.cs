using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFAnimation : GLTFChildRootProperty
    {
        /// <summary>
        /// An array of channels, each of which targets an animation's sampler at a node's property.
        /// Different channels of the same animation can't have equal targets.
        /// </summary>
        [DataMember(IsRequired = true)]
        public GLTFChannel[] channels { get; set; }

        /// <summary>
        /// An array of samplers that combines input and output accessors with an interpolation algorithm to define a keyframe graph (but not its target).
        /// </summary>
        [DataMember(IsRequired = true)]
        public GLTFAnimationSampler[] samplers { get; set; }

        public List<GLTFChannel> ChannelList { get; private set; }
        public List<GLTFAnimationSampler> SamplerList { get; private set; }

        public GLTFAnimation()
        {
            ChannelList = new List<GLTFChannel>();
            SamplerList = new List<GLTFAnimationSampler>();
        }

        public void Prepare()
        {
            // Do not export empty arrays
            if (ChannelList.Count > 0)
            {
                channels = ChannelList.ToArray();
            }
            if (SamplerList.Count > 0)
            {
                samplers = SamplerList.ToArray();
            }
        }

        public bool ShouldSerializechannels()
        {
            return (this.channels != null);
        }

        public bool ShouldSerializesamplers()
        {
            return (this.samplers != null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BabylonExport.Entities;
using GLTFExport.Entities;

namespace BabylonExport.Entities
{
    public partial class BabylonNode
    {
        private string animationTargetId;
        public string AnimationTargetId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(animationTargetId))
                {
                    return name;
                }

                return animationTargetId;
            }
            set => animationTargetId = value;
        }
    }

    public partial class BabylonBone
    {
        private string animationTargetId;
        public string AnimationTargetId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(animationTargetId))
                {
                    return name;
                }

                return animationTargetId;
            }
            set => animationTargetId = value;
        }
    }

}

namespace Babylon2GLTF
{
    #region Serializable glTF Objects

    [DataContract]
    class ASBAnimationRetargeting: GLTFProperty
    {
        [DataMember(EmitDefaultValue = false)]
        public string id { get; set; }
    }

    #endregion

    internal partial class GLTFExporter
    {
        private const string AsoboAnimationRetargeting = "ASOBO_animation_retargeting";

        public void ASOBOAnimationRetargetingTargetExtension(ref GLTF gltf,ref GLTFChannelTarget gltfChannel, BabylonNode babylonNode )
        {
            ASBAnimationRetargeting extensionObject = new ASBAnimationRetargeting
            {
                id = babylonNode.AnimationTargetId
            };

            if (gltfChannel != null)
            {
                if (gltfChannel.extensions == null)
                {
                    gltfChannel.extensions = new GLTFExtensions();
                }
                gltfChannel.extensions[AsoboAnimationRetargeting] = extensionObject;
            }

            if (gltf.extensionsUsed == null)
            {
                gltf.extensionsUsed = new List<string>();
            }
            if (!gltf.extensionsUsed.Contains(AsoboAnimationRetargeting))
            {
                gltf.extensionsUsed.Add(AsoboAnimationRetargeting);
            }
        }

        public void ASOBOAnimationRetargetingNodeExtension(ref GLTF gltf, ref GLTFNode gltfNode,BabylonNode babylonNode )
        {
            ASBAnimationRetargeting extensionObject = new ASBAnimationRetargeting
            {
                id = babylonNode.AnimationTargetId
            };

            if (gltfNode != null)
            {
                if (gltfNode.extensions == null)
                {
                    gltfNode.extensions = new GLTFExtensions();
                }
                gltfNode.extensions[AsoboAnimationRetargeting] = extensionObject;
            }

            if (gltf.extensionsUsed == null)
            {
                gltf.extensionsUsed = new List<string>();
            }
            if (!gltf.extensionsUsed.Contains(AsoboAnimationRetargeting))
            {
                gltf.extensionsUsed.Add(AsoboAnimationRetargeting);
            }
        }
    }

    
}

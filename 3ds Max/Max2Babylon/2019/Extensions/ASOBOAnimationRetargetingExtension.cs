using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BabylonExport.Entities;
using GLTFExport.Entities;

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
        private const string AsoboAnimationRetargeting = "ASB_animation_retargeting";

        public void ASOBOAnimationRetargetingExtension(ref GLTF gltf,ref GLTFChannel gltfChannel, ref GLTFNode gltfNode,BabylonNode babylonNode )
        {
            ASBAnimationRetargeting extensionObject = new ASBAnimationRetargeting
            {
                id = babylonNode.name
            };

            if (gltfChannel != null)
            {
                if (gltfChannel.extensions == null)
                {
                    gltfChannel.extensions = new GLTFExtensions();
                }
                gltfChannel.extensions[AsoboAnimationRetargeting] = extensionObject;
            }

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

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
    class ASBAnimationRetargeting: GLTFExtensions
    {
        [DataMember(EmitDefaultValue = false)]
        public string id { get; set; }
    }
    #endregion

    internal partial class GLTFExporter
    {
        public void ASOBOAnimationRetargetingExtension(ref GLTF gltf,ref GLTFChannel gltfChannel, ref GLTFNode gltfNode,BabylonNode babylonNode )
        {
            ASBAnimationRetargeting extension = new ASBAnimationRetargeting();
            extension.id = babylonNode.name;

            if (gltfChannel.extensions == null)
            {
                gltfChannel.extensions = new GLTFExtensions();
            }
            gltfChannel.extensions["ASB_animation_retargeting"] = extension;


            if (gltfNode.extensions == null)
            {
                gltfNode.extensions = new GLTFExtensions();
            }

            //gltfNode.extensions["ASB_animation_retargeting"] = extension;


            if (gltf.extensionsUsed == null)
            {
                gltf.extensionsUsed = new List<string>();
            }
            if (!gltf.extensionsUsed.Contains("ASB_animation_retargeting"))
            {
                gltf.extensionsUsed.Add("ASB_animation_retargeting");
            }
        }
    }

    
}

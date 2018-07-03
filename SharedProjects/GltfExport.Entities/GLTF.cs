using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTF
    {
        [DataMember(IsRequired = true)]
        public GLTFAsset asset { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<string> extensionsUsed { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<string> extensionsRequired { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? scene { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFScene[] scenes { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFNode[] nodes { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFCamera[] cameras { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFMesh[] meshes { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFAccessor[] accessors { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFBufferView[] bufferViews { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFBuffer[] buffers { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFMaterial[] materials { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFTexture[] textures { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFImage[] images { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFSampler[] samplers { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFAnimation[] animations { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFSkin[] skins { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFExtensions extensions { get; set; }

        public string OutputFolder { get; private set; }
        public string OutputFile { get; private set; }

        public List<GLTFNode> NodesList { get; private set; }
        public List<GLTFCamera> CamerasList { get; private set; }
        public List<GLTFBuffer> BuffersList { get; private set; }
        public List<GLTFBufferView> BufferViewsList { get; private set; }
        public List<GLTFAccessor> AccessorsList { get; private set; }
        public List<GLTFMesh> MeshesList { get; private set; }
        public List<GLTFMaterial> MaterialsList { get; private set; }
        public List<GLTFTexture> TexturesList { get; private set; }
        public List<GLTFImage> ImagesList { get; private set; }
        public List<GLTFSampler> SamplersList { get; private set; }
        public List<GLTFAnimation> AnimationsList { get; private set; }
        public List<GLTFSkin> SkinsList { get; private set; }


        public GLTFBuffer buffer;
        public GLTFBufferView bufferViewScalar;
        public GLTFBufferView bufferViewFloatVec3;
        public GLTFBufferView bufferViewFloatVec4;
        public GLTFBufferView bufferViewFloatMat4;
        public GLTFBufferView bufferViewUnsignedShortVec4;
        public GLTFBufferView bufferViewFloatVec2;
        public GLTFBufferView bufferViewImage;
        public GLTFBufferView bufferViewAnimationFloatScalar;
        public GLTFBufferView bufferViewAnimationFloatVec3;
        public GLTFBufferView bufferViewAnimationFloatVec4;

        public GLTF(string outputPath)
        {
            OutputFolder = Path.GetDirectoryName(outputPath);
            OutputFile = Path.GetFileNameWithoutExtension(outputPath);

            NodesList = new List<GLTFNode>();
            CamerasList = new List<GLTFCamera>();
            BuffersList = new List<GLTFBuffer>();
            BufferViewsList = new List<GLTFBufferView>();
            AccessorsList = new List<GLTFAccessor>();
            MeshesList = new List<GLTFMesh>();
            MaterialsList = new List<GLTFMaterial>();
            TexturesList = new List<GLTFTexture>();
            ImagesList = new List<GLTFImage>();
            SamplersList = new List<GLTFSampler>();
            AnimationsList = new List<GLTFAnimation>();
            SkinsList = new List<GLTFSkin>();
            extensionsUsed = new List<string>();
            extensionsRequired = new List<string>();
            extensions = new GLTFExtensions();
        }

        public void Prepare()
        {
            scenes[0].Prepare();

            // Do not export empty arrays
            if (NodesList.Count > 0)
            {
                nodes = NodesList.ToArray();
                NodesList.ForEach(node => node.Prepare());
            }
            if (CamerasList.Count > 0)
            {
                cameras = CamerasList.ToArray();
            }
            if (BuffersList.Count > 0)
            {
                buffers = BuffersList.ToArray();
            }
            if (BufferViewsList.Count > 0)
            {
                bufferViews = BufferViewsList.ToArray();
            }
            if (AccessorsList.Count > 0)
            {
                accessors = AccessorsList.ToArray();
            }
            if (MeshesList.Count > 0)
            {
                meshes = MeshesList.ToArray();
            }
            if (MaterialsList.Count > 0)
            {
                materials = MaterialsList.ToArray();
            }
            if (TexturesList.Count > 0)
            {
                textures = TexturesList.ToArray();
            }
            if (ImagesList.Count > 0)
            {
                images = ImagesList.ToArray();
            }
            if (SamplersList.Count > 0)
            {
                samplers = SamplersList.ToArray();
            }
            if (AnimationsList.Count > 0)
            {
                var animationsList = new List<GLTFAnimation>();
                AnimationsList.ForEach(animation =>
                {
                    animation.Prepare();
                    // Exclude empty animations
                    if (animation.channels != null)
                    {
                        animationsList.Add(animation);
                    }
                });
                if (animationsList.Count > 0)
                {
                    animations = animationsList.ToArray();
                }
            }
            if (SkinsList.Count > 0)
            {
                skins = SkinsList.ToArray();
            }
            if (extensionsUsed != null && extensionsUsed.Count == 0)
            {
                extensionsUsed = null;
            }
            if (extensionsRequired != null && extensionsRequired.Count == 0)
            {
                extensionsRequired = null;
            }
            if (extensions != null && extensions.Count == 0)
            {
                extensions = null;
            }
        }
        
        public GLTFSampler AddSampler()
        {
            GLTFSampler gltfSampler = new GLTFSampler();
            gltfSampler.index = SamplersList.Count;
            SamplersList.Add(gltfSampler);
            return gltfSampler;
        }

        public GLTFImage AddImage()
        {
            GLTFImage gltfImage = new GLTFImage();
            gltfImage.index = ImagesList.Count;
            ImagesList.Add(gltfImage);
            return gltfImage;
        }

        public GLTFTexture AddTexture(GLTFImage image, GLTFSampler sampler)
        {
            GLTFTexture gltfTexture = new GLTFTexture();
            gltfTexture.index = TexturesList.Count;
            gltfTexture.sampler = sampler?.index;
            gltfTexture.source = image?.index;
            TexturesList.Add(gltfTexture);
            return gltfTexture;
        }
    }
}

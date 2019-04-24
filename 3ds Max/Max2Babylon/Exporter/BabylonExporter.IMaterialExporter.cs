using System;
using System.Drawing;
using Autodesk.Max;
using BabylonExport.Entities;
using GLTFExport.Entities;

namespace Max2Babylon
{
    

    public interface IMaterialExporter
    {
        ClassIDWrapper MaterialClassID { get; }
    }

    public interface IBabylonMaterialExporter : IMaterialExporter
    {
        BabylonMaterial ExportBabylonMaterial(IIGameMaterial material);
    }

    delegate string TryWriteImageCallback(string sourceTexturePath);
    
    internal interface IGLTFMaterialExporter : IMaterialExporter
    {
        /// <summary>
        /// Creates a GLTF material using the given GameMaterial.
        /// </summary>
        /// <param name="exporter">The current exporter, for export parameters like CopyTexturesToOuput.</param>
        /// <param name="gltf">The GLTF output structure, for adding instances of classes such as GLTFSampler, GLTFImage and GLTFTexture.</param>
        /// <param name="material">The input material matching the MaterialClassID defined by the exporter. </param>
        /// <param name="tryWriteImageFunc">Callback function to verify images and to write images to the output folder. 
        /// Takes the source path and the output texture name.
        /// Returns null if the file was not written because of an error, else the output file extension.</param>
        /// <param name="raiseMessageAction">Callback function to raise messages. Takes a message and a color.</param>
        /// <param name="raiseWarningAction">Callback function to raise warnings.</param>
        /// <param name="raiseErrorAction">Callback function to raise errors.</param>
        /// <returns>The exported GLTF material.</returns>
        GLTFMaterial ExportGLTFMaterial(BabylonExporter exporter, GLTF gltf, IIGameMaterial material, Func<string, string, string> tryWriteImageFunc,
            Action<string, Color> raiseMessageAction, Action<string> raiseWarningAction, Action<string> raiseErrorAction);
    }

    // We require a separate struct, because the IClass_ID does not implement GetHashCode etc. to work with dictionaries
    public struct ClassIDWrapper : IEquatable<ClassIDWrapper>
    {
        public static ClassIDWrapper XRef_Material = new ClassIDWrapper(0x272c0d4b, 0x432a414b);
        public static ClassIDWrapper Advanced_Lighting_Override_Material = new ClassIDWrapper(0x2914493d, 0x6cff42f7);
        public static ClassIDWrapper Morpher_Material = new ClassIDWrapper(0x4b9937e0, 0x3a1c3da4);
        public static ClassIDWrapper Architectural_Material = new ClassIDWrapper(0x13d11bbe, 0x691e3037);
        public static ClassIDWrapper Autodesk_Generic_Material = new ClassIDWrapper(0x1ed415e4, 0x213daaf8);
        public static ClassIDWrapper Ink_n_Paint_Material = new ClassIDWrapper(0x01a8169a, 0x4d3960a5);
        public static ClassIDWrapper Map_to_Material_Conversion = new ClassIDWrapper(0x48e04183, 0xa129081c);
        public static ClassIDWrapper Standard_Material = new ClassIDWrapper(0x00000002, 0x00000000);
        public static ClassIDWrapper Multi_Sub_Object_Material = new ClassIDWrapper(0x00000200, 0x00000000);
        public static ClassIDWrapper Double_Sided_Material = new ClassIDWrapper(0x00000210, 0x00000000);
        public static ClassIDWrapper Blend_Material = new ClassIDWrapper(0x00000250, 0x00000000);
        public static ClassIDWrapper Matte_Shadow_Material = new ClassIDWrapper(0x00000260, 0x00000000);
        public static ClassIDWrapper Top_Bottom_Material = new ClassIDWrapper(0x00000100, 0x00000000);
        public static ClassIDWrapper Composite_Material = new ClassIDWrapper(0x61dc0cd7, 0x13640af6);
        public static ClassIDWrapper Shell_Material = new ClassIDWrapper(0x00000255, 0x00000000);
        public static ClassIDWrapper Physical_Material = new ClassIDWrapper(0x3d6b1cec, 0xdeadc001);
        public static ClassIDWrapper Raytrace_Material = new ClassIDWrapper(0x27190ff4, 0x329b106e);
        public static ClassIDWrapper Shellac_Material = new ClassIDWrapper(0x46ee536a, 0x00000000);
        public static ClassIDWrapper mental_ray_Material = new ClassIDWrapper(0x6926ba21, 0x7a10aca5);
        public static ClassIDWrapper Map_to_Material = new ClassIDWrapper(0x8ccdf7bc, 0x72928e19);
        public static ClassIDWrapper DirectX_Shader_Material = new ClassIDWrapper(0x0ed995e4, 0x6133daf2);
        public static ClassIDWrapper Ray_Switch_Shader_Material = new ClassIDWrapper(0x7e73161f, 0x4c074e86);
        public static ClassIDWrapper Lambert_Material = new ClassIDWrapper(0x7e73161f, 0xa80b5727);
        public static ClassIDWrapper Standard_Surface_Material = new ClassIDWrapper(0x7e73161f, 0x62f74b4c);
        public static ClassIDWrapper Standard_Hair_Material = new ClassIDWrapper(0x7e73161f, 0xa964c158);
        public static ClassIDWrapper Car_Paint_Material = new ClassIDWrapper(0x7e73161f, 0x770d4485);
        public static ClassIDWrapper Mix_Shader_Material = new ClassIDWrapper(0x7e73161f, 0x4f30a69d);
        public static ClassIDWrapper Atmosphere_Volume_Material = new ClassIDWrapper(0x7e73161f, 0x57215188);
        public static ClassIDWrapper Fog_Material = new ClassIDWrapper(0x7e73161f, 0x4659b384);
        public static ClassIDWrapper Standard_Volume_Material = new ClassIDWrapper(0x7e73161f, 0xac0b525b);
        public static ClassIDWrapper AOV_Write_Float_Material = new ClassIDWrapper(0x7e73161f, 0x8cff673a);
        public static ClassIDWrapper AOV_Write_Int_Material = new ClassIDWrapper(0x7e73161f, 0x8b688625);
        public static ClassIDWrapper AOV_Write_RGB_Material = new ClassIDWrapper(0x7e73161f, 0x925f862f);
        public static ClassIDWrapper Matte_Material = new ClassIDWrapper(0x7e73161f, 0xba66a526);
        public static ClassIDWrapper Passthrough_Material = new ClassIDWrapper(0x7e73161f, 0x625bb28f);
        public static ClassIDWrapper Switch_Shader_Material = new ClassIDWrapper(0x7e73161f, 0xa844c228);
        public static ClassIDWrapper Two_Sided_Material = new ClassIDWrapper(0x7e73161f, 0x7ffd6281);



        private uint partA, partB;
        public ClassIDWrapper(IClass_ID classID) { partA = classID.PartA; partB = classID.PartB; }
        public ClassIDWrapper(uint partA, uint partB) { this.partA = partA; this.partB = partB; }
        
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            ClassIDWrapper other = (ClassIDWrapper)obj;
            return Equals(other);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + partA.GetHashCode();
                hash = hash * 23 + partB.GetHashCode();
                return hash;
            }
        }

        public bool Equals(ClassIDWrapper other)
        {
            return partA.Equals(other.partA) && partB.Equals(other.partB);
        }
        public bool Equals(IClass_ID other)
        {
            return partA.Equals(other.PartA) && partB.Equals(other.PartB);
        }

        public static bool operator ==(ClassIDWrapper lhs, ClassIDWrapper rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(ClassIDWrapper lhs, ClassIDWrapper rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
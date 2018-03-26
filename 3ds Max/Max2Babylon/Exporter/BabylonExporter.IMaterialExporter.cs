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
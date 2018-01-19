using System;
using Autodesk.Max;
using BabylonExport.Entities;

namespace Max2Babylon
{
    public interface IMaterialExporter
    {
        ClassIDWrapper MaterialClassID { get; }

		bool IsBabylonExporter { get; }
		bool IsGltfExporter { get; }

        BabylonMaterial ExportBabylonMaterial(IIGameMaterial material);
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
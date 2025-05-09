using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;


namespace BabylonExport.Entities
{
    public interface IBabylonMeshData
    {
        float[] positions { get; set; }
        float[] normals { get; set; }
        float[] tangents { get; set; }
        float[] uvs { get; set; }
        float[] uvs2 { get; set; }
        float[] uvs3 { get; set; }
        float[] uvs4 { get; set; }
        float[] uvs5 { get; set; }
        float[] uvs6 { get; set; }
        float[] uvs7 { get; set; }
        float[] uvs8 { get; set; }
        float[] colors { get; set; }
        uint[] matricesIndices { get; set; }
        uint[] matricesIndicesExtra { get; set; }
        float[] matricesWeights { get; set; }
        float[] matricesWeightsExtra { get; set; }
        int[] indices { get; set; }

        bool TryPackIndexArrays();
    }

    [DataContract]
    public class BabylonMesh : BabylonAbstractMesh, IBabylonMeshData
    {
        [DataMember]
        public string materialId { get; set; }
        
        [DataMember]
        public string geometryId { get; set; }

        [DataMember]
        public bool isEnabled { get; set; }

        [DataMember]
        public bool isVisible { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float[] pivotMatrix { get; set; }

        [DataMember]
        public float[] positions { get; set; }

        [DataMember]
        public float[] normals { get; set; }

        [DataMember]
        public float[] tangents { get; set; }

        [DataMember]
        public float[] uvs { get; set; }

        [DataMember]
        public float[] uvs2 { get; set; }

        [DataMember]
        public float[] uvs3 { get; set; }

        [DataMember]
        public float[] uvs4 { get; set; }

        [DataMember]
        public float[] uvs5 { get; set; }

        [DataMember]
        public float[] uvs6 { get; set; }

        [DataMember]
        public float[] uvs7 { get; set; }

        [DataMember]
        public float[] uvs8 { get; set; }

        [DataMember]
        public float[] colors { get; set; }

        [DataMember]
        public bool hasVertexAlpha { get; set; }

        [DataMember]
        public uint[] matricesIndices { get; set; }

        [DataMember]
        public float[] matricesWeights { get; set; }

        [DataMember]
        public uint[] matricesIndicesExtra { get; set; }

        [DataMember]
        public float[] matricesWeightsExtra { get; set; }

        [DataMember]
        public int[] indices { get; set; }

        [DataMember]
        public bool receiveShadows { get; set; }    
    
        [DataMember]
        public bool infiniteDistance { get; set; }
        
        [DataMember]
        public int billboardMode { get; set; }

        [DataMember]
        public float visibility { get; set; }

        [DataMember]
        public BabylonSubMesh[] subMeshes { get; set; }

        [DataMember]
        public BabylonAbstractMesh[] instances { get; set; }

        [DataMember]
        public int skeletonId { get; set; }

        [DataMember]
        public int numBoneInfluencers { get; set; }

        [DataMember]
        public bool applyFog { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? morphTargetManagerId { get; set; }

        [DataMember]
        public string[] lodMeshIds { get; set; }

        [DataMember]
        public int[] lodDistances { get; set; }

        [DataMember]
        public float[] lodCoverages { get; set; }

        [DataMember]
        public bool matricesIndicesExpanded { get; set; }

        [DataMember]
        public bool matricesIndicesExtraExpanded { get; set; }

        public bool isDummy = false;

        public List<VertexData> VertexDatas { get; set; } = new List<VertexData>();

        public BabylonMesh()
        {
            isEnabled = true;
            isVisible = true;
            matricesIndicesExpanded = false;
            matricesIndicesExtraExpanded = false;

            billboardMode = 0;

            visibility = 1.0f;

            skeletonId = -1;

            pickable = true;

            numBoneInfluencers = 4;

            lodMeshIds = null;
            lodCoverages = null;
            lodDistances = null;

            position = new float[] { 0, 0, 0 };
        }

        // sometimes the skinning weights can be a tad off between otherwise identically skinned meshes. This factor allow a variance of 5% influence.
        internal static float SkinningWeightToleranceThreshold = 0.05f;


        internal static bool MeshesShareSkin(BabylonMesh matchingSkinnedMesh, BabylonMesh babylonMesh)
        {
            // check if the skinning matrix indices are equivalent
            if (!babylonMesh.matricesIndices.SequenceEqual(matchingSkinnedMesh.matricesIndices))
            {
                return false;
            }

            // finally, compare the skinning matrix weights within a tolerance threshold.
            var skinDifference = babylonMesh.matricesWeights.Zip(matchingSkinnedMesh.matricesWeights, (first, second) => first - second).ToArray();
            return skinDifference.All(value => Math.Abs(value) < BabylonMesh.SkinningWeightToleranceThreshold);
        }

        private uint[] CreatePackedArray(uint[] rawArray)
        {
            var arrayReplacement = new uint[rawArray.Length / 4];

            for (int i = 0; i < arrayReplacement.Length; i++)
            {
                int rawIndex = i * 4;
                uint bone0 = rawArray[rawIndex];
                uint bone1 = rawArray[rawIndex + 1];
                uint bone2 = rawArray[rawIndex + 2];
                uint bone3 = rawArray[rawIndex + 3];
                arrayReplacement[i] = (bone3 << 24) | (bone2 << 16) | (bone1 << 8) | bone0;
            }

            return arrayReplacement;
        }

        public bool TryPackIndexArrays()
        {
            bool result = true;

            if (matricesIndices != null && matricesIndices.Length != 0)
            {
                if (matricesIndices != null && matricesIndices.Any(a => a > 255))
                {
                    matricesIndicesExpanded = true;
                    result = false;
                }
                else
                {
                    matricesIndices = CreatePackedArray(matricesIndices);
                }
            }

            if (matricesIndicesExtra != null && matricesIndicesExtra.Length != 0)
            {
                if (matricesIndicesExtra != null && matricesIndicesExtra.Any(a => a > 255))
                {
                    matricesIndicesExtraExpanded = true;
                    result = false;
                }
                else
                {
                    matricesIndicesExtra = CreatePackedArray(matricesIndicesExtra);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Store the data of the vertex used to extract the geometry.
    /// It used by the morph target in order to have the same vertex order between the mesh and the target.
    /// </summary>
    public class VertexData
    {
        public int polygonId { get; set; }
        public int vertexIndexGlobal { get; set; }
        public int vertexIndexLocal { get; set; }

        public VertexData(int _polygonId, int _vertexIndexGlobal, int _vertexIndexLocal)
        {
            polygonId = _polygonId;
            vertexIndexGlobal = _vertexIndexGlobal;
            vertexIndexLocal = _vertexIndexLocal;
        }

    }
}

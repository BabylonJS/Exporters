using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonMesh : BabylonAbstractMesh
    {
        [DataMember]
        public string materialId { get; set; }

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
        public float[] colors { get; set; }

        [DataMember]
        public bool hasVertexAlpha { get; set; }

        [DataMember]
        public int[] matricesIndices { get; set; }

        [DataMember]
        public float[] matricesWeights { get; set; }

        [DataMember]
        public int[] matricesIndicesExtra { get; set; }

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

        [DataMember]
        public object metadata { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? morphTargetManagerId { get; set; }

        public bool isDummy = false;

        public List<VertexData> VertexDatas { get; set; } = new List<VertexData>();

        public BabylonMesh()
        {
            isEnabled = true;
            isVisible = true;

            billboardMode = 0;

            visibility = 1.0f;

            skeletonId = -1;

            pickable = true;

            numBoneInfluencers = 4;

            position = new float[] { 0, 0, 0 };
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
